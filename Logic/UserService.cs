using DataAccess.UnitOfWork;
using Entity;

namespace Logic
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IReadOnlyContext _readonlyContext;
        public UserService(IUnitOfWork unitOfWork, IReadOnlyContext readOnlyContext)
        {
            _unitOfWork = unitOfWork;
            _readonlyContext = readOnlyContext;
        }

        public Users GetByUniqueId(string uniqueId)
        {
            return _readonlyContext.UserReadOnlyRepository.GetByUniqueId(uniqueId);
        }

        public Users GetUser(string email)
        {
            return _readonlyContext.UserReadOnlyRepository.GetUser(email);
        }

        public Users GetUser(int userId)
        {
            return _readonlyContext.UserReadOnlyRepository.Find(userId);
        }
    }
}
