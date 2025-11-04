namespace HSEFinanceTracker.Application.Import
{
    public abstract class FileImportTemplate<TParsed>
    {
        public void Import(string path)
        {
            var raw = File.ReadAllText(path);
            var parsed = Parse(raw);
            Validate(parsed);
            Persist(parsed);
        }

        protected abstract TParsed Parse(string raw);
        protected virtual void Validate(TParsed data) { }
        protected abstract void Persist(TParsed data);
    }
}