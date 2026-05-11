using ServerSectorUz.Core.Brokers.DateTimes;
using ServerSectorUz.Core.Brokers.Loggings;
using ServerSectorUz.Core.Brokers.Storages;
using ServerSectorUz.Core.Models.Foundations.Expenses;
using ServerSectorUz.Models.Foundations.Expenses;

namespace ServerSectorUz.Infrastructure.Services.Foundations.Expenses;

public class ExpenseClaimService : IExpenseClaimService
{
    private readonly IStorageBroker storageBroker;
    private readonly IDateTimeBroker dateTimeBroker;
    private readonly ILoggingBroker loggingBroker;

    public ExpenseClaimService(
        IStorageBroker storageBroker,
        IDateTimeBroker dateTimeBroker,
        ILoggingBroker loggingBroker)
    {
        this.storageBroker = storageBroker;
        this.dateTimeBroker = dateTimeBroker;
        this.loggingBroker = loggingBroker;
    }

    public async ValueTask<ExpenseClaim> AddExpenseClaimAsync(
        ExpenseClaim expenseClaim)
    {
        expenseClaim.Id = Guid.NewGuid();
        expenseClaim.CreatedDate =
            this.dateTimeBroker.GetCurrentDateTimeOffset();

        expenseClaim.UpdatedDate =
            this.dateTimeBroker.GetCurrentDateTimeOffset();

        expenseClaim.Status = ExpenseClaimStatus.Draft;

        return await this.storageBroker.InsertExpenseClaimAsync(
            expenseClaim);
    }

    public IQueryable<ExpenseClaim> RetrieveAllExpenseClaims() =>
        this.storageBroker.SelectAllExpenseClaims();

    public async ValueTask<ExpenseClaim> RetrieveExpenseClaimByIdAsync(
        Guid expenseClaimId) =>
            await this.storageBroker.SelectExpenseClaimByIdAsync(
                expenseClaimId);

    public IQueryable<ExpenseClaim> RetrieveExpenseClaimsByEmployeeId(
        Guid employeeId) =>
            this.storageBroker
                .SelectAllExpenseClaims()
                .Where(expenseClaim =>
                    expenseClaim.EmployeeId == employeeId);

    public IQueryable<ExpenseClaim> RetrieveExpenseClaimsByStatus(
        ExpenseClaimStatus status) =>
            this.storageBroker
                .SelectAllExpenseClaims()
                .Where(expenseClaim =>
                    expenseClaim.Status == status);

    public async ValueTask<ExpenseClaim> SubmitExpenseClaimAsync(
        Guid expenseClaimId)
    {
        ExpenseClaim expenseClaim =
            await this.storageBroker.SelectExpenseClaimByIdAsync(
                expenseClaimId);

        expenseClaim.Status = ExpenseClaimStatus.Submitted;
        expenseClaim.UpdatedDate =
            this.dateTimeBroker.GetCurrentDateTimeOffset();

        return await this.storageBroker.UpdateExpenseClaimAsync(
            expenseClaim);
    }

    public async ValueTask<ExpenseClaim> ApproveExpenseClaimAsync(
        Guid expenseClaimId)
    {
        ExpenseClaim expenseClaim =
            await this.storageBroker.SelectExpenseClaimByIdAsync(
                expenseClaimId);

        expenseClaim.Status = ExpenseClaimStatus.Approved;
        expenseClaim.UpdatedDate =
            this.dateTimeBroker.GetCurrentDateTimeOffset();

        return await this.storageBroker.UpdateExpenseClaimAsync(
            expenseClaim);
    }

    public async ValueTask<ExpenseClaim> RejectExpenseClaimAsync(
        Guid expenseClaimId)
    {
        ExpenseClaim expenseClaim =
            await this.storageBroker.SelectExpenseClaimByIdAsync(
                expenseClaimId);

        expenseClaim.Status = ExpenseClaimStatus.Rejected;
        expenseClaim.UpdatedDate =
            this.dateTimeBroker.GetCurrentDateTimeOffset();

        return await this.storageBroker.UpdateExpenseClaimAsync(
            expenseClaim);
    }

    public async ValueTask<ExpenseClaim> ReimburseExpenseClaimAsync(
        Guid expenseClaimId)
    {
        ExpenseClaim expenseClaim =
            await this.storageBroker.SelectExpenseClaimByIdAsync(
                expenseClaimId);

        expenseClaim.Status = ExpenseClaimStatus.Reimbursed;
        expenseClaim.UpdatedDate =
            this.dateTimeBroker.GetCurrentDateTimeOffset();

        return await this.storageBroker.UpdateExpenseClaimAsync(
            expenseClaim);
    }

    private static void ValidateExpenseClaim(ExpenseClaim expenseClaim)
    {
        if (expenseClaim is null)
        {
            throw new ArgumentNullException(nameof(expenseClaim));
        }

        if (expenseClaim.EmployeeId == Guid.Empty)
        {
            throw new ArgumentException(
                "EmployeeId is required.");
        }
    }

    private static void ValidateExpenseClaimForSubmit(
        ExpenseClaim expenseClaim)
    {
        if (expenseClaim.ExpenseItems is null ||
            expenseClaim.ExpenseItems.Any() is false)
        {
            throw new ArgumentException(
                "Expense claim must have at least one item.");
        }
    }

    private static void ValidateApprovalStatus(
        ExpenseClaim expenseClaim)
    {
        if (expenseClaim.Status != ExpenseClaimStatus.Submitted)
        {
            throw new InvalidOperationException(
                "Only submitted claims can be approved or rejected.");
        }
    }

    private static void ValidateReimburseStatus(
        ExpenseClaim expenseClaim)
    {
        if (expenseClaim.Status != ExpenseClaimStatus.Approved)
        {
            throw new InvalidOperationException(
                "Only approved claims can be reimbursed.");
        }
    }
}