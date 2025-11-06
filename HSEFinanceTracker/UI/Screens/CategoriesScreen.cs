using HSEFinanceTracker.Application.Facades;
using HSEFinanceTracker.Base;
using HSEFinanceTracker.UI.Abstractions;
using HSEFinanceTracker.UI.Services;
using Spectre.Console;

namespace HSEFinanceTracker.UI.Screens
{
    public sealed class CategoriesScreen : IMenuScreen
    {
        public string Title => "Категории";

        private readonly CategoryFacade _categories;
        private readonly UiIo _io;

        public CategoriesScreen(CategoryFacade categories, UiIo io)
        {
            _categories = categories;
            _io = io;
        }

        public void Show()
        {
            while (true)
            {
                _io.Clear();
                var cmd = _io.Choose(Title, new[] { "Создать", "Список", "Переименовать", "Удалить", "Назад" });
                if (cmd == "Назад")
                {
                    return;
                }


                switch (cmd)
                {
                    case "Создать": Create(); break;
                    case "Список": List(); break;
                    case "Переименовать": Rename(); break;
                    case "Удалить": Delete(); break;
                }

                _io.ReadKey();
            }
        }

        private void Create()
        {
            var typeStr = _io.Choose("Тип категории", new[] { "Доход", "Расход" });
            var type = typeStr == "Доход" ? CategoryType.Income : CategoryType.Expense;
            var name = _io.AskNonEmpty("Название категории:");
            var cat = _categories.Create(type, name);
            _io.Info($"Категория создана: {cat.Name} ({cat.Type})");
        }

        private void List()
        {
            var filter = _io.Choose("Фильтр", new[] { "Все", "Только доходы", "Только расходы" });
            CategoryType? type = filter switch
            {
                "Только доходы" => CategoryType.Income,
                "Только расходы" => CategoryType.Expense,
                _ => null
            };

            var list = _categories.All(type).ToList();
            if (!list.Any())
            {
                _io.Warn("Категорий нет");
                return;
            }

            var t = _io.TableRounded("Id", "Название", "Тип");
            foreach (var c in list)
            {
                t.AddRow(c.Id.ToString(), c.Name, c.Type.ToString());
            }

            _io.WriteTable(t);
        }

        private void Rename()
        {
            var cat = PickAnyCategory();
            if (cat is null)
            {
                _io.Warn("Категорий нет");
                return;
            }

            var newName = _io.AskNonEmpty("Новое название категории:");
            _categories.Rename(cat.Id, newName);
            _io.Info("Готово");
        }

        private void Delete()
        {
            var cat = PickAnyCategory();
            if (cat is null)
            {
                _io.Warn("Категорий нет");
                return;
            }

            if (!_io.Confirm($"Удалить категорию '{cat.Name}' ({cat.Type})?"))
            {
                return;
            }

            _categories.Delete(cat.Id);
            _io.Info("Категория удалена");
        }

        private Base.Entities.Category? PickAnyCategory()
        {
            var list = _categories.All().ToList();
            if (!list.Any())
            {
                return null;
            }

            var choice = _io.Choose("Выберите категорию",
                list.Select(c => $"{c.Name} | {c.Type} | {c.Id}"));
            var id = choice.Split('|').Last().Trim();
            return list.First(c => c.Id.ToString() == id);
        }
    }
}