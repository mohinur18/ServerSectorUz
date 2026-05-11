using ServerSectorUz.Core.Models.Foundations.Expenses;
using ServerSectorUz.Models.Foundations.Expenses;

namespace ServerSectorUz.Infrastructure.Services.Foundations.Expenses;

public interface IExpenseClaimService
{
    ValueTask<ExpenseClaim> AddExpenseClaimAsync(ExpenseClaim expenseClaim);

    IQueryable<ExpenseClaim> RetrieveAllExpenseClaims();

    ValueTask<ExpenseClaim> RetrieveExpenseClaimByIdAsync(Guid expenseClaimId);

    ValueTask<ExpenseClaim> SubmitExpenseClaimAsync(Guid expenseClaimId);

    ValueTask<ExpenseClaim> ApproveExpenseClaimAsync(Guid expenseClaimId);

    ValueTask<ExpenseClaim> RejectExpenseClaimAsync(Guid expenseClaimId);

    ValueTask<ExpenseClaim> ReimburseExpenseClaimAsync(Guid expenseClaimId);

    IQueryable<ExpenseClaim> RetrieveExpenseClaimsByEmployeeId(Guid employeeId);

    IQueryable<ExpenseClaim> RetrieveExpenseClaimsByStatus(ExpenseClaimStatus status);
}