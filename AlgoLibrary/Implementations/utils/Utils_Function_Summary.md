# Utils 文件夹函数汇总

## 目录结构

```
AlgoLibrary/Implementations/utils/
├── AlgoUtils.cs
├── AlgoUtils.md
├── ContourFeatureExtractor.cs
├── dropsImageAlgo.cs
├── MathUtils.cs
└── Utils_Function_Summary.md (本文档)
```

## 文件详细说明

### 1,AlgoUtils.cs

* FitLineFromPoints：点集拟合直线，返回(A,B,C)标准式系数

* RadianToAngle0To180：弧度转角度并归一化到\[0,180)

* IsRectFullyInside：判断矩形是否完全在图像范围内

* DetectLinesHoughP：Canny + 概率霍夫检测线段

* DetectHoughCircleFromImage：霍夫圆检测，返回圆心与半径

* DetectConnectAreaDemo：连通域检测示例（含可视化显示）

### 2,ContourFeatureExtractor.cs

* Get14DNormalizedFeature：提取轮廓的14维固定长度归一化特征向量

* NormalizeMinMax：Min-Max归一化：把数组缩放到0\~1

* ExtractFourierDescriptors：基于傅里叶变换提取轮廓的特征向量

* ExtractFourierDescriptorsEx：基于傅里叶变换提取轮廓的特征向量（增强版本）

* ContourToPolarFeature：轮廓归一化 + 极坐标展开，输出固定长度特征向量

* ResampleAndCenterContour：通过线性插值对轮廓进行重采样，并将点集中心化到原点

* ExtractResampledContourFeature：基于重采样和中心化轮廓提取固定长度特征向量

### 3,dropsImageAlgo.cs

* Detect：检测图像中的液滴和线段

* EnsureGray8U：确保图像为灰度8位无符号格式

* IsCircleInside：检查圆是否完全在图像范围内

* TryFitDominantLine：尝试拟合主导线段

* TryClipInfiniteLineToImage：将无限直线裁剪到图像范围内

* Distance2：计算两点间距离的平方

### 4,MathUtils.cs

* CalculateMedian：计算可比项列表的中位数

* FindPeakInHistogram：将可比项列表分bin（直方图化），并找到最大的峰

* CalculateOptimalBins：使用Sturges公式计算最优的bin数量

## 总结

本文件夹包含4个核心工具类：

1. **AlgoUtils** - 提供基础的图像处理算法（直线拟合、圆检测、连通域检测等）
2. **ContourFeatureExtractor** - 专门用于轮廓特征提取，提供多种特征提取方法
3. **DropsImageAlgo** - 针对液滴图像分析的专用算法
4. **MathUtils** - 提供通用的数学计算工具

这些工具类共同构成了图像处理和计算机视觉算法的基础工具集。

