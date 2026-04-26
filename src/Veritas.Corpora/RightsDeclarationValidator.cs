using Veritas.Core.Models;

namespace Veritas.Corpora;

public static class RightsDeclarationValidator
{
    private static readonly HashSet<string> ValidValues = new(StringComparer.OrdinalIgnoreCase)
    {
        "own_content", "permission_granted", "licensed_for_private_use",
        "public_domain", "open_access", "unknown_needs_review"
    };

    public static bool IsValid(string value) => ValidValues.Contains(value);

    public static RightsDeclaration Parse(string value) => value.ToLowerInvariant() switch
    {
        "own_content"               => RightsDeclaration.OwnContent,
        "permission_granted"        => RightsDeclaration.PermissionGranted,
        "licensed_for_private_use"  => RightsDeclaration.LicensedForPrivateUse,
        "public_domain"             => RightsDeclaration.PublicDomain,
        "open_access"               => RightsDeclaration.OpenAccess,
        "unknown_needs_review"      => RightsDeclaration.UnknownNeedsReview,
        _                           => throw new ArgumentException($"Invalid rights declaration: {value}")
    };
}
