using System.Text;
using AbsClean.Api.Domain;
using AbsClean.Api.Infrastructure;
using AbsClean.Api.Infrastructure.Envoi;

namespace AbsClean.Api.Tests;

public class DevisPdfTests
{
    static DevisPdfTests() => QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

    /// <summary>Exemple réaliste : client existant (3e demande), formule mensuelle, parrainé, commentaire.</summary>
    public static FicheDevis Exemple()
    {
        var client = new Client
        {
            Id = 42, Numero = "CLI-00042", CreeLe = new DateTime(2026, 3, 12, 9, 0, 0, DateTimeKind.Utc),
            Nom = "Martin", Prenom = "Léa", Telephone = "0612345678", Email = "lea.martin@example.com",
        };
        var demande = new Demande
        {
            Id = 128, Numero = "ABS-2026-0128", CreeLe = new DateTime(2026, 11, 3, 9, 30, 0, DateTimeKind.Utc),
            Client = client, ClientId = client.Id,
            Profil = Profil.Particulier, Frequence = Frequence.Mensuel,
            NbVitres = 8, NbBaies = 2, NbVelux = 1, Total = 265m,
            Prenom = "Léa", Nom = "Martin", Telephone = "0612345678", Email = "lea.martin@example.com",
            Adresse = "12 rue des Lilas", CodePostal = "91000", Ville = "Évry-Courcouronnes", Acces = "3e étage, code 1234B",
            Creneaux = [Creneau.SemaineMatin, Creneau.Samedi],
            Commentaire = "Grandes baies côté jardin, accès par le portillon. Chien à la maison (gentil).",
            CodeParrainPersonnel = "ABS-1765321", CodeParrainUtilise = "ABS-4820596",
        };
        return new FicheDevis(demande, RangDemande: 3, DemandePrecedenteLe: new DateTime(2026, 6, 20, 14, 0, 0, DateTimeKind.Utc),
            QrParrainage: ServiceParrainage.QrSvgDe("https://abs-clean.example/devis?parrain=ABS-1765321"),
            QrValidation: ServiceParrainage.QrSvgDe("https://abs-clean.example/admin/intervention#demande=128&jeton=exemple"));
    }

    [Fact]
    public void Exemplaire_client_et_exemplaire_gerant_sont_des_pdf_distincts()
    {
        var pdf = new DevisPdf(new EntrepriseOptions());

        var client = pdf.GenererClient(Exemple());
        var gerant = pdf.GenererGerant(Exemple());

        Assert.StartsWith("%PDF-", Encoding.ASCII.GetString(client, 0, 5));
        Assert.StartsWith("%PDF-", Encoding.ASCII.GetString(gerant, 0, 5));
        Assert.NotEqual(client.Length, gerant.Length);
        Enregistrer("exemplaire-client.pdf", client);
        Enregistrer("exemplaire-gerant.pdf", gerant);
    }

    [Fact]
    public void Nouveau_client_sans_coordonnees_entreprise_ni_commentaire()
    {
        var fiche = Exemple() with { RangDemande = 1, DemandePrecedenteLe = null };
        fiche.Demande.Commentaire = null;
        fiche.Demande.CodeParrainUtilise = null;

        var pdf = new DevisPdf(new EntrepriseOptions());

        Assert.StartsWith("%PDF-", Encoding.ASCII.GetString(pdf.GenererGerant(fiche), 0, 5));
        Assert.StartsWith("%PDF-", Encoding.ASCII.GetString(pdf.GenererClient(fiche), 0, 5));
    }

    /// <summary>Si ABS_APERCU_PDF désigne un dossier, les exemples y sont écrits (aperçu du rendu).</summary>
    private static void Enregistrer(string nom, byte[] contenu)
    {
        if (Environment.GetEnvironmentVariable("ABS_APERCU_PDF") is { Length: > 0 } dossier)
            File.WriteAllBytes(Path.Combine(dossier, nom), contenu);
    }
}
