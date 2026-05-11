using ServerSectorUz.Core.Models.Foundations.Expenses;

namespace ServerSectorUz.Infrastructure.Services.Foundations.Expenses;

public interface IExpenseItemService
{
    ValueTask<ExpenseItem> AddExpenseItemAsync(ExpenseItem expenseItem);

    IQueryable<ExpenseItem> RetrieveAllExpenseItems();

    ValueTask<ExpenseItem> RetrieveExpenseItemByIdAsync(Guid expenseItemId);

    IQueryable<ExpenseItem> RetrieveExpenseItemsByClaimId(Guid expenseClaimId);
}