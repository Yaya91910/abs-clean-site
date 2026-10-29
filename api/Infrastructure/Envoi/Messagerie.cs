using System.Net.Http.Json;
using System.Text;
using Microsoft.Extensions.Options;

namespace AbsClean.Api.Infrastructure.Envoi;

public sealed record PieceJointe(string Nom, byte[] Contenu);

public sealed record EmailSortant(string A, string? NomDestinataire, string Sujet, string Html, string? RepondreA = null, PieceJointe? PieceJointe = null);

public interface IEnvoiEmail
{
    Task EnvoyerAsync(EmailSortant email, CancellationToken ct);
}

public interface IEnvoiSms
{
    /// <param name="numero">Numéro du destinataire, format international sans « + » (ex. 33612345678).</param>
    Task EnvoyerAsync(string numero, string texte, CancellationToken ct);
}

/// <summary>E-mail transactionnel via l'API Brevo (POST /v3/smtp/email).</summary>
public sealed class BrevoEmail(HttpClient http, IOptions<EmailOptions> options) : IEnvoiEmail
{
    public async Task EnvoyerAsync(EmailSortant email, CancellationToken ct)
    {
        var o = options.Value;
        var corps = new Dictionary<string, object?>
        {
            ["sender"] = new { email = o.From, name = "ABS clean" },
            ["to"] = new[] { new { email = email.A, name = email.NomDestinataire } },
            ["subject"] = email.Sujet,
            ["htmlContent"] = email.Html,
        };
        if (email.RepondreA is not null) corps["replyTo"] = new { email = email.RepondreA };
        if (email.PieceJointe is { } pj) corps["attachment"] = new[] { new { name = pj.Nom, content = Convert.ToBase64String(pj.Contenu) } };

        using var requete = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email") { Content = JsonContent.Create(corps) };
        requete.Headers.Add("api-key", o.ApiKey);
        using var reponse = await http.SendAsync(requete, ct);
        await Brevo.VerifierAsync(reponse, ct);
    }
}

/// <summary>SMS transactionnel via l'API Brevo (POST /v3/transactionalSMS/send).</summary>
public sealed class BrevoSms(HttpClient http, IOptions<SmsOptions> options) : IEnvoiSms
{
    public async Task EnvoyerAsync(string numero, string texte, CancellationToken ct)
    {
        var o = options.Value;
        var corps = new
        {
            sender = o.Sender,
            recipient = numero,
            content = texte,
            type = "transactional",
            // « — » n'existe pas dans l'alphabet SMS standard (GSM 7 bits) : envoi en Unicode.
            unicodeEnabled = !Sms.EstGsm7(texte),
        };
        using var requete = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/transactionalSMS/send") { Content = JsonContent.Create(corps) };
        requete.Headers.Add("api-key", o.ApiKey);
        using var reponse = await http.SendAsync(requete, ct);
        await Brevo.VerifierAsync(reponse, ct);
    }
}

internal static class Brevo
{
    public static async Task VerifierAsync(HttpResponseMessage reponse, CancellationToken ct)
    {
        if (reponse.IsSuccessStatusCode) return;
        var detail = await reponse.Content.ReadAsStringAsync(ct);
        throw new HttpRequestException($"Brevo a répondu {(int)reponse.StatusCode} : {detail}", null, reponse.StatusCode);
    }
}

/// <summary>Mode test : chaque e-mail est écrit en HTML (avec sa pièce jointe) dans le dossier configuré. Rien ne part.</summary>
public sealed class DossierEmail(IOptions<EnvoiTestOptions> options, IHostEnvironment env, TimeProvider horloge) : IEnvoiEmail
{
    public async Task EnvoyerAsync(EmailSortant email, CancellationToken ct)
    {
        var dossier = Dossier.Preparer(options.Value, env);
        var nom = $"{horloge.GetUtcNow():yyyyMMdd-HHmmssfff}-email-{Dossier.Slug(email.A)}";
        var entete = $"<!-- À : {email.A} | Répondre à : {email.RepondreA} | Sujet : {email.Sujet} | Pièce jointe : {email.PieceJointe?.Nom} -->\n";
        await File.WriteAllTextAsync(Path.Combine(dossier, nom + ".html"), entete + email.Html, Encoding.UTF8, ct);
        if (email.PieceJointe is { } pj)
            await File.WriteAllBytesAsync(Path.Combine(dossier, $"{nom}-{pj.Nom}"), pj.Contenu, ct);
    }
}

/// <summary>Mode test : chaque SMS est écrit dans un fichier texte. Rien ne part.</summary>
public sealed class DossierSms(IOptions<EnvoiTestOptions> options, IHostEnvironment env, TimeProvider horloge) : IEnvoiSms
{
    public Task EnvoyerAsync(string numero, string texte, CancellationToken ct)
    {
        var dossier = Dossier.Preparer(options.Value, env);
        var fichier = Path.Combine(dossier, $"{horloge.GetUtcNow():yyyyMMdd-HHmmssfff}-sms-{numero}.txt");
        return File.WriteAllTextAsync(fichier, $"À : {numero}\n{texte}\n", Encoding.UTF8, ct);
    }
}

internal static class Dossier
{
    public static string Preparer(EnvoiTestOptions o, IHostEnvironment env)
    {
        var chemin = Path.IsPathRooted(o.Dossier) ? o.Dossier : Path.Combine(env.ContentRootPath, o.Dossier);
        Directory.CreateDirectory(chemin);
        return chemin;
    }

    public static string Slug(string valeur) => new(valeur.Select(c => char.IsLetterOrDigit(c) ? c : '_').ToArray());
}
