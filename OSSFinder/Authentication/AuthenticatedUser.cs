﻿using System;
using OSSFinder.Entities;

namespace OSSFinder.Authentication
{
    public class AuthenticatedUser
    {
        public User User { get; private set; }
        public Credential CredentialUsed { get; private set; }

        public AuthenticatedUser(User user, Credential cred)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            if (cred == null)
            {
                throw new ArgumentNullException("cred");
            }

            User = user;
            CredentialUsed = cred;
        }
    }
}
