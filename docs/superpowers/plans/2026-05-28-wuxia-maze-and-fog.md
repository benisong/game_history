# 武侠传承录 (Wuxia Inheritance) 开发执行计划书 (第二阶段：五维加点、迷宫迷雾、与内嵌式地图对话)

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 实现20点五维属性初始加点（新增力量与敏捷）、50x80 水墨迷宫生成、基于 Canvas 的 Camera 摄像机跟随视口、视野迷雾（半径 3 格持久化）、以及大地图下完全无弹窗的内嵌式 NPC 对话和交互系统。

**Architecture:** 
1. 初始加点模块（力量与敏捷加入，总20点上限）。
2. 在服务端或配置层集成迷宫生成逻辑（DFS 自动生成，去死胡同开阔化）。
3. 扩展大地图至 50x80。在 `map-renderer.js` 中集成 Camera 裁剪绘制和迷雾遮罩。
4. 移除 `main.js` 中的 `alert()` 交互，在 Map 视图的右侧区域就地动态渲染水墨对话卡片和选项按钮。

**Tech Stack:** Node.js, Express, HTML5 Canvas, ES Modules, CSS3

---

## 核心任务列表

### Task 1: 升级创角界面至 20 点新五维分配 (根骨、悟性、力量、敏捷、家境)

**Files:**
- Modify: `wuxia-inheritance/public/index.html` (更新属性控制 DOM)
- Modify: `wuxia-inheritance/public/style.css` (美化属性分配面板)
- Modify: `wuxia-inheritance/public/main.js` (更新初始 state 结构与属性映射)
- Modify: `wuxia-inheritance/public/index.js` (更新加点加减与 20点上限检测逻辑)

- [ ] **Step 1: 修改 index.html 中的创角属性项，加入力量、敏捷、家境，设置可用点数为 20 点**

在 `#creation-screen` 的 `.creation-panel` 增加力量（Strength）、敏捷（Agility）、家境（Wealth）的增减按钮与数值显示。

- [ ] **Step 2: 修改 main.js 中的初始 gameState.player.stats 结构**

```javascript
window.gameState.player.stats = {
    bone: 0,       // 根骨 (唯一寿命/HP)
    savviness: 0,  // 悟性 (功法性价比)
    strength: 0,   // 力量 (唯一物理攻击力)
    agility: 0,    // 敏捷 (唯一出手速度/闪避)
    wealth: 0      // 家境 (初始财富)
};
```

- [ ] **Step 3: 修改 index.js 创角点数分配控制**

加入限制：
* 属性单项极值 20，初始可用总点数 20 点。
* 确保属性加满 20 点后才可开启“进入江湖”按钮。

- [ ] **Step 4: 运行并验证创角面板**

登录一个新号，验证属性分配。5个属性加减必须准确消耗 20点天命点，并且数值更新完全对齐。

---

### Task 2: 服务端 50x80 水墨迷宫生成器

**Files:**
- Create: `wuxia-inheritance/server/maze.js` (迷宫算法类)
- Modify: `wuxia-inheritance/server/server.js` (路由集成)
- Modify: `wuxia-inheritance/server/data/static_maps.json` (更新配置架构)

- [ ] **Step 1: 编写 `maze.js` 生成 50x80 的迷宫矩阵并自动进行开阔化处理**

使用简易 DFS 墙壁打通算法生成 `50 x 80` 的 `0` (通路) 和 `1` (山/障碍) 矩阵。然后随机挑选 25% 的墙体（1）变通（0）防止全是窄死路，并划定 `(2,2)` (起点村大殿) 区域为 $5\times 5$ 的空旷安全区。

```javascript
// maze.js
function generateWuxiaMaze(width = 50, height = 80) {
    // 基础迷宫打通
    const matrix = Array(height).fill(null).map(() => Array(width).fill(1));
    
    function dfs(x, y) {
        matrix[y][x] = 0;
        const dirs = [[0,2],[0,-2],[2,0],[-2,0]].sort(() => Math.random() - 0.5);
        for (let [dx, dy] of dirs) {
            let nx = x + dx, ny = y + dy;
            if (nx > 0 && nx < width - 1 && ny > 0 && ny < height - 1 && matrix[ny][nx] === 1) {
                matrix[y + dy/2][x + dx/2] = 0;
                dfs(nx, ny);
            }
        }
    }
    dfs(2, 2);

    // 随机打穿 25% 的障碍墙壁实现开阔化
    for (let y = 1; y < height - 1; y++) {
        for (let x = 1; x < width - 1; x++) {
            if (matrix[y][x] === 1 && Math.random() < 0.25) {
                matrix[y][x] = 0;
            }
        }
    }

    // 强制打通出生点安全大殿
    for (let y = 1; y <= 5; y++) {
        for (let x = 1; x <= 5; x++) {
            matrix[y][x] = 0;
        }
    }

    return matrix;
}
module.exports = { generateWuxiaMaze };
```

- [ ] **Step 2: 绑定路由到 `server.js` 并动态下发，大地图数据缓存**

更新大地图请求 API。如果请求的是 50x80 迷宫地图，服务器自动调用 `generateWuxiaMaze` 并在内存中缓存或持久化到 `static_maps.json`，下发给前端。

---

### Task 3: 前端 Canvas 摄像机跟随视口与视野迷雾遮罩

**Files:**
- Modify: `wuxia-inheritance/public/map-renderer.js` (添加 Camera 视口计算与迷雾叠加)
- Modify: `wuxia-inheritance/public/main.js` (坐标移动视野刷新及 explored 矩阵初始化/持久化)

- [ ] **Step 1: 编写 Camera 摄像机渲染裁剪。Canvas 恒为 450x450 (15x15格子)**

在 `map-renderer.js` 的 `render` 方法中加入相机的 `startX`, `startY` 计算：
* 以玩家 `(px, py)` 为中心，做边界裁剪：
  ```javascript
  const startX = Math.max(0, Math.min(50 - 15, px - 7));
  const startY = Math.max(0, Math.min(80 - 15, py - 7));
  ```
* 渲染瓦片时，利用 `(tileX - startX) * 30` 作为 Canvas 的真实像素绘制位置。

- [ ] **Step 2: 视野迷雾遮罩绘制 ( explored 二维布尔数组)**

```javascript
// 在绘制完地形和 NPC 后，覆盖上一层淡淡的水墨云雾：
if (!explored[y][x]) {
    this.ctx.fillStyle = "rgba(40, 40, 40, 0.95)"; // 迷雾层
    this.ctx.fillRect(drawX, drawY, this.tileSize, this.tileSize);
}
```

- [ ] **Step 3: 键盘 WASD 走动时刷新 3 格视野**

在 `main.js` 的玩家移动逻辑中：每次移动成功，触发 `updateFogOfWar(px, py)`，将玩家中心半径 3格以内的所有 `explored[y][x]` 设为 `true`。并在 `/api/game/save` 时一并将 explored 地图探索数组持久化到后端 `saves.json`。

---

### Task 4: 实现无弹窗、内嵌式 MUD 精致交互对话框

**Files:**
- Modify: `wuxia-inheritance/public/index.html` (更新右侧交互区域 UI 样式)
- Modify: `wuxia-inheritance/public/style.css` (设计水墨古卷对话样式)
- Modify: `wuxia-inheritance/public/main.js` (重构 `doLook` 与 `showNPCDetail` 实现无弹窗渲染)

- [ ] **Step 1: 设计国风水墨对话框卡片样式**

在 `style.css` 中增加对话框卷轴类：
```css
.ink-dialog-box {
    border: 3px double #5a4b3d;
    background: #eae2cc;
    padding: 12px;
    border-radius: 4px;
    margin-top: 10px;
    box-shadow: inset 0 0 10px rgba(0,0,0,0.1);
}
.dialog-content {
    font-size: 14px;
    line-height: 1.6;
    margin-bottom: 12px;
    color: #1a1a1a;
    font-style: italic;
}
```

- [ ] **Step 2: 重构 main.js 内的 `showNPCDetail` 方法，禁止任何 `alert` 动作**

* 移除原有的浏览器弹窗 `alert()` 和 `confirm()`。
* 点击 NPC "查看" 后，大地图右侧 `#look-detail` 区域原地刷新，绘制 `.ink-dialog-box` 卷轴卡片：
  * 文字区：显示 NPC 的台词（如：`冯铁匠沉声道：『听闻少林无相劫指威震天下，不知公子能使几成力？』`）。
  * 按钮区：绘制 `[交谈]`、`[切磋]`、`[赠礼]` 按钮。点击交谈后，直接修改该卡片内部的 `.dialog-content` 文字内容，实现零弹窗的极致静谧互动体验！

---

## Execution Handoff

Plan complete and saved to `docs/superpowers/plans/2026-05-28-wuxia-maze-and-fog.md`. Two execution options:

**1. Subagent-Driven (recommended)** - I dispatch a fresh subagent per task, review between tasks, fast iteration

**2. Inline Execution** - Execute tasks in this session using executing-plans, batch execution with checkpoints

**Which approach?**
