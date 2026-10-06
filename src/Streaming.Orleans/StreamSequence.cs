using System.Numerics;

namespace Continuum.Streaming.Orleans;

[GenerateSerializer, Immutable]
public readonly record struct StreamSequence(BigInteger SequenceNumber, ulong SubSequenceNumber)
{
    public static bool operator >(StreamSequence left, StreamSequence right)
    {
        if (left.SequenceNumber > right.SequenceNumber)
        {
            return true;
        }
        else if (left.SequenceNumber == right.SequenceNumber)
        {
            return left.SubSequenceNumber > right.SubSequenceNumber;
        }
        else
        {
            return false;
        }
    }

    public static bool operator <(StreamSequence left, StreamSequence right)
    {
        if (left.SequenceNumber < right.SequenceNumber)
        {
            return true;
        }
        else if (left.SequenceNumber == right.SequenceNumber)
        {
            return left.SubSequenceNumber < right.SubSequenceNumber;
        }
        else
        {
            return false;
        }
    }

    public static bool operator >=(StreamSequence left, StreamSequence right)
    {
        if (left.SequenceNumber > right.SequenceNumber)
        {
            return true;
        }
        else if (left.SequenceNumber == right.SequenceNumber)
        {
            return left.SubSequenceNumber >= right.SubSequenceNumber;
        }
        else
        {
            return false;
        }
    }

    public static bool operator <=(StreamSequence left, StreamSequence right)
    {
        if (left.SequenceNumber < right.SequenceNumber)
        {
            return true;
        }
        else if (left.SequenceNumber == right.SequenceNumber)
        {
            return left.SubSequenceNumber <= right.SubSequenceNumber;
        }
        else
        {
            return false;
        }
    }
}
