using System.Text.Json;
using System.Text.Json.Serialization;
using HSEFinanceTracker.Application.Facades;
using HSEFinanceTracker.Base.Entities;
using HSEFinanceTracker.Base.Factories;

namespace HSEFinanceTracker.Application.Export
{
    /// <summary>
    /// JSON-экспортёр.
    /// Делает один файл с массивами accounts/categories/operations.
    /// </summary>
    public sealed class JsonExport(IFinanceFactory factory) : IDataExporter
    {
        private readonly JsonSerializerOptions _opts = new()
        {
            WriteIndented = true, Converters = { new JsonStringEnumConverter() }
        };

        public void Export(DataSnapshot data, string path)
        {
            var dto = new ExportDto
            {
                Accounts = data.Accounts.Select(a => factory.CreateBankAccount(a.Id, a.Name, a.Balance)).ToList(),
                Categories = data.Categories.Select(c => factory.CreateCategory(c.Id, c.Type, c.Name)).ToList(),
                Operations = data.Operations.Select(o => factory.CreateOperation(
                    o.Id, o.Type, o.BankAccountId, o.CategoryId, o.Amount, o.Date, o.Description
                )).ToList()
            };

            var json = JsonSerializer.Serialize(dto, _opts);
            File.WriteAllText(path, json);
        }

        private sealed class ExportDto
        {
            public List<BankAccount> Accounts { get; set; } = [];
            public List<Category> Categories { get; set; } = [];
            public List<Operation> Operations { get; set; } = [];
        }
    }
}