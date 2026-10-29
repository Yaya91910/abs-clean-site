using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace AbsClean.Api.Tests;

public class NotificationsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreerClient();

    private static object Corps(string email, string? commentaire = null) => new
    {
        frequence = "mensuel", nbVitres = 8, nbBaies = 2, nbVelux = 1,
        nom = "Martin", prenom = "Léa", telephone = "06 12 34 56 78", email,
        adresse = "12 rue des Lilas", codePostal = "91000", ville = "Évry", commentaire,
        creneaux = new[] { "samedi" }, consentement = true,
    };

    private async Task<string> EnvoyerAsync(object corps)
    {
        var reponse = await _client.PostAsJsonAsync("/api/demandes", corps);
        Assert.Equal(HttpStatusCode.Created, reponse.StatusCode);
        return (await reponse.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("numero").GetString()!;
    }

    [Fact]
    public async Task Pdf_emails_et_sms_partent_apres_l_enregistrement()
    {
        var numero = await EnvoyerAsync(Corps("lea.envoi@example.com"));

        var client = Assert.Single(factory.Envois.Emails, e => e.A == "lea.envoi@example.com");
        Assert.Contains(numero, client.Sujet);
        Assert.Equal($"Devis-{numero}.pdf", client.PieceJointe?.Nom);
        Assert.StartsWith("%PDF-", Encoding.ASCII.GetString(client.PieceJointe!.Contenu, 0, 5));

        var gerant = Assert.Single(factory.Envois.Emails, e => e.A == "gerant@example.com" && e.Sujet.Contains(numero));
        Assert.Equal("lea.envoi@example.com", gerant.RepondreA);
        Assert.Contains("06 12 34 56 78", gerant.Html);
        Assert.NotNull(gerant.PieceJointe);

        Assert.Contains(factory.Envois.Sms, s => s.Numero == "33611223344" && s.Texte == "Nouveau devis ABS : Léa Martin, Évry, à partir de 265 € — 06 12 34 56 78");

        var demande = factory.AvecBase(db => db.Demandes.Single(d => d.Numero == numero));
        Assert.True(demande.EmailClientEnvoye);
        Assert.True(demande.EmailGerantEnvoye);
        Assert.True(demande.SmsEnvoye);
    }

    [Fact]
    public async Task Saisies_du_client_echappees_dans_l_email_du_gerant()
    {
        var numero = await EnvoyerAsync(Corps("lea.html@example.com", "<script>alert(1)</script>"));

        var gerant = Assert.Single(factory.Envois.Emails, e => e.A == "gerant@example.com" && e.Sujet.Contains(numero));
        Assert.DoesNotContain("<script>", gerant.Html);
        Assert.Contains("&lt;script&gt;", gerant.Html);
    }
}

public class SmsEnPanneFactory : ApiFactory;

public class EnvoisIndependantsTests(SmsEnPanneFactory factory) : IClassFixture<SmsEnPanneFactory>
{
    [Fact]
    public async Task Sms_en_echec_n_empeche_ni_les_emails_ni_l_enregistrement()
    {
        factory.Envois.SmsEnPanne = true;
        var client = factory.CreerClient();

        var reponse = await client.PostAsJsonAsync("/api/demandes", new
        {
            nbVitres = 2, nom = "Martin", prenom = "Léa", telephone = "0612345678", email = "lea.panne@example.com",
            adresse = "12 rue des Lilas", codePostal = "91000", ville = "Évry", consentement = true,
        });

        Assert.Equal(HttpStatusCode.Created, reponse.StatusCode);
        var numero = (await reponse.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("numero").GetString();
        var demande = factory.AvecBase(db => db.Demandes.Single(d => d.Numero == numero));
        Assert.True(demande.EmailClientEnvoye);
        Assert.True(demande.EmailGerantEnvoye);
        Assert.False(demande.SmsEnvoye);
        Assert.Equal(2, factory.Envois.Emails.Count);
    }
}
