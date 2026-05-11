using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerSectorUz.Core.Models.Foundations.Expenses;
using ServerSectorUz.Infrastructure.Services.Foundations.Expenses;
using ServerSectorUz.Models.Foundations.Expenses;

namespace ServerSectorUz.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExpenseClaimsController : ControllerBase
{
    private readonly IExpenseClaimService expenseClaimService;

    public ExpenseClaimsController(IExpenseClaimService expenseClaimService) =>
        this.expenseClaimService = expenseClaimService;

    [HttpPost]
    [Authorize(Roles = "Admin,User")]
    public async ValueTask<ActionResult<ExpenseClaim>> PostExpenseClaimAsync(
        ExpenseClaim expenseClaim) =>
            Ok(await this.expenseClaimService.AddExpenseClaimAsync(expenseClaim));

    [HttpGet("{expenseClaimId}")]
    [Authorize(Roles = "Admin,HR,Accountant,User")]
    public async ValueTask<ActionResult<ExpenseClaim>> GetExpenseClaimByIdAsync(
        Guid expenseClaimId) =>
            Ok(await this.expenseClaimService.RetrieveExpenseClaimByIdAsync(expenseClaimId));

    [HttpGet("employee/{employeeId}")]
    [Authorize(Roles = "Admin,HR,Accountant,User")]
    public ActionResult<IQueryable<ExpenseClaim>> GetExpenseClaimsByEmployeeId(
        Guid employeeId) =>
            Ok(this.expenseClaimService.RetrieveExpenseClaimsByEmployeeId(employeeId));

    [HttpGet("status/{status}")]
    [Authorize(Roles = "Admin,HR,Accountant")]
    public ActionResult<IQueryable<ExpenseClaim>> GetExpenseClaimsByStatus(
        ExpenseClaimStatus status) =>
            Ok(this.expenseClaimService.RetrieveExpenseClaimsByStatus(status));

    [HttpPost("{expenseClaimId}/submit")]
    [Authorize(Roles = "Admin,User")]
    public async ValueTask<ActionResult<ExpenseClaim>> SubmitExpenseClaimAsync(
        Guid expenseClaimId) =>
            Ok(await this.expenseClaimService.SubmitExpenseClaimAsync(expenseClaimId));

    [HttpPost("{expenseClaimId}/approve")]
    [Authorize(Roles = "Admin,Accountant")]
    public async ValueTask<ActionResult<ExpenseClaim>> ApproveExpenseClaimAsync(
        Guid expenseClaimId) =>
            Ok(await this.expenseClaimService.ApproveExpenseClaimAsync(expenseClaimId));

    [HttpPost("{expenseClaimId}/reject")]
    [Authorize(Roles = "Admin,Accountant")]
    public async ValueTask<ActionResult<ExpenseClaim>> RejectExpenseClaimAsync(
        Guid expenseClaimId) =>
            Ok(await this.expenseClaimService.RejectExpenseClaimAsync(expenseClaimId));

    [HttpPost("{expenseClaimId}/reimburse")]
    [Authorize(Roles = "Admin,Accountant")]
    public async ValueTask<ActionResult<ExpenseClaim>> ReimburseExpenseClaimAsync(
        Guid expenseClaimId) =>
            Ok(await this.expenseClaimService.ReimburseExpenseClaimAsync(expenseClaimId));
}