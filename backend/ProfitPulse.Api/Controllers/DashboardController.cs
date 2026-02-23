using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProfitPulse.Api.Services.Dashboard;

namespace ProfitPulse.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController(DashboardService dashboardService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetDashboard()
    {
        var cafeId = GetCafeId();
        var result = await dashboardService.GetDashboardAsync(cafeId);
        return Ok(result);
    }

    [HttpGet("brief")]
    public async Task<IActionResult> GetWeeklyBrief()
    {
        var cafeId = GetCafeId();
        var brief = await dashboardService.GetLatestBriefAsync(cafeId);
        if (brief is null) return NotFound(new { error = "No weekly brief available yet" });
        return Ok(brief);
    }

    private Guid GetCafeId() =>
        Guid.Parse(User.FindFirstValue("cafeId")!);
}
