using AbsClean.Api.Domain;

namespace AbsClean.Api.Tests;

public class CalculDevisTests
{
    [Fact]
    public void Aucun_element_est_refuse()
    {
        var erreurs = CalculDevis.Valider(new QuantitesDevis(0, 0, 0));

        Assert.Contains("quantites", erreurs.Keys);
        Assert.Throws<ArgumentException>(() => CalculDevis.Calculer(new QuantitesDevis(0, 0, 0), Profil.Particulier, Frequence.Ponctuel));
    }

    [Theory]
    [InlineData(1, 0, 0, 0, 20)]
    [InlineData(0, 1, 0, 0, 40)]
    [InlineData(0, 0, 1, 0, 25)]
    [InlineData(0, 0, 0, 1, 50)]
    [InlineData(8, 2, 1, 0, 8 * 20 + 2 * 40 + 1 * 25)]
    [InlineData(99, 99, 99, 99, 99 * (20 + 40 + 25 + 50))]
    public void Total_est_la_somme_des_lignes(int vitres, int baies, int velux, int vitrines, decimal attendu)
    {
        var resultat = CalculDevis.Calculer(new QuantitesDevis(vitres, baies, velux, vitrines), Profil.Particulier, Frequence.Ponctuel);

        Assert.Equal(attendu, resultat.Total);
        Assert.Equal(resultat.Lignes.Sum(l => l.Montant), resultat.Total);
        Assert.All(resultat.Lignes, l => Assert.True(l.Quantite > 0));
    }

    [Theory]
    [InlineData(-1, 0, 0, "nbVitres")]
    [InlineData(100, 0, 0, "nbVitres")]
    [InlineData(1, 100, 0, "nbBaies")]
    [InlineData(1, 0, -5, "nbVelux")]
    public void Quantites_hors_bornes_refusees(int vitres, int baies, int velux, string champ)
    {
        var erreurs = CalculDevis.Valider(new QuantitesDevis(vitres, baies, velux));

        Assert.Contains(champ, erreurs.Keys);
    }

    [Theory]
    [InlineData(Frequence.Mensuel)]
    [InlineData(Frequence.Trimestriel)]
    public void Reduction_inconnue_n_est_pas_appliquee(Frequence frequence)
    {
        var resultat = CalculDevis.Calculer(new QuantitesDevis(5, 0, 0), Profil.Particulier, frequence);

        Assert.Null(resultat.ReductionPourcent);
        Assert.Equal(100m, resultat.Total);
    }

    [Fact]
    public void Profil_ne_change_pas_le_prix()
    {
        var q = new QuantitesDevis(3, 1, 2);

        Assert.Equal(
            CalculDevis.Calculer(q, Profil.Particulier, Frequence.Ponctuel).Total,
            CalculDevis.Calculer(q, Profil.Professionnel, Frequence.Ponctuel).Total);
    }
}
