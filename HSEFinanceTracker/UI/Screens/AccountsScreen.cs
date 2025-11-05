using HSEFinanceTracker.Application.Facades;
using HSEFinanceTracker.UI.Abstractions;
using HSEFinanceTracker.UI.Services;
using Spectre.Console;

namespace HSEFinanceTracker.UI.Screens
{
    public sealed class AccountsScreen : IMenuScreen
    {
        public string Title => "Счета";

        private readonly BankAccountFacade _accounts;
        private readonly TimedScenario _timed;
        private readonly UiIo _io;

        public AccountsScreen(BankAccountFacade accounts, TimedScenario timed, UiIo io)
        {
            _accounts = accounts;
            _timed = timed;
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

                _timed.Run(cmd, () =>
                {
                    switch (cmd)
                    {
                        case "Создать": Create(); break;
                        case "Список": List(); break;
                        case "Переименовать": Rename(); break;
                        case "Удалить": Delete(); break;
                    }
                });
            }
        }

        private void Create()
        {
            var name = _io.AskNonEmpty("Название счёта:");
            var opening = _io.AskDecimal("Начальный баланс (>= 0):", v => v >= 0, "Введите число >= 0");
            var acc = _accounts.Create(name, opening);
            _io.Info($"Счёт создан: {acc.Name} (#{acc.Id}), баланс: {acc.Balance:0.##}");
        }

        private void List()
        {
            var list = _accounts.All().ToList();
            if (!list.Any())
            {
                _io.Warn("Счетов нет");
                return;
            }

            var t = _io.TableRounded("Id", "Название", "Баланс");
            foreach (var a in list)
            {
                t.AddRow(a.Id.ToString(), a.Name, a.Balance.ToString("0.##"));
            }

            Spectre.Console.AnsiConsole.Write(t);
        }

        private void Rename()
        {
            var acc = PickAccount();
            if (acc is null)
            {
                _io.Warn("Счетов нет");
                return;
            }

            var newName = _io.AskNonEmpty("Новое название:");
            _accounts.Rename(acc.Id, newName);
            _io.Info($"Готово: {newName} (#{acc.Id})");
        }

        private void Delete()
        {
            var acc = PickAccount();
            if (acc is null)
            {
                _io.Warn("Счетов нет");
                return;
            }

            if (!_io.Confirm($"Удалить счёт '{acc.Name}'?"))
            {
                return;
            }

            _accounts.Delete(acc.Id);
            _io.Info("Счёт удалён");
        }

        private Base.Entities.BankAccount? PickAccount()
        {
            var list = _accounts.All().ToList();
            if (!list.Any())
            {
                return null;
            }

            var choice = _io.Choose("Выберите счёт",
                list.Select(a => $"{a.Name} | {a.Balance:0.##} | {a.Id}"));
            var id = choice.Split('|').Last().Trim();
            return list.First(a => a.Id.ToString() == id);
        }
    }
}