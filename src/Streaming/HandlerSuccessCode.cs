namespace Continuum.Streaming.Orleans;

[Flags]
public enum HandlerSuccessCode
{
    Ignored = 0,
    Applied = 1,
}
