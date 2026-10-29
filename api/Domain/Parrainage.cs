namespace AbsClean.Api.Domain;

public enum StatutParrainage { EnAttente, Valide, Refuse }

/// <summary>
/// Lien parrain → filleul (CLAUDE.md §9 bis). La remise du parrain n'est activée (Valide)
/// qu'après confirmation par le gérant de la première intervention du filleul.
/// </summary>
public sealed class Parrainage
{
    public int Id { get; set; }
    public int ParrainId { get; set; }
    public Demande Parrain { get; set; } = null!;
    public int FilleulId { get; set; }
    public Demande Filleul { get; set; } = null!;
    public DateTime CreeLe { get; set; }
    public StatutParrainage Statut { get; set; } = StatutParrainage.EnAttente;
    public DateTime? ValideLe { get; set; }
    public DateTime? RemiseUtiliseeLe { get; set; }
}
