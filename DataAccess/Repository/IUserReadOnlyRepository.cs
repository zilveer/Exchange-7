using Entity;

namespace DataAccess.Repository
{
    public interface IUserReadOnlyRepository : IReadOnlyRepository<Users>
    {
        Users GetUser(string email);
        Users GetByUniqueId(string uniqueId);
    }
}
