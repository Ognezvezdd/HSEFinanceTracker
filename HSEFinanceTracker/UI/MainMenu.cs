using HSEFinanceTracker.Application.Commands;
using HSEFinanceTracker.UI.Services;
using HSEFinanceTracker.UI.Screens;

namespace HSEFinanceTracker.UI
{
    /// <summary>
    /// Корневое меню. Реализовано через паттерн Команда + Декоратор (таймер).
    /// Каждый пункт верхнего меню — это команда, обёрнутая в TimedMenuCommand.
    /// </summary>
    public sealed class MainMenu
    {
        private readonly UiIo _io;

        private string _pendingInfo = "";

        // Готовые команды верхнего уровня
        private readonly List<ICommand> _commands = [];

        public MainMenu(
            UiIo io,
            AccountsScreen accounts,
            CategoriesScreen categories,
            OperationsScreen operations,
            ReportsScreen reports,
            ImportExportScreen importExport,
            DataToolsScreen dataTools)
        {
            _io = io;
            _commands.Add(Timed(new ScreenCommand(accounts.Title, accounts.Show, _io)));
            _commands.Add(Timed(new ScreenCommand(categories.Title, categories.Show, _io)));
            _commands.Add(Timed(new ScreenCommand(operations.Title, operations.Show, _io)));
            _commands.Add(Timed(new ScreenCommand(reports.Title, reports.Show, _io)));
            _commands.Add(Timed(new ScreenCommand(importExport.Title, importExport.Show, _io)));
            _commands.Add(Timed(new ScreenCommand(dataTools.Title, dataTools.Show, _io)));
        }

        public void Run()
        {
            while (true)
            {
                try
                {
                    _io.Clear();
                    var choices = _commands.Select(c => c.Name).Concat(["Выход"]).ToArray();

                    // Сценарий Timer (Декоратор + Команда) 
                    if (!string.IsNullOrEmpty(_pendingInfo))
                    {
                        ConsoleManager.WriteColor(_pendingInfo, "gray");
                    }

                    var choice = _io.Choose("HSE Банк", choices);
                    if (choice == "Выход")
                    {
                        ConsoleManager.WriteColor("Выход из приложения...", "yellow");
                        return;
                    }

                    var cmd = _commands.FirstOrDefault(c => c.Name == choice);
                    if (cmd is null)
                    {
                        _io.Warn("Неизвестная команда");
                        continue;
                    }


                    cmd.Execute(); // здесь сработает декоратор-таймер
                }
                catch (Exception ex)
                {
                    ConsoleManager.WriteWarn($"Произошла ошибка: {ex.Message}");
                }
            }
        }

        private ICommand Timed(ICommand inner)
        {
            return new TimedMenuCommand(inner, msg => _pendingInfo = msg);
        }
    }
}