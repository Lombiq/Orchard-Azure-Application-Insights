namespace Lombiq.Hosting.Azure.ApplicationInsights.Models;

public enum EntraAuthenticationType
{
    /// <summary>
    /// Don't use Entra authentication.
    /// </summary>
    None,

    /// <summary>
    /// Use Managed Identity.
    /// </summary>
    ManagedIdentity,

    /// <summary>
    /// Use a service principal. This requires setting up ServicePrincipalCredentials.
    /// </summary>
    ServicePrincipal,
}
