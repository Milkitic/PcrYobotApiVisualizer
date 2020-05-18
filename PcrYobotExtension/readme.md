# Yobot API数据可视化程序

## 开发

### 拓展图表

需要前提知识：

* WPF的基本使用，数据上下文概念
* LiveCharts的使用，参考 [官方文档](https://lvcharts.net/App/examples/v1/wpf/Basics)
* 关注 `UserControls\StatsGraphControls` 内的两个Demo。

开发新图表仅需创建一个`UserControl`，并且实现`IChartSwitchControl`的方法。目前必须手动实现`InitModels()`。

进行一次数据更新前，建议重建图表 `IChartProvider.RecreateGraph()`，默认图表仅支持简单二维的可进行数据绑定的单列单行图表，若有高级需要可以使用其重载。

框架为初期，若有变化repo创建者可协助进行迁移升级。
