using System.Globalization;
using AbsClean.Api.Domain;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AbsClean.Api.Infrastructure.Envoi;

/// <summary>Tout ce qu'il faut pour éditer les deux exemplaires d'un devis.</summary>
/// <param name="RangDemande">Rang de cette demande parmi celles du client (1 = première).</param>
/// <param name="DemandePrecedenteLe">Date de la demande précédente du même client, s'il y en a une.</param>
/// <param name="QrParrainage">QR code SVG du lien de parrainage du client.</param>
/// <param name="QrValidation">QR code SVG du lien « intervention réalisée » (exemplaire gérant uniquement).</param>
public sealed record FicheDevis(
    Demande Demande,
    int RangDemande,
    DateTime? DemandePrecedenteLe,
    string? QrParrainage,
    string? QrValidation)
{
    public Client Client => Demande.Client;
    public bool ClientExistant => RangDemande > 1;
}

/// <summary>
/// Devis PDF (CAHIER-DES-CHARGES.md §9.4) en deux exemplaires :
/// - client : devis épuré (émetteur, client, détail, total « à partir de », conditions, code parrain) ;
/// - gérant : fiche d'intervention (client existant mis en avant, toutes les informations utiles, suivi à remplir,
///   QR code pour confirmer l'intervention).
/// Les coordonnées d'ABS clean n'apparaissent que si elles sont renseignées : rien n'est inventé.
/// </summary>
public sealed class DevisPdf(EntrepriseOptions entreprise)
{
    private static readonly CultureInfo Fr = CultureInfo.GetCultureInfo("fr-FR");
    private static readonly string Logo = LireLogo();

    /// <summary>Séparateur de milliers en espace simple : l'espace fine insécable de .NET n'existe pas dans l'alphabet SMS.</summary>
    private static readonly NumberFormatInfo Montants = new() { NumberGroupSeparator = " ", NumberDecimalSeparator = "," };

    private const string Ink = "#15345F";
    private const string Accent = "#2F6FD6";
    private const string AccentTexte = "#2559B8";
    private const string Gris = "#5B6B80";
    private const string GrisClair = "#8A97A8";
    private const string Trait = "#E3E8F0";
    private const string Fond = "#F4F8FF";
    private const string Ambre = "#FFF4D6";

    public byte[] GenererClient(FicheDevis fiche) => DocumentClient(fiche).GeneratePdf();

    public byte[] GenererGerant(FicheDevis fiche) => DocumentGerant(fiche).GeneratePdf();

    /// <summary>Exemplaire client, avant conversion (PDF, ou images pour un aperçu).</summary>
    public IDocument DocumentClient(FicheDevis fiche) => Document.Create(doc => doc.Page(page =>
    {
        MiseEnPage(page);
        page.Header().Element(c => EnTete(c, "DEVIS", fiche, exemplaireGerant: false));
        page.Content().PaddingTop(22).Column(col =>
        {
            col.Spacing(20);
            col.Item().Row(row =>
            {
                row.RelativeItem().Element(c => Carte(c, "Émetteur", Emetteur));
                row.ConstantItem(14);
                row.RelativeItem().Element(c => Carte(c, "Client", b => CoordonneesClient(b, fiche.Demande, complet: false)));
            });
            col.Item().Text(t =>
            {
                t.Span("Objet : ").SemiBold();
                t.Span($"nettoyage de surfaces vitrées — {Formule(fiche.Demande.Frequence)} · client {Libelle(fiche.Demande.Profil).ToLower(Fr)}");
            });
            col.Item().Element(c => DetailEtTotal(c, fiche.Demande));
            col.Item().Element(c => Conditions(c, fiche.Demande));
            if (fiche.QrParrainage is not null)
                col.Item().Element(c => Parrainage(c, fiche));
        });
        page.Footer().Element(PiedDePage);
    }));

    /// <summary>Exemplaire gérant, avant conversion (PDF, ou images pour un aperçu).</summary>
    public IDocument DocumentGerant(FicheDevis fiche) => Document.Create(doc => doc.Page(page =>
    {
        MiseEnPage(page);
        page.Header().Element(c => EnTete(c, "FICHE D'INTERVENTION", fiche, exemplaireGerant: true));
        page.Content().PaddingTop(18).Column(col =>
        {
            col.Spacing(16);
            col.Item().Element(c => BandeauClient(c, fiche));
            col.Item().Row(row =>
            {
                row.RelativeItem(3).Element(c => Carte(c, "Client", b => CoordonneesClient(b, fiche.Demande, complet: true)));
                row.ConstantItem(14);
                row.RelativeItem(2).Element(c => Carte(c, "Prestation", b =>
                {
                    Ligne(b, "Profil", Libelle(fiche.Demande.Profil));
                    Ligne(b, "Fréquence", Libelle(fiche.Demande.Frequence));
                    Ligne(b, "Créneaux", fiche.Demande.Creneaux.Count > 0 ? string.Join(", ", fiche.Demande.Creneaux.Select(Libelle)) : "non précisés");
                    if (fiche.Demande.CodeParrainUtilise is { } code)
                        Ligne(b, "Parrainé par", $"{code} — remise du parrain activée à la confirmation");
                }));
            });
            if (fiche.Demande.Commentaire is { } commentaire)
                col.Item().Background(Ambre).Padding(10).Column(c =>
                {
                    c.Item().Text("Commentaire du client").FontSize(8).SemiBold().FontColor("#6B4A00");
                    c.Item().PaddingTop(3).Text(commentaire);
                });
            col.Item().Element(c => DetailEtTotal(c, fiche.Demande));
            col.Item().Element(c => Suivi(c, fiche));
        });
        page.Footer().Element(PiedDePage);
    }));

    // --- Blocs communs ---------------------------------------------------------------------------------------------

    private static void MiseEnPage(PageDescriptor page)
    {
        page.Size(PageSizes.A4);
        page.MarginHorizontal(42);
        page.MarginVertical(34);
        page.DefaultTextStyle(t => t.FontSize(9.5f).FontColor(Ink).LineHeight(1.35f));
    }

    private static void EnTete(IContainer conteneur, string titre, FicheDevis fiche, bool exemplaireGerant)
    {
        conteneur.Column(col =>
        {
            col.Item().Row(row =>
            {
                row.ConstantItem(118).AlignMiddle().Svg(Logo);
                row.RelativeItem().AlignRight().Column(c =>
                {
                    c.Item().AlignRight().Text(titre).FontSize(exemplaireGerant ? 15 : 22).Bold().LetterSpacing(0.04f);
                    if (exemplaireGerant)
                        c.Item().AlignRight().PaddingTop(2).Text("Exemplaire gérant — usage interne").FontSize(8).SemiBold().FontColor(AccentTexte);
                    c.Item().AlignRight().PaddingTop(8).Table(t =>
                    {
                        t.ColumnsDefinition(d => { d.ConstantColumn(70); d.ConstantColumn(92); });
                        void Info(string libelle, string valeur)
                        {
                            t.Cell().AlignRight().PaddingRight(8).Text(libelle).FontSize(8).FontColor(Gris);
                            t.Cell().AlignRight().Text(valeur).FontSize(9).SemiBold();
                        }
                        Info("Devis n°", fiche.Demande.Numero);
                        Info("Client n°", fiche.Client.Numero);
                        Info("Date", Date(fiche.Demande.CreeLe));
                    });
                });
            });
            col.Item().PaddingTop(12).LineHorizontal(1.5f).LineColor(Accent);
        });
    }

    private static void Carte(IContainer conteneur, string titre, Action<ColumnDescriptor> contenu) =>
        conteneur.Background(Fond).Padding(12).Column(col =>
        {
            col.Spacing(1);
            col.Item().PaddingBottom(4).Text(titre.ToUpper(Fr)).FontSize(7.5f).Bold().LetterSpacing(0.08f).FontColor(AccentTexte);
            contenu(col);
        });

    private void Emetteur(ColumnDescriptor col)
    {
        col.Item().Text(entreprise.Nom).SemiBold();
        col.Item().Text("Nettoyage et entretien de surfaces vitrées").FontColor(Gris);
        col.Item().Text("Essonne et Île-de-France").FontColor(Gris);
        foreach (var info in new[] { entreprise.Adresse, entreprise.Telephone, entreprise.Email }.Where(v => !string.IsNullOrWhiteSpace(v)))
            col.Item().Text(info!);
    }

    private static void CoordonneesClient(ColumnDescriptor col, Demande d, bool complet)
    {
        col.Item().Text($"{d.Prenom} {d.Nom}").SemiBold();
        col.Item().Text(d.Adresse);
        col.Item().Text($"{d.CodePostal} {d.Ville}");
        if (complet && d.Acces is not null) col.Item().Text($"Accès : {d.Acces}").FontColor(Gris);
        col.Item().PaddingTop(3).Text(FormatTelephone(d.Telephone));
        col.Item().Text(d.Email);
    }

    private static void Ligne(ColumnDescriptor col, string libelle, string valeur) =>
        col.Item().Text(t =>
        {
            t.Span($"{libelle} : ").FontColor(Gris);
            t.Span(valeur);
        });

    /// <summary>Détail recalculé depuis les quantités enregistrées (même source de vérité que le total), puis le total.</summary>
    private static void DetailEtTotal(IContainer conteneur, Demande d)
    {
        var devis = CalculDevis.Calculer(new QuantitesDevis(d.NbVitres, d.NbBaies, d.NbVelux, d.NbVitrines), d.Profil, d.Frequence);

        conteneur.Column(col =>
        {
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(5);
                    c.RelativeColumn(1.3f);
                    c.RelativeColumn(2.2f);
                    c.RelativeColumn(2.2f);
                });

                table.Header(h =>
                {
                    IContainer Entete(IContainer c) => c.BorderBottom(1).BorderColor(Ink).PaddingVertical(6);
                    h.Cell().Element(Entete).Text("Désignation").FontSize(8).Bold().LetterSpacing(0.04f);
                    h.Cell().Element(Entete).AlignRight().Text("Qté").FontSize(8).Bold();
                    h.Cell().Element(Entete).AlignRight().Text("Prix unitaire").FontSize(8).Bold();
                    h.Cell().Element(Entete).AlignRight().Text("Montant").FontSize(8).Bold();
                });

                foreach (var l in devis.Lignes)
                {
                    IContainer Cellule(IContainer c) => c.BorderBottom(0.5f).BorderColor(Trait).PaddingVertical(7);
                    table.Cell().Element(Cellule).Text(l.Libelle);
                    table.Cell().Element(Cellule).AlignRight().Text(l.Quantite.ToString(Fr));
                    table.Cell().Element(Cellule).AlignRight().Text((l.APartirDe ? "dès " : "") + Euros(l.PrixUnitaire));
                    table.Cell().Element(Cellule).AlignRight().Text(Euros(l.Montant));
                }
            });

            col.Item().PaddingTop(10).AlignRight().Width(230).Background(Fond).Padding(12).Column(c =>
            {
                c.Item().Row(r =>
                {
                    r.RelativeItem().AlignBottom().Text("Total estimé").SemiBold();
                    r.AutoItem().Text($"à partir de {Euros(d.Total)}").FontSize(15).Bold().FontColor(Accent);
                });
                c.Item().PaddingTop(2).AlignRight().Text(devis.Mention).FontSize(8).FontColor(Gris);
            });
        });
    }

    private static void Conditions(IContainer conteneur, Demande d) =>
        conteneur.Column(col =>
        {
            col.Spacing(2);
            col.Item().PaddingBottom(2).Text("CONDITIONS").FontSize(7.5f).Bold().LetterSpacing(0.08f).FontColor(AccentTexte);
            col.Item().Text("Estimation établie à partir des éléments déclarés ; le prix définitif est confirmé avec vous avant l'intervention.").FontColor(Gris);
            if (Tarifs.ReductionPourcent(d.Frequence) is null)
                col.Item().Text($"{char.ToUpper(Formule(d.Frequence)[0])}{Formule(d.Frequence)[1..]} : la réduction applicable vous sera confirmée lors de l'échange.").FontColor(Gris);
            col.Item().Text(Tarifs.MentionOption + ".").FontColor(Gris);
            col.Item().Text("Interventions jusqu'à 2,50 m de hauteur.").FontColor(Gris);
        });

    private static void Parrainage(IContainer conteneur, FicheDevis fiche) =>
        conteneur.Border(0.75f).BorderColor(Trait).Padding(12).Row(row =>
        {
            row.ConstantItem(74).Height(74).Svg(fiche.QrParrainage!);
            row.ConstantItem(14);
            row.RelativeItem().AlignMiddle().Column(c =>
            {
                c.Item().Text("VOTRE CODE PARRAIN").FontSize(7.5f).Bold().LetterSpacing(0.08f).FontColor(AccentTexte);
                c.Item().Text(fiche.Demande.CodeParrainPersonnel).FontSize(14).Bold();
                c.Item().Text("−10 % sur votre prochaine intervention pour chaque proche qui devient client. Partagez ce QR code ou votre code.").FontColor(Gris);
            });
        });

    private void PiedDePage(IContainer conteneur) =>
        conteneur.Column(col =>
        {
            col.Item().LineHorizontal(0.5f).LineColor(Trait);
            col.Item().PaddingTop(6).Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    var mentions = new[] { entreprise.Nom, entreprise.Adresse, entreprise.Siret is { Length: > 0 } s ? $"SIRET {s}" : null }
                        .Where(v => !string.IsNullOrWhiteSpace(v));
                    c.Item().Text(string.Join(" · ", mentions)).FontSize(7.5f).FontColor(GrisClair);
                    var contact = new[] { entreprise.Telephone, entreprise.Email }.Where(v => !string.IsNullOrWhiteSpace(v));
                    if (contact.Any()) c.Item().Text(string.Join(" · ", contact)).FontSize(7.5f).FontColor(GrisClair);
                });
                row.AutoItem().AlignBottom().Text(t =>
                {
                    t.DefaultTextStyle(s => s.FontSize(7.5f).FontColor(GrisClair));
                    t.Span("Page ");
                    t.CurrentPageNumber();
                    t.Span(" / ");
                    t.TotalPages();
                });
            });
        });

    // --- Exemplaire gérant ------------------------------------------------------------------------------------------

    /// <summary>Client existant mis en avant, avec téléphone et e-mail cliquables pour un contact rapide.</summary>
    private static void BandeauClient(IContainer conteneur, FicheDevis fiche)
    {
        var d = fiche.Demande;
        conteneur.Background(fiche.ClientExistant ? "#E8F1FF" : Fond)
            .BorderLeft(3).BorderColor(fiche.ClientExistant ? Accent : Trait)
            .Padding(12).Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text(fiche.ClientExistant ? "CLIENT EXISTANT" : "NOUVEAU CLIENT").FontSize(7.5f).Bold().LetterSpacing(0.08f).FontColor(AccentTexte);
                    c.Item().Text($"{d.Prenom} {d.Nom} — {fiche.Client.Numero}").FontSize(12).Bold();
                    c.Item().Text(fiche.ClientExistant
                        ? $"{Rang(fiche.RangDemande)} demande · client depuis le {Date(fiche.Client.CreeLe)}" + (fiche.DemandePrecedenteLe is { } p ? $" · précédente le {Date(p)}" : "")
                        : "Première demande").FontColor(Gris);
                });
                row.AutoItem().AlignMiddle().Column(c =>
                {
                    c.Item().AlignRight().Hyperlink($"tel:{d.Telephone}").Text(FormatTelephone(d.Telephone)).FontSize(12).Bold().FontColor(Accent);
                    c.Item().AlignRight().Hyperlink($"mailto:{d.Email}").Text(d.Email).FontColor(Accent);
                });
            });
    }

    /// <summary>Partie à remplir par le gérant, et QR code pour confirmer l'intervention depuis son téléphone.</summary>
    private static void Suivi(IContainer conteneur, FicheDevis fiche) =>
        conteneur.Border(0.75f).BorderColor(Trait).Padding(12).Row(row =>
        {
            row.RelativeItem().Column(c =>
            {
                c.Spacing(9);
                c.Item().Text("SUIVI").FontSize(7.5f).Bold().LetterSpacing(0.08f).FontColor(AccentTexte);
                foreach (var champ in new[] { "Intervention prévue le", "Réalisée le", "Montant facturé", "Observations" })
                    c.Item().Row(r =>
                    {
                        r.ConstantItem(118).Text(champ).FontColor(Gris);
                        r.RelativeItem().PaddingTop(10).LineHorizontal(0.5f).LineColor(GrisClair);
                    });
                c.Item().PaddingTop(4).Row(r =>
                {
                    r.ConstantItem(118).Text("Signature du client").FontColor(Gris);
                    r.RelativeItem().Height(30).Border(0.5f).BorderColor(GrisClair);
                });
            });
            if (fiche.QrValidation is not null)
            {
                row.ConstantItem(18);
                row.ConstantItem(104).Column(c =>
                {
                    c.Item().Height(92).Svg(fiche.QrValidation);
                    c.Item().PaddingTop(4).AlignCenter().Text("Après l'intervention, scanner pour la confirmer").FontSize(7).FontColor(Gris);
                });
            }
        });

    // --- Formats ----------------------------------------------------------------------------------------------------

    public static string FormatTelephone(string chiffres) =>
        chiffres.Length == 10 ? string.Join(' ', Enumerable.Range(0, 5).Select(i => chiffres.Substring(i * 2, 2))) : chiffres;

    public static string Euros(decimal montant) => $"{montant.ToString("#,0.##", Montants)} €";

    public static string Libelle(Profil p) => p == Profil.Particulier ? "Particulier" : "Professionnel";

    public static string Libelle(Frequence f) => f switch
    {
        Frequence.Mensuel => "Mensuel",
        Frequence.Trimestriel => "Trimestriel",
        _ => "Ponctuel",
    };

    /// <summary>« formule mensuelle » : l'adjectif s'accorde avec « formule ».</summary>
    public static string Formule(Frequence f) => f switch
    {
        Frequence.Mensuel => "formule mensuelle",
        Frequence.Trimestriel => "formule trimestrielle",
        _ => "formule ponctuelle",
    };

    public static string Libelle(Creneau c) => c switch
    {
        Creneau.SemaineMatin => "Semaine · matin",
        Creneau.SemaineApresMidi => "Semaine · après-midi",
        _ => "Samedi",
    };

    private static string Date(DateTime utc) => HeureParis.Depuis(utc).ToString("d MMMM yyyy", Fr);

    private static string Rang(int rang) => rang == 1 ? "1re" : $"{rang}e";

    private static string LireLogo()
    {
        using var flux = typeof(DevisPdf).Assembly.GetManifestResourceStream("AbsClean.Api.logo.svg")!;
        using var lecteur = new StreamReader(flux);
        return lecteur.ReadToEnd();
    }
}
