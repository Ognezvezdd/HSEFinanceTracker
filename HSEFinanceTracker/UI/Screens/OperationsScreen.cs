using HSEFinanceTracker.Application.Facades;
using HSEFinanceTracker.Base;
using HSEFinanceTracker.Base.Entities;
using HSEFinanceTracker.UI.Abstractions;
using HSEFinanceTracker.UI.Services;
using Spectre.Console;

namespace HSEFinanceTracker.UI.Screens
{
    public sealed class OperationsScreen : IMenuScreen
    {
        public string Title => "Операции";

        private readonly OperationFacade _ops;
        private readonly BankAccountFacade _accounts;
        private readonly CategoryFacade _categories;
        private readonly UiIo _io;

        public OperationsScreen(OperationFacade ops, BankAccountFacade accounts, CategoryFacade categories, UiIo io)
        {
            _ops = ops;
            _accounts = accounts;
            _categories = categories;
            _io = io;
        }

        public void Show()
        {
            while (true)
            {
                _io.Clear();
                var cmd = _io.Choose(Title,
                [
                    "Добавить доход", "Добавить расход", "Показать по счёту за период",
                    "Показать все по счёту (без дат)", "Удалить", "Изменить (delete+create)", "Назад"
                ]);
                if (cmd == "Назад")
                {
                    return;
                }

                switch (cmd)
                {
                    case "Добавить доход": Create(OperationType.Income); break;
                    case "Добавить расход": Create(OperationType.Expense); break;
                    case "Показать по счёту за период": ShowByAccount(true); break;
                    case "Показать все по счёту (без дат)": ShowByAccount(false); break;
                    case "Удалить": Delete(); break;
                    case "Изменить (delete+create)": Edit(); break;
                }
                _io.ReadKey();
            }
        }

        private void Create(OperationType type)
        {
            var acc = PickAccount();
            if (acc is null)
            {
                _io.Warn("Счетов нет");
                return;
            }

            var cat = PickCategory(type);
            if (cat is null)
            {
                _io.Warn("Нет подходящих категорий");
                return;
            }

            var amount = _io.AskDecimal("Сумма (> 0):", v => v > 0, "Введите число > 0");
            var date = _io.AskDate("Дата (yyyy-mm-dd):");
            var desc = _io.AskOptional("Описание (необязательно):"); // <= НЕОБЯЗАТЕЛЬНО

            _ops.Create(type, acc.Id, cat.Id, amount, date, desc);
            _io.Info("Операция добавлена");
        }

        private void ShowByAccount(bool withPeriod)
        {
            var acc = PickAccount();
            if (acc is null)
            {
                _io.Warn("Счетов нет");
                return;
            }

            var ops = withPeriod
                ? _ops.ForAccount(acc.Id, _io.AskDate("Начало:"), _io.AskDate("Конец:"))
                : _ops.ForAccount(acc.Id, DateTime.MinValue.Date, DateTime.MaxValue.Date);

            var list = ops.OrderBy(o => o.Date).ToList();
            if (!list.Any())
            {
                _io.Warn("Операций не найдено");
                return;
            }

            var cats = _categories.All().ToDictionary(c => c.Id, c => c.Name);
            var t = _io.TableRounded("Id", "Дата", "Тип", "Сумма", "Категория", "Описание");
            foreach (var o in list)
            {
                var type = o.Type == OperationType.Income ? "Доход" : "Расход";
                var name = cats.GetValueOrDefault(o.CategoryId, "(?)");
                t.AddRow(o.Id.ToString(), o.Date.ToString("yyyy-MM-dd"), type,
                    o.Amount.ToString("0.##"), name, o.Description ?? "");
            }

            _io.WriteTable(t);
        }

        private void Delete()
        {
            var (acc, op) = PickOperationWithAccount();
            if (acc is null || op is null)
            {
                return;
            }

            if (!_io.Confirm($"Удалить операцию {op.Type} {op.Amount:0.##} от {op.Date:yyyy-MM-dd}?"))
            {
                return;
            }

            _ops.Delete(op.Id);
            _io.Info("Операция удалена");
        }

        private void Edit()
        {
            var (acc, op) = PickOperationWithAccount();
            if (acc is null || op is null)
            {
                return;
            }

            var amount = _io.AskDecimal($"Новая сумма (>0), текущее {op.Amount:0.##}:", v => v > 0,
                "Введите число > 0");
            var date = _io.AskDate($"Новая дата (yyyy-mm-dd), текущее {op.Date:yyyy-MM-dd}:");
            var cat = PickCategory(op.Type);
            if (cat is null)
            {
                _io.Warn("Нет подходящих категорий");
                return;
            }

            var desc = _io.AskOptional($"Новое описание (пусто — оставить '{op.Description ?? ""}'):");
            if (string.IsNullOrWhiteSpace(desc))
            {
                desc = op.Description;
            }

            if (!_io.Confirm("Сохранить изменения? Будет выполнено delete+create."))
            {
                return;
            }

            _ops.Delete(op.Id);
            _ops.Create(op.Type, acc!.Id, cat.Id, amount, date, desc);
            _io.Info("Операция изменена (delete+create)");
        }

        // helpers
        private BankAccount? PickAccount()
        {
            var list = _accounts.All().ToList();
            if (!list.Any())
            {
                return null;
            }

            var choice = _io.Choose("Выберите счёт", list.Select(a => $"{a.Name} | {a.Balance:0.##} | {a.Id}"));
            var id = choice.Split('|').Last().Trim();
            return list.First(a => a.Id.ToString() == id);
        }

        private Category? PickCategory(OperationType type)
        {
            var needed = type == OperationType.Income ? CategoryType.Income : CategoryType.Expense;
            var list = _categories.All(needed).ToList();
            if (!list.Any())
            {
                return null;
            }

            var choice = _io.Choose("Категория", list.Select(c => $"{c.Name} | {c.Type} | {c.Id}"));
            var id = choice.Split('|').Last().Trim();
            return list.First(c => c.Id.ToString() == id);
        }

        private (BankAccount? acc, Operation? op) PickOperationWithAccount()
        {
            var acc = PickAccount();
            if (acc is null)
            {
                return (null, null);
            }

            var (from, to) = _io.AskDateRange();
            var ops = _ops.ForAccount(acc.Id, from, to).OrderBy(o => o.Date).ToList();
            if (!ops.Any())
            {
                _io.Warn("Операций не найдено");
                return (acc, null);
            }

            var cats = _categories.All().ToDictionary(c => c.Id, c => c.Name);
            var choice = _io.Choose("Выберите операцию", ops.Select(o =>
                $"{o.Id} | {o.Date:yyyy-MM-dd} | {(o.Type == OperationType.Income ? "Доход" : "Расход")} | {o.Amount:0.##} | {cats.GetValueOrDefault(o.CategoryId, "(?)")}"));
            var id = Guid.Parse(choice.Split('|')[0].Trim());
            return (acc, ops.First(x => x.Id == id));
        }
    }
}