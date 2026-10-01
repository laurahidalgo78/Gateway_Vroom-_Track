namespace Gateway.Infrastructure.Configuration;

public class SuiteCrmOptions
{
    public const string SectionName = "SuiteCrm";

    public string BaseUrl { get; set; } = "http://172.16.0.21";
    public string ServicePath { get; set; } = "/legacy/service/v4_1/rest.php";
    public string Username { get; set; } = "admin";
    public string Password { get; set; } = "123Alejandro.";
    public string PasswordMd5 { get; set; } = "5aa1b8bbc427d07af4f3b440adaf1990";
    public string ApplicationName { get; set; } = "VroomService Gateway";
    public int TimeoutSeconds { get; set; } = 30;

    public string FullEndpointUrl => $"{BaseUrl.TrimEnd('/')}/{ServicePath.TrimStart('/')}";
}
