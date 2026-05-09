using ServerSectorUz.Core.Models.Foundations;

namespace ServerSectorUz.Core.Models.Foundations.Expenses;

public class ExpenseClaim : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public DateOnly ClaimDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "UZS";
}
