using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluExp_01
{
    public class TSRandom
    {
        private static Random random = new Random();
        private static object RandomLock = new object();

        public static Random NextRandom()
        {
            Random r;
            lock (RandomLock)
                r = new Random(random.Next());
            return r;
        }

        public static int Next() => NextRandom().Next();
        public static int Next(int maxValue) => NextRandom().Next(maxValue);
        public static int Next(int minValue, int maxValue) => NextRandom().Next(minValue, maxValue);

        public static double NextDouble() => NextRandom().NextDouble();
        public static double DoubleBetween(double min, double max) => NextRandom().DoubleBetween(min, max);
    }
}
