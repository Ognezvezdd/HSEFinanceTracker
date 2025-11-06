using HSEFinanceTracker.Base;
using HSEFinanceTracker.Base.Entities;
using HSEFinanceTracker.Base.Factories;
using HSEFinanceTracker.Base.Repositories;

namespace HSEFinanceTracker.Application.Facades
{
    public sealed class CategoryFacade
    {
        private readonly ICategoryRepo _categories;
        private readonly IFinanceFactory _factory;

        public CategoryFacade(ICategoryRepo categories, IFinanceFactory factory)
        {
            _categories = categories;
            _factory = factory;
        }

        public Category Create(CategoryType type, string name)
        {
            var cat = _factory.CreateCategory(type, name);
            _categories.Add(cat);
            return cat;
        }

        public IEnumerable<Category> All(CategoryType? type = null)
        {
            return _categories.All(type);
        }

        public void Delete(Guid id)
        {
            if (_categories.Get(id) != null)
            {
                _categories.Remove(id);
                return;
            }

            throw new Exception("Category not found");
        }

        public Category Rename(Guid catId, string newName)
        {
            if (_categories.Get(catId) == null)
            {
                throw new Exception("Category not found");
            }

            var type = _categories.Get(catId)!.Type;
            Delete(catId);
            var cat = _factory.CreateCategory(catId, type, newName);
            _categories.Add(cat);
            return cat;
        }
    }
}