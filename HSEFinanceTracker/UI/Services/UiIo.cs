using Spectre.Console;
using TTable = Spectre.Console.Table;

namespace HSEFinanceTracker.UI.Services
{
    public sealed class UiIo
    {
        public string Choose(string title, IEnumerable<string> choices)
        {
            return AnsiConsole.Prompt(new SelectionPrompt<string>().Title($"[green]{title}[/]").AddChoices(choices));
        }

        public void Info(string msg)
        {
            ConsoleManager.WriteMessage(msg);
        }

        public void Warn(string msg)
        {
            ConsoleManager.WriteWarn(msg, false);
        }

        public void Grey(string msg)
        {
            ConsoleManager.WriteColor(msg, "grey");
        }

        public void Clear()
        {
            Console.Clear();
        }

        public string AskNonEmpty(string prompt)
        {
            while (true)
            {
                var s = AnsiConsole.Ask<string>($"[yellow]{prompt}[/]");
                if (!string.IsNullOrWhiteSpace(s))
                {
                    return s.Trim();
                }

                Warn("Значение не может быть пустым");
            }
        }

        public string? AskOptional(string prompt)
        {
            var s = AnsiConsole.Ask<string>($"[yellow]{prompt}[/]");
            return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
        }

        public decimal AskDecimal(string prompt, Func<decimal, bool> ok, string err)
        {
            while (true)
            {
                var s = AnsiConsole.Ask<string>($"[yellow]{prompt}[/]");
                if (decimal.TryParse(s, out var v) && ok(v))
                {
                    return v;
                }

                Warn(err);
            }
        }

        public DateTime AskDate(string prompt)
        {
            while (true)
            {
                var s = AnsiConsole.Ask<string>($"[yellow]{prompt}[/]");
                if (DateTime.TryParse(s, out var d))
                {
                    return d.Date;
                }

                Warn("Неверный формат даты");
            }
        }

        public (DateTime from, DateTime to) AskDateRange()
        {
            while (true)
            {
                var from = AskDate("Начало периода (yyyy-mm-dd):");
                var to = AskDate("Конец периода (yyyy-mm-dd):");
                if (from <= to)
                {
                    return (from, to);
                }

                Warn("Начальная дата должна быть ≤ конечной");
            }
        }

        public bool Confirm(string question)
        {
            return Choose($"{question}", ["Да", "Нет"]) == "Да";
        }

        public TTable TableRounded(params string[] cols)
        {
            var t = new TTable().Border(TableBorder.Rounded);
            foreach (var c in cols)
            {
                t.AddColumn(c);
            }

            return t;
        }
    }
}