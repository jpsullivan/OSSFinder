using System;

namespace OSSFinder.Authentication
{
    public static class CredentialTypes
    {
        public static class Password
        {
            public static readonly string Prefix = "password.";
            public static readonly string Pbkdf2 = Prefix + "pbkdf2";
            public static readonly string Sha1 = Prefix + "sha1";
        }
        public static readonly string ApiKeyV1 = "apikey.v1";
        public static readonly string ExternalPrefix = "external.";


        public static bool IsPassword(string type)
        {
            return type.StartsWith(Password.Prefix, StringComparison.OrdinalIgnoreCase);
        }
    }
}
