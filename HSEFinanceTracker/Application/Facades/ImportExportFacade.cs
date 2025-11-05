using HSEFinanceTracker.Application.Export;
using HSEFinanceTracker.Application.Import;
using HSEFinanceTracker.Base.Repositories;

namespace HSEFinanceTracker.Application.Facades
{
    /// <summary>
    /// Единая точка входа для импорта/экспорта.
    /// Обходит репозитории и делегирует конкретным реализациям.
    /// </summary>
    public sealed class ImportExportFacade
    {
        private readonly IBankAccountRepo _accounts;
        private readonly ICategoryRepo _categories;
        private readonly IOperationRepo _operations;

        public ImportExportFacade(IBankAccountRepo accounts, ICategoryRepo categories, IOperationRepo operations)
        {
            _accounts = accounts;
            _categories = categories;
            _operations = operations;
        }

        /// <summary>
        /// Экспорт всех данных в файл (выбор формата — в переданном экспортере).
        /// </summary>
        public void ExportAll(IDataExporter exporter, string path)
        {
            var data = new DataSnapshot(
                _accounts.All().ToList(),
                _categories.All().ToList(),
                _operations.All().ToList()
            );

            exporter.Export(data, path);
        }

        /// <summary>
        /// Импорт данных из файла с помощью шаблонного импортера.
        /// Политика конфликтов — внутри импортера (минимум: fail on conflict).
        /// </summary>
        public void ImportFrom(IDataImporter importer, string path)
        {
            importer.Run(path);
        }
    }

    /// <summary>
    /// Снимок данных для экспорта.
    /// </summary>
    public sealed record DataSnapshot(
        IReadOnlyList<Base.Entities.BankAccount> Accounts,
        IReadOnlyList<Base.Entities.Category> Categories,
        IReadOnlyList<Base.Entities.Operation> Operations);
}