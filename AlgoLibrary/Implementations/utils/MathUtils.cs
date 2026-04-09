using System;
using System.Collections.Generic;

namespace AlgoLibrary.Implementations.Utils;

public static class MathUtils
{
    /// <summary>
    /// 计算可比项列表的中位数。列表在内部排序，因此原始顺序不会被修改。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static double CalculateMedian<T>(List<T> data) where T : IComparable<T>
    {
        if (data == null || data.Count == 0)
            throw new ArgumentException("List cannot be null or empty.");

        var sorted = new List<T>(data);
        sorted.Sort();

        int count = sorted.Count;
        int mid = count / 2;

        if (count % 2 == 0)
        {
            dynamic a = sorted[mid - 1];
            dynamic b = sorted[mid];
            return (a + b) / 2.0;
        }
        else
        {
            dynamic result = sorted[mid];
            return (double)result;
        }
    }
}
