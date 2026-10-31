using AbsClean.Api.Domain;

namespace AbsClean.Api.Tests;

public class CodeParrainTests
{
    [Fact]
    public void Code_ABS_et_7_chiffres_sans_zero_en_tete()
    {
        for (var i = 0; i < 500; i++)
            Assert.Matches("^ABS-[1-9][0-9]{6}$", CodeParrain.Generer());
    }

    [Theory]
    [InlineData(" abs-1765321 ", "ABS-1765321")]
    [InlineData("ABS - 176 5321", "ABS-1765321")]
    [InlineData("1765321", "ABS-1765321")]
    [InlineData("abs1765321", "ABS-1765321")]
    [InlineData("abs-marie7", "ABS-MARIE7")]
    [InlineData("176532", "176532")]
    [InlineData("", null)]
    [InlineData(null, null)]
    public void Code_saisi_normalise(string? saisi, string? attendu)
    {
        Assert.Equal(attendu, CodeParrain.Normaliser(saisi));
    }

    [Theory]
    [InlineData("ABS-1765321", true)]
    [InlineData("ABS-MARIE7", true)]
    [InlineData("ABS-INCONNUZZ", true)]
    [InlineData("176532", false)]
    [InlineData("ABS-", false)]
    [InlineData("ABS-1234567890123", false)]
    [InlineData("ABS-MARIE7'--", false)]
    [InlineData("ABS-١٢٣٤٥٦٧", false)]
    public void Format_du_code(string code, bool valide)
    {
        Assert.Equal(valide, CodeParrain.FormatValide(code));
    }
}
