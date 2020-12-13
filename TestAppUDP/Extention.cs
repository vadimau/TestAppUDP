using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestAppUDP
{
public static class Extensions
{
        /// <summary>
        /// Расчет среднего отклонения
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
    public static double StdDev(this IQueryable<int> values) 
    {
       double ret = 0;
       int count = values.Count();
       if (count  > 1)
       {
          double avg = values.Average();

          double sum = values.Sum(d => (d - avg) * (d - avg));

          ret = Math.Sqrt(sum / count);
       }
       return ret;
    }

        /// <summary>
        /// Расчет моды
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static double Mode(this IQueryable<int> values)
        {
            Dictionary<int, int> dict = new Dictionary<int, int>();

            foreach (int elem in values)
            {
                if (dict.ContainsKey(elem))
                    dict[elem]++;
                else
                    dict[elem] = 1;
            }

            int maxCount = 0;
            double mode = Double.NaN;
            foreach (int elem in dict.Keys)
            {
                if (dict[elem] > maxCount)
                {
                    maxCount = dict[elem];
                    mode = elem;
                }
            }
            
            return mode;
        }

        /// <summary>
        /// Расчет медианы
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static double Median(this IQueryable<int> values)
        {
            int[] sortedPNumbers = values.ToArray();
            Array.Sort(sortedPNumbers);

            int size = sortedPNumbers.Length;
            int mid = size / 2;
            long median = (size % 2 != 0) ? (long)sortedPNumbers[mid] : ((long)sortedPNumbers[mid] + (long)sortedPNumbers[mid - 1]) / 2;
            return (int)median;
        }
    }
}
