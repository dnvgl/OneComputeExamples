namespace PiEstimatorClient.ClientCredentials
{
    using System;
    using System.Threading.Tasks;

    using Microsoft.Identity.Client;

    using PiEstimatorClientLibrary;

    /// <summary>
    /// This is a .NET Core console application that does client credentials authentication with the OneCompute
    /// Service Registry and runs the PiSampler to create and submit a job to the PiEstimatorWorker.
    /// </summary>
    class Program
    {
        /// <summary>
        /// The OneCompute service registry tenant. You will get this when registering your app with the OneCompute.
        /// </summary>
        private const string OneComputeServiceRegistryTenant = "<Enter the tenant id for the OneCompute Service Registry>";

        /// <summary>
        /// The API scope for the OneCompute API. You will get this when registering your app with the OneCompute.
        /// </summary>
        private const string OneComputeApiScope = "<Enter the API scope for the OneCompute API>";

        /// <summary>
        /// The OneCompute application identifier. You will get this when registering your app with the OneCompute.
        /// </summary>
        private const string OneComputeAppId = "<Enter your OneCompute app id>";

        /// <summary>
        /// The OneCompute API client secret. You will get this when registering your app with the OneCompute.
        /// </summary>
        private const string OneComputeClientSecret = "<Enter your client secret>";

        static async Task Main(string[] args)
        {
            var authenticationResult = await Authenticate();

            if (authenticationResult?.AccessToken == null)
            {
                Console.WriteLine($"Failed to authenticate with OneComputeAppId {OneComputeAppId}.");
                Console.WriteLine("Check whether the OneComputeAppId and your OneComputeClientSecret are correct.");
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
            var authConfig = (AppId: OneComputeAppId,
                              ClientSecret: OneComputeClientSecret);

            return await LoginWithClientCredentials(
                       authConfig.AppId,
                       authConfig.ClientSecret);
        }

        /// <summary>Logins with client credentials.</summary>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="clientSecret">The client secret.</param>
        /// <returns>Auth result</returns>
        private static async Task<AuthenticationResult> LoginWithClientCredentials(string clientId, string clientSecret)
        {
            Console.WriteLine("Authenticating with client credentials ....");

            const string Tenant = OneComputeServiceRegistryTenant;
            const string Scope = OneComputeApiScope;
            var authority = $"https://login.microsoftonline.com/{Tenant}";

            var app = ConfidentialClientApplicationBuilder.Create(clientId)
                .WithClientSecret(clientSecret)
                .WithAuthority(new Uri(authority))
                .Build();

            return await app.AcquireTokenForClient(new[] { Scope }).ExecuteAsync();
        }
    }
}
