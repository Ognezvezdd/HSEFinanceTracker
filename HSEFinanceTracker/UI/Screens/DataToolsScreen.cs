using HSEFinanceTracker.Application.Facades;
using HSEFinanceTracker.UI.Abstractions;
using HSEFinanceTracker.UI.Services;

namespace HSEFinanceTracker.UI.Screens
{
    // Для независимости от конкретного класса пересчёта допускаем object? + dynamic
    public sealed class DataToolsScreen : IMenuScreen
    {
        public string Title => "Инструменты данных";

        private readonly object? _recalcFacade; // RecalcFacade (если зарегистрирован)
        private readonly BankAccountFacade _accounts;
        private readonly TimedScenario _timed;
        private readonly UiIo _io;

        public DataToolsScreen(BankAccountFacade accounts, TimedScenario timed, UiIo io, object? recalcFacade = null)
        {
            _accounts = accounts;
            _timed = timed;
            _io = io;
            _recalcFacade = recalcFacade;
        }

        public void Show()
        {
            while (true)
            {
                _io.Clear();
                var cmd = _io.Choose(Title,
                    new[] { "Проверить баланс счёта", "Пересчитать баланс счёта", "Пересчитать все счета", "Назад" });
                if (cmd == "Назад")
                {
                    return;
                }

                _timed.Run(cmd, () =>
                {
                    if (_recalcFacade == null)
                    {
                        _io.Warn("Модуль пересчёта не подключён.");
                        return;
                    }

                    dynamic r = _recalcFacade;

                    switch (cmd)
                    {
                        case "Проверить баланс счёта":
                            if (PickAccount() is { } acc1)
                            {
                                decimal diff = r.VerifyAccount(acc1.Id);
                                _io.Info($"Расхождение: {diff:0.##}");
                            }

                            break;
                        case "Пересчитать баланс счёта":
                            if (PickAccount() is { } acc2)
                            {
                                r.RecalculateAccount(acc2.Id);
                                _io.Info("Готово");
                            }

                            break;
                        case "Пересчитать все счета":
                            r.RecalculateAll();
                            _io.Info("Готово");
                            break;
                    }
                });
            }
        }

        private Base.Entities.BankAccount? PickAccount()
        {
            var list = _accounts.All().ToList();
            if (!list.Any())
            {
                _io.Warn("Счетов нет");
                return null;
            }

            var choice = _io.Choose("Выберите счёт", list.Select(a => $"{a.Name} | {a.Balance:0.##} | {a.Id}"));
            var id = choice.Split('|').Last().Trim();
            return list.First(a => a.Id.ToString() == id);
        }
    }
}