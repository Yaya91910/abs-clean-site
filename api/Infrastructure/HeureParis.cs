namespace AbsClean.Api.Infrastructure;

/// <summary>Heure légale française : dates affichées et année des numéros de devis (les dates sont stockées en UTC).</summary>
public static class HeureParis
{
    private static readonly TimeZoneInfo Fuseau = TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris");

    public static DateTime Depuis(DateTime utc) => TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), Fuseau);
}
