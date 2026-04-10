using System;
using AlgoLibrary.Implementations.Utils;

namespace SampleApp
{
    /// <summary>
    /// 测试 AlgoUtils 类的类
    /// </summary>
    public static class TestAlgoUtils
    {
        /// <summary>
        /// 运行测试
        /// </summary>
        public static void RunTest()
        {
            Console.WriteLine($"=== 测试 AlgoUtils 类 ===");
            Console.WriteLine();
            
            // 测试1: RadianToAngle0To180 函数 - 基本转换
            Console.WriteLine("测试1: RadianToAngle0To180 函数 - 基本转换");
            try
            {
                // 测试 0 弧度 -> 0 度
                double result1 = AlgoUtils.RadianToAngle0To180(0);
                Console.WriteLine($"输入: 0 弧度, 输出: {result1:F2} 度");
                Console.WriteLine($"预期: 0.00 度");
                Console.WriteLine($"测试结果: {(Math.Abs(result1 - 0) < 0.01 ? "通过" : "失败")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"测试结果: 失败 - {ex.Message}");
            }
            Console.WriteLine();
            
            // 测试2: RadianToAngle0To180 函数 - π/2 弧度 -> 90 度
            Console.WriteLine("测试2: RadianToAngle0To180 函数 - π/2 弧度 -> 90 度");
            try
            {
                double result2 = AlgoUtils.RadianToAngle0To180(Math.PI / 2);
                Console.WriteLine($"输入: π/2 弧度, 输出: {result2:F2} 度");
                Console.WriteLine($"预期: 90.00 度");
                Console.WriteLine($"测试结果: {(Math.Abs(result2 - 90) < 0.01 ? "通过" : "失败")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"测试结果: 失败 - {ex.Message}");
            }
            Console.WriteLine();
            
            // 测试3: RadianToAngle0To180 函数 - π 弧度 -> 0 度（归一化到 [0, 180)）
            Console.WriteLine("测试3: RadianToAngle0To180 函数 - π 弧度 -> 0 度");
            try
            {
                double result3 = AlgoUtils.RadianToAngle0To180(Math.PI);
                Console.WriteLine($"输入: π 弧度, 输出: {result3:F2} 度");
                Console.WriteLine($"预期: 0.00 度（归一化）");
                Console.WriteLine($"测试结果: {(Math.Abs(result3 - 0) < 0.01 ? "通过" : "失败")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"测试结果: 失败 - {ex.Message}");
            }
            Console.WriteLine();
            
            // 测试4: RadianToAngle0To180 函数 - 3π/2 弧度 -> 90 度（归一化）
            Console.WriteLine("测试4: RadianToAngle0To180 函数 - 3π/2 弧度 -> 90 度");
            try
            {
                double result4 = AlgoUtils.RadianToAngle0To180(3 * Math.PI / 2);
                Console.WriteLine($"输入: 3π/2 弧度, 输出: {result4:F2} 度");
                Console.WriteLine($"预期: 90.00 度（归一化）");
                Console.WriteLine($"测试结果: {(Math.Abs(result4 - 90) < 0.01 ? "通过" : "失败")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"测试结果: 失败 - {ex.Message}");
            }
            Console.WriteLine();
            
            // 测试5: RadianToAngle0To180 函数 - 负弧度测试
            Console.WriteLine("测试5: RadianToAngle0To180 函数 - 负弧度测试");
            try
            {
                double result5 = AlgoUtils.RadianToAngle0To180(-Math.PI / 4);
                Console.WriteLine($"输入: -π/4 弧度, 输出: {result5:F2} 度");
                Console.WriteLine($"预期: 135.00 度（-45° + 180°）");
                Console.WriteLine($"测试结果: {(Math.Abs(result5 - 135) < 0.01 ? "通过" : "失败")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"测试结果: 失败 - {ex.Message}");
            }
            Console.WriteLine();
            
            // 测试6: IsRectFullyInside 函数 - 基本测试
            Console.WriteLine("测试6: IsRectFullyInside 函数 - 基本测试");
            try
            {
                // 创建一个测试图像（100x100）
                using var testImage = new OpenCvSharp.Mat(100, 100, OpenCvSharp.MatType.CV_8UC1, new OpenCvSharp.Scalar(0));
                
                // 测试矩形完全在图像内
                var rect1 = new OpenCvSharp.Rect(10, 10, 50, 50);
                bool result6a = AlgoUtils.IsRectFullyInside(testImage, rect1);
                Console.WriteLine($"图像: 100x100, 矩形: (10,10,50,50)");
                Console.WriteLine($"结果: {result6a}, 预期: True");
                
                // 测试矩形部分在图像外
                var rect2 = new OpenCvSharp.Rect(90, 90, 20, 20);
                bool result6b = AlgoUtils.IsRectFullyInside(testImage, rect2);
                Console.WriteLine($"图像: 100x100, 矩形: (90,90,20,20)");
                Console.WriteLine($"结果: {result6b}, 预期: False");
                
                Console.WriteLine($"测试结果: {(result6a && !result6b ? "通过" : "失败")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"测试结果: 失败 - {ex.Message}");
            }
            
            Console.WriteLine("\n所有测试完成！按任意键继续...");
            Console.ReadKey();
        }
        
        // 可选：添加辅助测试方法
        private static void TestSpecificScenario()
        {
            // 具体测试逻辑
        }
    }
}
