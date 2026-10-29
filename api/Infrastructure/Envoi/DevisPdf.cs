using System.Globalization;
using AbsClean.Api.Domain;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AbsClean.Api.Infrastructure.Envoi;

/// <summary>Devis PDF (CAHIER-DES-CHARGES.md §9.4) : logo, numéro, date, client, lignes, total « à partir de », mention, coordonnées ABS clean.</summary>
public sealed class DevisPdf(EntrepriseOptions entreprise)
{
    private static readonly CultureInfo Fr = CultureInfo.GetCultureInfo("fr-FR");

    /// <summary>Séparateur de milliers en espace simple : l'espace fine insécable de .NET n'existe pas dans l'alphabet SMS.</summary>
    private static readonly NumberFormatInfo Montants = new() { NumberGroupSeparator = " ", NumberDecimalSeparator = "," };
    private static readonly string Logo = LireLogo();
    private static readonly TimeZoneInfo Paris = TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris");

    private const string Ink = "#15345F";
    private const string Accent = "#2F6FD6";
    private const string Gris = "#5B6B80";

    public byte[] Generer(Demande demande)
    {
        // Le détail est recalculé depuis les quantités enregistrées : même source de vérité que le total.
        var devis = CalculDevis.Calculer(
            new QuantitesDevis(demande.NbVitres, demande.NbBaies, demande.NbVelux, demande.NbVitrines),
            demande.Profil, demande.Frequence);

        return Document.Create(doc => doc.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(40);
            page.DefaultTextStyle(t => t.FontSize(10).FontColor(Ink));

            page.Header().Row(row =>
            {
                row.ConstantItem(130).Svg(Logo);
                row.RelativeItem().AlignRight().Column(col =>
                {
                    col.Item().Text("Devis").FontSize(22).Bold();
                    col.Item().Text(demande.Numero).FontSize(12).SemiBold().FontColor(Accent);
                    col.Item().Text($"Date : {TimeZoneInfo.ConvertTimeFromUtc(demande.CreeLe, Paris).ToString("d MMMM yyyy", Fr)}").FontColor(Gris);
                });
            });

            page.Content().PaddingVertical(24).Column(col =>
            {
                col.Spacing(18);

                col.Item().Row(row =>
                {
                    row.RelativeItem().Element(Bloc).Column(c =>
                    {
                        c.Item().Text("Client").SemiBold().FontColor(Accent);
                        c.Item().Text($"{demande.Prenom} {demande.Nom}").SemiBold();
                        c.Item().Text(demande.Adresse);
                        c.Item().Text($"{demande.CodePostal} {demande.Ville}");
                        if (demande.Acces is not null) c.Item().Text($"Accès : {demande.Acces}").FontColor(Gris);
                        c.Item().Text(FormatTelephone(demande.Telephone));
                        c.Item().Text(demande.Email);
                    });
                    row.ConstantItem(16);
                    row.RelativeItem().Element(Bloc).Column(c =>
                    {
                        c.Item().Text("Prestation").SemiBold().FontColor(Accent);
                        c.Item().Text($"Profil : {Libelle(demande.Profil)}");
                        c.Item().Text($"Fréquence : {Libelle(demande.Frequence)}");
                        if (demande.Creneaux.Count > 0)
                            c.Item().Text($"Créneaux préférés : {string.Join(", ", demande.Creneaux.Select(Libelle))}");
                    });
                });

                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(4);
                        c.RelativeColumn(1.4f);
                        c.RelativeColumn(2);
                        c.RelativeColumn(2);
                    });

                    table.Header(h =>
                    {
                        foreach (var titre in new[] { "Élément", "Quantité", "Prix unitaire" })
                            h.Cell().Background(Ink).Padding(8).Text(titre).FontColor(Colors.White).SemiBold();
                        h.Cell().Background(Ink).Padding(8).AlignRight().Text("Montant").FontColor(Colors.White).SemiBold();
                    });

                    foreach (var ligne in devis.Lignes)
                    {
                        table.Cell().Element(Cellule).Text(ligne.Libelle);
                        table.Cell().Element(Cellule).Text(ligne.Quantite.ToString(Fr));
                        table.Cell().Element(Cellule).Text((ligne.APartirDe ? "dès " : "") + Euros(ligne.PrixUnitaire));
                        table.Cell().Element(Cellule).AlignRight().Text(Euros(ligne.Montant));
                    }
                });

                col.Item().AlignRight().Text(t =>
                {
                    t.Span("Total : ").FontSize(13);
                    t.Span($"à partir de {Euros(demande.Total)}").FontSize(16).Bold().FontColor(Accent);
                });

                col.Item().Element(Bloc).Column(c =>
                {
                    c.Item().Text(devis.Mention).SemiBold();
                    if (devis.ReductionPourcent is null)
                        c.Item().Text($"Formule {Libelle(demande.Frequence)} : la réduction applicable vous sera confirmée lors de l'échange.").FontColor(Gris);
                    c.Item().Text(Tarifs.MentionOption).FontColor(Gris);
                });
            });

            page.Footer().AlignCenter().Column(col =>
            {
                col.Item().AlignCenter().Text(entreprise.Nom).SemiBold();
                var coordonnees = new[] { entreprise.Adresse, entreprise.Telephone, entreprise.Email }.Where(v => !string.IsNullOrWhiteSpace(v));
                if (coordonnees.Any()) col.Item().AlignCenter().Text(string.Join(" · ", coordonnees)).FontSize(9).FontColor(Gris);
                if (!string.IsNullOrWhiteSpace(entreprise.Siret)) col.Item().AlignCenter().Text($"SIRET : {entreprise.Siret}").FontSize(9).FontColor(Gris);
            });
        })).GeneratePdf();
    }

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

    public static string Libelle(Creneau c) => c switch
    {
        Creneau.SemaineMatin => "Semaine · matin",
        Creneau.SemaineApresMidi => "Semaine · après-midi",
        _ => "Samedi",
    };

    private static IContainer Bloc(IContainer c) => c.Background("#F4F8FF").Padding(12);

    private static IContainer Cellule(IContainer c) => c.BorderBottom(1).BorderColor("#E3E8F0").Padding(8);

    private static string LireLogo()
    {
        using var flux = typeof(DevisPdf).Assembly.GetManifestResourceStream("AbsClean.Api.logo.svg")!;
        using var lecteur = new StreamReader(flux);
        return lecteur.ReadToEnd();
    }
}
