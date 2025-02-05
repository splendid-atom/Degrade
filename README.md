# Degrade<br>
<strong>全局：<strong><br>
a game about two world<br>
初步绘制瓦片地图<br>
添加部分武器系统<br>
增加水渲染<br>
<hr>
<strong>Player：<strong><br>
2.5D视角<br>
增加新手指引：<br>
玩家根据指示按下对应按键（有音效提示）<br>
生命值（0-100）、护甲值（0-200）<br>
小地图：<br>
为了显示的物体不旋转，需要复制所有需要显示在小地图上的物体<br>
(environment中的大部分物体和grid)，并且将其layer设为onlyminimap。<br>
新增大地图：<br>
按M打开大地图，左键拖动，鼠标滑轮缩放<br>
任务栏：<br>
用inspector或脚本添加任务,添加展开/收起任务描述<br>
添加任务完成音效及动画<br>
文字自动resize,任务完成后划掉<br>
道具栏：<br>
空对象ItemManager，按键左上角数字对应键盘数字按键，<br>
屏幕下方显示道具栏，鼠标按下按钮或者对应数字可使用道具，<br>
按钮右下角显示道具数量，按钮右上角显示冷却时间，灰色时钟遮罩。<br>
