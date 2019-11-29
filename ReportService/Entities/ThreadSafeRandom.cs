using System;

public class ThreadSafeRandom
{
    private static readonly Random global = new Random();
    [ThreadStatic] private static Random local;

    public int Next()
    {
        if (local == null)
        {
            lock (global)
            {
                if (local == null)
                {
                    int seed = global.Next();
                    local = new Random(seed);
                }
            }
        }

        return local.Next();
    }

    public int Next(int minValue, int maxValue)
    {
        if (local == null)
        {
            lock (global)
            {
                if (local == null)
                {
                    int seed = global.Next();
                    local = new Random(seed);
                }
            }
        }

        return local.Next(minValue, maxValue);
    }
}