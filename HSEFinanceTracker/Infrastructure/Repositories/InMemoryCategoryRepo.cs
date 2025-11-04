using HSEFinanceTracker.Base;
using HSEFinanceTracker.Base.Entities;
using HSEFinanceTracker.Base.Repositories;

namespace HSEFinanceTracker.Infrastructure.Repositories
{
    public sealed class InMemoryCategoryRepo : ICategoryRepo
    {
        private readonly Dictionary<Guid, Category> _data = new();

        public Category Add(Category category)
        {
            return _data[category.Id] = category;
        }

        public Category? Get(Guid id)
        {
            return _data.TryGetValue(id, out var c) ? c : null;
        }

        public IEnumerable<Category> All(CategoryType? type = null)
        {
            return type is null ? _data.Values : _data.Values.Where(c => c.Type == type);
        }

        public void Remove(Guid id)
        {
            _data.Remove(id);
        }
    }
}