namespace AbsClean.Api.Domain;

public enum Creneau { SemaineMatin, SemaineApresMidi, Samedi }

public enum StatutDemande { Nouvelle }

/// <summary>Demande de devis enregistrée (CLAUDE.md §11). Base du futur CRM.</summary>
public sealed class Demande
{
    public int Id { get; set; }
    public string Numero { get; set; } = "";
    public DateTime CreeLe { get; set; }

    public Profil Profil { get; set; }
    public Frequence Frequence { get; set; }
    public int NbVitres { get; set; }
    public int NbBaies { get; set; }
    public int NbVelux { get; set; }
    public int NbVitrines { get; set; }
    public decimal Total { get; set; }

    public string Nom { get; set; } = "";
    public string Prenom { get; set; } = "";
    public string Telephone { get; set; } = "";
    public string Email { get; set; } = "";
    public string Adresse { get; set; } = "";
    public string CodePostal { get; set; } = "";
    public string Ville { get; set; } = "";
    public string? Acces { get; set; }
    public List<Creneau> Creneaux { get; set; } = [];
    public string? Commentaire { get; set; }
    public DateTime ConsentementLe { get; set; }

    public StatutDemande Statut { get; set; } = StatutDemande.Nouvelle;
    public bool EmailClientEnvoye { get; set; }
    public bool EmailGerantEnvoye { get; set; }
    public bool SmsEnvoye { get; set; }

    // Parrainage (§9 bis)
    public string CodeParrainPersonnel { get; set; } = "";
    public string? CodeParrainUtilise { get; set; }
    public int? ParrainId { get; set; }

    // Validation de l'intervention par le gérant (lien à usage unique ; seule l'empreinte du jeton est stockée)
    public string? JetonInterventionHash { get; set; }
    public DateTime? InterventionRealiseeLe { get; set; }
}
