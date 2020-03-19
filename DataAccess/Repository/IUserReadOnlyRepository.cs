using Entity;

namespace DataAccess.Repository
{
    public interface IUserReadOnlyRepository : IReadOnlyRepository<Users>
    {
        Users GetUser(int id);
        Users GetUser(string email);
        Users GetByUniqueId(string uniqueId);
    }
}
