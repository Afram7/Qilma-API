namespace Qilma_API.Validators;

public static class PasswordValidator
{
    // Checks if the provided password matches the hashed password stored in the database
    public static async Task<bool> ValidatePasswordAsync(string password, string hashedPassword)
    {

        bool isValidPassword = await Task.Run(() => BC.EnhancedVerify(password, hashedPassword));
        return isValidPassword;
    }
}
