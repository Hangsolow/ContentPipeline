namespace ContentPipeline.Utils;

public sealed record class GeneratorOptions
{
    public bool FormsEnabled { get; }

    public GeneratorOptions(string? formsEnabled)
    {
        FormsEnabled = IsFeatureEnabled(formsEnabled);
    }

    private static bool IsFeatureEnabled(string? formEnabledValue)
    {
        return StringComparer.OrdinalIgnoreCase.Equals("enable", formEnabledValue)
               || StringComparer.OrdinalIgnoreCase.Equals("enabled", formEnabledValue)
               || StringComparer.OrdinalIgnoreCase.Equals("true", formEnabledValue);
    }

    // Equals and GetHashCode
}
