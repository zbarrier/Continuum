using Microsoft.Extensions.Logging;

using Orleans.EventSourcing;
using Orleans.Serialization;

namespace Continuum.EventSourcing.Orleans;

/// <summary>
///     Functionality for use by log view adaptors that run distributed protocols.
///     This class allows access to these services to providers that cannot see runtime-internals.
///     It also stores grain-specific information like the grain reference, and caches.
/// </summary>
internal class DefaultProtocolServices : ILogConsistencyProtocolServices
{
    private readonly ILogger _logger;
    private readonly DeepCopier _deepCopier;
    private readonly IGrainContext _grainContext; // links to the grain that owns this service object

    /// <summary>
    /// </summary>
    /// <param name="grainContext"></param>
    /// <param name="loggerFactory"></param>
    /// <param name="deepCopier"></param>
    /// <param name="siloDetails"></param>
    public DefaultProtocolServices(IGrainContext grainContext, ILoggerFactory loggerFactory, DeepCopier deepCopier,
        ILocalSiloDetails siloDetails)
    {
        _grainContext = grainContext;
        _logger = loggerFactory.CreateLogger<DefaultProtocolServices>();
        _deepCopier = deepCopier;

        MyClusterId = siloDetails.ClusterId;
    }

    /// <inheritdoc />
    public GrainId GrainId => _grainContext.GrainId;

    /// <inheritdoc />
    public string MyClusterId { get; }

    /// <inheritdoc />
    public T DeepCopy<T>(T value) => _deepCopier.Copy(value)!;

    /// <inheritdoc />
    public void ProtocolError(string msg, bool throwException)
    {
        if (throwException)
        {
            _logger.ProtocolFatalError(_grainContext.GrainId, msg);
            throw new OrleansException($"{msg} (grain={_grainContext.GrainId}, cluster={MyClusterId})");
        }
        _logger.ProtocolError(_grainContext.GrainId, msg);
    }

    /// <inheritdoc />
    public void CaughtException(string where, Exception ex)
    {
        _logger.CaughtException(ex, _grainContext.GrainId, where);
    }

    /// <inheritdoc />
    public void CaughtUserCodeException(string callback, string where, Exception ex)
    {
        _logger.CaughtUserCodeException(ex, _grainContext.GrainId, callback, where);
    }

    /// <inheritdoc />
    public void Log(LogLevel level, string format, params object[] args)
    {
        if (_logger.IsEnabled(level))
        {
            var msg = $"{_grainContext.GrainId} {string.Format(format, args)}";
            _logger.Log(level, 0, msg, null, (m, _) => $"{m}");
        }
    }
}

internal static partial class DefaultProtocolServicesLog
{
    [LoggerMessage(EventId = (int)ErrorCode.LogConsistency_ProtocolError, Level = LogLevel.Error, Message = "{GrainId} Protocol Error: {Message}")]
    public static partial void ProtocolError(this ILogger logger, GrainId grainId, string message);

    [LoggerMessage(EventId = (int)ErrorCode.LogConsistency_ProtocolFatalError, Level = LogLevel.Error, Message = "{GrainId} Protocol Error: {Message}")]
    public static partial void ProtocolFatalError(this ILogger logger, GrainId grainId, string message);

    [LoggerMessage(EventId = (int)ErrorCode.LogConsistency_CaughtException, Level = LogLevel.Error, Message = "{GrainId} exception caught at {Location}")]
    public static partial void CaughtException(this ILogger logger, Exception exception, GrainId grainId, string location);

    [LoggerMessage(EventId = (int)ErrorCode.LogConsistency_UserCodeException, Level = LogLevel.Warning, Message = "{GrainId} exception caught in user code for {Callback}, called from {Location}")]
    public static partial void CaughtUserCodeException(this ILogger logger, Exception exception, GrainId grainId, string callback, string location);
}
