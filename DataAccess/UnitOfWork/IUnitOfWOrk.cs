using DataAccess.Repository;
using System;

namespace DataAccess.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IOrdersRepository OrdersRepository { get; }
        ICurrencyRepository CurrencyRepository { get; }
        IUserCredentialRepository UserCredentialRepository { get; }
        IUserRepository UserRepository { get; }
        void SaveChanges();
        IUnitOfWork GetNewUnitOfWork();
    }
}
