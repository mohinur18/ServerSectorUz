using ServerSectorUz.Core.Models.Foundations;

namespace ServerSectorUz.Core.Models.Foundations.Salaries;

public class EmployeeSalary : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Guid SalaryStructureId { get; set; }
    public DateOnly EffectiveFromDate { get; set; }
    public DateOnly? EffectiveToDate { get; set; }
    public decimal Amount { get; set; }
}
