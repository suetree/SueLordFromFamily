using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SueLordFromFamily
{
    class RandomUtils
    {

       public  static List<int> RandomNumbers(int n, int min, int max, List<int> excludes)
        {
            List<int> numbers = new List<int>();
            long tick = DateTime.Now.Ticks;
            Random ran = new Random((int)(tick & 0xffffffffL) | (int)(tick >> 32));
            while (numbers.Count != n)
            {
                int rk = ran.Next(min, max);
                if (!numbers.Contains(rk) && !excludes.Contains(rk))
                {
                    numbers.Add(rk);
                }
            }
            numbers.Sort();
            return numbers;
        }
    }
}
