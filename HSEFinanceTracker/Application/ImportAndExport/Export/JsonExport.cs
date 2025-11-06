using HSEFinanceTracker.Application.Facades;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HSEFinanceTracker.Application.ImportAndExport.Export
{
    /// <summary>
    /// JSON-экспортёр.
    /// Делает один файл с массивами accounts/categories/operations.
    /// </summary>
    public sealed class JsonExport : FileExportTemplate<ImportExportDto>
    {
        private readonly JsonSerializerOptions _opts = new()
        {
            WriteIndented = true, Converters = { new JsonStringEnumConverter() }
        };

        protected override void Export(DataSnapshot data, string path)
        {
            var dto = new ImportExportDto
            {
                Accounts = data.Accounts.Select(a => new AccountDto(a.Id, a.Name, a.Balance)).ToList(),
                Categories = data.Categories.Select(c => new CategoryDto(c.Id, c.Type.ToString(), c.Name)).ToList(),
                Operations = data.Operations.Select(o => new OperationDto(
                    o.Id, o.Type.ToString(), o.BankAccountId, o.CategoryId, o.Amount, o.Date, o.Description
                )).ToList()
            };

            var json = JsonSerializer.Serialize(dto, _opts);
            File.WriteAllText(path, json);
        }
    }
}