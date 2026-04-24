using Microsoft.AspNetCore.Identity;

namespace WEB.Data.Identity;

public class SpanishIdentityErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError PasswordRequiresNonAlphanumeric()
    {
        return new IdentityError
        {
            Code = nameof(PasswordRequiresNonAlphanumeric),
            Description = "La contraseña debe tener al menos un carácter no alfanumérico (por ejemplo: !@#$%^&*)."
        };
    }
    
    public override IdentityError PasswordRequiresDigit()
    {
        return new IdentityError
        {
            Code = nameof(PasswordRequiresDigit),
            Description = "La contraseña debe tener al menos un dígito ('0'-'9')."
        };
    }
    
    public override IdentityError PasswordRequiresLower()
    {
        return new IdentityError
        {
            Code = nameof(PasswordRequiresLower),
            Description = "La contraseña debe tener al menos una letra minúscula ('a'-'z')."
        };
    }
    
    public override IdentityError PasswordRequiresUpper()
    {
        return new IdentityError
        {
            Code = nameof(PasswordRequiresUpper),
            Description = "La contraseña debe tener al menos una letra mayúscula ('A'-'Z')."
        };
    }
    
    public override IdentityError PasswordTooShort(int length)
    {
        return new IdentityError
        {
            Code = nameof(PasswordTooShort),
            Description = $"La contraseña debe tener al menos {length} caracteres."
        };
    }
    
    public override IdentityError DuplicateUserName(string userName)
    {
        return new IdentityError
        {
            Code = nameof(DuplicateUserName),
            Description = $"El nombre de usuario '{userName}' ya está en uso."
        };
    }
    
    public override IdentityError InvalidUserName(string? userName)
    {
        return new IdentityError
        {
            Code = nameof(InvalidUserName),
            Description = $"El nombre de usuario '{userName}' no es válido, solo puede contener letras y números."
        };
    }
}