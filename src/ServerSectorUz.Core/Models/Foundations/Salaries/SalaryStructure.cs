using ServerSectorUz.Core.Models.Foundations;

namespace ServerSectorUz.Core.Models.Foundations.Salaries;

public class SalaryStructure : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal BaseAmount { get; set; }
    public string Currency { get; set; } = "UZS";
}
