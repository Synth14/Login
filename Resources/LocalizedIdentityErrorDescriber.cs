﻿using Microsoft.AspNetCore.Identity;

namespace Login.Resources;

public class LocalizedIdentityErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError DuplicateUserName(string userName)
    {
        return new IdentityError
        {
            Code = "DuplicateUserName",
            Description = string.Format(LocalizedIdentityErrors.DuplicateUserName, userName)
        };
    }
    public override IdentityError PasswordMismatch()
    {
        return new IdentityError
        {
            Code = nameof(PasswordMismatch),
            Description = string.Format(LocalizedIdentityErrors.PasswordMismatch)
        };
    }
}
