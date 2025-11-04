using System.Diagnostics;

namespace HSEFinanceTracker.Application.Commands.Decorators
{
    public sealed class TimedCommandDecorator : ICommand
    {
        private readonly ICommand _inner;
        private readonly Action<TimeSpan>? _onDone;

        public TimedCommandDecorator(ICommand inner, Action<TimeSpan>? onDone = null)
        {
            _inner = inner;
            _onDone = onDone;
        }

        public void Execute()
        {
            var sw = Stopwatch.StartNew();
            _inner.Execute();
            sw.Stop();
            _onDone?.Invoke(sw.Elapsed);
        }
    }
}