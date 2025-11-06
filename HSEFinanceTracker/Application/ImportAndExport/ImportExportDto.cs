using HSEFinanceTracker.Base;

namespace HSEFinanceTracker.Application.ImportAndExport
{
    public sealed class ImportExportDto
    {
        public List<AccountDto> Accounts { get; set; } = [];
        public List<CategoryDto> Categories { get; set; } = [];
        public List<OperationDto> Operations { get; set; } = [];
    }

    public sealed record AccountDto(Guid Id, string Name, decimal Balance);

    public sealed record CategoryDto(Guid Id, string Type, string Name);

    public sealed record OperationDto(
        Guid Id,
        string Type,
        Guid BankAccountId,
        Guid CategoryId,
        decimal Amount,
        DateTime Date,
        string? Description);
}