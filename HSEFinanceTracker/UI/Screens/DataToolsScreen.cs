using HSEFinanceTracker.Application.Facades;
using HSEFinanceTracker.UI.Abstractions;
using HSEFinanceTracker.UI.Services;

namespace HSEFinanceTracker.UI.Screens
{
    public sealed class DataToolsScreen(BankAccountFacade accounts, UiIo io, RecalcFacade recalc) : IMenuScreen
    {
        public string Title => "Инструменты данных";

        public void Show()
        {
            while (true)
            {
                io.Clear();
                var cmd = io.Choose(Title,
                    ["Проверить баланс счёта", "Пересчитать баланс счёта", "Пересчитать все счета", "Назад"]);
                if (cmd == "Назад")
                {
                    return;
                }

                switch (cmd)
                {
                    case "Проверить баланс счёта":
                        if (PickAccount() is { } acc1)
                        {
                            var diff = recalc.VerifyAccount(acc1.Id);
                            io.Info($"Расхождение: {diff:0.##}");
                        }

                        break;
                    case "Пересчитать баланс счёта":
                        if (PickAccount() is { } acc2)
                        {
                            recalc.RecalculateAccount(acc2.Id);
                            io.Info("Готово");
                        }

                        break;
                    case "Пересчитать все счета":
                        recalc.RecalculateAll();
                        io.Info("Готово");
                        break;
                }

                io.ReadKey();
            }
        }

        private Base.Entities.BankAccount? PickAccount()
        {
            var list = accounts.All().ToList();
            if (!list.Any())
            {
                io.Warn("Счетов нет");
                return null;
            }

            var choice = io.Choose("Выберите счёт", list.Select(a => $"{a.Name} | {a.Balance:0.##} | {a.Id}"));
            var id = choice.Split('|').Last().Trim();
            return list.First(a => a.Id.ToString() == id);
        }
    }
}