using System.Text.Json;
using System.Text.Json.Serialization;
using HSEFinanceTracker.Application.Facades;

namespace HSEFinanceTracker.Application.Export
{
    /// <summary>
    /// Простой JSON-экспортёр.
    /// Делает один файл с массивами accounts/categories/operations.
    /// </summary>
    public sealed class JsonExport : IDataExporter
    {
        private readonly JsonSerializerOptions _opts = new()
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public void Export(DataSnapshot data, string path)
        {
            var dto = new ExportDto
            {
                Accounts   = data.Accounts.Select(a => new AccountDto(a.Id, a.Name, a.Balance)).ToList(),
                Categories = data.Categories.Select(c => new CategoryDto(c.Id, c.Type.ToString(), c.Name)).ToList(),
                Operations = data.Operations.Select(o => new OperationDto(
                    o.Id, o.Type.ToString(), o.BankAccountId, o.CategoryId, o.Amount, o.Date, o.Description
                )).ToList()
            };

            var json = JsonSerializer.Serialize(dto, _opts);
            File.WriteAllText(path, json);
        }

        private sealed class ExportDto
        {
            public List<AccountDto>   Accounts   { get; set; } = new();
            public List<CategoryDto>  Categories { get; set; } = new();
            public List<OperationDto> Operations { get; set; } = new();
        }

        private sealed record AccountDto(Guid Id, string Name, decimal Balance);
        private sealed record CategoryDto(Guid Id, string Type, string Name);
        private sealed record OperationDto(Guid Id, string Type, Guid BankAccountId, Guid CategoryId,
                                           decimal Amount, DateTime Date, string? Description);
    }

    /// <summary>
    /// Контракт экспортёра (можно сделать ещё CsvExport, YamlExport).
    /// </summary>
    public interface IDataExporter
    {
        void Export(DataSnapshot data, string path);
    }
}