using Microsoft.AspNetCore.Mvc;

namespace StressTest.Api.Controllers;

/// <summary>
/// Test wrapper endpoints for Part 4 posture and mitigation validation.
/// </summary>
[ApiController]
[Route("api/v1")]
public sealed class StressTestController : ControllerBase
{
    /// <summary>
    /// CRITICAL route — must remain available during Alert/Critical unless absolute starvation.
    /// </summary>
    [HttpGet("payment/checkout")]
    [ProducesResponseType(typeof(CheckoutResponse), StatusCodes.Status200OK)]
    public IActionResult Checkout()
    {
        return Ok(new CheckoutResponse(
            TransactionId: Guid.NewGuid().ToString("N"),
            Status: "ready",
            RouteClass: "critical",
            TimestampUtc: DateTimeOffset.UtcNow));
    }

    /// <summary>
    /// NON-CRITICAL route — shed with HTTP 429 when posture is Critical.
    /// </summary>
    [HttpGet("promotions/ads")]
    [ProducesResponseType(typeof(PromotionAdsResponse), StatusCodes.Status200OK)]
    public IActionResult PromotionAds()
    {
        return Ok(new PromotionAdsResponse(
            CampaignId: "spring-burst",
            RouteClass: "non-critical",
            AdsReturned: 3,
            TimestampUtc: DateTimeOffset.UtcNow));
    }
}

public sealed record CheckoutResponse(
    string TransactionId,
    string Status,
    string RouteClass,
    DateTimeOffset TimestampUtc);

public sealed record PromotionAdsResponse(
    string CampaignId,
    string RouteClass,
    int AdsReturned,
    DateTimeOffset TimestampUtc);
