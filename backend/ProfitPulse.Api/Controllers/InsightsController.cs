using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProfitPulse.Api.Data.Entities;
using ProfitPulse.Api.Services.AI;

namespace ProfitPulse.Api.Controllers;

[ApiController]
[Route("api/insights")]
[Authorize]
public class InsightsController(InsightService insightService) : ControllerBase
{
    [HttpPost("generate")]
    public async Task<IActionResult> Generate(CancellationToken ct)
    {
        var cafeId = GetCafeId();
        try
        {
            var insights = await insightService.GenerateInsightsAsync(cafeId, ct);
            return Ok(new { count = insights.Count, insights });
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(502, new { error = $"AI provider error: {ex.Message}" });
        }
    }

    [HttpPost("generate-brief")]
    public async Task<IActionResult> GenerateBrief(CancellationToken ct)
    {
        var cafeId = GetCafeId();
        try
        {
            var brief = await insightService.GenerateWeeklyBriefAsync(cafeId, ct);
            if (brief is null) return StatusCode(500, new { error = "Failed to generate brief" });
            return Ok(brief);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(502, new { error = $"AI provider error: {ex.Message}" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetInsights([FromQuery] string? status = null)
    {
        var cafeId = GetCafeId();
        InsightStatus? statusFilter = status is not null && Enum.TryParse<InsightStatus>(status, true, out var s)
            ? s
            : null;
        var insights = await insightService.GetInsightsAsync(cafeId, statusFilter);
        return Ok(insights);
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest request)
    {
        var cafeId = GetCafeId();
        if (!Enum.TryParse<InsightStatus>(request.Status, true, out var newStatus))
            return BadRequest(new { error = "Invalid status" });

        var updated = await insightService.UpdateInsightStatusAsync(id, cafeId, newStatus);
        if (!updated) return NotFound(new { error = "Insight not found" });
        return Ok(new { status = newStatus.ToString() });
    }

    private Guid GetCafeId() =>
        Guid.Parse(User.FindFirstValue("cafeId")!);
}

public record UpdateStatusRequest(string Status);
