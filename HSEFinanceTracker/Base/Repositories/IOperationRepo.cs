using HSEFinanceTracker.Base.Entities;

namespace HSEFinanceTracker.Base.Repositories
{
    public interface IOperationRepo
    {
        Operation Add(Operation op);
        Operation? Get(Guid id);
        IEnumerable<Operation> All();
        IEnumerable<Operation> ForAccount(Guid accountId, DateTime? from = null, DateTime? to = null);
        void Remove(Guid id);
    }
}