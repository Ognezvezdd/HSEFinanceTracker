# Homework №2 - Software Design (КПО)

## HSEFinanceTracker

### Author: Капогузов Максим, БПИ-246

---

## Зависимости

- Microsoft.Extensions.DependencyInjection
- Spectre.Console
- .NET 8 SDK

---

## Структура проекта (смысловые модули)

- Application/
    - Commands/
        - `ICommand.cs` — контракт команды верхнего уровня.
        - `ScreenCommand.cs` — адаптер «экран → команда».
        - `TimedMenuCommand.cs` — декоратор для измерения времени выполнения команды.
    - Facades/
        - `BankAccountFacade.cs`, `CategoryFacade.cs`, `OperationFacade.cs` — фасады CRUD над доменом.
        - `AnalyticsFacade.cs` — фасад аналитики (разница доходы-расходы, группировка по категориям).
        - `ImportExportFacade.cs` — единая точка импорта/экспорта.
        - `RecalcFacade.cs` — пересчёт и верификация балансов счетов по операциям.
    - ImportAndExport/
        - Export/
            - `IDataExporter.cs` — стратегия/контракт экспортёра.
            - `FileExportTemplate.cs` — базовый шаблон экспорта в файл (Template Method).
            - `JsonExport.cs` — конкретный экспортёр JSON (использует DTO-снимок).
        - Import/
            - `IDataImporter.cs` — стратегия/контракт импортёра.
            - `FileImportTemplate.cs` — базовый шаблон импорта из файла (Template Method).
            - `JsonImport.cs` — импорт JSON (валидация + создание через фасады/фабрику).
            - `ImportExportDto.cs` — переносимые DTO (accounts/categories/operations).
- Base/
    - Entities/
        - `BankAccount.cs`, `Category.cs`, `Operation.cs` — доменные сущности и инварианты.
    - Factories/
        - `IFinanceFactory.cs`, `FinanceFactory.cs` — фабрика доменных объектов (валидации при создании).
    - Repositories/
        - `IBankAccountRepo.cs`, `ICategoryRepo.cs`, `IOperationRepo.cs` — контракты репозиториев.
    - `Enums.cs` — перечисления домена.
- Infrastructure/
    - Repositories/
        - `InMemoryBankAccountRepo.cs`, `InMemoryCategoryRepo.cs`, `InMemoryOperationRepo.cs` — простые in-memory реализации.
        - (при подключении БД сюда добавляется `Db*Repo` и Proxy-кэш поверх них)
- UI/
    - Abstractions/
        - `IMenuScreen.cs` — контракт «экрана» (меню).
    - Screens/
        - `AccountsScreen.cs`, `CategoriesScreen.cs`, `OperationsScreen.cs`,
          `ReportsScreen.cs`, `ImportExportScreen.cs`, `DataToolsScreen.cs` — тонкие оболочки над фасадами.
    - Services/
        - `Uilo.cs` (класс `UiIo`) — удобный ввод/вывод (вопросы, таблицы, подтверждения).
        - `ConsoleManager.cs` — цветной вывод с помощью Spectre.Console.
    - `MainMenu.cs` — корневое меню: собирает команды и оборачивает их в таймер-декоратор.
- `Program.cs` — DI-композиция и запуск.

---

## Реализованный функционал

- Счета/Категории/Операции: создание, просмотр, редактирование (переименование/удаление; у операций - через
  delete+create)
- Аналитика: разница доходов/расходов за период, группировка по категориям
- Импорт/Экспорт: JSON (единый снимок всех данных)
- Управление данными: проверка расхождений и пересчёт балансов (по счёту и для всех счетов)
- Статистика: измерение времени пользовательских сценариев верхнего уровня (через декоратор команд)

---

## GRASP

- High Cohesion => расчёты и операции собраны по смыслу во Facades; UI-слой тонкий (экраны без бизнес-логики)
- Low Coupling => UI зависит только от фасадов и интерфейсов; фабрика и репозитории подменяемы через DI

---

## SOLID

- S (Single Responsibility):
    - Экран отвечает только за диалог и отображение (например, AccountsScreen), расчёты - в фасадах (AnalyticsFacade),
      создание объектов - в фабрике (FinanceFactory).
- O (Open/Closed):
    - Добавление новых форматов импорта/экспорта и новых источников данных не требует правок существующего кода (новые
      классы IDataImporter/IDataExporter; новые Repo через DI).
- L (Liskov Substitution):
    - Любая реализация репозитория (InMemoryRepo, DbRepoProxy) подставляется по интерфейсу IBankAccountRepo/… без
      изменений вызывающего кода.
- I (Interface Segregation):
    - Узкие интерфейсы для хранилищ и фабрики: потребители тянут только то, что им нужно (например, IOperationRepo без
      знаний о счетах).
- D (Dependency Inversion):
    - Фасады зависят от абстракций (репозитории, фабрика), а не от конкретных классов. Подмена реализаций - через DI.

---

## Паттерны (8/8)

1) Фасад (Facade)

- Где: BankAccountFacade, CategoryFacade, OperationFacade, AnalyticsFacade, ImportExportFacade, RecalcFacade.
- Зачем: собрать связанную бизнес-логику в единые входные точки. UI вызывает 1-2 метода вместо набора низкоуровневых
  операций.

2) Команда (Command)

- Где: ScreenCommand (каждый пункт верхнего меню - это команда: Name + Execute).
- Зачем: представить сценарий как объект. Это упростило декорирование (таймер), логирование и возможный реентерабельный
  запуск.

3) Декоратор (Decorator) - таймер сценариев

- Где: TimedMenuCommand оборачивает любую ICommand и замеряет длительность.
- Зачем: единообразный сбор метрик без правок команд/экранов.

4) Шаблонный метод (Template Method)

- Где: FileImportTemplate<T>.
- Зачем: общий скелет импорта (читать файл => разобрать => провалидировать => сохранить). Конкретика (JSON) реализует
  только Parse/Validate/Persist.

5) Посетитель (Visitor)

- Где: IDataExportVisitor для обхода снимка данных (DataSnapshot) при экспорте; JsonExport умеет использовать визитёра
  для сериализации узлов (accounts/categories/operations).
- Зачем: отделить логику обхода модели/снимка от конкретного формата экспорта и упростить добавление других экспортёров.

6) Фабрика (Factory)

- Где: IFinanceFactory, FinanceFactory.
- Зачем: централизованное создание сущностей + инварианты (amount > 0, имя не пустое и т.д.). Никаких new в
  фасадах/экранах - только фабрика.

7) Прокси (Proxy)

- Где: уровень репозиториев. Сейчас InMemory*Repo (для учебной версии). При подключении БД добавляется Db*Repo +
  Proxy-кэш, который прозрачно кеширует чтения и синхронизирует записи.
- Зачем: не менять фасады/UI при смене источника данных, добавить кэширование поверх БД.

8) Стратегия (Strategy)

- Где: IDataExporter/IDataImporter - семейства алгоритмов экспорта/импорта (сейчас JSON).
- Зачем: единый интерфейс + DI-выбор конкретной реализации без условных блоков в коде.

---

## DI (пример)

В Program.cs регистрируются интерфейсы => реализации. За счёт этого можно подменить любую часть без правки кода:

- Репозитории: InMemoryRepo => DbRepo (+ Proxy-кэш)
- Импорт/Экспорт: JsonImport/JsonExport => другие форматы
- Фабрика: FinanceFactory => расширенная валидациями

Пример замены реестра репозиториев:

```C# 
services.AddSingleton<IBankAccountRepo, InMemoryBankAccountRepo>();
// на проде:
/// services.AddSingleton<IBankAccountRepo>(sp => new ProxyBankAccountRepo(new DbBankAccountRepo(conn)));
```

---

## Сборка и запуск

```bash 
dotnet build
dotnet run
```

---

## Примечания по качеству и проверке

- Вся бизнес-логика вне UI (GRASP)
- Инварианты при создании - в фабрике (единая точка валидации).
- Время сценариев - автоматически, через декоратор команд.
- Импорт безопасен: валидация структуры и ссылок перед сохранением.
- Экспорт формирует единый JSON-снимок (accounts/categories/operations).