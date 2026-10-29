using System.Net;
using AbsClean.Api.Domain;
using Microsoft.Extensions.Options;

namespace AbsClean.Api.Infrastructure.Envoi;

/// <summary>
/// Après l'enregistrement d'une demande (CLAUDE.md §9.4 à 9.8) : PDF, e-mail au client, e-mail au gérant, SMS au gérant.
/// Chaque envoi est indépendant : un échec est journalisé et n'empêche pas les autres.
/// </summary>
public sealed class NotificationsDemande(
    AbsCleanDb db,
    DevisPdf pdf,
    IEnvoiEmail email,
    IEnvoiSms sms,
    IOptions<EmailOptions> emailOptions,
    IOptions<SmsOptions> smsOptions,
    ILogger<NotificationsDemande> log)
{
    public async Task EnvoyerAsync(Demande demande, CancellationToken ct)
    {
        PieceJointe? devisPdf = null;
        try
        {
            devisPdf = new PieceJointe($"Devis-{demande.Numero}.pdf", pdf.Generer(demande));
        }
        catch (Exception e)
        {
            log.LogError(e, "Génération du PDF impossible pour {Numero} : les e-mails partent sans pièce jointe.", demande.Numero);
        }

        demande.EmailClientEnvoye = await TenterAsync("e-mail client", demande, () =>
            email.EnvoyerAsync(EmailClient(demande, devisPdf), ct));

        var gerant = emailOptions.Value.GerantTo;
        demande.EmailGerantEnvoye = string.IsNullOrWhiteSpace(gerant)
            ? Absent("e-mail gérant", "Email__GerantTo", demande)
            : await TenterAsync("e-mail gérant", demande, () => email.EnvoyerAsync(EmailGerant(demande, gerant, devisPdf), ct));

        var numeroGerant = Sms.VersInternational(smsOptions.Value.GerantNumero);
        demande.SmsEnvoye = numeroGerant is null
            ? Absent("SMS gérant", "Sms__GerantNumero", demande)
            : await TenterAsync("SMS gérant", demande, () => sms.EnvoyerAsync(numeroGerant, Sms.NouveauDevis(demande), ct));

        await db.SaveChangesAsync(ct);
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

    private static EmailSortant EmailClient(Demande d, PieceJointe? devisPdf) => new(
        d.Email,
        $"{d.Prenom} {d.Nom}",
        $"Votre devis ABS clean {d.Numero}",
        Gabarit($"""
            <p>Bonjour {H(d.Prenom)},</p>
            <p>Merci pour votre demande. {(devisPdf is null ? "Voici le récapitulatif de votre devis" : "Vous trouverez votre devis en pièce jointe")}
            (n° <strong>{H(d.Numero)}</strong>) : <strong>à partir de {DevisPdf.Euros(d.Total)}</strong>.</p>
            <p>{Tarifs.MentionEstimation}. ABS clean vous recontacte rapidement pour fixer l'intervention.</p>
            <p>À bientôt,<br>ABS clean</p>
            """),
        PieceJointe: devisPdf);

    private static EmailSortant EmailGerant(Demande d, string gerant, PieceJointe? devisPdf)
    {
        var lignes = new (string, string?)[]
        {
            ("Numéro", d.Numero),
            ("Total", $"à partir de {DevisPdf.Euros(d.Total)}"),
            ("Quantités", $"{d.NbVitres} vitre(s), {d.NbBaies} baie(s) vitrée(s), {d.NbVelux} velux" + (d.NbVitrines > 0 ? $", {d.NbVitrines} vitrine(s)" : "")),
            ("Profil", DevisPdf.Libelle(d.Profil)),
            ("Fréquence", DevisPdf.Libelle(d.Frequence)),
            ("Nom", $"{d.Prenom} {d.Nom}"),
            ("Téléphone", DevisPdf.FormatTelephone(d.Telephone)),
            ("E-mail", d.Email),
            ("Adresse", $"{d.Adresse}, {d.CodePostal} {d.Ville}"),
            ("Accès", d.Acces),
            ("Créneaux", d.Creneaux.Count > 0 ? string.Join(", ", d.Creneaux.Select(DevisPdf.Libelle)) : null),
            ("Commentaire", d.Commentaire),
        };
        var tableau = string.Concat(lignes.Where(l => !string.IsNullOrWhiteSpace(l.Item2)).Select(l =>
            $"<tr><th align=\"left\" style=\"padding:6px 12px 6px 0;color:#5B6B80;font-weight:600;vertical-align:top\">{l.Item1}</th><td style=\"padding:6px 0\">{H(l.Item2!)}</td></tr>"));

        return new EmailSortant(
            gerant,
            "ABS clean",
            $"Nouveau devis {d.Numero} — {d.Prenom} {d.Nom}, {d.Ville}",
            Gabarit($"<p>Nouvelle demande de devis reçue sur le site.</p><table>{tableau}</table>"),
            RepondreA: d.Email,
            PieceJointe: devisPdf);
    }

    private static string Gabarit(string contenu) =>
        $"""<div style="font-family:Arial,sans-serif;font-size:15px;line-height:1.6;color:#15345F;max-width:560px">{contenu}</div>""";

    /// <summary>Échappe les saisies du client avant de les placer dans un e-mail HTML.</summary>
    private static string H(string valeur) => WebUtility.HtmlEncode(valeur);
}
