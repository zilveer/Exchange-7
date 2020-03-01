using Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace DataAccess.Repository
{
    public class UserRepository : Repository<Users>, IUserRepository, IReadOnlyRepository<Users>
    {
        private readonly DbSet<Users> _usersDbSet;
        public UserRepository(DbSet<Users> dbset) : base(dbset)
        {
            _usersDbSet = dbset;
        }

        public Users GetByUniqueId(string uniqueId)
        {
            var guid = Guid.Parse(uniqueId);
            return _usersDbSet.FirstOrDefault(x => x.UniqueId == guid);
        }

        public Users GetUser(string email)
        {
            return _usersDbSet.FirstOrDefault(x => x.Email == email);
        }
    }
}
