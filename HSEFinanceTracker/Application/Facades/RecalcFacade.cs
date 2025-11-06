using HSEFinanceTracker.Base;
using HSEFinanceTracker.Base.Repositories;

namespace HSEFinanceTracker.Application.Facades
{
    /// <summary>
    /// Пересчёт фактического баланса счетов по операциям.
    /// GRASP: вся логика перерасчёта сконцентрирована в одном месте (High Cohesion),
    /// внешние слои видят только фасад (Low Coupling).
    /// </summary>
    public sealed class RecalcFacade
    {
        private readonly IBankAccountRepo _accounts;
        private readonly IOperationRepo _operations;
        private readonly ICategoryRepo _categories; // Сейчас не используется

        public RecalcFacade(IBankAccountRepo accounts, IOperationRepo operations, ICategoryRepo categories)
        {
            _accounts = accounts;
            _operations = operations;
            _categories = categories;
        }

        /// <summary>
        /// Возвращает дельту между «фактическим» балансом счёта (по операциям)
        /// и текущим сохранённым в самом счёте. Положительное значение означает,
        /// что по операциям вышло БОЛЬШЕ, чем записано в счёте; отрицательное — меньше.
        /// </summary>
        public decimal VerifyAccount(Guid accountId)
        {
            var acc = _accounts.Get(accountId)
                      ?? throw new InvalidOperationException($"Счёт {accountId} не найден");

            var expected = ComputeExpectedBalance(accountId);
            return expected - acc.Balance;
        }

        /// <summary>
        /// Пересчитывает фактический баланс указанного счёта по всем его операциям
        /// и записывает результат в репозиторий.
        /// </summary>
        public void RecalculateAccount(Guid accountId)
        {
            var acc = _accounts.Get(accountId)
                      ?? throw new InvalidOperationException($"Счёт {accountId} не найден");

            var expected = ComputeExpectedBalance(accountId);

            // Обновляем модель и сохраняем
            acc.Apply(-acc.Balance);
            acc.Apply(expected);
            _accounts.Update(acc);
        }

        /// <summary>
        /// Пересчитывает и синхронизирует балансы по всем существующим счетам.
        /// </summary>
        public void RecalculateAll()
        {
            foreach (var acc in _accounts.All().ToList())
            {
                var expected = ComputeExpectedBalance(acc.Id);
                if (acc.Balance != expected)
                {
                    acc.Apply(-acc.Balance);
                    acc.Apply(expected);
                    _accounts.Update(acc);
                }
            }
        }

        /// <summary>
        /// Вычисляет «правильный» баланс счёта на основе всех его операций:
        /// сумма всех доходов минус сумма всех расходов.
        /// Предполагается, что Amount в Operation неотрицательный.
        /// </summary>
        private decimal ComputeExpectedBalance(Guid accountId)
        {
            var opsForAccount = _operations
                .All()
                .Where(o => o.BankAccountId == accountId);

            var forAccount = opsForAccount.ToList();
            var income = forAccount
                .Where(o => o.Type == OperationType.Income)
                .Sum(o => o.Amount);

            var expense = forAccount
                .Where(o => o.Type == OperationType.Expense)
                .Sum(o => o.Amount);

            return income - expense;
        }
    }
}