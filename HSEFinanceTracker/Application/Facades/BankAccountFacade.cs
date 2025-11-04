using HSEFinanceTracker.Base.Entities;
using HSEFinanceTracker.Base.Factories;
using HSEFinanceTracker.Base.Repositories;

namespace HSEFinanceTracker.Application.Facades
{
    public sealed class BankAccountFacade
    {
        private readonly IBankAccountRepo _accounts;
        private readonly IFinanceFactory _factory;

        public BankAccountFacade(IBankAccountRepo accounts, IFinanceFactory factory)
        {
            _accounts = accounts;
            _factory = factory;
        }

        public BankAccount Create(string name, decimal openingBalance = 0m)
        {
            var acc = _factory.CreateBankAccount(name, openingBalance);
            _accounts.Add(acc);
            return acc;
        }

        public IEnumerable<BankAccount> All()
        {
            return _accounts.All();
        }

        public void Rename(Guid id, string newName)
        {
            var acc = _accounts.Get(id) ?? throw new InvalidOperationException("Account not found");
            acc.Rename(newName);
            _accounts.Update(acc);
        }

        public void Delete(Guid id)
        {
            _accounts.Remove(id);
        }
    }
}