using HSEFinanceTracker.Base;
using HSEFinanceTracker.Base.Entities;
using HSEFinanceTracker.Base.Repositories;

namespace HSEFinanceTracker.Application.Facades
{
    /// <summary>
    /// Фасад аналитики: разница доходов/расходов и группировка по категориям.
    /// Держим расчёты вне UI (GRASP: High Cohesion, Low Coupling).
    /// </summary>
    public sealed class AnalyticsFacade
    {
        private readonly IOperationRepo _operations;
        private readonly ICategoryRepo _categories;

        public AnalyticsFacade(IOperationRepo operations, ICategoryRepo categories)
        {
            _operations = operations;
            _categories = categories;
        }

        public (decimal income, decimal expense, decimal diff) GetDiff(DateTime from, DateTime to)
        {
            var all = _operations.All().Where(o => o.Date >= from && o.Date <= to);
            var operations = all as Operation[] ?? all.ToArray();
            var income = operations.Where(o => o.Type == OperationType.Income).Sum(o => o.Amount);
            var expense = operations.Where(o => o.Type == OperationType.Expense).Sum(o => o.Amount);
            return (income, expense, income - expense);
        }

        public IReadOnlyList<(Guid categoryId, string categoryName, decimal sum)>
            GroupByCategory(DateTime from, DateTime to, CategoryType? type = null)
        {
            var cats = _categories.All().ToDictionary(c => c.Id, c => c);
            var ops = _operations.All().Where(o => o.Date >= from && o.Date <= to);

            if (type.HasValue)
            {
                ops = ops.Where(o =>
                    o.Type == (type.Value == CategoryType.Income ? OperationType.Income : OperationType.Expense));
            }

            var grouped = ops
                .GroupBy(o => o.CategoryId)
                .Select(g =>
                {
                    cats.TryGetValue(g.Key, out var c);
                    var name = c?.Name ?? "(unknown)";
                    return (g.Key, name, g.Sum(x => x.Amount));
                })
                .OrderByDescending(x => x.Item3)
                .ToList();

            return grouped;
        }
    }
}