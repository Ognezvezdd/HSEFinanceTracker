using HSEFinanceTracker.Application.Facades;
using HSEFinanceTracker.Base;
using HSEFinanceTracker.UI.Abstractions;
using HSEFinanceTracker.UI.Services;
using Spectre.Console;

namespace HSEFinanceTracker.UI.Screens
{
    public sealed class ReportsScreen : IMenuScreen
    {
        public string Title => "Отчёты";

        private readonly AnalyticsFacade _analytics;
        private readonly UiIo _io;

        public ReportsScreen(AnalyticsFacade analytics, UiIo io)
        {
            _analytics = analytics;
            _io = io;
        }

        public void Show()
        {
            while (true)
            {
                _io.Clear();
                var cmd = _io.Choose(Title,
                    new[] { "Разница (доходы - расходы) за период", "Группировка по категориям", "Назад" });
                if (cmd == "Назад")
                {
                    return;
                }

                switch (cmd)
                {
                    case "Разница (доходы - расходы) за период": Diff(); break;
                    case "Группировка по категориям": Group(); break;
                }

                _io.ReadKey();
            }
        }

        private void Diff()
        {
            var (from, to) = _io.AskDateRange();
            var (income, expense, diff) = _analytics.GetDiff(from, to);
            var t = _io.TableRounded("Показатель", "Сумма");
            t.AddRow("Доходы", income.ToString("0.##"));
            t.AddRow("Расходы", expense.ToString("0.##"));
            t.AddRow("Разница", diff.ToString("0.##"));
            _io.WriteTable(t);
        }

        private void Group()
        {
            var (from, to) = _io.AskDateRange();
            var typeStr = _io.Choose("Тип", new[] { "Все", "Доходы", "Расходы" });
            CategoryType? type = typeStr switch
            {
                "Доходы" => CategoryType.Income,
                "Расходы" => CategoryType.Expense,
                _ => null
            };
            var data = _analytics.GroupByCategory(from, to, type);
            if (!data.Any())
            {
                _io.Warn("Нет данных");
                return;
            }

            var t = _io.TableRounded("Категория", "Сумма");
            foreach (var (_, name, sum) in data)
            {
                t.AddRow(name, sum.ToString("0.##"));
            }

            _io.WriteTable(t);
        }
    }
}