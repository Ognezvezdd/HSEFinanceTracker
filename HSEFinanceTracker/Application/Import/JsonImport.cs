using System.Text.Json;
using System.Text.Json.Serialization;
using HSEFinanceTracker.Base.Entities;
using HSEFinanceTracker.Base.Factories;
using HSEFinanceTracker.Application.Facades;
using HSEFinanceTracker.Base;

namespace HSEFinanceTracker.Application.Import
{
    /// <summary>
    /// Импорт JSON-файла.
    /// Политика конфликтов: минимально — fail on conflict (TODO: сделать настраиваемой).
    /// Создание доменных сущностей — через фабрику и фасады.
    /// </summary>
    public sealed class JsonImport : FileImportTemplate<JsonImport.ImportDto>
    {
        private readonly IFinanceFactory _factory;
        private readonly BankAccountFacade _accounts;
        private readonly CategoryFacade _categories;
        private readonly OperationFacade _operations;

        public JsonImport(IFinanceFactory factory,
            BankAccountFacade accounts,
            CategoryFacade categories,
            OperationFacade operations)
        {
            _factory = factory;
            _accounts = accounts;
            _categories = categories;
            _operations = operations;
        }

        protected override ImportDto Parse(string raw)
        {
            var opts = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true, Converters = { new JsonStringEnumConverter() }
            };
            var dto = JsonSerializer.Deserialize<ImportDto>(raw, opts);
            if (dto is null)
            {
                throw new InvalidDataException("Не удалось разобрать JSON");
            }

            return dto;
        }

        protected override void Validate(ImportDto parsed)
        {
            var duplAcc = parsed.Accounts.GroupBy(a => a.Id).FirstOrDefault(g => g.Count() > 1);
            if (duplAcc is not null)
            {
                throw new InvalidDataException($"Дублирующийся Account Id: {duplAcc.Key}");
            }

            var duplCat = parsed.Categories.GroupBy(a => a.Id).FirstOrDefault(g => g.Count() > 1);
            if (duplCat is not null)
            {
                throw new InvalidDataException($"Дублирующийся Category Id: {duplCat.Key}");
            }

            var duplOp = parsed.Operations.GroupBy(a => a.Id).FirstOrDefault(g => g.Count() > 1);
            if (duplOp is not null)
            {
                throw new InvalidDataException($"Дублирующийся Operation Id: {duplOp.Key}");
            }

            // TODO: при желании — кросс-валидировать ссылки (BankAccountId/CategoryId должны существовать).
        }

        protected override void Persist(ImportDto parsed)
        {
            // TODO: можно добавить режим "merge/skip/replace".

            // Счета
            var existingAccByName = _accounts.All().ToDictionary(a => a.Name, a => a, StringComparer.OrdinalIgnoreCase);
            foreach (var a in parsed.Accounts)
            {
                if (existingAccByName.ContainsKey(a.Name))
                {
                    throw new InvalidOperationException($"Счёт с именем '{a.Name}' уже существует");
                }

                _accounts.Create(a.Name, a.Balance);
            }

            // Категории
            var existingCats = _categories.All().ToList();
            foreach (var c in parsed.Categories)
            {
                var type = c.Type.Equals("Income", StringComparison.OrdinalIgnoreCase)
                    ? CategoryType.Income
                    : CategoryType.Expense;

                var dup = existingCats.Any(x =>
                    x.Type == type && x.Name.Equals(c.Name, StringComparison.OrdinalIgnoreCase));
                if (dup)
                {
                    throw new InvalidOperationException($"Категория '{c.Name}' ({type}) уже существует");
                }

                _categories.Create(type, c.Name);
            }

            // Операции
            var accByName = _accounts.All().ToDictionary(a => a.Name, a => a, StringComparer.OrdinalIgnoreCase);
            var catByKey = _categories.All().ToDictionary(
                c => (c.Type, c.Name.ToLowerInvariant()),
                c => c);

            foreach (var o in parsed.Operations)
            {
                var type = o.Type.Equals("Income", StringComparison.OrdinalIgnoreCase)
                    ? OperationType.Income
                    : OperationType.Expense;


                // TODO: если хочешь использовать Id из файла — нужно сначала построить карты IdFile→IdRuntime.

                if (!accByName.TryGetValue(o.BankAccountName ?? string.Empty, out var acc))
                {
                    throw new InvalidOperationException($"Не найден счёт '{o.BankAccountName}' для операции {o.Id}");
                }

                var catKey = (type == OperationType.Income ? CategoryType.Income : CategoryType.Expense,
                    (o.CategoryName ?? string.Empty).ToLowerInvariant());

                if (!catByKey.TryGetValue(catKey, out var cat))
                {
                    throw new InvalidOperationException(
                        $"Не найдена категория '{o.CategoryName}' типа {catKey.Item1} для операции {o.Id}");
                }

                _operations.Create(type, acc.Id, cat.Id, o.Amount, o.Date, o.Description);
            }
        }

        // ===== DTO =====
        public sealed class ImportDto
        {
            public List<AccountDto> Accounts { get; set; } = new();
            public List<CategoryDto> Categories { get; set; } = new();
            public List<OperationDto> Operations { get; set; } = new();
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
            string? Description,
            string? BankAccountName = null,
            string? CategoryName = null
        );
    }
}