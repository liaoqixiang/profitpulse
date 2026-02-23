using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProfitPulse.Api.Services.Analysis;

namespace ProfitPulse.Api.Controllers;

[ApiController]
[Route("api/staff")]
[Authorize]
public class StaffController(StaffCostService staffService) : ControllerBase
{
    [HttpGet("costs")]
    public async Task<IActionResult> GetCosts([FromQuery] int days = 7)
    {
        var cafeId = Guid.Parse(User.FindFirstValue("cafeId")!);
        var result = await staffService.GetCostsAsync(cafeId, days);
        return Ok(result);
    }
}
