using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Options;

namespace remoteControllerApp.Firebase;

public class FirebaseAuthService
{
    private readonly FirebaseSettings _settings;
    private readonly IWebHostEnvironment _environment;
    private GoogleCredential? _credential;

    private static readonly string[] Scopes =
    {
        "https://www.googleapis.com/auth/userinfo.email",
        "https://www.googleapis.com/auth/firebase.database"
    };

    public FirebaseAuthService(
        IOptions<FirebaseSettings> options,
        IWebHostEnvironment environment)
    {
        _settings = options.Value;
        _environment = environment;
    }

    public async Task<string> GetAccessTokenAsync()
    {
        if (string.IsNullOrWhiteSpace(_settings.CredentialPath))
        {
            throw new InvalidOperationException("Firebase CredentialPath is missing.");
        }

        var credentialPath = Path.Combine(
            _environment.ContentRootPath,
            _settings.CredentialPath
        );

        if (!File.Exists(credentialPath))
        {
            throw new FileNotFoundException(
                $"Firebase credential file not found: {credentialPath}"
            );
        }

        _credential ??= GoogleCredential
            .FromFile(credentialPath)
            .CreateScoped(Scopes);

        return await _credential.UnderlyingCredential.GetAccessTokenForRequestAsync();
    }
}