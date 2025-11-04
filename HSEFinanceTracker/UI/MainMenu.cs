using HSEFinanceTracker.Application.Facades;
using HSEFinanceTracker.Base;
using HSEFinanceTracker.Base.Entities;
using Spectre.Console;
using TTable = Spectre.Console.Table;

namespace HSEFinanceTracker.UI
{
    /// <summary>
    /// Главный цикл меню и сценарии взаимодействия с пользователем.
    /// Работает поверх фасадов и использует ConsoleManager для вывода.
    /// </summary>
    public sealed class MainMenu
    {
        private readonly BankAccountFacade _accounts;
        private readonly CategoryFacade _categories;
        private readonly OperationFacade _operations;

        public MainMenu(BankAccountFacade accounts, CategoryFacade categories, OperationFacade operations)
        {
            _accounts = accounts;
            _categories = categories;
            _operations = operations;
        }

        public void Run()
        {
            while (true)
            {
                try
                {
                    Console.Clear();

                    var option = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("[green]Выберите действие[/]")
                            .AddChoices(
                                "Счета: создать",
                                "Счета: список",
                                "Категории: создать",
                                "Категории: список",
                                "Операции: добавить доход",
                                "Операции: добавить расход",
                                "Операции: список по счёту",
                                "Отчёт: доходы/расходы за период",
                                "Выход"));

                    switch (option)
                    {
                        case "Счета: создать":
                            CreateAccountFlow();
                            break;
                        case "Счета: список":
                            ShowAccounts();
                            break;

                        case "Категории: создать":
                            CreateCategoryFlow();
                            break;
                        case "Категории: список":
                            ShowCategories();
                            break;

                        case "Операции: добавить доход":
                            CreateOperationFlow(OperationType.Income);
                            break;
                        case "Операции: добавить расход":
                            CreateOperationFlow(OperationType.Expense);
                            break;
                        case "Операции: список по счёту":
                            ShowOperationsByAccount();
                            break;

                        case "Отчёт: доходы/расходы за период":
                            ShowPeriodReport();
                            break;

                        case "Выход":
                            ConsoleManager.WriteColor("Выход из приложения...", "yellow");
                            return;
                    }

                    ConsoleManager.WriteColor("Нажмите любую клавишу...", "grey");
                    Console.ReadKey(true);
                }
                catch (Exception ex)
                {
                    ConsoleManager.WriteWarn(ex.Message);
                }
            }
        }

        // Сценарии

        private void CreateAccountFlow()
        {
            Console.Clear();
            var name = AskNonEmpty("Название счёта:");
            var opening = AskDecimalNonNegative("Начальный баланс (>= 0):");
            var acc = _accounts.Create(name, opening);

            ConsoleManager.WriteMessage($"Счёт создан: {acc.Name} (#{acc.Id}), баланс: {acc.Balance:0.##}");
        }

        private void ShowAccounts()
        {
            Console.Clear();
            var list = _accounts.All().ToList();
            if (!list.Any())
            {
                ConsoleManager.WriteWarn("Счетов нет");
                return;
            }

            var t = new TTable().Border(TableBorder.Rounded);
            t.AddColumn("Id");
            t.AddColumn("Название");
            t.AddColumn("Баланс");

            foreach (var a in list)
            {
                t.AddRow(a.Id.ToString(), a.Name, a.Balance.ToString("0.##"));
            }

            AnsiConsole.Write(t);
        }

        private void CreateCategoryFlow()
        {
            Console.Clear();
            var type = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[green]Тип категории[/]")
                    .AddChoices("Доход", "Расход"));

            var name = AskNonEmpty("Название категории:");

            var cat = _categories.Create(
                type == "Доход" ? CategoryType.Income : CategoryType.Expense,
                name);

            ConsoleManager.WriteMessage($"Категория создана: {cat.Name} ({cat.Type})");
        }

        private void ShowCategories()
        {
            Console.Clear();
            var filter = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[green]Фильтр[/]")
                    .AddChoices("Все", "Только доходы", "Только расходы"));

            CategoryType? type = filter switch
            {
                "Только доходы" => CategoryType.Income,
                "Только расходы" => CategoryType.Expense,
                _ => null
            };

            var list = _categories.All(type).ToList();
            if (!list.Any())
            {
                ConsoleManager.WriteWarn("Категорий нет");
                return;
            }

            var t = new TTable().Border(TableBorder.Rounded);
            t.AddColumn("Id");
            t.AddColumn("Название");
            t.AddColumn("Тип");

            foreach (var c in list)
            {
                t.AddRow(c.Id.ToString(), c.Name, c.Type.ToString());
            }

            AnsiConsole.Write(t);
        }

        private void CreateOperationFlow(OperationType opType)
        {
            Console.Clear();
            var acc = PickAccount() ?? throw new InvalidOperationException("Нет счетов");
            var cat = PickCategory(opType) ?? throw new InvalidOperationException("Нет подходящих категорий");

            var amount = AskDecimalPositive("Сумма (> 0):");
            var date = AskDate("Дата операции (yyyy-mm-dd):");
            var desc = AskOptional("Описание (необязательно):");

            _operations.Create(opType, acc.Id, cat.Id, amount, date, desc);
            var sign = opType == OperationType.Income ? "+" : "-";

            ConsoleManager.WriteMessage($"Операция добавлена: {sign}{amount:0.##} / {cat.Name} / {date:yyyy-MM-dd}");
            ConsoleManager.WriteColor($"Баланс счёта {acc.Name}: {acc.Balance:0.##}", "green");
        }

        private void ShowOperationsByAccount()
        {
            Console.Clear();
            var acc = PickAccount();
            if (acc is null)
            {
                ConsoleManager.WriteWarn("Счетов нет");
                return;
            }

            var from = AskDate("Начало периода (yyyy-mm-dd):");
            var to = AskDate("Конец периода (yyyy-mm-dd):");

            var ops = _operations.ForAccount(acc.Id, from, to).OrderBy(o => o.Date).ToList();
            if (!ops.Any())
            {
                ConsoleManager.WriteWarn("Операций не найдено");
                return;
            }

            var t = new TTable().Border(TableBorder.Rounded);
            t.AddColumn("Дата");
            t.AddColumn("Тип");
            t.AddColumn("Сумма");
            t.AddColumn("Категория");
            t.AddColumn("Описание");

            var catNames = _categories.All().ToDictionary(c => c.Id, c => c.Name);

            foreach (var o in ops)
            {
                var type = o.Type == OperationType.Income ? "Доход" : "Расход";
                var name = catNames.GetValueOrDefault(o.CategoryId, "(?)");
                t.AddRow(o.Date.ToString("yyyy-MM-dd"),
                    type,
                    o.Amount.ToString("0.##"),
                    name,
                    o.Description ?? "");
            }

            AnsiConsole.Write(t);
            ConsoleManager.WriteColor($"\nТекущий баланс счёта {acc.Name}: {acc.Balance:0.##}", "green");
        }

        private void ShowPeriodReport()
        {
            Console.Clear();

            var from = AskDate("Начало периода (yyyy-mm-dd):");
            var to = AskDate("Конец периода (yyyy-mm-dd):");

            var allOps = _operations.All()
                .Where(o => o.Date >= from && o.Date <= to)
                .ToList();

            var income = allOps.Where(o => o.Type == OperationType.Income).Sum(o => o.Amount);
            var expense = allOps.Where(o => o.Type == OperationType.Expense).Sum(o => o.Amount);
            var diff = income - expense;

            var t = new TTable().Border(TableBorder.Rounded);
            t.AddColumn("Показатель");
            t.AddColumn("Сумма");

            t.AddRow("Доходы", income.ToString("0.##"));
            t.AddRow("Расходы", expense.ToString("0.##"));
            t.AddRow("Разница (Доходы - Расходы)", diff.ToString("0.##"));

            AnsiConsole.Write(t);
        }

        // Вспомогательные функции


        private static string AskNonEmpty(string prompt)
        {
            while (true)
            {
                var s = AnsiConsole.Ask<string>($"[yellow]{prompt}[/]");
                if (!string.IsNullOrWhiteSpace(s))
                {
                    return s.Trim();
                }

                ConsoleManager.WriteWarn("Значение не может быть пустым");
            }
        }

        private static string? AskOptional(string prompt)
        {
            var s = AnsiConsole.Ask<string>($"[yellow]{prompt}[/]");
            return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
        }

        private static decimal AskDecimalNonNegative(string prompt)
        {
            while (true)
            {
                var s = AnsiConsole.Ask<string>($"[yellow]{prompt}[/]");
                if (decimal.TryParse(s, out var v) && v >= 0)
                {
                    return v;
                }

                ConsoleManager.WriteWarn("Введите число >= 0");
            }
        }

        private static decimal AskDecimalPositive(string prompt)
        {
            while (true)
            {
                var s = AnsiConsole.Ask<string>($"[yellow]{prompt}[/]");
                if (decimal.TryParse(s, out var v) && v > 0)
                {
                    return v;
                }

                ConsoleManager.WriteWarn("Введите число > 0");
            }
        }

        private static DateTime AskDate(string prompt)
        {
            while (true)
            {
                var s = AnsiConsole.Ask<string>($"[yellow]{prompt}[/]");
                if (DateTime.TryParse(s, out var d))
                {
                    return d.Date;
                }

                ConsoleManager.WriteWarn("Неверный формат даты");
            }
        }

        private BankAccount? PickAccount()
        {
            var list = _accounts.All().ToList();
            if (!list.Any())
            {
                return null;
            }

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[green]Выберите счёт[/]")
                    .AddChoices(list.Select(a => $"{a.Name} | {a.Balance:0.##} | {a.Id}")));

            var idStr = choice.Split('|').Last().Trim();
            return list.First(a => a.Id.ToString() == idStr);
        }

        private Category? PickCategory(OperationType opType)
        {
            var needed = opType == OperationType.Income ? CategoryType.Income : CategoryType.Expense;
            var list = _categories.All(needed).ToList();
            if (!list.Any())
            {
                return null;
            }

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[green]Выберите категорию[/]")
                    .AddChoices(list.Select(c => $"{c.Name} | {c.Type} | {c.Id}")));

            var idStr = choice.Split('|').Last().Trim();
            return list.First(c => c.Id.ToString() == idStr);
        }
    }
}