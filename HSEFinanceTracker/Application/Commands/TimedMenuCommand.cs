using System.Diagnostics;

namespace HSEFinanceTracker.Application.Commands
{
    public sealed class TimedMenuCommand(ICommand inner, Action<string> report) : ICommand
    {
        public string Name => inner.Name;

        public void Execute()
        {
            var sw = Stopwatch.StartNew();
            try
            {
                inner.Execute();
            }
            finally
            {
                sw.Stop();
                report($"⏱ Время сценария «{Name}»: {sw.Elapsed.TotalMilliseconds:0.##} ms");
            }
        }
    }
}