using Firebase.Auth.Providers;
using Firebase.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ContainerFarmManagement.Config;
using ContainerFarmManagement.Models;

namespace ContainerFarmManagement.Services
{
    public static class AuthService
    {
        // Configure...
        private static FirebaseAuthConfig config = new FirebaseAuthConfig
        {
            ApiKey = ResourceStrings.Apikey,
            AuthDomain = ResourceStrings.AuthorizedDomain,
            Providers = new FirebaseAuthProvider[]
            {
                // Add and configure individual providers
                new EmailProvider()
            },
        };
        // ...and create your FirebaseAuthClient
        public static FirebaseAuthClient Client { get; } = new FirebaseAuthClient(config);
        public static UserCredential UserCreds { get; set; }
        public static Account Account { get; set; }
    }
}
