using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerSectorUz.Core.Exceptions.Services;
using ServerSectorUz.Core.Exceptions.Validations;
using ServerSectorUz.Core.Models.Constants;
using ServerSectorUz.Core.Models.Foundations.Salaries;
using ServerSectorUz.Core.Services.Foundations.Salaries;

namespace ServerSectorUz.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SalaryStructuresController : ControllerBase
{
    private readonly ISalaryStructureService salaryStructureService;

    public SalaryStructuresController(ISalaryStructureService salaryStructureService) =>
        this.salaryStructureService = salaryStructureService;

    [HttpGet]
    [Authorize(Roles = SystemRoles.Admin + "," + SystemRoles.Accountant + "," + SystemRoles.Hr)]
    public ActionResult<IQueryable<SalaryStructure>> GetAllSalaryStructures() =>
        Ok(this.salaryStructureService.RetrieveAllSalaryStructures());

    [HttpGet("{salaryStructureId:guid}")]
    [Authorize(Roles = SystemRoles.Admin + "," + SystemRoles.Accountant + "," + SystemRoles.Hr)]
    public async ValueTask<ActionResult<SalaryStructure>> GetSalaryStructureByIdAsync(Guid salaryStructureId)
    {
        try
        {
            SalaryStructure? structure = await this.salaryStructureService.RetrieveSalaryStructureByIdAsync(salaryStructureId);
            return structure is null ? NotFound() : Ok(structure);
        }
        catch (SalaryValidationException exception)
        {
            return BadRequest(new { Error = exception.Message });
        }
        catch (SalaryServiceException exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = exception.Message });
        }
    }

    [HttpPost]
    [Authorize(Roles = SystemRoles.Admin + "," + SystemRoles.Accountant)]
    public async ValueTask<ActionResult<SalaryStructure>> CreateSalaryStructureAsync(SalaryStructure salaryStructure)
    {
        try
        {
            salaryStructure.CreatedByUserId = ResolveAuthenticatedUserId();

            SalaryStructure created = await this.salaryStructureService.AddSalaryStructureAsync(salaryStructure);

            return CreatedAtAction(nameof(GetSalaryStructureByIdAsync),
                new { salaryStructureId = created.Id },
                created);
        }
        catch (SalaryValidationException exception)
        {
            return BadRequest(new { Error = exception.Message });
        }
        catch (SalaryServiceException exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = exception.Message });
        }
    }

    [HttpPut("{salaryStructureId:guid}")]
    [Authorize(Roles = SystemRoles.Admin + "," + SystemRoles.Accountant)]
    public async ValueTask<ActionResult<SalaryStructure>> UpdateSalaryStructureAsync(
        Guid salaryStructureId,
        SalaryStructure salaryStructure)
    {
        try
        {
            salaryStructure.Id = salaryStructureId;
            salaryStructure.UpdatedByUserId = ResolveAuthenticatedUserId();

            SalaryStructure updated = await this.salaryStructureService.ModifySalaryStructureAsync(salaryStructure);
            return Ok(updated);
        }
        catch (SalaryValidationException exception)
        {
            return BadRequest(new { Error = exception.Message });
        }
        catch (SalaryServiceException exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = exception.Message });
        }
    }

    [HttpDelete("{salaryStructureId:guid}")]
    [Authorize(Roles = SystemRoles.Admin)]
    public async ValueTask<ActionResult<SalaryStructure>> DeleteSalaryStructureAsync(Guid salaryStructureId)
    {
        try
        {
            SalaryStructure removed = await this.salaryStructureService.RemoveSalaryStructureAsync(salaryStructureId);
            return Ok(removed);
        }
        catch (SalaryValidationException exception)
        {
            return BadRequest(new { Error = exception.Message });
        }
        catch (SalaryServiceException exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = exception.Message });
        }
    }

    private Guid? ResolveAuthenticatedUserId()
    {
        string? userIdValue = User.Claims.FirstOrDefault(claim => claim.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdValue, out Guid userId) || userId == Guid.Empty)
        {
            return null;
        }

        return userId;
    }
}
