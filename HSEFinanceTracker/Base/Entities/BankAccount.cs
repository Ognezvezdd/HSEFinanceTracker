namespace HSEFinanceTracker.Base.Entities
{
    public sealed class BankAccount
    {
        public Guid Id { get; }
        public string Name { get; private set; }
        public decimal Balance { get; private set; }

        internal BankAccount(Guid id, string name, decimal balance = 0m)
        {
            Id = id;
            Name = name.Trim();
            Balance = balance;
        }

        public void Rename(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
            {
                throw new ArgumentException("Name required", nameof(newName));
            }

            Name = newName.Trim();
        }

        public void Apply(decimal delta)
        {
            Balance += delta;
        }
    }
}