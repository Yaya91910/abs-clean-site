namespace AbsClean.Api.Domain;

/// <summary>
/// Client d'ABS clean, avec un numéro stable (CLI-00001) commun à toutes ses demandes.
/// Un client qui refait un devis est reconnu par son e-mail (casse ignorée) ou son téléphone : pas de doublon.
/// </summary>
public sealed class Client
{
    public int Id { get; set; }
    public string Numero { get; set; } = "";
    public DateTime CreeLe { get; set; }

    // Coordonnées de la première demande ; chaque demande garde aussi les siennes.
    public string Nom { get; set; } = "";
    public string Prenom { get; set; } = "";
    /// <summary>10 chiffres, sans espace.</summary>
    public string Telephone { get; set; } = "";
    /// <summary>En minuscules.</summary>
    public string Email { get; set; } = "";

    public List<Demande> Demandes { get; set; } = [];
}
