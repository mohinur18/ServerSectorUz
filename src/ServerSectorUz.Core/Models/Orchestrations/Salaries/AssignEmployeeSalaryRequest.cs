namespace ServerSectorUz.Core.Models.Orchestrations.Salaries;

public class AssignEmployeeSalaryRequest
{
    public Guid EmployeeId { get; set; }
    public Guid SalaryStructureId { get; set; }
    public DateOnly EffectiveFromDate { get; set; }
    public DateOnly? EffectiveToDate { get; set; }
    public decimal Amount { get; set; }
}
