namespace ProfitPulse.Api.Services.AI;

public static class ProfitPulsePrompts
{
    public const string SystemPrompt = """
        You are ProfitPulse AI, a profit advisor for New Zealand cafe owners.
        You provide actionable, data-driven insights to improve cafe profitability.

        Context:
        - All prices are in NZD
        - GST is 15% inclusive (tax = total × 3/23)
        - NZ minimum wage is $23.15/hour (2024)
        - Target food cost: 28-35% of revenue
        - Target labour cost: 25-32% of revenue
        - Common NZ cafe suppliers: Bidfood, Service Foods, Gilmours, NZSB (coffee)
        - Peak seasons: Summer (Dec-Feb), school holidays
        - Typical Auckland cafe revenue: $3,000-5,000/day

        Always respond with specific, actionable recommendations.
        Reference NZ-specific suppliers, regulations, and market conditions.
        Include estimated dollar impact where possible.
        """;

    public static string BuildInsightPrompt(string dataContext)
    {
        return $$"""
            Analyze the following cafe performance data and generate profit optimization insights.

            {{dataContext}}

            Respond with a JSON array of insights, each with this structure:
            {
              "title": "Short actionable title",
              "summary": "One-sentence summary of the insight",
              "detailedAnalysis": "2-3 paragraph analysis with specific data points",
              "recommendedAction": "Specific action to take",
              "category": "Menu|Staff|Revenue|Cost|Opportunity|Warning",
              "priority": "Low|Medium|High|Critical",
              "potentialImpact": 500.00
            }

            Generate 3-5 insights. Focus on the biggest profit improvement opportunities.
            Return ONLY the JSON array, no other text.
            """;
    }

    public static string BuildWeeklyBriefPrompt(string dataContext)
    {
        return $$"""
            Generate a weekly business brief for this NZ cafe owner based on the following data.

            {{dataContext}}

            Respond with a JSON object:
            {
              "summary": "2-3 sentence executive summary of the week",
              "highlights": "Bullet points of positive trends (use • for bullets)",
              "concerns": "Bullet points of areas needing attention (use • for bullets)",
              "recommendations": "Bullet points of specific actions for next week (use • for bullets)"
            }

            Be concise, specific, and reference actual numbers from the data.
            Return ONLY the JSON object, no other text.
            """;
    }
}
