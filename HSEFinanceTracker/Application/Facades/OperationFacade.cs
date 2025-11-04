using HSEFinanceTracker.Base;
using HSEFinanceTracker.Base.Entities;
using HSEFinanceTracker.Base.Factories;
using HSEFinanceTracker.Base.Repositories;

namespace HSEFinanceTracker.Application.Facades
{
    public sealed class OperationFacade
    {
        private readonly IBankAccountRepo _accounts;
        private readonly ICategoryRepo _categories;
        private readonly IOperationRepo _operations;
        private readonly IFinanceFactory _factory;

        public OperationFacade(IBankAccountRepo accs, ICategoryRepo cats, IOperationRepo ops, IFinanceFactory factory)
        {
            _accounts = accs;
            _categories = cats;
            _operations = ops;
            _factory = factory;
        }

        public Operation Create(OperationType type, Guid accountId, Guid categoryId,
            decimal amount, DateTime date, string? description = null)
        {
            var account = _accounts.Get(accountId) ?? throw new InvalidOperationException("Account not found");
            var category = _categories.Get(categoryId) ?? throw new InvalidOperationException("Category not found");

            if ((type == OperationType.Income && category.Type != CategoryType.Income) ||
                (type == OperationType.Expense && category.Type != CategoryType.Expense))
            {
                throw new InvalidOperationException("Operation type must match category type");
            }

            var op = _factory.CreateOperation(type, accountId, categoryId, amount, date, description);
            _operations.Add(op);

            var delta = type == OperationType.Income ? amount : -amount;
            account.Apply(delta);
            _accounts.Update(account);

            return op;
        }

        public IEnumerable<Operation> All()
        {
            return _operations.All();
        }

        public IEnumerable<Operation> ForAccount(Guid accountId, DateTime? from = null, DateTime? to = null)
        {
            return _operations.ForAccount(accountId, from, to);
        }

        public void Delete(Guid opId)
        {
            var op = _operations.Get(opId) ?? throw new InvalidOperationException("Operation not found");
            var account = _accounts.Get(op.BankAccountId) ?? throw new InvalidOperationException("Account not found");

            var delta = op.Type == OperationType.Income ? -op.Amount : op.Amount; // откат
            account.Apply(delta);
            _accounts.Update(account);

            _operations.Remove(opId);
        }
    }
}