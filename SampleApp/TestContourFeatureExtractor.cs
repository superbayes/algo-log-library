using System;

namespace SampleApp
{
    public static class TestContourFeatureExtractor
    {
        /// <summary>
        /// 测试轮廓特征提取函数
        /// </summary>
        public static void RunTest()
        {
            Console.WriteLine("=== 测试轮廓特征提取函数 ===");
            Console.WriteLine();
            
            // 创建一个简单的矩形轮廓
            var rectangleContour = new OpenCvSharp.Point[]
            {
                new OpenCvSharp.Point(10, 10),
                new OpenCvSharp.Point(50, 10),
                new OpenCvSharp.Point(50, 30),
                new OpenCvSharp.Point(10, 30)
            };
            
            // 创建一个相同的矩形轮廓，但位置不同（平移了(20, 20)）
            var translatedRectangleContour = new OpenCvSharp.Point[]
            {
                new OpenCvSharp.Point(30, 30),
                new OpenCvSharp.Point(70, 30),
                new OpenCvSharp.Point(70, 50),
                new OpenCvSharp.Point(30, 50)
            };
            
            // 测试1: ResampleAndCenterContour 函数
            Console.WriteLine("测试1: ResampleAndCenterContour 函数");
            Console.WriteLine("原始矩形轮廓: (10,10)-(50,30)");
            Console.WriteLine("平移后矩形轮廓: (30,30)-(70,50)");
            Console.WriteLine();
            
            try
            {
                // 测试原始轮廓
                var centered1 = ContourFeatureExtractor.ResampleAndCenterContour(rectangleContour, 8);
                Console.WriteLine($"原始轮廓重采样中心化结果 (8个点):");
                for (int i = 0; i < centered1.Length; i++)
                {
                    Console.WriteLine($"  点{i}: ({centered1[i].X:F2}, {centered1[i].Y:F2})");
                }
                
                // 计算质心验证中心化
                double sumX1 = 0, sumY1 = 0;
                foreach (var p in centered1)
                {
                    sumX1 += p.X;
                    sumY1 += p.Y;
                }
                double centerX1 = sumX1 / centered1.Length;
                double centerY1 = sumY1 / centered1.Length;
                Console.WriteLine($"质心: ({centerX1:F6}, {centerY1:F6}) - 应接近(0,0)");
                Console.WriteLine($"中心化验证: {(Math.Abs(centerX1) < 0.01 && Math.Abs(centerY1) < 0.01 ? "通过" : "失败")}");
                Console.WriteLine();
                
                // 测试平移后的轮廓
                var centered2 = ContourFeatureExtractor.ResampleAndCenterContour(translatedRectangleContour, 8);
                Console.WriteLine($"平移轮廓重采样中心化结果 (8个点):");
                for (int i = 0; i < centered2.Length; i++)
                {
                    Console.WriteLine($"  点{i}: ({centered2[i].X:F2}, {centered2[i].Y:F2})");
                }
                
                // 计算质心验证中心化
                double sumX2 = 0, sumY2 = 0;
                foreach (var p in centered2)
                {
                    sumX2 += p.X;
                    sumY2 += p.Y;
                }
                double centerX2 = sumX2 / centered2.Length;
                double centerY2 = sumY2 / centered2.Length;
                Console.WriteLine($"质心: ({centerX2:F6}, {centerY2:F6}) - 应接近(0,0)");
                Console.WriteLine($"中心化验证: {(Math.Abs(centerX2) < 0.01 && Math.Abs(centerY2) < 0.01 ? "通过" : "失败")}");
                Console.WriteLine();
                
                // 验证两个轮廓中心化后形状相似（点坐标应接近）
                bool shapesSimilar = true;
                double maxDiff = 0;
                for (int i = 0; i < Math.Min(centered1.Length, centered2.Length); i++)
                {
                    double diffX = Math.Abs(centered1[i].X - centered2[i].X);
                    double diffY = Math.Abs(centered1[i].Y - centered2[i].Y);
                    maxDiff = Math.Max(maxDiff, Math.Max(diffX, diffY));
                    if (diffX > 1.0 || diffY > 1.0)
                    {
                        shapesSimilar = false;
                    }
                }
                Console.WriteLine($"形状相似性验证: {(shapesSimilar ? "通过" : "失败")}");
                Console.WriteLine($"最大坐标差异: {maxDiff:F2}");
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"测试1失败: {ex.Message}");
                Console.WriteLine($"详细错误: {ex}");
            }
            
            // 测试2: ExtractResampledContourFeature 函数
            Console.WriteLine("测试2: ExtractResampledContourFeature 函数");
            Console.WriteLine("测试特征向量提取和归一化");
            Console.WriteLine();
            
            try
            {
                // 提取特征向量
                var feature1 = ContourFeatureExtractor.ExtractResampledContourFeature(rectangleContour, 16);
                var feature2 = ContourFeatureExtractor.ExtractResampledContourFeature(translatedRectangleContour, 16);
                
                Console.WriteLine($"特征向量1 (16维):");
                Console.WriteLine($"  [{string.Join(", ", Array.ConvertAll(feature1, f => f.ToString("F4")))}]");
                Console.WriteLine($"特征向量2 (16维):");
                Console.WriteLine($"  [{string.Join(", ", Array.ConvertAll(feature2, f => f.ToString("F4")))}]");
                Console.WriteLine();
                
                // 验证特征向量维度
                Console.WriteLine($"特征向量维度验证: {(feature1.Length == 16 && feature2.Length == 16 ? "通过" : "失败")}");
                
                // 验证特征向量归一化（L2范数应接近1）
                double norm1 = 0, norm2 = 0;
                for (int i = 0; i < feature1.Length; i++)
                {
                    norm1 += feature1[i] * feature1[i];
                    norm2 += feature2[i] * feature2[i];
                }
                norm1 = Math.Sqrt(norm1);
                norm2 = Math.Sqrt(norm2);
                
                Console.WriteLine($"特征向量1 L2范数: {norm1:F6} - 应接近1.0");
                Console.WriteLine($"特征向量2 L2范数: {norm2:F6} - 应接近1.0");
                Console.WriteLine($"归一化验证: {(Math.Abs(norm1 - 1.0) < 0.01 && Math.Abs(norm2 - 1.0) < 0.01 ? "通过" : "失败")}");
                Console.WriteLine();
                
                // 验证平移不变性：相同形状不同位置的轮廓应产生相似特征
                double similarity = 0;
                for (int i = 0; i < feature1.Length; i++)
                {
                    similarity += feature1[i] * feature2[i];
                }
                similarity = Math.Abs(similarity); // 点积的绝对值
                
                Console.WriteLine($"特征向量相似度（点积）: {similarity:F6}");
                Console.WriteLine($"平移不变性验证: {(similarity > 0.95 ? "通过（高度相似）" : similarity > 0.8 ? "通过（基本相似）" : "失败（差异较大）")}");
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"测试2失败: {ex.Message}");
                Console.WriteLine($"详细错误: {ex}");
            }
            
            // 测试3: 旧函数 ResampleContour（标记为过时）
            Console.WriteLine("测试3: 旧函数 ResampleContour（兼容性测试）");
            Console.WriteLine("验证旧函数仍然可用但会显示过时警告");
            Console.WriteLine();
            
            try
            {
                #pragma warning disable CS0618 // 类型或成员已过时
                var oldResult = ContourFeatureExtractor.ResampleContour(rectangleContour, 8);
                #pragma warning restore CS0618 // 类型或成员已过时
                
                Console.WriteLine($"旧函数返回 {oldResult.Length} 个点");
                Console.WriteLine($"第一个点: ({oldResult[0].X}, {oldResult[0].Y})");
                Console.WriteLine($"旧函数兼容性测试: 通过");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"测试3失败: {ex.Message}");
            }
            
            Console.WriteLine("\n所有轮廓特征提取测试完成！按任意键继续...");
            Console.ReadKey();
        }
    }
}
