namespace PiEstimatorClient
{
    using System;
    using System.Threading.Tasks;

    using Microsoft.Identity.Client;

    using PiEstimatorClientLibrary;

    /// <summary>
    /// This is a .NET Framework console application that does interactive Veracity authentication and runs the
    /// PiSampler to create and submit a job to the PiEstimatorWorker.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The veracity application identifier. You will get this by registering your app with Veracity.
        /// </summary>
        private const string VeracityAppId = "<Enter your Veracity app id>";

        /// <summary>
        /// The veracity API client identifier. You will get this by registering your app with Veracity.
        /// </summary>
        private const string VeracityApiClientId = "<Enter your Veracity client id>";

        public static async Task Main(string[] args)
        {
            var authenticationResult = await Authenticate();

            if (authenticationResult?.AccessToken == null)
            {
                Console.WriteLine($"Failed to authenticate with VeracityAppId {VeracityAppId} and ApiClientId {VeracityApiClientId}.");
                Console.WriteLine("Check whether the VeracityAppId and ApiClientId are correct.");
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
                return;
            }

            await PiSampler.Run(authenticationResult.AccessToken);
        }

        /// <summary>Authenticate with the specified auth provider.</summary>
        /// <returns>Auth result</returns>
        private static async Task<AuthenticationResult> Authenticate()
        {
            Console.WriteLine("Authenticating....");
            var authConfig = (AppId: VeracityAppId,
                                     ApiClientId: VeracityApiClientId);

            return await LoginWithVeracityAsync(authConfig.AppId, authConfig.ApiClientId);
        }

        /// <summary>
        /// Login with Veracity
        /// </summary>
        /// <param name="appClientId">The application client identifier.</param>
        /// <param name="apiClientId">The API client identifier.</param>
        /// <returns>
        /// Returns a Task
        /// </returns>
        private static async Task<AuthenticationResult> LoginWithVeracityAsync(string appClientId, string apiClientId)
        {
            try
            {
                // Active Directory Tenant where app is registered
                const string Tenant = "dnvglb2cprod.onmicrosoft.com";

                // Policy for authentication
                const string PolicySignUpSignIn = "B2C_1A_SignInWithADFSIdp";

                // List of scopes for tenant
                // openid and offline_access added by default, no need to repeat
                string[] apiScopes =
                {
                    $"https://dnvglb2cprod.onmicrosoft.com/{apiClientId}/user_impersonation"
                };

                // Url where authentication will take place. 
                var authority = $"https://login.microsoftonline.com/tfp/{Tenant}/{PolicySignUpSignIn}/oauth2/v2.0/authorize";

                // Tries to connect to Authority with given Scopes and Policies.
                // Results with separate dialog where user needs to specify credentials.
                var clientApplication = PublicClientApplicationBuilder.Create(appClientId)
                    .WithB2CAuthority(authority)
                    .Build();

                return await clientApplication.AcquireTokenInteractive(apiScopes)
                                 .WithUseEmbeddedWebView(true)
                                 .ExecuteAsync();
            }
            catch (Exception ex)
            {
                var errorMsg = $"Error...{Environment.NewLine}{ex.Message}{Environment.NewLine}";
                Console.WriteLine(errorMsg);
            }

            return null;
        }
    }
}
