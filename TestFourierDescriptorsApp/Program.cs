using System;
using System.Collections.Generic;
using OpenCvSharp;
using AlgoLibrary.Implementations.Utils;

class TestFourierDescriptors
{
    static void Main()
    {
        Console.WriteLine("测试傅里叶描述符特征提取...");
        
        // 创建一个简单的轮廓（正方形）- 使用更多点以获得更好的傅里叶变换结果
        var squarePoints = new List<Point2f>();
        int squarePointsCount = 32;
        for (int i = 0; i < squarePointsCount; i++)
        {
            float t = (float)i / squarePointsCount;
            if (t < 0.25f)
                squarePoints.Add(new Point2f(40 * t * 4, 0));
            else if (t < 0.5f)
                squarePoints.Add(new Point2f(10, 40 * (t - 0.25f) * 4));
            else if (t < 0.75f)
                squarePoints.Add(new Point2f(10 - 40 * (t - 0.5f) * 4, 10));
            else
                squarePoints.Add(new Point2f(0, 10 - 40 * (t - 0.75f) * 4));
        }
        
        Console.WriteLine($"正方形轮廓点数量: {squarePoints.Count}");
        
        try
        {
            // 测试基本函数 - 使用4个描述符（小于点集数量）
            Console.WriteLine("\n1. 测试基本傅里叶描述符提取:");
            var descriptors1 = AlgoUtils.ExtractFourierDescriptors(squarePoints, 4);
            Console.WriteLine($"提取到 {descriptors1.Length} 个描述符:");
            for (int i = 0; i < descriptors1.Length; i++)
            {
                Console.WriteLine($"  描述符[{i}] = {descriptors1[i]:F6}");
            }
            
            // 测试增强函数
            Console.WriteLine("\n2. 测试增强版傅里叶描述符提取:");
            var descriptors2 = AlgoUtils.ExtractFourierDescriptorsEx(
                squarePoints, 
                descriptorLength: 4,
                normalizeScale: true,
                normalizeRotation: true,
                normalizeStartingPoint: true);
            Console.WriteLine($"提取到 {descriptors2.Length} 个描述符:");
            for (int i = 0; i < descriptors2.Length; i++)
            {
                Console.WriteLine($"  描述符[{i}] = {descriptors2[i]:F6}");
            }
            
            // 创建一个圆形轮廓
            var circlePoints = new List<Point2f>();
            int circlePointsCount = 32;
            double radius = 5.0;
            for (int i = 0; i < circlePointsCount; i++)
            {
                double angle = 2 * Math.PI * i / circlePointsCount;
                circlePoints.Add(new Point2f(
                    (float)(radius * Math.Cos(angle) + 10),
                    (float)(radius * Math.Sin(angle) + 10)));
            }
            
            Console.WriteLine($"\n圆形轮廓点数量: {circlePoints.Count}");
            
            // 测试圆形轮廓
            Console.WriteLine("\n3. 测试圆形轮廓的傅里叶描述符:");
            var descriptors3 = AlgoUtils.ExtractFourierDescriptors(circlePoints, 8);
            Console.WriteLine($"提取到 {descriptors3.Length} 个描述符:");
            for (int i = 0; i < descriptors3.Length; i++)
            {
                Console.WriteLine($"  描述符[{i}] = {descriptors3[i]:F6}");
            }
            
            // 测试错误处理
            Console.WriteLine("\n4. 测试错误处理:");
            try
            {
                var emptyPoints = new List<Point2f>();
                var descriptors4 = AlgoUtils.ExtractFourierDescriptors(emptyPoints, 8);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"  预期错误: {ex.Message}");
            }
            
            try
            {
                var fewPoints = new List<Point2f> { new Point2f(0, 0), new Point2f(1, 1) };
                var descriptors5 = AlgoUtils.ExtractFourierDescriptors(fewPoints, 8);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"  预期错误: {ex.Message}");
            }
            
            Console.WriteLine("\n测试完成！");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"测试失败: {ex.Message}");
            Console.WriteLine($"堆栈跟踪: {ex.StackTrace}");
        }
    }
}
