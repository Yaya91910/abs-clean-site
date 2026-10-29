using AbsClean.Api.Domain;
using AbsClean.Api.Infrastructure.Envoi;

namespace AbsClean.Api.Tests;

public class SmsTests
{
    private static Demande Demande(string prenom = "Léa", string nom = "Martin", string ville = "Évry-Courcouronnes", decimal total = 265m) => new()
    {
        Prenom = prenom, Nom = nom, Ville = ville, Total = total, Telephone = "0612345678",
    };

    [Fact]
    public void Message_suit_le_modele_du_cahier_des_charges()
    {
        Assert.Equal(
            "Nouveau devis ABS : Léa Martin, Évry-Courcouronnes, à partir de 265 € — 06 12 34 56 78",
            Sms.NouveauDevis(Demande()));
    }

    [Fact]
    public void Message_trop_long_est_raccourci_a_160_caracteres()
    {
        var texte = Sms.NouveauDevis(Demande(prenom: new string('P', 60), nom: new string('N', 60), ville: new string('V', 100), total: 13365m));

        Assert.True(texte.Length <= Sms.LongueurMax, $"{texte.Length} caractères");
        Assert.StartsWith("Nouveau devis ABS : ", texte);
        Assert.EndsWith("à partir de 13 365 € — 06 12 34 56 78", texte);
    }

    [Theory]
    [InlineData("06 11 22 33 44", "33611223344")]
    [InlineData("0611223344", "33611223344")]
    [InlineData("+33 6 11 22 33 44", "33611223344")]
    [InlineData("", null)]
    [InlineData(null, null)]
    public void Numero_du_gerant_au_format_international(string? saisi, string? attendu)
    {
        Assert.Equal(attendu, Sms.VersInternational(saisi));
    }

    [Fact]
    public void Tiret_long_impose_l_unicode()
    {
        Assert.True(Sms.EstGsm7("Nouveau devis ABS : 265 €"));
        Assert.False(Sms.EstGsm7("265 € — 06"));
    }
}
