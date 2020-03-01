using Entity;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace DataAccess.Repository
{
    public class UserCredentialRepository : Repository<UserCredentials>, IUserCredentialRepository
    {
        private readonly DbSet<UserCredentials> _dbSetUserCredentials;
        public UserCredentialRepository(DbSet<UserCredentials> dbSetUserCredentials) : base(dbSetUserCredentials)
        {
            _dbSetUserCredentials = dbSetUserCredentials;
        }

        public UserCredentials GetByEmailOrPhone(string emailOrPhone)
        {
            return _dbSetUserCredentials.FirstOrDefault(x => x.User.Email == emailOrPhone || x.User.MobileNumber == emailOrPhone);
        }

        public UserCredentials GetByUserId(int userId)
        {
            return _dbSetUserCredentials.FirstOrDefault(x => x.UserId == userId);
        }
    }
}
