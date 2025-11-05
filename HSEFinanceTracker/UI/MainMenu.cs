using System.Globalization;
using HSEFinanceTracker.Application.Export;
using HSEFinanceTracker.Application.Facades;
using HSEFinanceTracker.Application.Import;
using HSEFinanceTracker.Base;
using HSEFinanceTracker.Base.Entities;
using Spectre.Console;
using TTable = Spectre.Console.Table;

namespace HSEFinanceTracker.UI
{
    /// <summary>
    /// Консольный UI. Иерархическое меню.
    /// </summary>
    public sealed class MainMenu
    {
        // Фасады домена
        private readonly BankAccountFacade _accounts;
        private readonly CategoryFacade _categories;
        private readonly OperationFacade _operations;
        private readonly AnalyticsFacade _analytics;
        private readonly ImportExportFacade _io;

        // Импорт/экспорт 
        private readonly IEnumerable<IDataExporter> _exporters;
        private readonly IEnumerable<IDataImporter> _importers;

        private readonly object? _recalcFacade;

        public MainMenu(
            BankAccountFacade accounts,
            CategoryFacade categories,
            OperationFacade operations,
            AnalyticsFacade analytics,
            ImportExportFacade io,
            IEnumerable<IDataExporter> exporters,
            IEnumerable<IDataImporter> importers,
            object? recalcFacade = null
        )
        {
            _accounts = accounts;
            _categories = categories;
            _operations = operations;
            _analytics = analytics;
            _io = io;
            _exporters = exporters;
            _importers = importers;
            _recalcFacade = recalcFacade;
        }

        // ENTRY 

        public void Run()
        {
            while (true)
            {
                try
                {
                    Console.Clear();
                    var option = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("[green]Главное меню[/]")
                            .AddChoices(
                                "Счета",
                                "Категории",
                                "Операции",
                                "Отчёты",
                                "Импорт/Экспорт",
                                "Инструменты данных",
                                "Выход"));

                    switch (option)
                    {
                        case "Счета":
                            AccountsMenu();
                            break;
                        case "Категории":
                            CategoriesMenu();
                            break;
                        case "Операции":
                            OperationsMenu();
                            break;
                        case "Отчёты":
                            ReportsMenu();
                            break;
                        case "Импорт/Экспорт":
                            ImportExportMenu();
                            break;
                        case "Инструменты данных":
                            DataToolsMenu();
                            break;
                        case "Выход":
                            ConsoleManager.WriteColor("Выход из приложения...", "yellow");
                            return;
                    }
                }
                catch (Exception ex)
                {
                    ConsoleManager.WriteWarn(ex.Message);
                }
            }
        }

        // SUB-MENUS 

        private void AccountsMenu()
        {
            while (true)
            {
                Console.Clear();
                var option = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[green]Счета[/]")
                        .AddChoices(
                            "Создать",
                            "Список",
                            "Переименовать",
                            "Удалить",
                            "Назад"));

                if (option == "Назад")
                {
                    return;
                }

                Measure(option, () =>
                {
                    switch (option)
                    {
                        case "Создать": CreateAccountFlow(); break;
                        case "Список": ShowAccounts(); break;
                        case "Переименовать": RenameAccountFlow(); break;
                        case "Удалить": DeleteAccountFlow(); break;
                    }
                });
            }
        }

        private void CategoriesMenu()
        {
            while (true)
            {
                Console.Clear();
                var option = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[green]Категории[/]")
                        .AddChoices(
                            "Создать",
                            "Список",
                            "Переименовать",
                            "Удалить",
                            "Назад"));

                if (option == "Назад")
                {
                    return;
                }

                Measure(option, () =>
                {
                    switch (option)
                    {
                        case "Создать": CreateCategoryFlow(); break;
                        case "Список": ShowCategories(); break;
                        case "Переименовать": RenameCategoryFlow(); break;
                        case "Удалить": DeleteCategoryFlow(); break;
                    }
                });
            }
        }

        private void OperationsMenu()
        {
            while (true)
            {
                Console.Clear();
                var option = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[green]Операции[/]")
                        .AddChoices(
                            "Добавить доход",
                            "Добавить расход",
                            "Показать по счёту за период",
                            "Показать все по счёту (без дат)",
                            "Удалить",
                            "Изменить (delete+create)",
                            "Назад"));

                if (option == "Назад")
                {
                    return;
                }

                Measure(option, () =>
                {
                    switch (option)
                    {
                        case "Добавить доход": CreateOperationFlow(OperationType.Income); break;
                        case "Добавить расход": CreateOperationFlow(OperationType.Expense); break;
                        case "Показать по счёту за период": ShowOperationsByAccount(true); break;
                        case "Показать все по счёту (без дат)": ShowOperationsByAccount(false); break;
                        case "Удалить": DeleteOperationFlow(); break;
                        case "Изменить (delete+create)": EditOperationFlow(); break;
                    }
                });
            }
        }

        private void ReportsMenu()
        {
            while (true)
            {
                Console.Clear();
                var option = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[green]Отчёты[/]")
                        .AddChoices(
                            "Разница (доходы - расходы) за период",
                            "Группировка по категориям",
                            "Назад"));

                if (option == "Назад")
                {
                    return;
                }

                Measure(option, () =>
                {
                    switch (option)
                    {
                        case "Разница (доходы - расходы) за период": ShowDiffReport(); break;
                        case "Группировка по категориям": ShowGroupedByCategory(); break;
                    }
                });
            }
        }

        private void ImportExportMenu()
        {
            while (true)
            {
                Console.Clear();
                var option = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[green]Импорт/Экспорт[/]")
                        .AddChoices(
                            "Экспорт: JSON",
                            "Экспорт: CSV",
                            "Импорт: JSON",
                            "Импорт: CSV",
                            "Назад"));

                if (option == "Назад")
                {
                    return;
                }

                Measure(option, () =>
                {
                    switch (option)
                    {
                        case "Экспорт: JSON": ExportFlow("json"); break;
                        case "Импорт: JSON": ImportFlow("json"); break;
                        case "Экспорт: CSV": NotAvailable("CSV экспорт"); break;
                        case "Импорт: CSV": NotAvailable("CSV импорт"); break;
                    }
                });
            }
        }

        private void DataToolsMenu()
        {
            while (true)
            {
                Console.Clear();
                var option = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[green]Инструменты данных[/]")
                        .AddChoices(
                            "Проверить баланс счёта",
                            "Пересчитать баланс счёта",
                            "Пересчитать все счета",
                            "Назад"));

                if (option == "Назад")
                {
                    return;
                }

                Measure(option, () =>
                {
                    if (_recalcFacade == null)
                    {
                        ConsoleManager.WriteWarn("Модуль пересчёта балансов не подключён (RecalcFacade отсутствует).",
                            false);
                        return;
                    }

                    dynamic recalc = _recalcFacade;

                    switch (option)
                    {
                        case "Проверить баланс счёта":
                            {
                                var acc = PickAccount();
                                if (acc is null)
                                {
                                    ConsoleManager.WriteWarn("Счетов нет", false);
                                    return;
                                }

                                var diff = (decimal)recalc.VerifyAccount(acc.Id);
                                ConsoleManager.WriteColor($"Расхождение баланса счёта '{acc.Name}': {diff:0.##}",
                                    diff == 0 ? "green" : "yellow");
                                break;
                            }
                        case "Пересчитать баланс счёта":
                            {
                                var acc = PickAccount();
                                if (acc is null)
                                {
                                    ConsoleManager.WriteWarn("Счетов нет", false);
                                    return;
                                }

                                recalc.RecalculateAccount(acc.Id);
                                ConsoleManager.WriteMessage($"Баланс счёта '{acc.Name}' пересчитан.");
                                break;
                            }
                        case "Пересчитать все счета":
                            recalc.RecalculateAll();
                            ConsoleManager.WriteMessage("Баланс всех счетов пересчитан.");
                            break;
                    }
                });
            }
        }

        // СЧЕТА 

        private void CreateAccountFlow()
        {
            var name = AskNonEmpty("Название счёта:");
            var opening = AskDecimalNonNegative("Начальный баланс (>= 0):");
            var acc = _accounts.Create(name, opening);
            ConsoleManager.WriteMessage($"Счёт создан: {acc.Name} (#{acc.Id}), баланс: {acc.Balance:0.##}");
        }

        private void ShowAccounts()
        {
            var list = _accounts.All().ToList();
            if (!list.Any())
            {
                ConsoleManager.WriteWarn("Счетов нет", false);
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

            if (!Confirm($"Удалить счёт '{acc.Name}'?"))
            {
                return;
            }

            _accounts.Delete(acc.Id);
            ConsoleManager.WriteMessage("Счёт удалён");
        }

        // КАТЕГОРИИ 

        private void CreateCategoryFlow()
        {
            var typeStr = AnsiConsole.Prompt(
                new SelectionPrompt<string>().Title("[green]Тип категории[/]").AddChoices("Доход", "Расход"));
            var type = typeStr == "Доход" ? CategoryType.Income : CategoryType.Expense;

            var name = AskNonEmpty("Название категории:");

            var dup = _categories.All(type).Any(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (dup)
            {
                ConsoleManager.WriteWarn("Категория уже существует", false);
                return;
            }

            var cat = _categories.Create(type, name);
            ConsoleManager.WriteMessage($"Категория создана: {cat.Name} ({cat.Type})");
        }

        private void ShowCategories()
        {
            var filter = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[green]Фильтр[/]").AddChoices("Все", "Только доходы", "Только расходы"));

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
            var cat = PickCategoryForAnyType();
            if (cat is null)
            {
                ConsoleManager.WriteWarn("Категорий нет", false);
                return;
            }

            var newName = AskNonEmpty("Новое название категории:");
            var dup = _categories.All(cat.Type)
                .Any(c => c.Id != cat.Id && c.Name.Equals(newName, StringComparison.OrdinalIgnoreCase));
            if (dup)
            {
                ConsoleManager.WriteWarn("Категория с таким названием уже есть", false);
                return;
            }

            _categories.Rename(cat.Id, newName);
            ConsoleManager.WriteMessage($"Категория переименована: {newName} (#{cat.Id})");
        }

        private void DeleteCategoryFlow()
        {
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

        // ОПЕРАЦИИ 

        private void CreateOperationFlow(OperationType opType)
        {
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
            var (acc, op) = PickOperationWithAccount();
            if (acc is null || op is null)
            {
                return;
            }

            if (!Confirm($"Удалить операцию {op.Type} на {op.Amount:0.##} от {op.Date:yyyy-MM-dd}?"))
            {
                return;
            }

            _operations.Delete(op.Id);
            ConsoleManager.WriteMessage("Операция удалена, баланс пересчитан.");
            ConsoleManager.WriteColor($"Баланс счёта {acc.Name}: {acc.Balance:0.##}", "green");
        }

        private void EditOperationFlow()
        {
            var (acc, op) = PickOperationWithAccount();
            if (acc is null || op is null)
            {
                return;
            }

            var newAmount = AskDecimalPositive($"Новая сумма (>0), текущее {op.Amount:0.##}:");
            var newDate = AskDate($"Новая дата (yyyy-mm-dd), текущая {op.Date:yyyy-MM-dd}:");
            var newCat = PickCategory(op.Type) ?? throw new InvalidOperationException("Нет подходящих категорий");
            var newDesc = AskOptional($"Новое описание (пусто — оставить текущее '{op.Description ?? ""}'):");
            if (string.IsNullOrWhiteSpace(newDesc))
            {
                newDesc = op.Description;
            }

            if (!Confirm("Сохранить изменения? Будет выполнено delete+create."))
            {
                return;
            }

            _operations.Delete(op.Id);
            _operations.Create(op.Type, acc.Id, newCat.Id, newAmount, newDate, newDesc);

            ConsoleManager.WriteMessage("Операция изменена (delete+create).");
            ConsoleManager.WriteColor($"Баланс счёта {acc.Name}: {acc.Balance:0.##}", "green");
        }

        private void ShowOperationsByAccount(bool periodRequired)
        {
            var acc = PickAccount();
            if (acc is null)
            {
                ConsoleManager.WriteWarn("Счетов нет", false);
                return;
            }

            IEnumerable<Operation> ops;
            if (periodRequired)
            {
                var (from, to) = AskDateRange();
                ops = _operations.ForAccount(acc.Id, from, to);
            }
            else
            {
                ops = _operations.ForAccount(acc.Id, DateTime.MinValue.Date, DateTime.MaxValue.Date);
            }

            var list = ops.OrderBy(o => o.Date).ToList();
            if (!list.Any())
            {
                ConsoleManager.WriteWarn("Операций не найдено", false);
                return;
            }

            var catNames = _categories.All().ToDictionary(c => c.Id, c => c.Name);
            var t = new TTable().Border(TableBorder.Rounded);
            t.AddColumn("Id");
            t.AddColumn("Дата");
            t.AddColumn("Тип");
            t.AddColumn("Сумма");
            t.AddColumn("Категория");
            t.AddColumn("Описание");

            foreach (var o in list)
            {
                var type = o.Type == OperationType.Income ? "Доход" : "Расход";
                var name = catNames.GetValueOrDefault(o.CategoryId, "(?)");
                t.AddRow(o.Id.ToString(), o.Date.ToString("yyyy-MM-dd"), type, o.Amount.ToString("0.##"), name,
                    o.Description ?? "");
            }

            AnsiConsole.Write(t);
            ConsoleManager.WriteColor($"\nТекущий баланс счёта {acc.Name}: {acc.Balance:0.##}", "green");
        }

        // ОТЧЁТЫ 

        private void ShowDiffReport()
        {
            var (from, to) = AskDateRange();
            var (income, expense, diff) = _analytics.GetDiff(from, to);

            var t = new TTable().Border(TableBorder.Rounded);
            t.AddColumn("Показатель");
            t.AddColumn("Сумма");
            t.AddRow("Доходы", income.ToString("0.##"));
            t.AddRow("Расходы", expense.ToString("0.##"));
            t.AddRow("Разница", diff.ToString("0.##"));
            AnsiConsole.Write(t);
        }

        private void ShowGroupedByCategory()
        {
            var (from, to) = AskDateRange();
            var typeStr = AnsiConsole.Prompt(new SelectionPrompt<string>()
                .Title("[green]Тип[/]").AddChoices("Все", "Доходы", "Расходы"));

            CategoryType? type = typeStr switch
            {
                "Доходы" => CategoryType.Income,
                "Расходы" => CategoryType.Expense,
                _ => null
            };

            var data = _analytics.GroupByCategory(from, to, type);
            if (!data.Any())
            {
                ConsoleManager.WriteWarn("Нет данных в выбранном периоде", false);
                return;
            }

            var t = new TTable().Border(TableBorder.Rounded);
            t.AddColumn("Категория");
            t.AddColumn("Сумма");
            foreach (var (_, name, sum) in data)
            {
                t.AddRow(name, sum.ToString("0.##"));
            }

            AnsiConsole.Write(t);
        }

        // ИМПОРТ/ЭКСПОРТ 

        private void ExportFlow(string format)
        {
            var path = AskNonEmpty($"Путь к файлу для экспорта ({format}):");
            var exporter = ResolveExporter(format);
            if (exporter is null)
            {
                NotAvailable($"{format.ToUpperInvariant()} экспорт");
                return;
            }

            _io.ExportAll(exporter, path);
            ConsoleManager.WriteMessage($"Экспорт выполнен: {path}");
        }

        private void ImportFlow(string format)
        {
            var path = AskNonEmpty($"Путь к файлу для импорта ({format}):");
            if (!File.Exists(path))
            {
                ConsoleManager.WriteWarn("Файл не найден");
                return;
            }

            var importer = ResolveImporter(format);
            if (importer is null)
            {
                NotAvailable($"{format.ToUpperInvariant()} импорт");
                return;
            }

            _io.ImportFrom(importer, path);
            ConsoleManager.WriteMessage("Импорт завершён");
        }

        private IDataExporter? ResolveExporter(string format)
        {
            return _exporters.FirstOrDefault(e =>
                e.GetType().Name.StartsWith(format, true, CultureInfo.InvariantCulture));
        }

        private IDataImporter? ResolveImporter(string format)
        {
            return _importers.FirstOrDefault(i =>
                i.GetType().Name.StartsWith(format, true, CultureInfo.InvariantCulture));
        }

        private static void NotAvailable(string feature)
        {
            ConsoleManager.WriteWarn($"{feature} пока не подключён. Добавьте реализацию и зарегистрируйте в DI.",
                false);
        }

        // HELPERS 

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

                ConsoleManager.WriteWarn("Начальная дата должна быть <= конечной", false);
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

        /// <summary>Простейшая метрика: измеряет время сценария и печатает.</summary>
        private static void Measure(string scenarioName, Action action)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            try { action(); }
            finally
            {
                sw.Stop();
                ConsoleManager.WriteColor($"'{scenarioName}' заняло {sw.ElapsedMilliseconds} мс", "grey");
                ConsoleManager.WriteColor("Нажмите любую клавишу...", "grey");
                Console.ReadKey(true);
            }
        }
    }
}