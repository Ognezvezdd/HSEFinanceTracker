using HSEFinanceTracker.Base;
using HSEFinanceTracker.Base.Entities;

namespace HSEFinanceTracker.Base.Factories
{
    public sealed class FinanceFactory : IFinanceFactory
    {
        public BankAccount CreateBankAccount(string name, decimal openingBalance = 0m)
        {
            return new BankAccount(Guid.NewGuid(), name, openingBalance);
        }

        public Category CreateCategory(CategoryType type, string name)
        {
            return new Category(Guid.NewGuid(), type, name);
        }

        public Operation CreateOperation(OperationType type, Guid accountId, Guid categoryId,
            decimal amount, DateTime date, string? description = null)
        {
            return new Operation(Guid.NewGuid(), type, accountId, categoryId, amount, date, description);
        }
    }
}