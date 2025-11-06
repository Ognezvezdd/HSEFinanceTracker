namespace HSEFinanceTracker.Application.Commands
{
    public interface ICommand
    {
        string Name { get; }
        void Execute();
    }
}