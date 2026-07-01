# NetSim 单元测试用例记录

> 测试框架：NUnit 3.13.3 | 测试适配器：NUnit3TestAdapter 4.5.0 | 目标框架：.NET Framework 4.8

---

## 一、ModelsTests（数据模型测试）

### 1. Device 构造测试

| 编号 | 测试名称 | 测试输入 | 预期结果 |
|------|----------|----------|----------|
| D01 | `PC_Creation_ShouldHaveSinglePort` | `new Device(DeviceType.PC, "TestPC")` | Ports.Count=1, Ports[0].Name="eth0" |
| D02 | `PC_Creation_ShouldHaveSize80x70` | 同上 | Size=(80, 70) |
| D03 | `Switch_Creation_ShouldHave24Ports` | `new Device(DeviceType.Switch, "TestSW")` | Ports.Count=24 |
| D04 | `Switch_Creation_ShouldHavePorts1To12OnLeft` | 同上 | Ports[0..11] Side=Left, Name=P1..P12 |
| D05 | `Switch_Creation_ShouldHavePorts13To24OnRight` | 同上 | Ports[12..23] Side=Right, Name=P13..P24 |
| D06 | `Switch_Creation_ShouldHaveSize120x180` | 同上 | Size=(120, 180) |
| D07 | `Router_Creation_ShouldHave4Ports` | `new Device(DeviceType.Router, "TestRouter")` | Ports.Count=4, 名称依次为 eth0/eth1/eth2/eth3 |
| D08 | `Router_Creation_ShouldHaveEth0Eth1Left_Eth2Eth3Right` | 同上 | eth0/eth1 Side=Left, eth2/eth3 Side=Right |
| D09 | `Router_Creation_ShouldHaveNatEnabled` | 同上 | NatTable.Enabled=true |
| D10 | `Router_Creation_ShouldHaveSize100x80` | 同上 | Size=(100, 80) |
| D11 | `DNSServer_Creation_ShouldHaveDefaultDnsEntries` | `new Device(DeviceType.DNSServer, "DNS1")` | DnsEntries.Count=2, 含 www.gov.cn→203.0.113.10 和 www.bank.com→203.0.113.20 |
| D12 | `DNSServer_Creation_ShouldDefaultIP` | 同上 | IP="8.8.8.8" |
| D13 | `DNSServer_Creation_ShouldHaveSize80x60` | 同上 | Size=(80, 60) |
| D14 | `WebSite_Creation_ShouldDefaultIP` | `new Device(DeviceType.WebSite, "www.test.com")` | IP="203.0.113.10" |
| D15 | `WebSite_Creation_ShouldHaveSize80x50` | 同上 | Size=(80, 50) |
| D16 | `WebSite_Creation_ShouldHaveSinglePort` | 同上 | Ports.Count=1, Ports[0].Name="eth0" |

### 2. Device 通用属性测试

| 编号 | 测试名称 | 测试输入 | 预期结果 |
|------|----------|----------|----------|
| D17 | `Device_Creation_ShouldGenerateId_Length6` | `new Device(DeviceType.PC, "TestPC")` | Id 非空且长度为 6 |
| D18 | `Device_Creation_ShouldHaveUniqueIds` | 创建两个 PC 设备 | 两个 Id 互不相同 |
| D19 | `Device_Creation_ShouldSetName` | `new Device(DeviceType.PC, "MyPC")` | Name="MyPC" |
| D20 | `Device_Creation_ShouldSetType` | 分别创建 5 种类型设备 | Type 与构造参数一致 |
| D21 | `PC_DefaultMask_ShouldBe255_255_255_0` | `new Device(DeviceType.PC, "PC1")` | Mask="255.255.255.0" |
| D22 | `PC_DefaultIP_ShouldBeEmpty` | 同上 | IP="" |
| D23 | `PC_DefaultGateways_ShouldBeEmpty` | 同上 | Gateways 为空列表 |
| D24 | `Device_DefaultSelected_ShouldBeFalse` | 同上 | Selected=false |
| D25 | `Device_DefaultStatus_ShouldBeNormal` | 同上 | Status=RouterStatus.Normal |
| D26 | `Device_DefaultAlgorithm_ShouldBeRIP` | 同上 | Algorithm=RoutingAlgo.RIP |
| D27 | `Device_DefaultPackets_ShouldBeEmpty` | 同上 | Packets 为空 |
| D28 | `Device_DefaultLogs_ShouldBeEmpty` | 同上 | Logs 为空 |

### 3. Device 方法测试

| 编号 | 测试名称 | 测试输入 | 预期结果 |
|------|----------|----------|----------|
| D29 | `AddPort_ShouldAddPortWithCorrectProperties` | 调用 AddPort("eth1", "test desc", PortSide.Left) | Ports.Count=2, 新端口属性匹配 |
| D30 | `AddPort_DefaultParams_ShouldUseRightSide` | 调用 AddPort("eth1") | 新端口 Side=Right |
| D31 | `LogText_WithNoLogs_ShouldBeEmpty` | 无日志的 PC | LogText="" |
| D32 | `LogText_WithLogs_ShouldJoinWithCrLf` | 添加两条日志 | 包含 "\r\n" 分隔且包含日志内容 |
| D33 | `ClearLogs_ShouldEmptyList` | 添加一条日志后清除 | Logs 为空 |

### 4. Port 测试

| 编号 | 测试名称 | 测试输入 | 预期结果 |
|------|----------|----------|----------|
| P01 | `Constructor_ShouldSetProperties` | `new Port("eth1", "desc", device, PortSide.Left)` | Name/Description/Device/Side 正确 |
| P02 | `Anchor_RightSide_ShouldCalculateCorrectly` | Location=(100,200), Size=(80,70), 右侧端口 | Anchor.X=180 |
| P03 | `Anchor_LeftSide_ShouldCalculateCorrectly` | Location=(100,200), 左侧端口 | Anchor.X=100 |
| P04 | `IsConnected_WhenNull_ShouldReturnFalse` | ConnectedTo=null | IsConnected=false |
| P05 | `IsConnected_WhenConnected_ShouldReturnTrue` | 设置双向连接 | IsConnected=true |
| P06 | `ConnectedDeviceName_WhenNotConnected_ShouldReturnDash` | ConnectedTo=null | ConnectedDeviceName="-" |
| P07 | `ConnectedDeviceName_WhenConnected_ShouldReturnDeviceName` | 连接到 PC2 | ConnectedDeviceName="PC2" |
| P08 | `ConnectedPortName_WhenNotConnected_ShouldReturnDash` | ConnectedTo=null | ConnectedPortName="-" |
| P09 | `ConnectedPortName_WhenConnected_ShouldReturnPortName` | 连接到 PC2 的 eth0 | ConnectedPortName="eth0" |
| P10 | `Index_ShouldReturnCorrectPosition` | Switch 设备各端口 | Ports[0].Index=0, Ports[12].Index=12, Ports[23].Index=23 |
| P11 | `ConnectedTo_DefaultShouldBeNull` | 新建端口 | ConnectedTo=null |

### 5. ArpTable 测试

| 编号 | 测试名称 | 测试输入 | 预期结果 |
|------|----------|----------|----------|
| A01 | `Set_NewEntry_ShouldAdd` | Set("192.168.1.1", "MAC-1") | Entries.Count=1, IP/MAC 正确 |
| A02 | `Set_ExistingEntry_ShouldUpdateMacAndTime` | 重复 Set 同一 IP | Count 不变, MAC 更新, Time 更新 |
| A03 | `GetMac_ExistingIP_ShouldReturnMac` | 查询已有 IP | 返回对应 MAC |
| A04 | `GetMac_NonExistingIP_ShouldReturnNull` | 查询不存在的 IP | 返回 null |
| A05 | `Entries_DefaultShouldBeEmpty` | 新建 ArpTable | Entries 为空 |
| A06 | `Set_MultipleEntries_ShouldNotOverwriteUnrelated` | 添加 3 个不同条目 | Count=3, 各条目互不影响 |

### 6. MacTable 测试

| 编号 | 测试名称 | 测试输入 | 预期结果 |
|------|----------|----------|----------|
| M01 | `Learn_NewEntry_ShouldAdd` | Learn("MAC-1", "P1") | Entries.Count=1, MAC/Port 正确 |
| M02 | `Learn_ExistingEntry_ShouldUpdatePortAndTime` | 重复 Learn 同一 MAC | Count 不变, Port 更新, Time 更新 |
| M03 | `GetPort_ExistingMAC_ShouldReturnPort` | 查询已知 MAC | 返回对应端口 |
| M04 | `GetPort_NonExistingMAC_ShouldReturnNull` | 查询未知 MAC | 返回 null |
| M05 | `Entries_DefaultShouldBeEmpty` | 新建 MacTable | Entries 为空 |
| M06 | `Learn_MultipleEntries_ShouldTrackCorrectly` | 添加 3 个条目 | Count=3, 各条目可独立查询 |

### 7. RoutingTable 测试

| 编号 | 测试名称 | 测试输入 | 预期结果 |
|------|----------|----------|----------|
| R01 | `FindRoute_ExactMatch_ShouldReturnRoute` | 192.168.1.0/24，查询 192.168.1.100 | 匹配该路由的 Network/NextHop/OutPort |
| R02 | `FindRoute_LongestPrefixMatch_ShouldReturnMoreSpecific` | 两条路由：10.0.0.0/8 和 10.1.0.0/16，查询 10.1.2.3 | 返回 /16 的精准路由 |
| R03 | `FindRoute_DefaultRoute_ShouldMatch` | 0.0.0.0/0 默认路由 | 任意 IP 均匹配 |
| R04 | `FindRoute_NoMatch_ShouldReturnNull` | 仅 192.168.1.0/24，查询 10.0.0.1 | 返回 null |
| R05 | `FindRoute_OutPortNull_ShouldNotMatch` | 默认路由但 OutPort=null | 返回 null |
| R06 | `FindRoute_EmptyTable_ShouldReturnNull` | 空路由表 | 返回 null |
| R07 | `FindRoute_Mask24Match_ShouldWork` | 172.16.0.0/24 | 172.16.0.50 匹配，172.16.1.50 不匹配 |
| R08 | `FindRoute_InvalidIP_ShouldNotThrow` | 查询 "not-an-ip" | 不抛异常，返回 null |
| R09 | `FindRoute_TieBreakByPrefixLen_ShouldPickLongest` | 192.168.0.0/16 和 192.168.1.0/24 | 查询 192.168.1.100 返回 /24 路由 |

### 8. Packet 测试

| 编号 | 测试名称 | 测试输入 | 预期结果 |
|------|----------|----------|----------|
| PK01 | `DefaultValues_ShouldBeSet` | `new Packet()` | EtherType/TTL/Protocol/Flags/SrcPort/DstPort/TcpFlags/AppProtocol/AppData 均有默认值 |
| PK02 | `GetL2View_ShouldContainSourceAndDestinationMacs` | 设置 SrcMAC/DstMAC/FrameSize | 视图中包含两个 MAC 和帧大小、Ethernet Frame 标识 |
| PK03 | `GetL3View_ShouldContainIPInfo` | 设置 SrcIP/DstIP/TotalLength | 视图中包含 IP 地址、长度、IPv4、IP Datagram 标识 |
| PK04 | `GetL4View_ShouldContainPortInfo` | 设置 SrcPort/DstPort/SeqNum/AckNum | 视图中包含端口号、序号、确认号、TCP Segment 标识 |
| PK05 | `GetAppView_ShouldContainAppData` | 设置 AppProtocol/AppData | 视图中包含协议名和请求数据 |
| PK06 | `GetL2View_ShouldContainEtherType` | 设置 EtherType="0x86DD(IPv6)" | 视图中包含 EtherType |
| PK07 | `GetL3View_ShouldContainTtlAndProtocol` | 设置 TTL=128, Protocol="UDP" | 视图中包含 TTL 和协议名 |
| PK08 | `GetL4View_ShouldContainTcpFlags` | 设置 TcpFlags="SYN" | 视图中包含 TCP 标志 |

### 9. LogEntry / NatTable / DnsEntry / SimStep 测试

| 编号 | 测试名称 | 测试输入 | 预期结果 |
|------|----------|----------|----------|
| L01 | `ToString_ShouldFormatCorrectly` | Time=14:30:45, Layer="ARP", Message="Request sent" | "[14:30:45][ARP] Request sent" |
| L02 | `ToString_ShouldHandleAllLayers` | 逐一测试 ARP/MAC/IP/Route/DNS/NAT | 每种 Layer 均正确显示 |
| L03 | `Time_CanBeSet` | 显式设置 Time | Time 值正确保存 |
| N01 | `NatTable_Default_Enabled_ShouldBeFalse` | 新建 NatTable | Enabled=false |
| N02 | `NatTable_Default_ExternalIP_ShouldBe203_0_113_1` | 新建 NatTable | ExternalIP="203.0.113.1" |
| N03 | `NatTable_Entries_DefaultShouldBeEmpty` | 新建 NatTable | Entries 为空 |
| DNS01 | `DnsEntry_ShouldStoreDomainAndIP` | Domain="example.com", IP="1.2.3.4" | Domain/IP 正确 |
| SS01 | `SimStep_DefaultValues_ShouldBeSet` | 新建 SimStep | Index=0, IsError=false, Title/Detail/Device/Layer 为 null |

---

## 二、SimEngineTests（仿真引擎测试）

### 10. IsSameNetwork 子网判断

| 编号 | 测试名称 | 测试输入 | 预期结果 |
|------|----------|----------|----------|
| S01 | `IsSameNetwork_SameSubnet_ReturnsTrue` | 192.168.1.10 与 192.168.1.20, 掩码 255.255.255.0 | true |
| S02 | `IsSameNetwork_DifferentSubnet_ReturnsFalse` | 192.168.1.10 与 192.168.2.20, 掩码 255.255.255.0 | false |
| S03 | `IsSameNetwork_SameSubnetSlash16_ReturnsTrue` | 172.16.1.1 与 172.16.100.1, 掩码 255.255.0.0 | true |
| S04 | `IsSameNetwork_DifferentSubnetSlash16_ReturnsFalse` | 172.16.1.1 与 172.17.1.1, 掩码 255.255.0.0 | false |
| S05 | `IsSameNetwork_SameNetwork10_ReturnsTrue` | 10.0.0.1 与 10.255.255.254, 掩码 255.0.0.0 | true |
| S06 | `IsSameNetwork_InvalidIP_ReturnsFalse` | "invalid" 与 192.168.1.1 | false（异常降级） |
| S07 | `IsSameNetwork_InvalidMask_ReturnsFalse` | 正常 IP 但非法掩码 | false（异常降级） |
| S08 | `IsSameNetwork_NullInputs_ReturnsFalse` | 逐一测试 IP1/IP2/Mask 为 null | 均返回 false |
| S09 | `IsSameNetwork_BroadcastAddress_SameSubnet_ReturnsTrue` | 192.168.1.255 与 192.168.1.1, /24 | true |
| S10 | `IsSameNetwork_NetworkAddress_SameSubnet_ReturnsTrue` | 192.168.1.0 与 192.168.1.1, /24 | true |

### 11. IsPrivateIP 内网地址判断

| 编号 | 测试名称 | 测试输入 | 预期结果 |
|------|----------|----------|----------|
| P01 | `IsPrivateIP_192_168_x_x_ReturnsTrue` | 192.168.0.1 / 192.168.1.1 / 192.168.255.254 | 全部 true |
| P02 | `IsPrivateIP_10_x_x_x_ReturnsTrue` | 10.0.0.1 / 10.255.255.254 | 全部 true |
| P03 | `IsPrivateIP_172_16_x_x_Through_172_31_x_x_ReturnsTrue` | 172.16.0.1 / 172.20.0.1 / 172.31.255.254 | 全部 true |
| P04 | `IsPrivateIP_172_15_x_x_ReturnsFalse` | 172.15.0.1 | false |
| P05 | `IsPrivateIP_172_32_x_x_ReturnsFalse` | 172.32.0.1 | false |
| P06 | `IsPrivateIP_PublicIPs_ReturnsFalse` | 8.8.8.8 / 1.1.1.1 / 203.0.113.10 / 100.64.0.1 | 全部 false |
| P07 | `IsPrivateIP_InvalidFormat_ReturnsFalse` | "not-an-ip" / "" / null | 全部 false（异常降级） |
| P08 | `IsPrivateIP_Loopback_ReturnsFalse` | 127.0.0.1 | false |

### 12. SimEngine 构造与 Reset

| 编号 | 测试名称 | 测试输入 | 预期结果 |
|------|----------|----------|----------|
| E01 | `Constructor_ShouldInitializeSteps` | 创建 SimEngine | Steps 非空且为空列表 |
| E02 | `Constructor_ShouldInitializeCurrentStepToNegative` | 同上 | CurrentStep=-1 |
| E03 | `IsSimulating_ShouldReturnFalse_WhenNoSteps` | 无仿真步骤 | IsSimulating=false |
| E04 | `StatusMessage_DefaultIsEmpty` | 新建引擎 | StatusMessage="" |
| E05 | `Reset_ShouldClearStepsAndCurrentStep` | 添加 Step 后 Reset | Steps 清空, CurrentStep=-1, StatusMessage="" |
| E06 | `Reset_ShouldClearAllDeviceLogsAndPackets` | 设备有日志和报文后 Reset | Logs 和 Packets 均清空 |

### 13. ArpResolve ARP 解析

| 编号 | 测试名称 | 拓扑 | 预期结果 |
|------|----------|------|----------|
| ARP01 | `ArpResolve_CacheHit_ReturnsMac` | PC1 的 ARP 表已有 192.168.1.2 的条目 | 直接返回缓存 MAC |
| ARP02 | `ArpResolve_DirectConnection_FindsPC` | PC1 -- PC2 直连 | 通过遍历找到 PC2 MAC |
| ARP03 | `ArpResolve_ThroughSwitch_FindsPC` | PC1 -- SW1 -- PC2 | 通过交换机遍历找到 PC2 |
| ARP04 | `ArpResolve_NotFound_ReturnsNull` | 孤立 PC，查询不存在的 IP | 返回 null |

### 14. FindPCByIP 按 IP 查找设备

| 编号 | 测试名称 | 拓扑 | 预期结果 |
|------|----------|------|----------|
| F01 | `FindPCByIP_DirectlyConnected_ReturnsPC` | PC1(192.168.1.1) -- PC2(192.168.1.2) | 从 PC1 找到 PC2 |
| F02 | `FindPCByIP_ThroughSwitch_ReturnsPC` | PC1 -- SW1 -- PC2 | 从 PC1 经交换机找到 PC2 |
| F03 | `FindPCByIP_NotFound_ReturnsNull` | 孤立 PC，查询不存在的 IP | 返回 null |
| F04 | `FindPCByIP_VisitedPreventsLoop` | PC1 -- PC2 环形连接，PC1 已访问 | 从 PC2 查询 PC1 时因 visited 控制返回 null |

### 15. FindRouterOnPath 路径中查找路由器

| 编号 | 测试名称 | 拓扑 | 预期结果 |
|------|----------|------|----------|
| FR01 | `FindRouterOnPath_ThroughSwitch_FindsRouter` | PC1 -- SW1 -- R1 | 从 SW1 找到 R1 |
| FR02 | `FindRouterOnPath_NoRouter_ReturnsNull` | PC1 -- PC2（无路由器） | 返回 null |

### 16. PreLearnMacs MAC/ARP 预学习

| 编号 | 测试名称 | 拓扑 | 预期结果 |
|------|----------|------|----------|
| PL01 | `PreLearnMacs_PCToSwitch_PopulatesSwitchMacTable` | PC1 -- SW1 -- PC2 | SW1 的 MAC 表包含 PC1 和 PC2 的 MAC |
| PL02 | `PreLearnMacs_PCSwitchRouter_PopulatesARPTables` | PC1 -- SW1 -- R1 | SW1 MAC 表含 R1 MAC，R1 ARP 表含 PC1 IP |

### 17. Simulate 输入校验（异常场景）

| 编号 | 测试名称 | 测试输入 | 预期结果 |
|------|----------|----------|----------|
| V01 | `Simulate_SourceNull_Fails` | src=null | StatusMessage="请选择源和目标设备" |
| V02 | `Simulate_DestinationNull_Fails` | dst=null | StatusMessage="请选择源和目标设备" |
| V03 | `Simulate_SourceNotPC_Fails` | src=Switch | StatusMessage="源必须是PC" |
| V04 | `Simulate_SourceNoIP_Fails` | PC 无 IP | StatusMessage="源PC需配置IP地址" |
| V05 | `Simulate_SourceNotConnected_Fails` | PC 未连线 | StatusMessage 包含 "失败：PC未连接线缆" |
| V06 | `Simulate_DestinationNoIP_Fails` | WebSite 无 IP | StatusMessage="目标未配置IP" |

### 18. Simulate 同网段通信

| 编号 | 测试名称 | 拓扑 | 预期结果 |
|------|----------|------|----------|
| SS01 | `Simulate_PCToPCSameSubnetViaSwitch_ShouldSucceed` | PC1(192.168.1.1/24) -- SW1 -- PC2(192.168.1.2/24) | 成功到达, Steps≥2, PC2 有日志 |
| SS02 | `Simulate_PCToPCDirect_ShouldSucceed` | PC1 -- PC2 (同子网直连) | 成功到达 |
| SS03 | `Simulate_PCToPCSameSubnet_CreatesPacketOnSource` | 同上 | PC1.Packets 包含 SrcIP/DstIP 正确的报文 |
| SS04 | `Simulate_PCToPCSameSubnet_HasCorrectStepSequence` | 同上 | Steps 包含检查目的地址(IP层)→ARP→发送帧(MAC层) |
| SS05 | `Simulate_PCToSameSubnet_ShouldLogIPLayerSteps` | 同上 | Steps 包含 IP 和 MAC 层记录 |

### 19. Simulate 跨网段通信

| 编号 | 测试名称 | 拓扑 | 预期结果 |
|------|----------|------|----------|
| RT01 | `Simulate_DifferentSubnetNoGateway_Fails` | PC1(192.168.1.1) -- PC2(10.0.0.1)，无网关 | StatusMessage 包含 "失败：未配网关" |
| RT02 | `Simulate_DifferentSubnetViaRouter_ShouldSucceed` | PC1(192.168.1.10, gateway=192.168.1.1) -- R1 -- PC2(10.0.0.10)，R1 路由 10.0.0.0/24→eth1 | 成功到达 |
| RT03 | `Simulate_DifferentSubnet_ShouldHaveRouteStep` | 同上 | Steps 包含 Route 层和查路由表步骤 |
| RT04 | `Simulate_RouterNoRoute_Fails` | 同 RT02 但 R1 无路由表 | StatusMessage 包含 "失败：无路由" |

### 20. Simulate NAT 测试

| 编号 | 测试名称 | 拓扑 | 预期结果 |
|------|----------|------|----------|
| NAT01 | `Simulate_PrivateToPublic_ShouldHaveNATStep` | PC1(192.168.1.10) -- R1(SNAT=203.0.113.50) -- WebServer(8.8.8.8) | Steps 包含 NAT 层记录 |
| NAT02 | `Simulate_PrivateToPublic_ShouldAddNatEntry` | 同上 | R1.NatTable.Entries 新增条目 InternalIP=192.168.1.10, ExternalIP=203.0.113.50 |

### 21. Simulate DNS 测试

| 编号 | 测试名称 | 拓扑 | 预期结果 |
|------|----------|------|----------|
| DNS01 | `Simulate_PCWithDNSToWebSite_ShouldHaveDNSStep` | PC1(DNS=8.8.8.8) -- DNS服务器 -- WebSite(www.gov.cn) | Steps 包含 DNS 层 |
| DNS02 | `Simulate_PCWithoutDNSToWebSite_ShouldHaveNoDnsStep` | PC1(无 DNS) -- WebSite(203.0.113.10) | Steps 包含 "无DNS配置" |
| DNS03 | `Simulate_PCWithDNSToPC_ShouldHaveNoDnsStep` | PC1(有 DNS) -- PC2(同子网) | 无 DNS 步骤 |

### 22. 公共 API 集成测试

| 编号 | 测试名称 | 测试输入 | 预期结果 |
|------|----------|----------|----------|
| API01 | `StartSimulation_ShouldResetAndRun` | PC1→PC2 同网段 | 回调参数含 "成功"，refresh 回调被触发 |

---

## 测试统计

| 类别 | 测试数量 |
|------|----------|
| Device 构造 | 16 |
| Device 属性与方法 | 17 |
| Port | 11 |
| ArpTable | 6 |
| MacTable | 6 |
| RoutingTable | 9 |
| Packet | 8 |
| LogEntry / NatTable / DnsEntry / SimStep | 6 |
| IsSameNetwork | 10 |
| IsPrivateIP | 8 |
| SimEngine 生命周期 | 6 |
| ArpResolve | 4 |
| FindPCByIP | 4 |
| FindRouterOnPath | 2 |
| PreLearnMacs | 2 |
| Simulate 输入校验 | 6 |
| Simulate 同网段 | 5 |
| Simulate 跨网段 | 4 |
| Simulate NAT | 2 |
| Simulate DNS | 3 |
| 公共 API | 1 |
| **总计** | **138** |
