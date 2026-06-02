using AIScaling.PredictiveMiddleware.Core.Storage;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace AIScaling.PredictiveMiddleware.Infrastructure.Redis;

/// <summary>
/// Redis implementation of <see cref="ITelemetryBufferStore"/> using atomic INCRBY + EXPIRE.
/// </summary>
/// <remarks>
/// <para>
/// Designed for high-density Apache JMeter and Grafana k6 bursts: each temporal bucket is a
/// dedicated Redis string key shaped as <c>{KeyPrefix}:{route}:window:{unixSeconds}</c>.
/// <c>INCRBY</c> is atomic cluster-wide, so horizontally scaled API nodes and load generators
/// never lose increments to read-modify-write races.
/// </para>
/// <para>
/// <c>EXPIRE</c> is applied only when the bucket is first created (count transitions to 1),
/// guaranteeing self-cleaning memory without a sweeper—critical when burst scripts create
/// thousands of keys per minute for ML.NET velocity training in later pipeline stages.
/// </para>
/// </remarks>
public sealed class RedisTelemetryBufferStore : ITelemetryBufferStore
{
    /// <summary>
    /// Atomically increments a bucket and sets TTL on first observation (single round-trip).
    /// KEYS[1] = bucket key, ARGV[1] = increment delta, ARGV[2] = expire seconds.
    /// </summary>
    private static readonly LuaScript IncrementWithExpireScript = LuaScript.Prepare(
        """
        local current = redis.call('INCRBY', @redisKey, @delta)
        if current == tonumber(@delta) then
            redis.call('EXPIRE', @redisKey, @ttlSeconds)
        end
        return current
        """);

    private readonly IConnectionMultiplexer _multiplexer;
    private readonly RedisTelemetryOptions _options;

    public RedisTelemetryBufferStore(
        IConnectionMultiplexer multiplexer,
        IOptions<RedisTelemetryOptions> options)
    {
        ArgumentNullException.ThrowIfNull(multiplexer);
        ArgumentNullException.ThrowIfNull(options);
        _multiplexer = multiplexer;
        _options = options.Value;
    }

    /// <inheritdoc />
    public async Task IncrementMetricAsync(
        string key,
        TimeSpan slidingWindowDuration,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var bucketSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var redisKey = BuildWindowKey(key, bucketSeconds);
        var expireSeconds = Math.Max(1, (int)Math.Ceiling(slidingWindowDuration.TotalSeconds));

        var database = _multiplexer.GetDatabase();

        // Lua keeps INCRBY and conditional EXPIRE atomic—no lost TTL if instances race on a new bucket.
        await IncrementWithExpireScript
            .EvaluateAsync(database, new { redisKey, delta = 1, ttlSeconds = expireSeconds })
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<long[]> GetSeriesAsync(
        string key,
        int totalBuckets,
        TimeSpan bucketSize,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        if (totalBuckets <= 0)
        {
            return [];
        }

        var bucketSizeSeconds = Math.Max(1L, (long)bucketSize.TotalSeconds);
        var anchorSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var alignedAnchor = anchorSeconds - (anchorSeconds % bucketSizeSeconds);

        var database = _multiplexer.GetDatabase();
        var batch = database.CreateBatch();
        var pending = new Task<RedisValue>[totalBuckets];

        for (var i = 0; i < totalBuckets; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var bucketIndexFromOldest = totalBuckets - 1 - i;
            var bucketSeconds = alignedAnchor - (bucketIndexFromOldest * bucketSizeSeconds);
            var redisKey = BuildWindowKey(key, bucketSeconds);
            pending[i] = batch.StringGetAsync(redisKey);
        }

        batch.Execute();

        var series = new long[totalBuckets];
        for (var i = 0; i < totalBuckets; i++)
        {
            var value = await pending[i].ConfigureAwait(false);
            series[i] = value.IsNullOrEmpty ? 0L : (long)value;
        }

        return series;
    }

    private string BuildWindowKey(string seriesKey, long timestampSeconds) =>
        $"{_options.KeyPrefix}:{SanitizeSeriesKey(seriesKey)}:window:{timestampSeconds}";

    private static string SanitizeSeriesKey(string key) =>
        key.Trim().Replace(" ", "_", StringComparison.Ordinal);
}
