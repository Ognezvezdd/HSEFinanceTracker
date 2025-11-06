using HSEFinanceTracker.Application.Facades;

namespace HSEFinanceTracker.Application.ImportAndExport.Export
{
    /// <summary>
    /// Шаблонный метод для импорта: ReadFile -> Parse -> Validate -> Persist.
    /// Наследники реализуют Parse/Validate/Persist под свой формат.
    /// </summary>
    public abstract class FileExportTemplate<TParsed> : IDataExporter
    {
        public void Run(DataSnapshot data, string path)
        {
            Export(data, path);
        }

        protected abstract void Export(DataSnapshot data, string path);
    }
}