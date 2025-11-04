using HSEFinanceTracker.Base.Entities;

namespace HSEFinanceTracker.Base.Repositories
{
    public interface IBankAccountRepo
    {
        BankAccount Add(BankAccount account);
        BankAccount? Get(Guid id);
        IEnumerable<BankAccount> All();
        void Update(BankAccount account);
        void Remove(Guid id);
    }
}