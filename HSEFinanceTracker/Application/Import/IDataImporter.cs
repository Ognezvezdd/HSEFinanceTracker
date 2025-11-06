namespace HSEFinanceTracker.Application.Import
{
    /// <summary>
    /// Контракт импортёра (CSV/JSON/YAML и т.д.).
    /// </summary>
    public interface IDataImporter
    {
        void Run(string path);
    }
}