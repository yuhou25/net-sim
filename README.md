# NetSim · 网络协议可视化教学模拟器

> **开源版 Packet Tracer 教学版** —— 双击即用、零环境依赖、把协议过程"白盒讲解"的轻量级计算机网络教学工具。

[![Build Status](https://github.com/yuhou25/net-sim/actions/workflows/build.yml/badge.svg)](https://github.com/yuhou25/net-sim/actions/workflows/build.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[.NET Framework 4.8](https://img.shields.io/badge/.NET%20Framework-4.8-blue)
[Platform](https://img.shields.io/badge/platform-Windows-lightgrey)

---

## 这是什么？

NetSim 是一个用于**计算机网络入门课程**的桌面教学工具。学生拖拽设备、连线、点击"开始模拟"，就能**逐跳、逐层地看到**一个数据包从 PC 到网站的全过程：

- PC 怎么判断目的 IP 在不在同网段？
- 没找到 MAC 地址时，ARP 是怎么工作的？
- 交换机收到帧以后，MAC 表是怎么学习、怎么转发的？
- 路由器是怎么查路由表、重新封装、做 NAT 转换的？
- DNS 是怎么把域名解析成 IP 的？

每一步都有 ✓/✗ 标记和详细说明 —— **不只是告诉你"通了"，而是告诉你"为什么通了"**。

---

## 为什么不直接用 Packet Tracer / GNS3 / Mininet？

| 项目 | 真实协议 | 安装成本 | 教学可解释性 | 适合场景 |
|---|:---:|:---:|:---:|---|
| **Cisco Packet Tracer** | ✅（仿真） | 中（需注册思科账号） | ⭐⭐ | CCNA 实验 |
| **GNS3** | ✅（真实镜像） | **高**（Linux/Docker/镜像文件） | ⭐ | CCNP/CCIE 实验 |
| **Mininet** | ✅（真实网络栈） | **高**（仅 Linux + root） | ⭐ | SDN 研究 |
| **Kathara** | ✅（容器化） | 高（Docker + 镜像） | ⭐ | 网络工程实验 |
| **NetSim（本项目）** | ❌（教学模拟） | **零**（双击 exe） | ⭐⭐⭐⭐⭐ | **计算机网络原理入门** |

NetSim 不和 GNS3/Mininet 比"协议真实度"（那条路它们已经走得很远），而是占了一个**它们都没做的空白生态位**：**5 分钟上手、零环境配置、把协议过程像教科书插图一样画给你看的桌面小工具**。

> 💡 一句话定位：**GNS3 是"虚拟机房"，NetSim 是"会动的协议教科书"。**

---

## 功能特性

### 设备
- 💻 **PC** —— 配置 IP/掩码/网关/DNS，查看 ARP 表
- 🔌 **交换机**（24 端口）—— 查看实时 MAC 地址表
- 🌐 **路由器**（4 端口）—— 配置路由表，查看路由表/ARP 表/NAT 表，支持 RIP/OSPF 算法切换、链路拥塞状态切换
- 📖 **DNS 服务器** —— 配置域名解析记录
- 🌍 **网站** —— 作为通信目的端

### 模拟流程（逐步可见）
1. **DNS 解析** —— 区分同网段/跨网段查询
2. **网段判断** —— 检查目的 IP 是否在本网段
3. **路由查找** —— PC 查默认网关 / 路由器最长前缀匹配
4. **ARP 解析** —— 含缓存命中、广播查找、路由器回退
5. **二层转发** —— 交换机 MAC 学习、查表转发、未知泛洪
6. **三层转发** —— 路由器解封装、查路由表、重新封装、TTL 递减
7. **NAT 转换** —— 私网 IP ↔ 公网 IP 的 SNAT
8. **动态路由交换** —— RIP/OSPF 邻居路由器互相学习路由

### 可视化数据包查看
- **交换机** 只看 L2（以太网帧头）
- **路由器** 看 L3（IP 数据报）
- **PC/网站** 看 App 层（HTTP 请求）

—— 完美对应"不同设备工作在不同协议层"的教学点。

### 交互
- 🖱 拖拽设备布局，左键点击端口锚点连线
- 🖱 右键空白处取消连线，右键连线断开
- 🖱 双击设备打开配置，右键设备打开上下文菜单（查看各类表/数据包/日志）
- 🧰 工具栏：添加设备 / 清空 / 源/目的选择 / 开始模拟 / 路由交换 / 重置

---

## 截图

> 📌 截图待补充。请将主界面截图保存为 `docs/images/main-ui.png`，DNS/路由/数据包查看等关键场景保存到 `docs/images/`，下方引用会自动显示。

![主界面](docs/images/main-ui.png)

![数据包分层查看](docs/images/packet-view.png)

---

## 5 分钟上手

### 方式 1：下载编译好的 exe（推荐，零环境配置）

前往 [Releases](https://github.com/yuhou25/net-sim/releases)，下载最新的 `NetSim.zip`，解压后双击 `NetSim.exe` 即可运行。

> 仅需 Windows 7+ 和 .NET Framework 4.8（Windows 10/11 已预装）。

### 方式 2：源码编译

```bash
git clone https://github.com/yuhou25/net-sim.git
cd net-sim
```

**Visual Studio**：双击 `net-sim.csproj` 打开 → F5 运行

**命令行（MSBuild）**：
```bash
msbuild net-sim.csproj /p:Configuration=Release
# 输出位于 bin/Release/NetSim.exe
```

### 3 步完成第一次模拟

1. 启动后内置了一套示例拓扑：`PC-A → SW1 → R1 → SW2 → PC-B + DNS + www.gov.cn`
2. 工具栏右侧选择 **源** = `PC-A`，**目的** = `www.gov.cn`
3. 点击 **开始模拟** → 右侧日志区会逐行展示全过程

---

## 项目结构

```
net-sim/
├── Program.cs        # WinForms 入口
├── MainForm.cs       # UI 层：拓扑画布 / 工具栏 / 配置弹窗 / GDI+ 绘图 (~670 行)
├── Models.cs         # 数据模型：Device / Port / ARP/MAC/路由/NAT 表 / 分层 Packet (~320 行)
├── SimEngine.cs      # 核心引擎：逐跳追踪、ARP/路由/NAT/MAC 学习模拟 (~540 行)
├── net-sim.csproj    # .NET Framework 4.8 工程文件
└── .github/
    └── workflows/
        └── build.yml # CI：自动编译 + Tag 触发 Release
```

约 **1600 行代码**，零第三方依赖，纯 .NET Framework 4.8 + GDI+，源码可读性高，适合作为：
- 教学示范项目
- 二次开发起点
- 毕业设计/课程设计参考

---

## 协议覆盖

| OSI 层 | 覆盖知识点 | 状态 |
|---|---|:---:|
| 应用层 | HTTP 请求、DNS 解析 | ✅ |
| 传输层 | TCP/UDP 端口、序列号 | 🟡 (简化) |
| 网络层 | IP 路由、TTL、NAT、ICMP | 🟡 (无 ICMP) |
| 数据链路层 | ARP、以太网帧、MAC 学习/转发 | ✅ |
| 物理层 | —— | ❌ (不模拟) |

**动态路由**：RIP / OSPF（简化实现，仅演示邻居学习与代价计算）

---

## 适用场景

- 📚 **计算机网络原理课程**：替代枯燥的 PPT，让学生"看见"协议工作
- 🎓 **HCIA / CCNA 入门辅助**：理解 ARP、路由、交换、NAT 基础原理
- 💼 **面试复习工具**：快速回顾"浏览器输入 URL 到页面显示"全流程
- 🧪 **故障演示**：故意不配网关/不连线/删路由，让学生观察失败步骤、定位问题

---

## 局限性（坦诚说明）

- ✗ **非真实协议实现** —— 为简化教学，MAC/ARP 表采用预学习机制，不模拟真实协议报文交互
- ✗ **无 TCP 三次握手 / 分片 / ICMP** —— 当前聚焦于二层/三层转发主线
- ✗ **无 CLI 命令行** —— 不打算支持（CLI 模拟留给 GNS3/CPT）
- ✗ **仅 Windows** —— 基于 WinForms，跨平台支持需要重写 UI 层
- ✗ **无拓扑保存/加载** —— 后续计划支持

---

## 路线图

- [x] 基础拓扑搭建 + 通信模拟
- [x] ARP / MAC 学习 / 路由查找 / NAT / DNS
- [x] 分层 Packet 查看
- [ ] 📷 README 截图与 GIF
- [ ] 🎬 **数据包逐跳移动动画**（教学价值最高的下一个特性）
- [ ] 📚 **预设场景库**（"跨网段通信故障""NAT 原理""DNS 劫持"等一键加载）
- [ ] 🤝 TCP 三次握手可视化
- [ ] 💾 拓扑保存/加载（JSON）
- [ ] 🌐 Web 版（基于 Blazor/SVG 重写前端）

---

## 贡献

欢迎 Issue / PR。特别是以下方向：

- 🐛 复现并提交 Bug（请附拓扑截图、操作步骤、期望 vs 实际）
- 💡 教学场景建议（哪个知识点最需要可视化？）
- 🎨 UI 美化（图标、配色、动画）
- 📖 教案/习题配套（欢迎在 `docs/` 下贡献教学材料）

---

## License

[MIT License](LICENSE) © yuhou25

教学用途欢迎使用、修改、分发。商用请保留原作者署名。

---

## 致谢

灵感来源于：
- **Cisco Packet Tracer** —— 经典的网络教学模拟器
- **华为 eNSP** —— 国产网络模拟工具
- 所有把抽象的协议讲明白的计算机网络教材
