using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerSectorUz.Core.Models.Foundations.Expenses;
using ServerSectorUz.Infrastructure.Services.Foundations.Expenses;

namespace ServerSectorUz.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExpenseItemsController : ControllerBase
{
    private readonly IExpenseItemService expenseItemService;

    public ExpenseItemsController(IExpenseItemService expenseItemService) =>
        this.expenseItemService = expenseItemService;

    [HttpPost]
    [Authorize(Roles = "Admin,User")]
    public async ValueTask<ActionResult<ExpenseItem>> PostExpenseItemAsync(
        ExpenseItem expenseItem) =>
            Ok(await this.expenseItemService.AddExpenseItemAsync(expenseItem));

    [HttpGet("{expenseItemId}")]
    [Authorize(Roles = "Admin,HR,Accountant,User")]
    public async ValueTask<ActionResult<ExpenseItem>> GetExpenseItemByIdAsync(
        Guid expenseItemId) =>
            Ok(await this.expenseItemService.RetrieveExpenseItemByIdAsync(expenseItemId));

    [HttpGet("claim/{expenseClaimId}")]
    [Authorize(Roles = "Admin,HR,Accountant,User")]
    public ActionResult<IQueryable<ExpenseItem>> GetExpenseItemsByClaimId(
        Guid expenseClaimId) =>
            Ok(this.expenseItemService.RetrieveExpenseItemsByClaimId(expenseClaimId));
}