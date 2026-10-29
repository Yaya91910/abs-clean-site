using System.Net;
using AbsClean.Api.Domain;
using Microsoft.Extensions.Options;

namespace AbsClean.Api.Infrastructure.Envoi;

/// <summary>
/// Après l'enregistrement d'une demande (CAHIER-DES-CHARGES.md §9.4 à 9.8) : PDF, e-mail au client, e-mail au gérant, SMS au gérant.
/// Chaque envoi est indépendant : un échec est journalisé et n'empêche pas les autres.
/// </summary>
public sealed class NotificationsDemande(
    AbsCleanDb db,
    DevisPdf pdf,
    IEnvoiEmail email,
    IEnvoiSms sms,
    IOptions<EmailOptions> emailOptions,
    IOptions<SmsOptions> smsOptions,
    IOptions<SiteOptions> site,
    ServiceParrainage parrainage,
    ILogger<NotificationsDemande> log)
{
    public async Task EnvoyerAsync(DemandeEnregistree enregistree, CancellationToken ct)
    {
        var demande = enregistree.Demande;
        PieceJointe? devisPdf = null;
        try
        {
            devisPdf = new PieceJointe($"Devis-{demande.Numero}.pdf", pdf.Generer(demande, parrainage.QrSvg(demande.CodeParrainPersonnel)));
        }
        catch (Exception e)
        {
            log.LogError(e, "Génération du PDF impossible pour {Numero} : les e-mails partent sans pièce jointe.", demande.Numero);
        }

        demande.EmailClientEnvoye = await TenterAsync("e-mail client", demande, () =>
            email.EnvoyerAsync(EmailClient(demande, devisPdf, parrainage.Lien(demande.CodeParrainPersonnel), QrPng(demande)), ct));

        var gerant = emailOptions.Value.GerantTo;
        demande.EmailGerantEnvoye = string.IsNullOrWhiteSpace(gerant)
            ? Absent("e-mail gérant", "Email__GerantTo", demande)
            : await TenterAsync("e-mail gérant", demande, () => email.EnvoyerAsync(
                EmailGerant(demande, gerant, devisPdf, parrainage.LienValidation(demande.Id, enregistree.JetonIntervention)), ct));

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

    /// <summary>QR code en PNG servi par l'API (les messageries n'affichent pas le SVG) ; suppose /api servi sur le domaine du site.</summary>
    private string QrPng(Demande d) => $"{site.Value.Url.TrimEnd('/')}/api/parrainage/{Uri.EscapeDataString(d.CodeParrainPersonnel)}/qr.png";

    private static EmailSortant EmailClient(Demande d, PieceJointe? devisPdf, string lienParrainage, string qrPng) => new(
        d.Email,
        $"{d.Prenom} {d.Nom}",
        $"Votre devis ABS clean {d.Numero}",
        Gabarit($"""
            <p>Bonjour {H(d.Prenom)},</p>
            <p>Merci pour votre demande. {(devisPdf is null ? "Voici le récapitulatif de votre devis" : "Vous trouverez votre devis en pièce jointe")}
            (n° <strong>{H(d.Numero)}</strong>) : <strong>à partir de {DevisPdf.Euros(d.Total)}</strong>.</p>
            <p>{Tarifs.MentionEstimation}. ABS clean vous recontacte rapidement pour fixer l'intervention.</p>
            <table style="margin:20px 0;border:1px solid #D7E0EA;border-radius:12px;padding:12px"><tr>
              <td style="padding-right:14px"><img src="{H(qrPng)}" width="110" height="110" alt="QR code de parrainage"></td>
              <td><div style="font-size:12px;font-weight:bold;color:#2F6FD6;text-transform:uppercase">Votre code parrain</div>
                <div style="font-size:20px;font-weight:bold">{H(d.CodeParrainPersonnel)}</div>
                <div style="color:#5B6B80">−10 % sur votre prochaine intervention pour chaque proche qui devient client.
                Partagez ce lien : <a href="{H(lienParrainage)}">{H(lienParrainage)}</a></div></td>
            </tr></table>
            <p>À bientôt,<br>ABS clean</p>
            """),
        PieceJointe: devisPdf);

    private static EmailSortant EmailGerant(Demande d, string gerant, PieceJointe? devisPdf, string lienValidation)
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
            ("Code parrain utilisé", d.CodeParrainUtilise),
        };
        var tableau = string.Concat(lignes.Where(l => !string.IsNullOrWhiteSpace(l.Item2)).Select(l =>
            $"<tr><th align=\"left\" style=\"padding:6px 12px 6px 0;color:#5B6B80;font-weight:600;vertical-align:top\">{l.Item1}</th><td style=\"padding:6px 0\">{H(l.Item2!)}</td></tr>"));

        return new EmailSortant(
            gerant,
            "ABS clean",
            $"Nouveau devis {d.Numero} — {d.Prenom} {d.Nom}, {d.Ville}",
            Gabarit($"""
                <p>Nouvelle demande de devis reçue sur le site.</p><table>{tableau}</table>
                <p style="margin-top:24px">Une fois la première intervention réalisée :<br>
                <a href="{H(lienValidation)}" style="display:inline-block;margin-top:8px;padding:10px 18px;border-radius:999px;background:#2F6FD6;color:#FFFFFF;text-decoration:none;font-weight:bold">Marquer l'intervention comme réalisée</a></p>
                <p style="font-size:12px;color:#5B6B80">Lien personnel à usage unique : ne le transférez pas.{(d.CodeParrainUtilise is null ? "" : " La confirmation active la remise de −10 % du parrain et le prévient par e-mail.")}</p>
                """),
            RepondreA: d.Email,
            PieceJointe: devisPdf);
    }

    private static string Gabarit(string contenu) =>
        $"""<div style="font-family:Arial,sans-serif;font-size:15px;line-height:1.6;color:#15345F;max-width:560px">{contenu}</div>""";

    /// <summary>Échappe les saisies du client avant de les placer dans un e-mail HTML.</summary>
    private static string H(string valeur) => WebUtility.HtmlEncode(valeur);
}
