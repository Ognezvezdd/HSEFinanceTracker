using HSEFinanceTracker.Application.Commands;
using HSEFinanceTracker.Application.Commands.Decorators;
using HSEFinanceTracker.Application.Facades;
using HSEFinanceTracker.Base;
using HSEFinanceTracker.Base.Factories;
using HSEFinanceTracker.Base.Repositories;
using HSEFinanceTracker.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

// DI
var services = new ServiceCollection();

// Base (домен)
services.AddSingleton<IFinanceFactory, FinanceFactory>();
services.AddSingleton<IBankAccountRepo, InMemoryBankAccountRepo>();
services.AddSingleton<ICategoryRepo, InMemoryCategoryRepo>();
services.AddSingleton<IOperationRepo, InMemoryOperationRepo>();

// Facades
services.AddSingleton<BankAccountFacade>();
services.AddSingleton<CategoryFacade>();
services.AddSingleton<OperationFacade>();

var provider = services.BuildServiceProvider();

// Примерчик
var accounts = provider.GetRequiredService<BankAccountFacade>();
var categories = provider.GetRequiredService<CategoryFacade>();
var ops = provider.GetRequiredService<OperationFacade>();

var createAcc = new TimedCommandDecorator(
    new CreateBankAccountCommand(accounts, "Основной", 0m),
    t => Console.WriteLine($"CreateAccount {t.TotalMilliseconds:F1} ms"));
createAcc.Execute();

var acc = accounts.All().First();
var salary = categories.Create(CategoryType.Income, "Зарплата");
var cafe = categories.Create(CategoryType.Expense, "Кафе");

ops.Create(OperationType.Income, acc.Id, salary.Id, 100000m, DateTime.Today, "Оклад");
ops.Create(OperationType.Expense, acc.Id, cafe.Id, 1500m, DateTime.Today, "Латте");

Console.WriteLine($"Account: {acc.Name}, Balance: {acc.Balance}");
foreach (var o in ops.ForAccount(acc.Id))
{
    Console.WriteLine($"{o.Date:yyyy-MM-dd} {o.Type} {o.Amount}");
}

Console.WriteLine("Done.");

// TODO: Тут потом все удалить и сделать вызов меню