using ServerSectorUz.Core.Brokers.DateTimes;
using ServerSectorUz.Core.Brokers.Loggings;
using ServerSectorUz.Core.Brokers.Storages;
using ServerSectorUz.Core.Models.Foundations.Expenses;

namespace ServerSectorUz.Infrastructure.Services.Foundations.Expenses;

public class ExpenseItemService : IExpenseItemService
{
    private readonly IStorageBroker storageBroker;
    private readonly IDateTimeBroker dateTimeBroker;
    private readonly ILoggingBroker loggingBroker;

    public ExpenseItemService(
        IStorageBroker storageBroker,
        IDateTimeBroker dateTimeBroker,
        ILoggingBroker loggingBroker)
    {
        this.storageBroker = storageBroker;
        this.dateTimeBroker = dateTimeBroker;
        this.loggingBroker = loggingBroker;
    }

    public async ValueTask<ExpenseItem> AddExpenseItemAsync(
        ExpenseItem expenseItem)
    {
        expenseItem.Id = Guid.NewGuid();

        return await this.storageBroker.InsertExpenseItemAsync(
            expenseItem);
    }

    public IQueryable<ExpenseItem> RetrieveAllExpenseItems() =>
        this.storageBroker.SelectAllExpenseItems();

    public async ValueTask<ExpenseItem> RetrieveExpenseItemByIdAsync(
        Guid expenseItemId) =>
            await this.storageBroker.SelectExpenseItemByIdAsync(
                expenseItemId);

    public IQueryable<ExpenseItem> RetrieveExpenseItemsByClaimId(
        Guid expenseClaimId) =>
            this.storageBroker
                .SelectAllExpenseItems()
                .Where(expenseItem =>
                    expenseItem.ExpenseClaimId == expenseClaimId);
}