using HSEFinanceTracker.Base.Entities;
using HSEFinanceTracker.Base.Factories;
using HSEFinanceTracker.Base.Repositories;

namespace HSEFinanceTracker.Infrastructure.Repositories
{
    public sealed class InMemoryOperationRepo : IOperationRepo
    {
        private readonly Dictionary<Guid, Operation> _data = new();

        public Operation Add(Operation op)
        {
            return _data[op.Id] = op;
        }

        public Operation? Get(Guid id)
        {
            return _data.TryGetValue(id, out var o) ? o : null;
        }

        public IEnumerable<Operation> All()
        {
            return _data.Values;
        }

        public IEnumerable<Operation> ForAccount(Guid accountId, DateTime? from = null, DateTime? to = null)
        {
            return _data.Values.Where(o => o.BankAccountId == accountId &&
                                           (from is null || o.Date >= from) &&
                                           (to is null || o.Date <= to));
        }

        public void Remove(Guid id)
        {
            _data.Remove(id);
        }
    }
}