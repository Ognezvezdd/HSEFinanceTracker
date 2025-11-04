namespace HSEFinanceTracker.Base.Entities
{
    public sealed class Category
    {
        public Guid Id { get; }
        public CategoryType Type { get; }
        public string Name { get; }

        public Category(Guid id, CategoryType type, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Name required", nameof(name));
            }

            Id = id;
            Type = type;
            Name = name.Trim();
        }
    }
}