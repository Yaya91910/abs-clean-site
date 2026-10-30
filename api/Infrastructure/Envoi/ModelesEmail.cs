using System.Net;
using AbsClean.Api.Domain;

namespace AbsClean.Api.Infrastructure.Envoi;

/// <summary>Contenu des e-mails envoyés par le site. Toute saisie du client est échappée (<see cref="H"/>).</summary>
public static class ModelesEmail
{
    public static EmailSortant Client(Demande d, PieceJointe? devisPdf, string lienParrainage, string qrPng) => new(
        d.Email,
        $"{d.Prenom} {d.Nom}",
        $"Votre devis ABS clean {d.Numero}",
        Gabarit($"""
            <p>Bonjour {H(d.Prenom)},</p>
            <p>Merci pour votre demande. {(devisPdf is null ? "Voici le récapitulatif de votre devis" : "Vous trouverez votre devis en pièce jointe")}
            (n° <strong>{H(d.Numero)}</strong>) : <strong>à partir de {DevisPdf.Euros(d.Total)}</strong>.</p>
            <p>Votre numéro client : <strong>{H(d.Client.Numero)}</strong> — à rappeler pour toute demande.</p>
            <p>{Tarifs.MentionEstimation}. ABS clean vous recontacte rapidement pour fixer l'intervention.</p>
            <table style="margin:20px 0;border:1px solid #D7E0EA;border-radius:12px;padding:12px"><tr>
              <td style="padding-right:14px"><img src="{H(qrPng)}" width="110" height="110" alt="QR code de parrainage"></td>
              <td><div style="font-size:12px;font-weight:bold;color:#2559B8;text-transform:uppercase">Votre code parrain</div>
                <div style="font-size:20px;font-weight:bold">{H(d.CodeParrainPersonnel)}</div>
                <div style="color:#5B6B80">−10 % sur votre prochaine intervention pour chaque proche qui devient client.
                Partagez ce lien : <a href="{H(lienParrainage)}">{H(lienParrainage)}</a></div></td>
            </tr></table>
            <p>À bientôt,<br>ABS clean</p>
            """),
        PieceJointe: devisPdf);

    public static EmailSortant Gerant(FicheDevis fiche, string adresseGerant, PieceJointe? fichePdf, string lienValidation)
    {
        var d = fiche.Demande;
        var lignes = new (string Titre, string? Valeur)[]
        {
            ("Numéro", d.Numero),
            ("N° client", d.Client.Numero),
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
        var tableau = string.Concat(lignes.Where(l => !string.IsNullOrWhiteSpace(l.Valeur)).Select(l =>
            $"""<tr><th align="left" style="padding:6px 12px 6px 0;color:#5B6B80;font-weight:600;vertical-align:top">{l.Titre}</th><td style="padding:6px 0">{H(l.Valeur!)}</td></tr>"""));
        var noteParrainage = d.CodeParrainUtilise is null ? "" : " La confirmation active la remise de −10 % du parrain et le prévient par e-mail.";

        // Client existant : mis en avant, avec téléphone et e-mail cliquables pour un contact rapide et personnalisé.
        var bandeau = $"""
            <div style="background:{(fiche.ClientExistant ? "#E8F1FF" : "#F4F8FF")};border-left:4px solid {(fiche.ClientExistant ? "#2F6FD6" : "#D7E0EA")};padding:12px 16px;margin-bottom:16px">
              <div style="font-size:12px;font-weight:bold;color:#2559B8;letter-spacing:0.06em">{(fiche.ClientExistant ? $"CLIENT EXISTANT · {fiche.RangDemande}e DEMANDE" : "NOUVEAU CLIENT")}</div>
              <div style="font-size:17px;font-weight:bold">{H(d.Prenom)} {H(d.Nom)} — {H(d.Client.Numero)}</div>
              <div style="margin-top:6px"><a href="tel:{H(d.Telephone)}" style="font-weight:bold;color:#2F6FD6;text-decoration:none">📞 {DevisPdf.FormatTelephone(d.Telephone)}</a>
              &nbsp;·&nbsp; <a href="mailto:{H(d.Email)}" style="color:#2F6FD6">✉ {H(d.Email)}</a></div>
            </div>
            """;

        return new EmailSortant(
            adresseGerant,
            "ABS clean",
            (fiche.ClientExistant ? "Client existant · " : "") + $"Nouveau devis {d.Numero} — {d.Prenom} {d.Nom}, {d.Ville}",
            Gabarit($"""
                {bandeau}<p>Nouvelle demande de devis reçue sur le site. La fiche d'intervention est jointe.</p><table>{tableau}</table>
                <p style="margin-top:24px">Une fois la première intervention réalisée :<br>
                <a href="{H(lienValidation)}" style="display:inline-block;margin-top:8px;padding:10px 18px;border-radius:999px;background:#2F6FD6;color:#FFFFFF;text-decoration:none;font-weight:bold">Marquer l'intervention comme réalisée</a></p>
                <p style="font-size:12px;color:#5B6B80">Lien personnel à usage unique : ne le transférez pas.{noteParrainage}</p>
                """),
            RepondreA: d.Email,
            PieceJointe: fichePdf);
    }

    public static EmailSortant RemiseParrain(Demande parrain, Demande filleul) => new(
        parrain.Email,
        $"{parrain.Prenom} {parrain.Nom}",
        $"Votre proche {filleul.Prenom} est devenu client : −10 % sur votre prochaine intervention",
        Gabarit($"""
            <p>Bonjour {H(parrain.Prenom)},</p>
            <p>Votre proche {H(filleul.Prenom)} est devenu client : <strong>−10 % sur votre prochaine intervention</strong>.</p>
            <p>Merci pour votre confiance,<br>ABS clean</p>
            """));

    private static string Gabarit(string contenu) =>
        $"""<div style="font-family:Arial,sans-serif;font-size:15px;line-height:1.6;color:#15345F;max-width:560px">{contenu}</div>""";

    private static string H(string valeur) => WebUtility.HtmlEncode(valeur);
}
