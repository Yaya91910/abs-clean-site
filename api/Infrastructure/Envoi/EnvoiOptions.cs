namespace AbsClean.Api.Infrastructure.Envoi;

/// <summary>Coordonnées d'ABS clean pour le PDF (section « Entreprise »). Valeurs À FOURNIR par le gérant : jamais inventées.</summary>
public sealed class EntrepriseOptions
{
    public string Nom { get; set; } = "ABS clean";
    public string? Siret { get; set; }
    public string? Adresse { get; set; }
    public string? Telephone { get; set; }
    public string? Email { get; set; }
}

/// <summary>E-mail transactionnel (section « Email »). Sans ApiKey : mode test, les messages sont écrits dans un dossier local.</summary>
public sealed class EmailOptions
{
    public string? ApiKey { get; set; }
    public string? From { get; set; }
    public string? GerantTo { get; set; }
}

/// <summary>SMS (section « Sms »). Sans ApiKey : mode test.</summary>
public sealed class SmsOptions
{
    public string? ApiKey { get; set; }
    public string? Sender { get; set; }
    public string? GerantNumero { get; set; }
}

/// <summary>Mode test : dossier où sont déposés e-mails, pièces jointes et SMS au lieu d'être envoyés.</summary>
public sealed class EnvoiTestOptions
{
    public string Dossier { get; set; } = "boite-envoi";
}

/// <summary>Envois après une demande (section « Envois »).</summary>
public sealed class EnvoisOptions
{
    /// <summary>
    /// true (par défaut) : la réponse part dès l'enregistrement et les envois suivent en arrière-plan.
    /// false : la réponse attend la fin des envois (tests).
    /// </summary>
    public bool EnArrierePlan { get; set; } = true;
}

/// <summary>Envois réels (Brevo) ou mode test, selon la présence des clés d'API au démarrage.</summary>
public sealed record ModeEnvois(bool EmailReel, bool SmsReel)
{
    public bool Complet => EmailReel && SmsReel;
}
