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
                                // Счета
                                "Счета: создать",
                                "Счета: список",
                                "Счета: переименовать",
                                "Счета: удалить",
                                // Категории
                                "Категории: создать",
                                "Категории: список",
                                "Категории: переименовать",
                                "Категории: удалить",
                                // Операции
                                "Операции: добавить доход",
                                "Операции: добавить расход",
                                "Операции: список по счёту",
                                "Операции: удалить",
                                "Операции: изменить (delete+create)",
                                // Отчёты
                                "Отчёт: доходы/расходы за период",
                                // Служебное
                                "Выход"));

                    switch (option)
                    {
                        // СЧЕТА 
                        case "Счета: создать":
                            CreateAccountFlow();
                            break;
                        case "Счета: список":
                            ShowAccounts();
                            break;
                        case "Счета: переименовать":
                            RenameAccountFlow();
                            break;
                        case "Счета: удалить":
                            DeleteAccountFlow();
                            break;

                        // КАТЕГОРИИ 
                        case "Категории: создать":
                            CreateCategoryFlow();
                            break;
                        case "Категории: список":
                            ShowCategories();
                            break;
                        case "Категории: переименовать":
                            RenameCategoryFlow();
                            break;
                        case "Категории: удалить":
                            DeleteCategoryFlow();
                            break;

                        // ОПЕРАЦИИ 
                        case "Операции: добавить доход":
                            CreateOperationFlow(OperationType.Income);
                            break;
                        case "Операции: добавить расход":
                            CreateOperationFlow(OperationType.Expense);
                            break;
                        case "Операции: список по счёту":
                            ShowOperationsByAccount();
                            break;
                        case "Операции: удалить":
                            DeleteOperationFlow();
                            break;
                        case "Операции: изменить (delete+create)":
                            EditOperationFlow();
                            break;

                        // ОТЧЁТ 
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

        // СЧЕТА 

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

        private void RenameAccountFlow()
        {
            Console.Clear();
            var acc = PickAccount();
            if (acc is null)
            {
                ConsoleManager.WriteWarn("Счетов нет", false);
                return;
            }

            var newName = AskNonEmpty("Новое название счёта:");
            _accounts.Rename(acc.Id, newName);

            ConsoleManager.WriteMessage($"Счёт переименован: {newName} (#{acc.Id})");
        }

        private void DeleteAccountFlow()
        {
            Console.Clear();
            var acc = PickAccount();
            if (acc is null)
            {
                ConsoleManager.WriteWarn("Счетов нет", false);
                return;
            }

            var hasOps = _operations.All().Any(o => o.BankAccountId == acc.Id);
            if (hasOps)
            {
                ConsoleManager.WriteWarn("Нельзя удалить счёт: к нему привязаны операции", false);
                return;
            }

            if (!Confirm($"Удалить счёт '{acc.Name}'? Это действие необратимо."))
            {
                return;
            }

            _accounts.Delete(acc.Id);
            ConsoleManager.WriteMessage("Счёт удалён");
        }

        //  КАТЕГОРИИ 

        private void CreateCategoryFlow()
        {
            Console.Clear();
            var typeStr = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[green]Тип категории[/]")
                    .AddChoices("Доход", "Расход"));

            var type = typeStr == "Доход" ? CategoryType.Income : CategoryType.Expense;
            var name = AskNonEmpty("Название категории:");

            // Пред-проверка дубликатов (UI-уровень; основная защита должна быть в фасаде)
            var exists = _categories.All(type)
                .Any(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (exists)
            {
                ConsoleManager.WriteWarn("Такая категория уже существует", false);
                return;
            }

            var cat = _categories.Create(type, name);
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
                ConsoleManager.WriteWarn("Категорий нет", false);
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

        private void RenameCategoryFlow()
        {
            Console.Clear();
            var cat = PickCategoryForAnyType();
            if (cat is null)
            {
                ConsoleManager.WriteWarn("Категорий нет", false);
                return;
            }

            var newName = AskNonEmpty("Новое название категории:");

            // Пред-проверка дубликатов в рамках того же типа
            var exists = _categories.All(cat.Type)
                .Any(c => c.Id != cat.Id && c.Name.Equals(newName, StringComparison.OrdinalIgnoreCase));
            if (exists)
            {
                ConsoleManager.WriteWarn("Категория с таким названием уже существует", false);
                return;
            }

            _categories.Rename(cat.Id, newName);
            ConsoleManager.WriteMessage($"Категория переименована: {newName} (#{cat.Id})");
        }

        private void DeleteCategoryFlow()
        {
            Console.Clear();
            var cat = PickCategoryForAnyType();
            if (cat is null)
            {
                ConsoleManager.WriteWarn("Категорий нет", false);
                return;
            }

            var hasOps = _operations.All().Any(o => o.CategoryId == cat.Id);
            if (hasOps)
            {
                ConsoleManager.WriteWarn("Нельзя удалить категорию: к ней привязаны операции", false);
                return;
            }

            if (!Confirm($"Удалить категорию '{cat.Name}' ({cat.Type})?"))
            {
                return;
            }

            _categories.Delete(cat.Id);
            ConsoleManager.WriteMessage("Категория удалена");
        }

        //  ОПЕРАЦИИ 

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

        private void DeleteOperationFlow()
        {
            Console.Clear();
            var (acc, op) = PickOperationWithAccount();
            if (acc is null || op is null)
            {
                return;
            }

            if (!Confirm(
                    $"Удалить операцию на сумму {op.Amount:0.##} ({op.Type}) от {op.Date:yyyy-MM-dd}? Это откатит баланс счёта."))
            {
                return;
            }

            _operations.Delete(op.Id);
            ConsoleManager.WriteMessage("Операция удалена и баланс пересчитан");
            ConsoleManager.WriteColor($"Текущий баланс счёта {acc.Name}: {acc.Balance:0.##}", "green");
        }

        /// <summary>
        /// Изменение операции как delete+create (иммутабельная модель).
        /// Разрешаем менять сумму/дату/описание и категорию (тип операции не меняем).
        /// </summary>
        private void EditOperationFlow()
        {
            Console.Clear();
            var (acc, op) = PickOperationWithAccount();
            if (acc is null || op is null)
            {
                return;
            }

            var newAmount = AskDecimalPositive($"Новая сумма (>0), текущее {op.Amount:0.##}:");
            var newDate = AskDate($"Новая дата (yyyy-mm-dd), текущая {op.Date:yyyy-MM-dd}:");
            var newCat = PickCategory(op.Type) ?? throw new InvalidOperationException("Нет подходящих категорий");

            var newDesc = AskOptional($"Новое описание (пусто — оставить текущее: '{op.Description ?? ""}'):");
            if (string.IsNullOrWhiteSpace(newDesc))
            {
                newDesc = op.Description;
            }

            if (!Confirm("Сохранить изменения? Текущая операция будет удалена и создана новая."))
            {
                return;
            }

            // 1) удалить старую (баланс откатится)
            _operations.Delete(op.Id);
            // 2) создать новую с новыми данными (баланс применится)
            _operations.Create(op.Type, acc.Id, newCat.Id, newAmount, newDate, newDesc);

            ConsoleManager.WriteMessage("Операция изменена (delete+create) и баланс пересчитан");
            ConsoleManager.WriteColor($"Текущий баланс счёта {acc.Name}: {acc.Balance:0.##}", "green");
        }

        private void ShowOperationsByAccount()
        {
            Console.Clear();
            var acc = PickAccount();
            if (acc is null)
            {
                ConsoleManager.WriteWarn("Счетов нет", false);
                return;
            }

            var (from, to) = AskDateRange();
            var ops = _operations.ForAccount(acc.Id, from, to).OrderBy(o => o.Date).ToList();
            if (!ops.Any())
            {
                ConsoleManager.WriteWarn("Операций не найдено", false);
                return;
            }

            var t = new TTable().Border(TableBorder.Rounded);
            t.AddColumn("Id");
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
                t.AddRow(o.Id.ToString(),
                    o.Date.ToString("yyyy-MM-dd"),
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

            var (from, to) = AskDateRange();

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

        //  HELPERS 

        private static bool Confirm(string question)
        {
            var ans = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[yellow]{Markup.Escape(question)}[/]")
                    .AddChoices("Да", "Нет"));
            return ans == "Да";
        }

        private static (DateTime from, DateTime to) AskDateRange()
        {
            while (true)
            {
                var from = AskDate("Начало периода (yyyy-mm-dd):");
                var to = AskDate("Конец периода (yyyy-mm-dd):");
                if (from <= to)
                {
                    return (from, to);
                }

                ConsoleManager.WriteWarn("Начальная дата должна быть меньше или равна конечной", false);
            }
        }

        private static string AskNonEmpty(string prompt)
        {
            while (true)
            {
                var s = AnsiConsole.Ask<string>($"[yellow]{prompt}[/]");
                if (!string.IsNullOrWhiteSpace(s))
                {
                    return s.Trim();
                }

                ConsoleManager.WriteWarn("Значение не может быть пустым", false);
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

                ConsoleManager.WriteWarn("Введите число >= 0", false);
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

                ConsoleManager.WriteWarn("Введите число > 0", false);
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

                ConsoleManager.WriteWarn("Неверный формат даты", false);
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

        private Category? PickCategoryForAnyType()
        {
            var list = _categories.All().ToList();
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

        /// <summary>
        /// Выбор операции по счёту и периоду. Возвращает (счёт, операция).
        /// </summary>
        private (BankAccount? acc, Operation? op) PickOperationWithAccount()
        {
            var acc = PickAccount();
            if (acc is null)
            {
                ConsoleManager.WriteWarn("Счетов нет", false);
                return (null, null);
            }

            var (from, to) = AskDateRange();
            var ops = _operations.ForAccount(acc.Id, from, to)
                .OrderBy(o => o.Date)
                .ToList();

            if (!ops.Any())
            {
                ConsoleManager.WriteWarn("Операций не найдено", false);
                return (acc, null);
            }

            var catNames = _categories.All().ToDictionary(c => c.Id, c => c.Name);

            var choices = ops.Select(o =>
            {
                var type = o.Type == OperationType.Income ? "Доход" : "Расход";
                var name = catNames.GetValueOrDefault(o.CategoryId, "(?)");
                return $"{o.Id} | {o.Date:yyyy-MM-dd} | {type} | {o.Amount:0.##} | {name}";
            }).ToList();

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[green]Выберите операцию[/]")
                    .AddChoices(choices));

            var idStr = choice.Split('|').First().Trim();
            var id = Guid.Parse(idStr);
            var op = ops.First(x => x.Id == id);

            return (acc, op);
        }
    }
}