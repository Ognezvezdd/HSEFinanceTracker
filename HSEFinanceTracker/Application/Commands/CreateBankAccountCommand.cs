using HSEFinanceTracker.Application.Facades;

namespace HSEFinanceTracker.Application.Commands
{
    public sealed class CreateBankAccountCommand : ICommand
    {
        private readonly BankAccountFacade _facade;
        private readonly string _name;
        private readonly decimal _opening;

        public CreateBankAccountCommand(BankAccountFacade facade, string name, decimal opening = 0m)
        {
            _facade = facade;
            _name = name;
            _opening = opening;
        }

        public void Execute()
        {
            _facade.Create(_name, _opening);
        }
    }
}