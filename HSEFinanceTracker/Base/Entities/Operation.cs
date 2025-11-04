namespace HSEFinanceTracker.Base.Entities
{
    public sealed class Operation
    {
        public Guid Id { get; }
        public OperationType Type { get; }
        public Guid BankAccountId { get; }
        public Guid CategoryId { get; }
        public decimal Amount { get; }
        public DateTime Date { get; }
        public string? Description { get; }

        public Operation(Guid id, OperationType type, Guid bankAccountId, Guid categoryId,
            decimal amount, DateTime date, string? description = null)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be > 0");
            }

            Id = id;
            Type = type;
            BankAccountId = bankAccountId;
            CategoryId = categoryId;
            Amount = amount;
            Date = date;
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        }
    }
}