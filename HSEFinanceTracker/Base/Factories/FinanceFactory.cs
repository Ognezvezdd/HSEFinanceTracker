using HSEFinanceTracker.Base;
using HSEFinanceTracker.Base.Entities;

namespace HSEFinanceTracker.Base.Factories
{
    public sealed class FinanceFactory : IFinanceFactory
    {
        public BankAccount CreateBankAccount(string name, decimal openingBalance = 0m)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Empty name");
            }

            ArgumentOutOfRangeException.ThrowIfNegative(openingBalance);

            return new BankAccount(Guid.NewGuid(), name.Trim(), openingBalance);
        }

        public BankAccount CreateBankAccount(Guid id, string name, decimal openingBalance = 0m)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Empty name");
            }

            ArgumentOutOfRangeException.ThrowIfNegative(openingBalance);
            return new BankAccount(id, name, openingBalance);
        }


        public Category CreateCategory(CategoryType type, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Empty name");
            }

            return new Category(Guid.NewGuid(), type, name);
        }

        public Category CreateCategory(Guid id, CategoryType type, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Empty name");
            }

            return new Category(id, type, name);
        }


        public Operation CreateOperation(OperationType type, Guid accountId, Guid categoryId,
            decimal amount, DateTime date, string? description = null)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);
            return new Operation(Guid.NewGuid(), type, accountId, categoryId, amount, date, description);
        }

        public Operation CreateOperation(Guid operationId, OperationType type, Guid accountId, Guid categoryId,
            decimal amount, DateTime date, string? description = null)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);

            return new Operation(operationId, type, accountId, categoryId, amount, date, description);
        }
    }
}