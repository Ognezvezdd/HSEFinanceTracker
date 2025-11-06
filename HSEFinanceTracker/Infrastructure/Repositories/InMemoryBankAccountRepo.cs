using HSEFinanceTracker.Base.Entities;
using HSEFinanceTracker.Base.Repositories;

namespace HSEFinanceTracker.Infrastructure.Repositories
{
    public sealed class InMemoryBankAccountRepo : IBankAccountRepo
    {
        private readonly Dictionary<Guid, BankAccount> _data = new();

        public BankAccount Add(BankAccount account)
        {
            return _data[account.Id] = account;
        }

        public BankAccount? Get(Guid id)
        {
            return _data.GetValueOrDefault(id);
        }

        public IEnumerable<BankAccount> All()
        {
            return _data.Values;
        }

        public void Update(BankAccount account)
        {
            _data[account.Id] = account;
        }

        public void Remove(Guid id)
        {
            _data.Remove(id);
        }
    }
}