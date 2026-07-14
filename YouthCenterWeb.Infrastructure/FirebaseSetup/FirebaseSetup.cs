// Infrastructure/Firebase/FirebaseSetup.cs
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

namespace YouthCenterWeb.Infrastructure.Firebase
{
    public static class FirebaseSetup
    {
        public static void InitializeFromJson(string serviceAccountJson)
        {
            if (FirebaseApp.DefaultInstance != null) return;

            if (string.IsNullOrWhiteSpace(serviceAccountJson))
            {
                throw new InvalidOperationException(
                    "Firebase:ServiceAccountJson configuration value is missing or empty.");
            }

            var credential = CredentialFactory
                .FromJson<ServiceAccountCredential>(serviceAccountJson);

            FirebaseApp.Create(new AppOptions
            {
                Credential = credential.ToGoogleCredential(),
            });
        }
    }
}