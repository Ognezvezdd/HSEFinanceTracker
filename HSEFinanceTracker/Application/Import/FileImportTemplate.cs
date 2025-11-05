namespace HSEFinanceTracker.Application.Import
{
    /// <summary>
    /// Шаблонный метод для импорта: ReadFile -> Parse -> Validate -> Persist.
    /// Наследники реализуют Parse/Validate/Persist под свой формат.
    /// </summary>
    public abstract class FileImportTemplate<TParsed> : IDataImporter
    {
        public void Run(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Файл не найден", path);
            }

            var raw = File.ReadAllText(path);
            var parsed = Parse(raw);
            Validate(parsed);
            Persist(parsed);
        }

        protected abstract TParsed Parse(string raw);
        protected virtual void Validate(TParsed parsed) { /* опционально */ }
        protected abstract void Persist(TParsed parsed);
    }

    /// <summary>
    /// Контракт импортёра (CSV/JSON/YAML и т.д.).
    /// </summary>
    public interface IDataImporter
    {
        void Run(string path);
    }
}