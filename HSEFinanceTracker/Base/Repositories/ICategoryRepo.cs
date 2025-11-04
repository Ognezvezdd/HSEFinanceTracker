using HSEFinanceTracker.Base.Entities;

namespace HSEFinanceTracker.Base.Repositories
{
    public interface ICategoryRepo
    {
        Category Add(Category category);
        Category? Get(Guid id);
        IEnumerable<Category> All(CategoryType? type = null);
        void Remove(Guid id);
    }
}