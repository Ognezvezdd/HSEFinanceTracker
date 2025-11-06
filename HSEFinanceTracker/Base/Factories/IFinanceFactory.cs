using HSEFinanceTracker.Base;
using HSEFinanceTracker.Base.Entities;

namespace HSEFinanceTracker.Base.Factories
{
    public interface IFinanceFactory
    {
        BankAccount CreateBankAccount(string name, decimal openingBalance = 0m);
        BankAccount CreateBankAccount(Guid id, string name, decimal openingBalance = 0m);

        Category CreateCategory(CategoryType type, string name);
        Category CreateCategory(Guid id, CategoryType type, string name);

        Operation CreateOperation(OperationType type, Guid accountId, Guid categoryId,
            decimal amount, DateTime date, string? description = null);

        Operation CreateOperation(Guid operationId, OperationType type, Guid accountId, Guid categoryId,
            decimal amount, DateTime date, string? description = null);
    }
}