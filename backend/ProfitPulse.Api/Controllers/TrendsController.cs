using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProfitPulse.Api.Services.Analysis;

namespace ProfitPulse.Api.Controllers;

[ApiController]
[Route("api/trends")]
[Authorize]
public class TrendsController(TrendsService trendsService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetTrends([FromQuery] int days = 30)
    {
        var cafeId = Guid.Parse(User.FindFirstValue("cafeId")!);
        var result = await trendsService.GetTrendsAsync(cafeId, days);
        return Ok(result);
    }
}
