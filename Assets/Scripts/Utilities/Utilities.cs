using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Utility
{
    public static class Utilities
    {
        public static float? NullableMin(float? a, float? b)
        {
            if (!a.HasValue && !b.HasValue)
            {
                return null;
            }

            if (a.HasValue && !b.HasValue)
            {
                return a;
            }

            if (!a.HasValue && b.HasValue)
            {
                return b;
            }

            return Mathf.Min(a.Value, b.Value);
        }

        public static float? NullableMax(float? a, float? b)
        {
            if (!a.HasValue && !b.HasValue)
            {
                return null;
            }

            if (a.HasValue && !b.HasValue)
            {
                return a;
            }

            if (!a.HasValue && b.HasValue)
            {
                return b;
            }

            return Mathf.Max(a.Value, b.Value);
        }

        public static StatSubType GetSubTypeByString(string subTypeNameString)
        {
            List<Stat> allStats = DataSimulationManager.Instance.StatDatabase;
            foreach (Stat s in allStats)
            {
                if (s.statSubType.ToString() == subTypeNameString)
                {
                    return s.statSubType;
                }
            }

            return StatSubType.NONE;
        }

        /// <summary>
        /// normalized distribution
        /// </summary>
        public static double NextGaussian(double mu = 0, double sigma = 1)
        {
            var u1 = UnityEngine.Random.Range(0, 1f);
            var u2 = UnityEngine.Random.Range(0, 1f);
            var rand_std_normal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                                Math.Sin(2.0 * Math.PI * u2);

            var rand_normal = mu + sigma * rand_std_normal;

            return rand_normal;
        }

        /// <summary>
        /// Copy a list of a custom class with no connected refereces
        /// </summary>
        //example         List<Wave> newWaves = Utilities.DeepClone(currentWaveset.waves);
        public static T DeepClone<T>(T obj)
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;

                return (T)formatter.Deserialize(ms);
            }
        }

        public static void SetIText(this TextMeshProUGUI text, string str)
        {
            text.text = Utilities.InterceptText(str);
        }

        public static void SetIText(this Text text, string str)
        {
            text.text = Utilities.InterceptText(str);
        }

        public static string InterceptText(this string str)
        {
            bool recheck = true;

            while (recheck)
            {
                recheck = false;

                foreach (KeyValuePair<string, string> kvp in InterceptedText.TextKVPs)
                {
                    if (str.Contains(kvp.Key))
                    {
                        str = str.Replace(kvp.Key, kvp.Value);
                        recheck = true;
                    }
                }
            }

            return str;
        }

        public static string FormatNumberForDisplay(float n)
        {
            if (Mathf.Abs(n) < 1000)
            {
                return (Mathf.Round(n * 100) / 100).ToString();
            }
            else if (Mathf.Abs(n) < 1000000)
            {
                int showableDigits = 6 - Mathf.FloorToInt(Mathf.Log10(Mathf.Abs(n)));

                if (showableDigits > 2)
                {
                    showableDigits = 2;
                }

                return (Mathf.Round(n / Mathf.Pow(10, 3 - showableDigits)) / Mathf.Pow(10, showableDigits)).ToString() + "k";
            }
            else
            {
                int showableDigits = 9 - Mathf.FloorToInt(Mathf.Log10(Mathf.Abs(n)));

                if (showableDigits > 2)
                {
                    showableDigits = 2;
                }

                return (Mathf.Round(n / Mathf.Pow(10, 6 - showableDigits)) / Mathf.Pow(10, showableDigits)).ToString() + "m";
            }
        }
    }
}
