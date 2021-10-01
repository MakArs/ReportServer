using System;

namespace ReportService.Entities
{
    public class ThreadSafeRandom
    {
        private static readonly Random sGlobal = new Random();

        [ThreadStatic] 
        private static Random sLocal;

        private void InitializeLocal()
        {
            if (sLocal == null)
            {
                lock (sGlobal)
                {
                    if (sLocal == null)
                    {
                        int seed = sGlobal.Next();
                        sLocal = new Random(seed);
                    }
                }
            }
        }

        public int Next()
        {
            InitializeLocal();
            return sLocal.Next();
        }

        public int Next(int minValue, int maxValue)
        {
            InitializeLocal();

            return sLocal.Next(minValue, maxValue);
        }
    }
}