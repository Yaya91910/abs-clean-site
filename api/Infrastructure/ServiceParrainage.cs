using AbsClean.Api.Domain;
using AbsClean.Api.Infrastructure.Envoi;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QRCoder;

namespace AbsClean.Api.Infrastructure;

public enum ResultatValidation { Validee, Introuvable, JetonInvalide, DejaValidee }

/// <summary>Parrainage (CAHIER-DES-CHARGES.md §9 bis) : liens, QR codes, validation de l'intervention par le gérant.</summary>
public sealed class ServiceParrainage(
    AbsCleanDb db,
    IEnvoiEmail email,
    TimeProvider horloge,
    IOptions<SiteOptions> site,
    ILogger<ServiceParrainage> log)
{
    public string Lien(string code) => $"{site.Value.Base}/devis?parrain={Uri.EscapeDataString(code)}";

    /// <summary>QR code en PNG servi par l'API, pour les e-mails ; suppose /api servi sur le domaine du site (image Docker unique).</summary>
    public string LienQrPng(string code) => $"{site.Value.Base}/api/parrainage/{Uri.EscapeDataString(code)}/qr.png";

    /// <summary>
    /// Lien du gérant. Le jeton est placé après « # » : cette partie d'une adresse n'est jamais envoyée au serveur
    /// (ni journaux, ni hébergeur) ; seule la page la lit, puis l'envoie dans le corps du POST de confirmation.
    /// </summary>
    public string LienValidation(int demandeId, string jeton) =>
        $"{site.Value.Base}/admin/intervention#demande={demandeId}&jeton={Uri.EscapeDataString(jeton)}";

    public Task<bool> ExisteAsync(string code, CancellationToken ct) =>
        db.Demandes.AnyAsync(d => d.CodeParrainPersonnel == code, ct);

    /// <summary>QR code du lien de parrainage, en SVG, niveau de correction M.</summary>
    public string QrSvg(string code) => QrSvgDe(Lien(code));

    /// <summary>QR code SVG d'une adresse quelconque (ex. lien de validation du gérant), niveau de correction M.</summary>
    public static string QrSvgDe(string adresse)
    {
        using var generateur = new QRCodeGenerator();
        using var donnees = generateur.CreateQrCode(adresse, QRCodeGenerator.ECCLevel.M);
        return new SvgQRCode(donnees).GetGraphic(8, "#15345F", "#FFFFFF", true, SvgQRCode.SizingMode.ViewBoxAttribute);
    }

    /// <summary>Même QR code en PNG (la plupart des messageries n'affichent pas le SVG).</summary>
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
    /// Chaque étape est une mise à jour conditionnelle : un double clic ne valide (et ne prévient) qu'une fois.
    /// </summary>
    public async Task<ResultatValidation> ValiderInterventionAsync(int demandeId, string? jeton, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(jeton))
            return await RaisonDuRefusAsync(demandeId, ct);

        var empreinte = JetonIntervention.Empreinte(jeton);
        var maintenant = horloge.GetUtcNow().UtcDateTime;

        var validee = await db.Demandes
            .Where(d => d.Id == demandeId && d.InterventionRealiseeLe == null && d.JetonInterventionHash == empreinte)
            .ExecuteUpdateAsync(s => s
                .SetProperty(d => d.InterventionRealiseeLe, maintenant)
                .SetProperty(d => d.JetonInterventionHash, (string?)null), ct);
        if (validee == 0)
            return await RaisonDuRefusAsync(demandeId, ct);

        log.LogInformation("Intervention réalisée pour la demande {Id}.", demandeId);

        var remiseActivee = await db.Parrainages
            .Where(p => p.FilleulId == demandeId && p.Statut == StatutParrainage.EnAttente)
            .ExecuteUpdateAsync(s => s
                .SetProperty(p => p.Statut, StatutParrainage.Valide)
                .SetProperty(p => p.ValideLe, maintenant), ct);
        if (remiseActivee > 0)
            await PrevenirParrainAsync(demandeId, ct);

        return ResultatValidation.Validee;
    }

    private async Task<ResultatValidation> RaisonDuRefusAsync(int demandeId, CancellationToken ct)
    {
        var dejaValidee = await db.Demandes.Where(d => d.Id == demandeId).Select(d => (bool?)(d.InterventionRealiseeLe != null)).FirstOrDefaultAsync(ct);
        return dejaValidee switch
        {
            null => ResultatValidation.Introuvable,
            true => ResultatValidation.DejaValidee,
            false => ResultatValidation.JetonInvalide,
        };
    }

    private async Task PrevenirParrainAsync(int filleulId, CancellationToken ct)
    {
        var parrainage = await db.Parrainages.AsNoTracking()
            .Include(p => p.Parrain)
            .Include(p => p.Filleul)
            .FirstAsync(p => p.FilleulId == filleulId, ct);
        try
        {
            await email.EnvoyerAsync(ModelesEmail.RemiseParrain(parrainage.Parrain, parrainage.Filleul), CancellationToken.None);
        }
        catch (Exception e)
        {
            log.LogError(e, "E-mail au parrain {Code} non envoyé (remise bien activée en base).", parrainage.Parrain.CodeParrainPersonnel);
        }
    }
}
