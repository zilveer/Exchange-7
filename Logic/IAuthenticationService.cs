using Entity;

namespace Logic
{
    public interface IAuthenticationService
    {
        BusinessOperationResult<UserCredentials> Authenticate(string emailOrMobile, string password);
        BusinessOperationResult<UserCredentials> VerifyTotp(int userId, string totp);
        bool IsTwoFactorEnabled(int userId);
    }
}