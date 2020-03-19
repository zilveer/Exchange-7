using DataAccess.Repository;
using Entity;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.UnitOfWork
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1063:Implement IDisposable Correctly", Justification = "<Pending>")]
    public class UnitOfWork : IUnitOfWork
    {

        private readonly ExchangeContext _dbContext;
        private IOrdersRepository _ordersRepository;
        private ICurrencyRepository _currencyRepository;
        private IUserCredentialRepository _userCredentialRepository;
        private IUserRepository _userRepository;
        public UnitOfWork(ExchangeContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IOrdersRepository OrdersRepository
        {
            get
            {
                if (_ordersRepository == null)
                    _ordersRepository = new OrdersRepository(_dbContext.Set<Orders>(), _dbContext);

                return _ordersRepository;
            }
        }

        public ICurrencyRepository CurrencyRepository
        {
            get
            {
                if (_currencyRepository == null)
                    _currencyRepository = new CurrencyRepository(_dbContext.Set<Currency>());

                return _currencyRepository;
            }
        }

        public IUserCredentialRepository UserCredentialRepository
        {
            get
            {
                if (_userCredentialRepository == null)
                    _userCredentialRepository = new UserCredentialRepository(_dbContext.Set<UserCredentials>());

                return _userCredentialRepository;
            }
        }

        public IUserRepository UserRepository
        {
            get
            {
                if (_userRepository == null)
                    _userRepository = new UserRepository(_dbContext.Set<Users>());

                return _userRepository;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1063:Implement IDisposable Correctly", Justification = "<Pending>")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize", Justification = "<Pending>")]
        public void Dispose()
        {
            _dbContext.Dispose();
        }

        public IUnitOfWork GetNewUnitOfWork()
        {
            return new UnitOfWork(new ExchangeContext(new GlobalQueryFilterRegisterer(), "Host=localhost;Database=Exchange;Username=postgres;Password=root"));
        }

        public void SaveChanges()
        {
            using (_dbContext.Database.BeginTransaction())
            {
                _dbContext.SaveChanges();
            }
        }
    }
}
