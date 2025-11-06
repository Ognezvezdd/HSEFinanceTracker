using System.Globalization;
using HSEFinanceTracker.Application.Export;
using HSEFinanceTracker.Application.Facades;
using HSEFinanceTracker.Application.Import;
using HSEFinanceTracker.UI.Abstractions;
using HSEFinanceTracker.UI.Services;

namespace HSEFinanceTracker.UI.Screens
{
    public sealed class ImportExportScreen(
        ImportExportFacade ioFacade,
        IEnumerable<IDataExporter> exporters,
        IEnumerable<IDataImporter> importers,
        UiIo io)
        : IMenuScreen
    {
        public string Title => "Импорт/Экспорт";

        public void Show()
        {
            while (true)
            {
                io.Clear();
                var cmd = io.Choose(Title,
                [
                    "Экспорт: JSON", "Экспорт: CSV", "Экспорт: YAML", "Импорт: JSON", "Импорт: CSV",
                    "Импорт: YAML", "Назад"
                ]);
                if (cmd == "Назад")
                {
                    return;
                }

                switch (cmd)
                {
                    case "Экспорт: JSON": Export("json"); break;
                    case "Импорт: JSON": Import("json"); break;
                    // case "Экспорт: CSV": NotAvailable("CSV экспорт"); break;
                    // case "Экспорт: YAML": NotAvailable("YAML экспорт"); break;
                    // case "Импорт: CSV": NotAvailable("CSV импорт"); break;
                    // case "Импорт: YAML": NotAvailable("YAML импорт"); break;
                }

                io.ReadKey();
            }
        }

        private void Export(string fmt)
        {
            var path = io.AskNonEmpty($"Путь к файлу ({fmt}):");
            var exporter =
                exporters.FirstOrDefault(e => e.GetType().Name.StartsWith(fmt, true, CultureInfo.InvariantCulture));
            if (exporter is null)
            {
                NotAvailable($"{fmt.ToUpper()} экспорт");
                return;
            }

            ioFacade.ExportAll(exporter, path);
            io.Info($"Экспорт выполнен: {path}");
        }

        private void Import(string fmt)
        {
            var path = io.AskNonEmpty($"Путь к файлу ({fmt}):");
            if (!File.Exists(path))
            {
                io.Warn("Файл не найден");
                return;
            }

            var importer =
                importers.FirstOrDefault(i => i.GetType().Name.StartsWith(fmt, true, CultureInfo.InvariantCulture));
            if (importer is null)
            {
                NotAvailable($"{fmt.ToUpper()} импорт");
                return;
            }

            ioFacade.ImportFrom(importer, path);
            io.Info("Импорт завершён");
        }

        private void NotAvailable(string feature)
        {
            io.Warn($"Модуль {feature} не готов. Ожидайте будущих релизов");
        }
    }
}