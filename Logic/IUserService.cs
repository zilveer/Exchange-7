using Entity;

namespace Logic
{
    public interface IUserService
    {
        Users GetUser(string email);
        Users GetUser(int userId);
        Users GetByUniqueId(string uniqueId);
    }
}
