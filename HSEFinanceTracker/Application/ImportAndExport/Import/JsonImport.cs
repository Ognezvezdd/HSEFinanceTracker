using HSEFinanceTracker.Application.Facades;
using HSEFinanceTracker.Base;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HSEFinanceTracker.Application.ImportAndExport.Import
{
    /// <summary>
    /// Импорт JSON-файла.
    /// Создание доменных сущностей — через фасады.
    /// Id из файла используются как внешние ссылки (карта IdFile→IdRuntime).
    /// </summary>
    public sealed class JsonImport : FileImportTemplate<ImportExportDto>
    {
        private readonly BankAccountFacade _accounts;
        private readonly CategoryFacade _categories;
        private readonly OperationFacade _operations;

        public JsonImport(
            BankAccountFacade accounts,
            CategoryFacade categories,
            OperationFacade operations)
        {
            _accounts = accounts;
            _categories = categories;
            _operations = operations;
        }

        protected override ImportExportDto Parse(string raw)
        {
            var opts = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true, Converters = { new JsonStringEnumConverter() }
            };
            var dto = JsonSerializer.Deserialize<ImportExportDto>(raw, opts);
            if (dto is null)
            {
                throw new InvalidDataException("Не удалось разобрать JSON");
            }

            return dto;
        }

        protected override void Validate(ImportExportDto parsed)
        {
            // Дубли Id в Accounts
            var duplAcc = parsed.Accounts
                .GroupBy(a => a.Id)
                .FirstOrDefault(g => g.Count() > 1);
            if (duplAcc is not null)
            {
                throw new InvalidDataException($"Дублирующийся Account Id: {duplAcc.Key}");
            }

            // Дубли Id в Categories
            var duplCat = parsed.Categories
                .GroupBy(c => c.Id)
                .FirstOrDefault(g => g.Count() > 1);
            if (duplCat is not null)
            {
                throw new InvalidDataException($"Дублирующийся Category Id: {duplCat.Key}");
            }

            // Дубли Id в Operations
            var duplOp = parsed.Operations
                .GroupBy(o => o.Id)
                .FirstOrDefault(g => g.Count() > 1);
            if (duplOp is not null)
            {
                throw new InvalidDataException($"Дублирующийся Operation Id: {duplOp.Key}");
            }

            // Валидация типов категорий и операций + кросс-ссылки

            var accountIds = parsed.Accounts.Select(a => a.Id).ToHashSet();
            var categoryIds = parsed.Categories.Select(c => c.Id).ToHashSet();

            foreach (var c in parsed.Categories)
            {
                ParseCategoryTypeOrThrow(c.Type, $"Category Id={c.Id}, Name='{c.Name}'");
            }

            foreach (var o in parsed.Operations)
            {
                ParseOperationTypeOrThrow(o.Type, $"Operation Id={o.Id}");

                if (!accountIds.Contains(o.BankAccountId))
                {
                    throw new InvalidDataException(
                        $"Для операции {o.Id} указан несуществующий BankAccountId {o.BankAccountId}");
                }

                if (!categoryIds.Contains(o.CategoryId))
                {
                    throw new InvalidDataException(
                        $"Для операции {o.Id} указан несуществующий CategoryId {o.CategoryId}");
                }
            }
        }

        protected override void Persist(ImportExportDto parsed)
        {
            // Снимки существующих данных (по одному разу)
            var existingAccounts = _accounts.All().ToList();
            var existingCategories = _categories.All().ToList();

            // Проверяем конфликты по имени и Id для счетов
            var existingAccByName = existingAccounts
                .ToDictionary(a => a.Name, a => a, StringComparer.OrdinalIgnoreCase);
            var existingAccIds = existingAccounts
                .ToDictionary(a => a.Id, a => a);

            foreach (var a in parsed.Accounts)
            {
                if (existingAccIds.ContainsKey(a.Id))
                {
                    throw new InvalidOperationException(
                        $"Счёт с Id '{a.Id}' уже существует");
                }

                if (existingAccByName.ContainsKey(a.Name))
                {
                    throw new InvalidOperationException(
                        $"Счёт с именем '{a.Name}' уже существует");
                }
            }

            // Проверяем конфликты по категории (Id + (Type,Name))
            var existingCatById = existingCategories
                .ToDictionary(c => c.Id, c => c);
            foreach (var c in parsed.Categories)
            {
                var type = ParseCategoryTypeOrThrow(c.Type, $"Category Id={c.Id}, Name='{c.Name}'");

                if (existingCatById.ContainsKey(c.Id))
                {
                    throw new InvalidOperationException(
                        $"Категория с Id '{c.Id}' уже существует (имя: '{existingCatById[c.Id].Name}')");
                }

                var dup = existingCategories.Any(x =>
                    x.Type == type &&
                    x.Name.Equals(c.Name, StringComparison.OrdinalIgnoreCase));

                if (dup)
                {
                    throw new InvalidOperationException(
                        $"Категория '{c.Name}' ({type}) уже существует");
                }
            }

            //  Создаём счета и строим карту IdFile → IdRuntime 

            var accountIdMap = new Dictionary<Guid, Guid>(); // fileId -> runtimeId

            foreach (var a in parsed.Accounts)
            {
                // Предполагаем, что фасад возвращает созданный аккаунт
                var created = _accounts.Create(a.Name, a.Balance);
                accountIdMap[a.Id] = created.Id;
            }

            //  Создаём категории и строим карту IdFile → IdRuntime 

            var categoryIdMap = new Dictionary<Guid, Guid>(); // fileId -> runtimeId

            foreach (var c in parsed.Categories)
            {
                var type = ParseCategoryTypeOrThrow(c.Type, $"Category Id={c.Id}, Name='{c.Name}'");
                var created = _categories.Create(type, c.Name);
                categoryIdMap[c.Id] = created.Id;
            }

            //  Создаём операции, используя Id из файла как внешние ссылки 

            foreach (var o in parsed.Operations)
            {
                var type = ParseOperationTypeOrThrow(o.Type, $"Operation Id={o.Id}");

                if (!accountIdMap.TryGetValue(o.BankAccountId, out var runtimeAccId))
                {
                    throw new InvalidOperationException(
                        $"Не найден счёт BankAccountId={o.BankAccountId} (из файла) для операции {o.Id}");
                }

                if (!categoryIdMap.TryGetValue(o.CategoryId, out var runtimeCatId))
                {
                    throw new InvalidOperationException(
                        $"Не найдена категория CategoryId={o.CategoryId} (из файла) для операции {o.Id}");
                }

                // Id операции из файла сейчас НЕ пробрасываем в домен,
                // он используется как внешний идентификатор/для валидации.
                // При желании можно добавить overload в фасад/фабрику.
                _operations.Create(
                    type,
                    runtimeAccId,
                    runtimeCatId,
                    o.Amount,
                    o.Date,
                    o.Description
                );
            }
        }

        // = Helpers =

        private static CategoryType ParseCategoryTypeOrThrow(string type, string context)
        {
            if (type.Equals("Income", StringComparison.OrdinalIgnoreCase))
            {
                return CategoryType.Income;
            }

            if (type.Equals("Expense", StringComparison.OrdinalIgnoreCase))
            {
                return CategoryType.Expense;
            }

            throw new InvalidDataException(
                $"Некорректный тип категории '{type}' для {context}. Ожидается 'Income' или 'Expense'.");
        }

        private static OperationType ParseOperationTypeOrThrow(string type, string context)
        {
            if (type.Equals("Income", StringComparison.OrdinalIgnoreCase))
            {
                return OperationType.Income;
            }

            if (type.Equals("Expense", StringComparison.OrdinalIgnoreCase))
            {
                return OperationType.Expense;
            }

            throw new InvalidDataException(
                $"Некорректный тип операции '{type}' для {context}. Ожидается 'Income' или 'Expense'.");
        }
    }
}