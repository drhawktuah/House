using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace House.House.Extensions;

public static class RandomExtensions
{
    public static long NextPositiveLong(this Random random)
    {
        Span<byte> buffer = stackalloc byte[8];
        random.NextBytes(buffer);

        long value = BitConverter.ToInt64(buffer);

        return value & 0x7FFFFFFFFFFFFFFF;
    }

    public static long NextNegativeLong(this Random random)
    {
        Span<byte> buffer = stackalloc byte[8];
        random.NextBytes(buffer);

        long value = BitConverter.ToInt64(buffer);

        return -(value & 0x7FFFFFFFFFFFFFFF);
    }

    public static long NextLongWithinRange(this Random random, long minimum, long maximum)
    {
        if (minimum >= maximum)
        {
            throw new ArgumentOutOfRangeException(nameof(minimum), $"minimum has a higher value than {nameof(maximum)}");
        }

        ulong range = (ulong) (maximum - minimum);

        Span<byte> buffer = stackalloc byte[8];
        random.NextBytes(buffer);

        ulong randomInt = BitConverter.ToUInt64(buffer);

        return (long)(randomInt % range) + minimum;
    }
}