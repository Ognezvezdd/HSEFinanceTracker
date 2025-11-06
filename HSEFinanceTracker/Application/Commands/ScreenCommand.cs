using HSEFinanceTracker.UI.Services;

namespace HSEFinanceTracker.Application.Commands
{
    /// Адаптер: «пункт меню → вызов экрана»
    public sealed class ScreenCommand : ICommand
    {
        public string Name { get; }
        private readonly Action _runScreen;
        private readonly UiIo _io;

        public ScreenCommand(string name, Action runScreen, UiIo io)
        {
            Name = name;
            _runScreen = runScreen;
            _io = io;
        }

        public void Execute()
        {
            _runScreen();
        }
    }
}