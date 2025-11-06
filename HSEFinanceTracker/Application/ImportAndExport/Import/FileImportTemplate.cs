namespace HSEFinanceTracker.Application.ImportAndExport.Import
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

        protected abstract void Validate(TParsed parsed);
        protected abstract void Persist(TParsed parsed);
    }
}