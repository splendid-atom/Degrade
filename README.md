# Degrade

## 全局：
这是一个关于两个世界的游戏。

- 初步绘制瓦片地图
- 添加部分武器系统
- 增加水渲染

---

## Player：

- **2.5D视角**
- **新手指引：**
  - 玩家根据指示按下对应按键（有音效提示）
- **生命值**（0-100）、**护甲值**（0-200）
- **小地图：**
  - 为了在小地图上显示的物体不旋转，需要复制所有需要显示在小地图上的物体（环境中的大部分物体和grid），并将其layer设为`onlyminimap`。
- **大地图：**
  - 按下`M`键打开大地图，左键拖动，鼠标滑轮缩放
- **任务栏：**
  - 使用inspector或脚本添加任务
  - 添加展开/收起任务描述的功能
  - 添加任务完成音效及动画
  - 文字自动resize，任务完成后划掉
- **道具栏：**
  - 空对象`ItemManager`，按键左上角数字对应键盘数字按键
  - 屏幕下方显示道具栏，鼠标按下按钮或者对应数字可使用道具
  - 按钮右下角显示道具数量，按钮右上角显示冷却时间，灰色时钟遮罩
  - 现有两行道具，每次只显示一行。鼠标悬浮在道具栏上滑动滚轮可切换行，也可用鼠标拖动滑轮改变行数
- **人物信息显示：**
  - 右上角显示玩家生命值、护甲值、昵称
