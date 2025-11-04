using Microsoft.Extensions.DependencyInjection;
using HSEFinanceTracker.Base.Factories;
using HSEFinanceTracker.Base.Repositories;
using HSEFinanceTracker.Infrastructure.Repositories;
using HSEFinanceTracker.Application.Facades;
using HSEFinanceTracker.UI;

// DI-контейнер
var services = new ServiceCollection();

// База (домен)
services.AddSingleton<IFinanceFactory, FinanceFactory>();
services.AddSingleton<IBankAccountRepo, InMemoryBankAccountRepo>();
services.AddSingleton<ICategoryRepo, InMemoryCategoryRepo>();
services.AddSingleton<IOperationRepo, InMemoryOperationRepo>();

// Фасады
services.AddSingleton<BankAccountFacade>();
services.AddSingleton<CategoryFacade>();
services.AddSingleton<OperationFacade>();

// UI
services.AddSingleton<MainMenu>();

// Сборка и запуск
var provider = services.BuildServiceProvider();
var menu = provider.GetRequiredService<MainMenu>();
menu.Run();