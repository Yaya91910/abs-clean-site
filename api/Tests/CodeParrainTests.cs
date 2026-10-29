using AbsClean.Api.Domain;

namespace AbsClean.Api.Tests;

public class CodeParrainTests
{
    [Theory]
    [InlineData("Marie", "ABS-MARIE")]
    [InlineData("Élodie", "ABS-ELODIE")]
    [InlineData("Zoé", "ABS-ZOE")]
    [InlineData("Jean-François", "ABS-JEANFR")]
    [InlineData("  ", "ABS-CLIENT")]
    public void Racine_prenom_en_majuscules_sans_accents_6_lettres_max(string prenom, string attendu)
    {
        Assert.Equal(attendu, CodeParrain.Racine(prenom));
    }

    [Fact]
    public void Caracteres_aleatoires_sans_ambiguite()
    {
        for (var i = 0; i < 500; i++)
        {
            var code = CodeParrain.Generer("Marie", 2);
            Assert.Matches("^ABS-MARIE[A-Z2-9]{2}$", code);
            Assert.DoesNotContain(code[9..], c => c is '0' or 'O' or '1' or 'I');
        }
    }

    [Theory]
    [InlineData(" abs-marie7 ", "ABS-MARIE7")]
    [InlineData("ABS - MARIE7", "ABS-MARIE7")]
    [InlineData("", null)]
    [InlineData(null, null)]
    public void Code_saisi_normalise(string? saisi, string? attendu)
    {
        Assert.Equal(attendu, CodeParrain.Normaliser(saisi));
    }
}
