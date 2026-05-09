using ServerSectorUz.Core.Models.Foundations;

namespace ServerSectorUz.Core.Models.Foundations.Expenses;

public class ExpenseItem : BaseEntity
{
    public Guid ExpenseClaimId { get; set; }
    public DateOnly ExpenseDate { get; set; }
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
}
