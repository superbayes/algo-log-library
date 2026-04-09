using System;
using System.Collections.Generic;
using System.Linq;

namespace AlgoLibrary.Implementations.Utils;

public static class MathUtils
{
    /// <summary>
    /// 计算可比项列表的中位数。列表在内部排序，因此原始顺序不会被修改。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <returns>中位数</returns>
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

    /// <summary>
    /// 将可比项列表分bin（直方图化），并找到最大的峰（count最多的bin）。
    /// </summary>
    /// <typeparam name="T">可比项类型</typeparam>
    /// <param name="data">输入数据列表</param>
    /// <param name="numBins">bin的数量。如果为null，则使用Sturges公式自动计算：ceil(log2(n) + 1)</param>
    /// <returns>包含最大峰的count和所代表数值的元组</returns>
    /// <exception cref="ArgumentException">当输入数据为空或null时抛出</exception>
    public static (int count, double value) FindPeakInHistogram<T>(List<T> data, int? numBins = null) where T : IComparable<T>
    {
        if (data == null || data.Count == 0)
            throw new ArgumentException("List cannot be null or empty.");

        // 转换为double列表以便计算
        var doubleData = data.Select(item => Convert.ToDouble(item)).ToList();
        
        // 确定bin的数量
        int bins = numBins ?? CalculateOptimalBins(doubleData.Count);
        
        // 计算数据范围
        double min = doubleData.Min();
        double max = doubleData.Max();
        
        // 如果所有值都相同，直接返回
        if (Math.Abs(max - min) < double.Epsilon)
        {
            return (doubleData.Count, min);
        }
        
        // 计算bin宽度
        double binWidth = (max - min) / bins;
        
        // 初始化bin计数
        int[] binCounts = new int[bins];
        
        // 统计每个bin的count
        foreach (double value in doubleData)
        {
            int binIndex = (int)Math.Floor((value - min) / binWidth);
            
            // 处理最大值的情况（会落在最后一个bin之外）
            if (binIndex >= bins)
            {
                binIndex = bins - 1;
            }
            
            binCounts[binIndex]++;
        }
        
        // 找到最大count的bin
        int maxCount = 0;
        int maxBinIndex = 0;
        
        for (int i = 0; i < bins; i++)
        {
            if (binCounts[i] > maxCount)
            {
                maxCount = binCounts[i];
                maxBinIndex = i;
            }
        }
        
        // 计算该bin的代表值（使用bin的中心值）
        double binCenter = min + (maxBinIndex + 0.5) * binWidth;
        
        return (maxCount, binCenter);
    }

    /// <summary>
    /// 使用Sturges公式计算最优的bin数量。
    /// </summary>
    /// <param name="dataCount">数据点数量</param>
    /// <returns>推荐的bin数量</returns>
    private static int CalculateOptimalBins(int dataCount)
    {
        // Sturges公式: k = ceil(log2(n) + 1)
        return (int)Math.Ceiling(Math.Log(dataCount, 2) + 1);
    }
}
