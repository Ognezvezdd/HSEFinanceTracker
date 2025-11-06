// Program.cs
// ReSharper disable All

using HSEFinanceTracker.Application.Export;
using HSEFinanceTracker.Application.Facades;
using HSEFinanceTracker.Application.Import;
using HSEFinanceTracker.UI.Services; // <-- уже есть для Uio; нужен и для TimedScenario
using HSEFinanceTracker.Base.Factories;
using HSEFinanceTracker.Base.Repositories;
using HSEFinanceTracker.Infrastructure.Repositories;
using HSEFinanceTracker.UI;
using HSEFinanceTracker.UI.Screens;
using HSEFinanceTracker.UI.Services;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

// ============================
// 1) Repositories (InMemory)
// ============================
services.AddSingleton<IBankAccountRepo, InMemoryBankAccountRepo>();
services.AddSingleton<ICategoryRepo, InMemoryCategoryRepo>();
services.AddSingleton<IOperationRepo, InMemoryOperationRepo>();

// ============================
// 2) Factory (Фабрика доменных сущностей)
// ============================
services.AddSingleton<IFinanceFactory, FinanceFactory>();

// ============================
// 3) Facades (Фасады приложения)
// ============================
services.AddSingleton<BankAccountFacade>();
services.AddSingleton<CategoryFacade>();
services.AddSingleton<OperationFacade>();
services.AddSingleton<AnalyticsFacade>();
services.AddSingleton<ImportExportFacade>();

// ============================
// 4) Import / Export (Template Method + Visitor)
// по умолчанию JSON (при необходимости позже добавишь CSV/YAML)
// ============================
services.AddSingleton<IDataExporter, JsonExport>();
services.AddSingleton<IDataImporter, JsonImport>();

// ============================
// 5) UI services (ввод/вывод)
// ОБРАТИ ВНИМАНИЕ: регистрируем Uio — этот класс
// должен находиться в HSEFinanceTracker.UI.Services и называться именно Uio.
// Если у тебя файл/класс сейчас называется "Uilo", переименуй в "Uio".
// ============================
services.AddSingleton<UiIo>();
// UI services (ввод/вывод, тайминг сценариев)

// ============================
// 6) Screens (экраны-меню, тонкая оболочка над фасадами/командами)
// ============================
services.AddSingleton<AccountsScreen>();
services.AddSingleton<CategoriesScreen>();
services.AddSingleton<OperationsScreen>();
services.AddSingleton<ReportsScreen>();
services.AddSingleton<ImportExportScreen>();
services.AddSingleton<DataToolsScreen>();

// ============================
// 7) Root UI (главное меню)
// ============================
services.AddSingleton<MainMenu>();

// ============================
// Build & Run
// ============================
using var provider = services.BuildServiceProvider();

var menu = provider.GetRequiredService<MainMenu>();
menu.Run();