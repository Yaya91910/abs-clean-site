using AbsClean.Api.Domain;

namespace AbsClean.Api.Tests;

public class ValidationDemandeTests
{
    internal static NouvelleDemande Valide() => new(
        NbVitres: 8, NbBaies: 2, NbVelux: 1,
        Nom: "Martin", Prenom: "Léa", Telephone: "06 12 34 56 78", Email: "lea.martin@example.com",
        Adresse: "12 rue des Lilas", CodePostal: "91000", Ville: "Évry-Courcouronnes",
        Creneaux: [Creneau.SemaineMatin, Creneau.Samedi], Consentement: true);

    [Fact]
    public void Demande_complete_est_valide()
    {
        Assert.Empty(ValidationDemande.Valider(Valide()));
    }

    [Theory]
    [InlineData("06 12 34 56 78", "0612345678")]
    [InlineData("0712345678", "0712345678")]
    [InlineData("01 23 45 67 89", "0123456789")]
    [InlineData("05 23 45 67 89", "0523456789")]
    [InlineData("09 87 65 43 21", "0987654321")]
    public void Telephone_francais_accepte_et_normalise(string saisi, string attendu)
    {
        Assert.Equal(attendu, ValidationDemande.NormaliserTelephone(saisi));
    }

    [Theory]
    [InlineData("08 12 34 56 78")]
    [InlineData("+33 6 12 34 56 78")]
    [InlineData("06 12 34 56")]
    [InlineData("06-12-34-56-78")]
    [InlineData("00 12 34 56 78")]
    public void Telephone_invalide_refuse(string saisi)
    {
        Assert.Null(ValidationDemande.NormaliserTelephone(saisi));
    }

    public static TheoryData<string, NouvelleDemande> Invalides => new()
    {
        { "nom", Valide() with { Nom = " " } },
        { "nom", Valide() with { Nom = new string('a', 61) } },
        { "prenom", Valide() with { Prenom = null } },
        { "telephone", Valide() with { Telephone = "" } },
        { "telephone", Valide() with { Telephone = "08 00 00 00 00" } },
        { "email", Valide() with { Email = "" } },
        { "email", Valide() with { Email = "lea@" } },
        { "adresse", Valide() with { Adresse = "" } },
        { "codePostal", Valide() with { CodePostal = "9100" } },
        { "codePostal", Valide() with { CodePostal = "91 000" } },
        { "ville", Valide() with { Ville = null } },
        { "acces", Valide() with { Acces = new string('a', 201) } },
        { "commentaire", Valide() with { Commentaire = new string('a', 1001) } },
        { "consentement", Valide() with { Consentement = false } },
        { "quantites", Valide() with { NbVitres = 0, NbBaies = 0, NbVelux = 0 } },
        { "nbVitres", Valide() with { NbVitres = 100 } },
        // Chiffres d'une autre écriture, invisibles à l'œil dans un formulaire.
        { "telephone", Valide() with { Telephone = "٠٦١٢٣٤٥٦٧٨" } },
        { "codePostal", Valide() with { CodePostal = "٩١٠٠٠" } },
        // Inversion du sens du texte (U+202E), espace sans largeur, séparateur de ligne Unicode, moitié de caractère orpheline.
        { "nom", Valide() with { Nom = "Martin\u202Eenitram" } },
        { "telephone", Valide() with { Telephone = "06 12 34\u200B 56 78" } },
        { "ville", Valide() with { Ville = "Évry\u2028Nouveau devis" } },
        { "commentaire", Valide() with { Commentaire = "Merci\u202E !" } },
        { "prenom", Valide() with { Prenom = "Léa\uD83D" } },
    };

    [Theory]
    [MemberData(nameof(Invalides))]
    public void Champ_invalide_renvoie_son_erreur(string champ, NouvelleDemande demande)
    {
        var erreurs = ValidationDemande.Valider(demande);

        Assert.Contains(champ, erreurs.Keys);
    }

    [Fact]
    public void Commentaire_de_1000_caracteres_accepte()
    {
        Assert.Empty(ValidationDemande.Valider(Valide() with { Commentaire = new string('a', 1000) }));
    }

    [Fact]
    public void Emojis_et_liants_acceptes_dans_le_commentaire()
    {
        Assert.Empty(ValidationDemande.Valider(Valide() with { Commentaire = "Famille 👨\u200D👩\u200D👧 à la maison.\nMerci 🙏" }));
    }
}
