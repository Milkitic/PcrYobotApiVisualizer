# YobotExtension

Yobot API数据可视化程序

## 当前重点问题

1. UI的原型设计
2. 多图表实例，支持同屏多图且自由排版
3. 多版本的Yobot API兼容，支持自定义个API接口实例、实现自动匹配

## 参与开发

### 拓展图表

需要前提知识：

* C#基本使用
* LiveCharts的使用，参考 [官方文档](https://lvcharts.net/App/examples/v1/wpf/Basics)
* 关注 `YobotExtension\ChartFramework\StatsProviders` 内的两个Demo。

开发新图表仅需创建一个实现`IStatsProvider`接口的类，并标注Provider的元信息。再按一定规范在此类中编写方法提供数据模板，框架会自动读取方法，并且生成多个图表。

Provider示例：

```csharp
[StatsProviderMetadata("GUID码",
    Author = "作者名",
    Name = "类型名称",
    Description = "类型描述")]
public class DemoStatsProvider : IStatsProvider
{
    [StatsMethod("行会每天伤害趋势")]
    [StatsMethodAcceptGranularity(GranularityType.Total)]
    [StatsMethodThumbnail("行会每天伤害趋势.jpg")]
    [UsedImplicitly]
    public CartesianChartConfigModel 自定义名称方法()
    // public PieChartConfigModel 自定义名称方法() // 饼图
    // public HeatChartConfigModel 自定义名称方法() // 热力图
    // public CartesianChartConfigModel 自定义名称方法(GranularityModel granularity) // 调用时搜索条件
    // public async Task<CartesianChartConfigModel> 自定义名称方法(GranularityModel granularity) // 异步
    {
        return new CartesianChartConfigModel
        {
            // ...
        };
    }
}
```
