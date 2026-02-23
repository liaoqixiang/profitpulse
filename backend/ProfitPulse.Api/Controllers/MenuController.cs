using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProfitPulse.Api.Services.Analysis;

namespace ProfitPulse.Api.Controllers;

[ApiController]
[Route("api/menu")]
[Authorize]
public class MenuController(MenuAnalysisService menuService) : ControllerBase
{
    [HttpGet("performance")]
    public async Task<IActionResult> GetPerformance([FromQuery] int days = 7)
    {
        var cafeId = Guid.Parse(User.FindFirstValue("cafeId")!);
        var result = await menuService.GetPerformanceAsync(cafeId, days);
        return Ok(result);
    }
}
