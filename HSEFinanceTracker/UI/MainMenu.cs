using HSEFinanceTracker.UI.Abstractions;
using HSEFinanceTracker.UI.Services;
using HSEFinanceTracker.UI.Screens;

namespace HSEFinanceTracker.UI
{
    /// <summary>
    /// Корневое меню: только маршрутизация между экранами.
    /// </summary>
    public sealed class MainMenu
    {
        private readonly UiIo _io;
        private readonly AccountsScreen _accounts;
        private readonly CategoriesScreen _categories;
        private readonly OperationsScreen _operations;
        private readonly ReportsScreen _reports;
        private readonly ImportExportScreen _importExport;
        private readonly DataToolsScreen _dataTools;

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
            _accounts = accounts;
            _categories = categories;
            _operations = operations;
            _reports = reports;
            _importExport = importExport;
            _dataTools = dataTools;
        }

        public void Run()
        {
            while (true)
            {
                _io.Clear();
                var choice = _io.Choose("Главное меню",
                    new[]
                    {
                        _accounts.Title, _categories.Title, _operations.Title, _reports.Title, _importExport.Title,
                        _dataTools.Title, "Выход"
                    });

                if (choice == "Выход")
                {
                    ConsoleManager.WriteColor("Выход из приложения...", "yellow");
                    return;
                }

                if (choice == _accounts.Title)
                {
                    _accounts.Show();
                }
                else if (choice == _categories.Title)
                {
                    _categories.Show();
                }
                else if (choice == _operations.Title)
                {
                    _operations.Show();
                }
                else if (choice == _reports.Title)
                {
                    _reports.Show();
                }
                else if (choice == _importExport.Title)
                {
                    _importExport.Show();
                }
                else if (choice == _dataTools.Title)
                {
                    _dataTools.Show();
                }
            }
        }
    }
}