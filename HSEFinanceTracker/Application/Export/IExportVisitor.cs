using HSEFinanceTracker.Base.Entities;

namespace HSEFinanceTracker.Application.Export
{
    public interface IExportVisitor
    {
        void Visit(BankAccount account);
        void Visit(Category category);
        void Visit(Operation operation);
    }
}