using System.Globalization;
using HSEFinanceTracker.Application.Export;
using HSEFinanceTracker.Application.Facades;
using HSEFinanceTracker.Application.Import;
using HSEFinanceTracker.UI.Abstractions;
using HSEFinanceTracker.UI.Services;

namespace HSEFinanceTracker.UI.Screens
{
    public sealed class ImportExportScreen : IMenuScreen
    {
        public string Title => "Импорт/Экспорт";

        private readonly ImportExportFacade _ioFacade;
        private readonly IEnumerable<IDataExporter> _exporters;
        private readonly IEnumerable<IDataImporter> _importers;
        private readonly UiIo _io;

        public ImportExportScreen(ImportExportFacade ioFacade,
            IEnumerable<IDataExporter> exporters,
            IEnumerable<IDataImporter> importers, UiIo io)
        {
            _ioFacade = ioFacade;
            _exporters = exporters;
            _importers = importers;
            _io = io;
        }

        public void Show()
        {
            while (true)
            {
                _io.Clear();
                var cmd = _io.Choose(Title,
                    new[]
                    {
                        "Экспорт: JSON", "Экспорт: CSV", "Экспорт: YAML", "Импорт: JSON", "Импорт: CSV",
                        "Импорт: YAML", "Назад"
                    });
                if (cmd == "Назад")
                {
                    return;
                }

                switch (cmd)
                {
                    case "Экспорт: JSON": Export("json"); break;
                    case "Импорт: JSON": Import("json"); break;
                    case "Экспорт: CSV": NotAvailable("CSV экспорт"); break;
                    case "Экспорт: YAML": NotAvailable("YAML экспорт"); break;
                    case "Импорт: CSV": NotAvailable("CSV импорт"); break;
                    case "Импорт: YAML": NotAvailable("YAML импорт"); break;
                }

                _io.ReadKey();
            }
        }

        private void Export(string fmt)
        {
            var path = _io.AskNonEmpty($"Путь к файлу ({fmt}):");
            var exporter =
                _exporters.FirstOrDefault(e => e.GetType().Name.StartsWith(fmt, true, CultureInfo.InvariantCulture));
            if (exporter is null)
            {
                NotAvailable($"{fmt.ToUpper()} экспорт");
                return;
            }

            _ioFacade.ExportAll(exporter, path);
            _io.Info($"Экспорт выполнен: {path}");
        }

        private void Import(string fmt)
        {
            var path = _io.AskNonEmpty($"Путь к файлу ({fmt}):");
            if (!File.Exists(path))
            {
                _io.Warn("Файл не найден");
                return;
            }

            var importer =
                _importers.FirstOrDefault(i => i.GetType().Name.StartsWith(fmt, true, CultureInfo.InvariantCulture));
            if (importer is null)
            {
                NotAvailable($"{fmt.ToUpper()} импорт");
                return;
            }

            _ioFacade.ImportFrom(importer, path);
            _io.Info("Импорт завершён");
        }

        private void NotAvailable(string feature)
        {
            _io.Warn($"{feature} не подключён. Добавьте реализацию и зарегистрируйте в DI.");
        }
    }
}