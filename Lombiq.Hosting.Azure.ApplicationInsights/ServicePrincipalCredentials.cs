namespace Lombiq.Hosting.Azure.ApplicationInsights;

public class ServicePrincipalCredentials
{
    /// <summary>
    /// Gets or sets the (directory) tenant ID of the Microsoft Entra application used to secure the control channel.
    /// </summary>
    public string TenantId { get; set; }

    /// <summary>
    /// Gets or sets the application (client) ID of the Microsoft Entra application used to secure the control channel.
    /// </summary>
    public string ClientId { get; set; }

    /// <summary>
    /// Gets or sets the client secret of the Microsoft Entra application used to secure the control channel.
    /// </summary>
    public string ClientSecret { get; set; }
}
