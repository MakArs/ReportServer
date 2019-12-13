using System;

namespace ReportService.Entities
{
    public class ThreadSafeRandom
    {
        private static readonly Random Global = new Random();
        [ThreadStatic] private static Random _local;

        private void InitializeLocal()
        {
            if (_local == null)
            {
                lock (Global)
                {
                    if (_local == null)
                    {
                        int seed = Global.Next();
                        _local = new Random(seed);
                    }
                }
            }
        }

        public int Next()
        {
            InitializeLocal();
            return _local.Next();
        }

        public int Next(int minValue, int maxValue)
        {
            InitializeLocal();

            return _local.Next(minValue, maxValue);
        }
    }
}