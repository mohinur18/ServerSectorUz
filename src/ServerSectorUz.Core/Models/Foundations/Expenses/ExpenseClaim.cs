using ServerSectorUz.Core.Models.Foundations;
using ServerSectorUz.Models.Foundations.Expenses;
using System.Collections.Generic;

namespace ServerSectorUz.Core.Models.Foundations.Expenses;

public class ExpenseClaim : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public DateOnly ClaimDate { get; set; }
    public ExpenseClaimStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "UZS";
    public ICollection<ExpenseItem> ExpenseItems { get; set; } =
    new List<ExpenseItem>();
}
