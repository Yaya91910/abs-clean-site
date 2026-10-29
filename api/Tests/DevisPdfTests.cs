using System.Text;
using AbsClean.Api.Domain;
using AbsClean.Api.Infrastructure.Envoi;

namespace AbsClean.Api.Tests;

public class DevisPdfTests
{
    static DevisPdfTests() => QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

    private static Demande Demande() => new()
    {
        Numero = "ABS-2026-0001", CreeLe = new DateTime(2026, 11, 3, 9, 30, 0, DateTimeKind.Utc),
        Profil = Profil.Particulier, Frequence = Frequence.Mensuel,
        NbVitres = 8, NbBaies = 2, NbVelux = 1, Total = 265m,
        Prenom = "Léa", Nom = "Martin", Telephone = "0612345678", Email = "lea@example.com",
        Adresse = "12 rue des Lilas", CodePostal = "91000", Ville = "Évry-Courcouronnes", Acces = "3e étage",
        Creneaux = [Creneau.Samedi],
    };

    [Fact]
    public void Genere_un_pdf_sans_coordonnees_entreprise()
    {
        var octets = new DevisPdf(new EntrepriseOptions()).Generer(Demande());

        Assert.StartsWith("%PDF-", Encoding.ASCII.GetString(octets, 0, 5));
        Assert.True(octets.Length > 5_000);
    }

    [Fact]
    public void Genere_un_pdf_avec_les_coordonnees_entreprise()
    {
        var entreprise = new EntrepriseOptions { Siret = "000 000 000 00000", Adresse = "Adresse test", Telephone = "01 00 00 00 00", Email = "contact@example.com" };

        var octets = new DevisPdf(entreprise).Generer(Demande());

        Assert.StartsWith("%PDF-", Encoding.ASCII.GetString(octets, 0, 5));
    }
}
