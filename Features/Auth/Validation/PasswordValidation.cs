namespace Nexus.Features.Auth.Validation;

public class PasswordValidation : IPasswordValidation
{
    public void ValidatePassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentNullException(nameof(password), "Password cannot be null or empty.");
        
        if (!password.Any(char.IsUpper))
            throw new ArgumentNullException(nameof(password), "Password must contain uppercase letter");

        if (!password.Any(char.IsLower))
            throw new ArgumentNullException(nameof(password), "Password must contain lowercase letter");

        if (!password.Any(char.IsDigit))
            throw new ArgumentNullException(nameof(password), "Password must contain a number");
    }
}