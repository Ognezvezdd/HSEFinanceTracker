namespace HSEFinanceTracker.UI.Services
{
    public sealed class TimedScenario(UiIo io)
    {
        public void Run(string name, Action action)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            try { action(); }
            finally
            {
                sw.Stop();
                io.Grey($"[{name}] заняло {sw.ElapsedMilliseconds} мс");
                io.Grey("Нажмите любую клавишу...");
                Console.ReadKey(true);
            }
        }
    }
}