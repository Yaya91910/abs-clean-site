using AbsClean.Api.Domain;
using AbsClean.Api.Infrastructure.Envoi;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QRCoder;

namespace AbsClean.Api.Infrastructure;

/// <summary>Adresse publique du site (section « Site ») : base des liens de parrainage et du lien de validation. Domaine À FOURNIR.</summary>
public sealed class SiteOptions
{
    public string Url { get; set; } = "http://localhost:4200";
}

public enum ResultatValidation { Validee, Introuvable, JetonInvalide, DejaValidee }

/// <summary>Parrainage (CLAUDE.md §9 bis) : liens, QR codes, validation de l'intervention par le gérant.</summary>
public sealed class ServiceParrainage(
    AbsCleanDb db,
    IEnvoiEmail email,
    TimeProvider horloge,
    IOptions<SiteOptions> site,
    ILogger<ServiceParrainage> log)
{
    private string Base => site.Value.Url.TrimEnd('/');

    public string Lien(string code) => $"{Base}/devis?parrain={Uri.EscapeDataString(code)}";

    public string LienValidation(int demandeId, string jeton) => $"{Base}/admin/intervention?demande={demandeId}&jeton={Uri.EscapeDataString(jeton)}";

    public Task<bool> ExisteAsync(string code, CancellationToken ct) =>
        db.Demandes.AnyAsync(d => d.CodeParrainPersonnel == code, ct);

    /// <summary>QR code du lien de parrainage, en SVG, niveau de correction M.</summary>
    public string QrSvg(string code)
    {
        using var donnees = Generer(code);
        return new SvgQRCode(donnees).GetGraphic(8, "#15345F", "#FFFFFF", true, SvgQRCode.SizingMode.ViewBoxAttribute);
    }

    /// <summary>Même QR code en PNG, pour les e-mails (la plupart des messageries n'affichent pas le SVG).</summary>
    public byte[] QrPng(string code)
    {
        using var donnees = Generer(code);
        return new PngByteQRCode(donnees).GetGraphic(8, [0x15, 0x34, 0x5F, 0xFF], [0xFF, 0xFF, 0xFF, 0xFF], true);
    }

    public QRCodeData Generer(string code)
    {
        using var generateur = new QRCodeGenerator();
        return generateur.CreateQrCode(Lien(code), QRCodeGenerator.ECCLevel.M);
    }

    /// <summary>
    /// Le gérant confirme la première intervention d'un client (lien à usage unique reçu par e-mail).
    /// Si ce client a été parrainé, la remise du parrain est activée et le parrain est prévenu.
    /// </summary>
    public async Task<ResultatValidation> ValiderInterventionAsync(int demandeId, string? jeton, CancellationToken ct)
    {
        var demande = await db.Demandes.FirstOrDefaultAsync(d => d.Id == demandeId, ct);
        if (demande is null) return ResultatValidation.Introuvable;
        if (demande.InterventionRealiseeLe is not null) return ResultatValidation.DejaValidee;
        if (string.IsNullOrEmpty(jeton) || demande.JetonInterventionHash is null
            || !CryptographicEquals(demande.JetonInterventionHash, DemandeStore.Empreinte(jeton)))
            return ResultatValidation.JetonInvalide;

        var maintenant = horloge.GetUtcNow().UtcDateTime;
        demande.InterventionRealiseeLe = maintenant;
        demande.JetonInterventionHash = null;

        var parrainage = await db.Parrainages.Include(p => p.Parrain)
            .FirstOrDefaultAsync(p => p.FilleulId == demande.Id && p.Statut == StatutParrainage.EnAttente, ct);
        if (parrainage is not null)
        {
            parrainage.Statut = StatutParrainage.Valide;
            parrainage.ValideLe = maintenant;
        }
        await db.SaveChangesAsync(ct);
        log.LogInformation("Intervention réalisée pour {Numero}.", demande.Numero);

        if (parrainage is not null)
            await PrevenirParrainAsync(parrainage.Parrain, demande);

        return ResultatValidation.Validee;
    }

    private async Task PrevenirParrainAsync(Demande parrain, Demande filleul)
    {
        try
        {
            await email.EnvoyerAsync(new EmailSortant(
                parrain.Email,
                $"{parrain.Prenom} {parrain.Nom}",
                $"Votre proche {filleul.Prenom} est devenu client : −10 % sur votre prochaine intervention",
                $"""
                <div style="font-family:Arial,sans-serif;font-size:15px;line-height:1.6;color:#15345F;max-width:560px">
                <p>Bonjour {System.Net.WebUtility.HtmlEncode(parrain.Prenom)},</p>
                <p>Votre proche {System.Net.WebUtility.HtmlEncode(filleul.Prenom)} est devenu client : <strong>−10 % sur votre prochaine intervention</strong>.</p>
                <p>Merci pour votre confiance,<br>ABS clean</p>
                </div>
                """), CancellationToken.None);
        }
        catch (Exception e)
        {
            log.LogError(e, "E-mail au parrain {Code} non envoyé (remise bien activée en base).", parrain.CodeParrainPersonnel);
        }
    }

    private static bool CryptographicEquals(string a, string b) =>
        System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(
            System.Text.Encoding.ASCII.GetBytes(a), System.Text.Encoding.ASCII.GetBytes(b));
}
