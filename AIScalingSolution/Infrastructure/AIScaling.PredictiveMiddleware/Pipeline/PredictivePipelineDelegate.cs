namespace AIScaling.PredictiveMiddleware.Pipeline;

/// <summary>
/// Continuation delegate for the predictive HTTP middleware chain.
/// </summary>
/// <param name="context">The current HTTP context.</param>
/// <param name="cancellationToken">Request abort token.</param>
public delegate ValueTask PredictivePipelineDelegate(
    HttpContext context,
    CancellationToken cancellationToken);
