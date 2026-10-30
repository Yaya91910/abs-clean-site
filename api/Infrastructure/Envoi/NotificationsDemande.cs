using AbsClean.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AbsClean.Api.Infrastructure.Envoi;

/// <summary>
/// Après l'enregistrement d'une demande (CAHIER-DES-CHARGES.md §9.4 à 9.8) : PDF, e-mail au client, e-mail au gérant, SMS au gérant.
/// Chaque envoi est indépendant : un échec est journalisé, enregistré sur la demande, et n'empêche pas les autres.
/// </summary>
public sealed class NotificationsDemande(
    AbsCleanDb db,
    DevisPdf pdf,
    IEnvoiEmail email,
    IEnvoiSms sms,
    IOptions<EmailOptions> emailOptions,
    IOptions<SmsOptions> smsOptions,
    ServiceParrainage parrainage,
    ILogger<NotificationsDemande> log)
{
    /// <param name="jetonIntervention">Jeton brut du lien « intervention réalisée » placé dans l'e-mail du gérant.</param>
    public async Task EnvoyerAsync(Demande demande, string jetonIntervention, CancellationToken ct)
    {
        var code = demande.CodeParrainPersonnel;
        var lienValidation = parrainage.LienValidation(demande.Id, jetonIntervention);
        var fiche = await FicheAsync(demande, lienValidation, ct);
        var exemplaireClient = GenererPdf(demande, $"Devis-{demande.Numero}.pdf", () => pdf.GenererClient(fiche));
        var exemplaireGerant = GenererPdf(demande, $"Fiche-{demande.Numero}-{demande.Client.Numero}.pdf", () => pdf.GenererGerant(fiche));

        demande.EmailClientEnvoye = await TenterAsync("e-mail client", demande, () =>
            email.EnvoyerAsync(ModelesEmail.Client(demande, exemplaireClient, parrainage.Lien(code), parrainage.LienQrPng(code)), ct));

        var adresseGerant = emailOptions.Value.GerantTo;
        demande.EmailGerantEnvoye = string.IsNullOrWhiteSpace(adresseGerant)
            ? Absent("e-mail gérant", "Email__GerantTo", demande)
            : await TenterAsync("e-mail gérant", demande, () => email.EnvoyerAsync(
                ModelesEmail.Gerant(fiche, adresseGerant, exemplaireGerant, lienValidation), ct));

        var numeroGerant = Sms.VersInternational(smsOptions.Value.GerantNumero);
        demande.SmsEnvoye = numeroGerant is null
            ? Absent("SMS gérant", "Sms__GerantNumero", demande)
            : await TenterAsync("SMS gérant", demande, () => sms.EnvoyerAsync(numeroGerant, Sms.NouveauDevis(demande), ct));

        await db.SaveChangesAsync(ct);
    }

    /// <summary>Historique du client (rang de la demande, demande précédente) et QR codes des deux exemplaires.</summary>
    private async Task<FicheDevis> FicheAsync(Demande demande, string lienValidation, CancellationToken ct)
    {
        var precedentes = db.Demandes.Where(d => d.ClientId == demande.ClientId && d.Id < demande.Id);
        var rang = await precedentes.CountAsync(ct) + 1;
        var precedenteLe = await precedentes.OrderByDescending(d => d.Id).Select(d => (DateTime?)d.CreeLe).FirstOrDefaultAsync(ct);
        return new FicheDevis(demande, rang, precedenteLe,
            parrainage.QrSvg(demande.CodeParrainPersonnel), ServiceParrainage.QrSvgDe(lienValidation));
    }

    private PieceJointe? GenererPdf(Demande demande, string nom, Func<byte[]> generer)
    {
        try
        {
            return new PieceJointe(nom, generer());
        }
        catch (Exception e)
        {
            log.LogError(e, "Génération du PDF {Nom} impossible pour {Numero} : l'e-mail part sans pièce jointe.", nom, demande.Numero);
            return null;
        }
    }

    private async Task<bool> TenterAsync(string envoi, Demande demande, Func<Task> action)
    {
        try
        {
            await action();
            return true;
        }
        catch (Exception e)
        {
            log.LogError(e, "Échec de l'envoi ({Envoi}) pour la demande {Numero}.", envoi, demande.Numero);
            return false;
        }
    }

    private bool Absent(string envoi, string variable, Demande demande)
    {
        log.LogError("Envoi impossible ({Envoi}) pour {Numero} : {Variable} n'est pas configuré.", envoi, demande.Numero, variable);
        return false;
    }
}
