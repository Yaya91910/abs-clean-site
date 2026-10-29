using AbsClean.Api.Domain;
using Microsoft.Extensions.Options;

namespace AbsClean.Api.Infrastructure.Envoi;

/// <summary>
/// Après l'enregistrement d'une demande (CLAUDE.md §9.4 à 9.8) : PDF, e-mail au client, e-mail au gérant, SMS au gérant.
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
        var devisPdf = GenererPdf(demande);
        var code = demande.CodeParrainPersonnel;

        demande.EmailClientEnvoye = await TenterAsync("e-mail client", demande, () =>
            email.EnvoyerAsync(ModelesEmail.Client(demande, devisPdf, parrainage.Lien(code), parrainage.LienQrPng(code)), ct));

        var adresseGerant = emailOptions.Value.GerantTo;
        demande.EmailGerantEnvoye = string.IsNullOrWhiteSpace(adresseGerant)
            ? Absent("e-mail gérant", "Email__GerantTo", demande)
            : await TenterAsync("e-mail gérant", demande, () => email.EnvoyerAsync(
                ModelesEmail.Gerant(demande, adresseGerant, devisPdf, parrainage.LienValidation(demande.Id, jetonIntervention)), ct));

        var numeroGerant = Sms.VersInternational(smsOptions.Value.GerantNumero);
        demande.SmsEnvoye = numeroGerant is null
            ? Absent("SMS gérant", "Sms__GerantNumero", demande)
            : await TenterAsync("SMS gérant", demande, () => sms.EnvoyerAsync(numeroGerant, Sms.NouveauDevis(demande), ct));

        await db.SaveChangesAsync(ct);
    }

    private PieceJointe? GenererPdf(Demande demande)
    {
        try
        {
            return new PieceJointe($"Devis-{demande.Numero}.pdf", pdf.Generer(demande, parrainage.QrSvg(demande.CodeParrainPersonnel)));
        }
        catch (Exception e)
        {
            log.LogError(e, "Génération du PDF impossible pour {Numero} : les e-mails partent sans pièce jointe.", demande.Numero);
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
