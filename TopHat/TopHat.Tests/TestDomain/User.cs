﻿namespace TopHat.Tests.TestDomain
{
    public class User
    {
        public virtual string Username { get; set; }

        public virtual string EmailAddress { get; set; }

        public virtual string Password { get; set; }

        public virtual bool IsEnabled { get; set; }
    }
}