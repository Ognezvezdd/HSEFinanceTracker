using HSEFinanceTracker.Application.Facades;

namespace HSEFinanceTracker.Application.ImportAndExport.Export
{
    /// <summary>
    /// Контракт экспортёра (можно сделать ещё CsvExport, YamlExport).
    /// </summary>
    public interface IDataExporter
    {
        void Run(DataSnapshot data, string path);
    }
}