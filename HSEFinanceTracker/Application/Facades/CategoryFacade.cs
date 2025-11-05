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
            _categories.Remove(id);
        }

        public void Rename(Guid catId, string newName)
        {
            Create(GetCategoryType(catId), newName);
            Delete(catId);
        }

        private CategoryType GetCategoryType(Guid id)
        {
            return GetCategory(id).Type;
        }

        private Category GetCategory(Guid id)
        {
            if (_categories.Get(id) != null)
            {
                return _categories.Get(id)!;
            }

            throw new InvalidOperationException("Категории с таким ID нет");
        }
    }
}