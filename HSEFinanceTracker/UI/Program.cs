using HSEFinanceTracker.Application.Export;
using HSEFinanceTracker.Application.Facades;
using HSEFinanceTracker.Application.Import;
using HSEFinanceTracker.Base.Factories;
using HSEFinanceTracker.Base.Repositories;
using HSEFinanceTracker.Infrastructure.Repositories;
using HSEFinanceTracker.UI;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable All

var services = new ServiceCollection();

// Repositories (InMemory)
services.AddSingleton<IBankAccountRepo, InMemoryBankAccountRepo>();
services.AddSingleton<ICategoryRepo,    InMemoryCategoryRepo>();
services.AddSingleton<IOperationRepo,   InMemoryOperationRepo>();

// Factory
services.AddSingleton<IFinanceFactory, FinanceFactory>();

// Facades
services.AddSingleton<BankAccountFacade>();
services.AddSingleton<CategoryFacade>();
services.AddSingleton<OperationFacade>();
services.AddSingleton<AnalyticsFacade>();
services.AddSingleton<ImportExportFacade>();

// Import/Export services
services.AddSingleton<IDataExporter, JsonExport>(); // по умолчанию JSON
services.AddSingleton<IDataImporter, JsonImport>(); // по умолчанию JSON-импорт

// UI
services.AddSingleton<MainMenu>();

var provider = services.BuildServiceProvider();
var menu = provider.GetRequiredService<MainMenu>();
menu.Run();