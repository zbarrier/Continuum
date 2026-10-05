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
        var errorCode = (int)(throwException ? ErrorCode.LogConsistency_ProtocolFatalError : ErrorCode.LogConsistency_ProtocolError);
        _logger.LogError(errorCode, "{GrainId} Protocol Error: {Message}", _grainContext.GrainId, msg);
        if (throwException)
        {
            throw new OrleansException($"{msg} (grain={_grainContext.GrainId}, cluster={MyClusterId})");
        }
    }

    /// <inheritdoc />
    public void CaughtException(string where, Exception ex)
    {
        const int errorCode = (int)ErrorCode.LogConsistency_CaughtException;
        _logger.LogError(errorCode, ex, "{GrainId} exception caught at {Location}", _grainContext.GrainId, where);
    }

    /// <inheritdoc />
    public void CaughtUserCodeException(string callback, string where, Exception ex)
    {
        const int errorCode = (int)ErrorCode.LogConsistency_UserCodeException;
        _logger.LogWarning(errorCode, ex, "{GrainId} exception caught in user code for {Callback}, called from {Location}", _grainContext.GrainId, callback, where);
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
