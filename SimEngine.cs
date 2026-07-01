using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetSim
{
    public class SimEngine
    {
        private List<Device> _devices;
        private Action _refresh;
        private Action<string> _onComplete;

        public List<SimStep> Steps { get; private set; } = new List<SimStep>();
        public int CurrentStep { get; set; } = -1;
        public string StatusMessage { get; set; } = "";
        public bool IsSimulating => CurrentStep >= 0 && CurrentStep < Steps.Count;

        public SimEngine(List<Device> devices, Action refresh, Action<string> onComplete)
        {
            _devices = devices; _refresh = refresh; _onComplete = onComplete;
        }

        public void Reset()
        {
            Steps.Clear(); CurrentStep = -1; StatusMessage = "";
            foreach (var d in _devices) { d.ClearLogs(); d.Packets.Clear(); }
        }

        public async void StartSimulation(Device src, Device dst)
        {
            Reset();
            await Task.Run(() => Simulate(src, dst));
            _refresh?.Invoke();
            _onComplete?.Invoke(StatusMessage);
        }

        private void Simulate(Device src, Device dst)
        {
            try
            {
                if (src == null || dst == null) { StatusMessage = "请选择源和目标设备"; return; }
                if (src.Type != DeviceType.PC) { StatusMessage = "源必须是PC"; return; }
                if (string.IsNullOrEmpty(src.IP)) { StatusMessage = "源PC需配置IP地址"; return; }

                // Check PC has cable connection
                if (!src.Ports.Any(p => p.IsConnected))
                {
                    AddStep(new SimStep { Title = "连接错误", Detail = $"PC {src.Name} 末连接任何设备，请先连线", Device = src, Layer = "IP", IsError = true });
                    StatusMessage = "失败：PC未连接线缆"; return;
                }

                // Pre-learn MACs on all switches from topology
                PreLearnMacs();

                // DNS resolution if needed
                string dstIP = dst.IP;
                if (dst.Type == DeviceType.WebSite || dst.Type == DeviceType.DNSServer)
                {
                    if (string.IsNullOrEmpty(dst.IP)) { StatusMessage = "目标未配置IP"; return; }
                    dstIP = dst.IP;
                }
                if (string.IsNullOrEmpty(dstIP)) { StatusMessage = "目标未配置IP"; return; }

                // DNS step if PC has DNS configured
                if (!string.IsNullOrEmpty(src.DnsServer) && dst.Type == DeviceType.WebSite)
                {
                    var dnsServer = _devices.FirstOrDefault(d => d.Type == DeviceType.DNSServer && d.IP == src.DnsServer);
                    string domain = dst.Name;
                    if (dnsServer != null)
                    {
                        var dnsEntry = dnsServer.DnsEntries.FirstOrDefault(e => e.IP == dst.IP);
                        if (dnsEntry != null) domain = dnsEntry.Domain;
                        bool dnsSameNet = IsSameNetwork(src.IP, src.DnsServer, src.Mask);
                        AddStep(new SimStep
                        {
                            Title = $"PC {src.Name} DNS查询",
                            Detail = dnsSameNet
                                ? $"向 DNS({src.DnsServer}) 查询 {domain} → 解析为 {dstIP}"
                                : $"向 DNS({src.DnsServer}) 跨网段查询 {domain} → 解析为 {dstIP}（经网关 {src.Gateways.FirstOrDefault() ?? "-"}）",
                            Device = src, Layer = "DNS"
                        });
                    }
                    else
                    {
                        AddStep(new SimStep
                        {
                            Title = $"PC {src.Name} DNS未找到",
                            Detail = $"配置DNS={src.DnsServer} 但拓扑中无此服务器",
                            Device = src, Layer = "DNS"
                        });
                    }
                }
                else if (dst.Type == DeviceType.WebSite) {
                    AddStep(new SimStep { Title = $"PC {src.Name} 无DNS配置", Detail = $"未配置DNS", Device = src, Layer = "DNS" });
                }

                var srcIP = src.IP; var srcMask = src.Mask;
                string srcMAC = "MAC:" + src.Id;
                string dstMAC = "MAC:" + dst.Id;

                // Step 1: Check if same network
                bool sameNet = IsSameNetwork(srcIP, dstIP, srcMask);
                AddStep(new SimStep
                {
                    Title = $"PC {src.Name} 检查目的地址",
                    Detail = sameNet
                        ? $"{dstIP} 与本机同一网段，直接发送"
                        : $"{dstIP} 不在本网段，需要经过网关",
                    Device = src, Layer = "IP"
                });

                string nextHopIP;
                if (sameNet) nextHopIP = dstIP;
                else
                {
                    if (src.Gateways.Count == 0)
                    {
                        AddStep(new SimStep { Title = "无网关配置", Detail = "PC未配置网关，无法到达非本网段地址", Device = src, Layer = "Route", IsError = true });
                        StatusMessage = "失败：未配网关"; return;
                    }
                    nextHopIP = src.Gateways[0];
                    AddStep(new SimStep
                    {
                        Title = $"PC {src.Name} 查路由表",
                        Detail = $"默认网关: {nextHopIP}",
                        Device = src, Layer = "Route"
                    });
                }

                // Step 2: ARP for next hop
                string nextHopMAC = ArpResolve(src, nextHopIP);
                if (nextHopMAC == null)
                {
                    AddStep(new SimStep { Title = "ARP 查询失败", Detail = $"无法解析 {nextHopIP} 的 MAC 地址", Device = src, Layer = "ARP", IsError = true });
                    StatusMessage = "失败：ARP超时"; return;
                }

                // Step 3: Send frame from src to next hop
                var pkt = new Packet { SrcMAC = srcMAC, DstMAC = nextHopMAC, SrcIP = srcIP, DstIP = dstIP, FrameSize = 1500, TotalLength = 1460, SeqNum = new Random().Next(1000, 9999), AppData = dst.Type == DeviceType.WebSite ? $"GET / HTTP/1.1\r\nHost: {dst.Name}" : $"DATA: {srcIP}->{dstIP}" };
                src.Packets.Add(pkt);
                AddStep(new SimStep
                {
                    Title = $"PC {src.Name} 发送帧",
                    Detail = $"源MAC={srcMAC} 目的MAC={nextHopMAC} 源IP={srcIP} 目的IP={dstIP}",
                    Device = src, Layer = "MAC"
                });

                // Step 4: Trace through network from PC's port
                var fromDevice = src;
                var port = src.Ports.FirstOrDefault();
                if (port == null || port.ConnectedTo == null)
                {
                    AddStep(new SimStep { Title = "PC未连接网络", Detail = "PC端口未连接任何设备", Device = src, Layer = "IP", IsError = true });
                    StatusMessage = "失败：PC未连接"; return;
                }
                var nextDevice = port.ConnectedTo.Device;
                var destMAC = nextHopMAC;
                bool reached = false;
                int maxHops = 20;

                while (nextDevice != null && maxHops-- > 0)
                {
                    if (nextDevice.Type == DeviceType.Switch)
                    {
                        // Learn source MAC
                        var inPort = FindConnectedPort(nextDevice, fromDevice);
                        nextDevice.MacTable.Learn(srcMAC, inPort?.Name ?? "");
                        AddStep(new SimStep
                        {
                            Title = $"交换机 {nextDevice.Name} MAC学习",
                            Detail = $"记录 {srcMAC} → 端口 {inPort?.Name ?? "-"}",
                            Device = nextDevice, Layer = "MAC"
                        });

                        // Look up destination MAC
                        string outPortName = nextDevice.MacTable.GetPort(destMAC);
                        if (outPortName == null)
                        {
                            AddStep(new SimStep
                            {
                                Title = $"交换机 {nextDevice.Name} 泛洪",
                                Detail = $"MAC表中无 {destMAC}，向所有端口泛洪",
                                Device = nextDevice, Layer = "MAC"
                            });
                            // Flood to all connected devices except source
                            foreach (var p in nextDevice.Ports)
                            {
                                if (p.ConnectedTo != null && p.ConnectedTo.Device != fromDevice)
                                {
                                    fromDevice = nextDevice;
                                    nextDevice = p.ConnectedTo.Device;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            var outPort = nextDevice.Ports.FirstOrDefault(p => p.Name == outPortName);
                            AddStep(new SimStep
                            {
                                Title = $"交换机 {nextDevice.Name} 转发",
                                Detail = $"{destMAC} → 端口 {outPortName}",
                                Device = nextDevice, Layer = "MAC"
                            });
                            if (outPort?.ConnectedTo == null) break;
                            fromDevice = nextDevice;
                            nextDevice = outPort.ConnectedTo.Device;
                        }
                        continue;
                    }
                    else if (nextDevice.Type == DeviceType.Router)
                    {
                        var router = nextDevice;
                        router.MacTable.Learn(srcMAC, FindConnectedPort(router, fromDevice)?.Name ?? "");
                        AddStep(new SimStep { Title = $"路由器 {router.Name} 接收帧", Detail = $"解封装查看IP层", Device = router, Layer = "MAC" });

                        var route = router.RoutingTable.FindRoute(dstIP);
                        if (route == null) { AddStep(new SimStep { Title = "路由查找失败", Detail = $"无到达 {dstIP} 的路由", Device = router, Layer = "Route", IsError = true }); StatusMessage = "失败：无路由"; return; }

                        AddStep(new SimStep { Title = $"路由器 {router.Name} 路由查找", Detail = $"匹配: {route.Network}/{route.Mask} → 下一跳 {route.NextHop} 端口 {route.OutPort}", Device = router, Layer = "Route" });

                        string newMAC = ArpResolve(router, string.IsNullOrEmpty(route.NextHop) || route.NextHop == "0.0.0.0" ? dstIP : route.NextHop, false);
                        if (newMAC == null) { AddStep(new SimStep { Title = "ARP失败", Detail = "无法解析下一跳MAC", Device = router, Layer = "ARP", IsError = true }); StatusMessage = "失败：下一跳ARP超时"; return; }

                        // NAT translation: only when going from private to public
                        string origSrcIP = srcIP;
                        if (router.NatTable.Enabled && IsPrivateIP(srcIP) && !IsPrivateIP(dstIP))
                        {
                            string natIP = router.NatTable.ExternalIP;
                            int idx = router.NatTable.Entries.Count + 1;
                            router.NatTable.Entries.Add(new NatEntry { InternalIP = srcIP, ExternalIP = natIP, Time = DateTime.Now });
                            srcIP = natIP;
                            AddStep(new SimStep { Title = $"路由器 {router.Name} NAT转换", Detail = $"SNAT: {origSrcIP} → {natIP}（外部地址）", Device = router, Layer = "NAT" });
                        }

                        string routerOutMAC = "MAC:" + router.Id + "_" + (route.OutPort ?? "out");
                        srcMAC = routerOutMAC;
                        destMAC = newMAC;

                        AddStep(new SimStep { Title = $"路由器 {router.Name} 重新封装帧", Detail = $"新源MAC={srcMAC} 新目的MAC={newMAC}", Device = router, Layer = "MAC" });
                        router.Packets.Add(new Packet { SrcMAC = srcMAC, DstMAC = newMAC, SrcIP = srcIP, DstIP = dstIP, TTL = 63, TotalLength = 1460, SeqNum = new Random().Next(1000, 9999) });

                        var outPort = router.Ports.FirstOrDefault(p => p.Name == route.OutPort);
                        if (outPort?.ConnectedTo == null) break;
                        fromDevice = router;
                        nextDevice = outPort.ConnectedTo.Device;
                    }
                        else if (nextDevice.Type == DeviceType.PC || nextDevice.Type == DeviceType.WebSite)
                        {
                            if (nextDevice == dst || nextDevice.Type == DeviceType.WebSite) { reached = true; AddStep(new SimStep { Title = $"{(nextDevice.Type == DeviceType.WebSite ? "网站" : "PC")} {dst.Name} 收到", Detail = $"正确到达。源={src.IP} 目的={dstIP}", Device = dst, Layer = "IP" }); StatusMessage = "成功到达！"; return; }
                            else break;
                        }
                }

                StatusMessage = "路由追踪异常结束";
            }
            catch (Exception ex)
            {
                StatusMessage = "模拟失败：" + ex.Message;
            }
        }

        private bool IsSameNetwork(string ip1, string ip2, string mask)
        {
            try
            {
                var b1 = System.Net.IPAddress.Parse(ip1).GetAddressBytes();
                var b2 = System.Net.IPAddress.Parse(ip2).GetAddressBytes();
                var bm = System.Net.IPAddress.Parse(mask).GetAddressBytes();
                for (int i = 0; i < 4; i++) if ((b1[i] & bm[i]) != (b2[i] & bm[i])) return false;
                return true;
            }
            catch { return false; }
        }

        private string ArpResolve(Device device, string ip, bool allowRouterFallback = true)
        {
            // Check local ARP cache
            var mac = device.ArpTable.GetMac(ip);
            if (mac != null)
            {
                AddStep(new SimStep { Title = $"ARP缓存命中", Detail = $"{ip} → {mac}", Device = device, Layer = "ARP" });
                return mac;
            }

            // Search through all connected switches for the target IP
            foreach (var port in device.Ports)
            {
                if (port.ConnectedTo != null)
                {
                    var found = FindPCByIP(port.ConnectedTo.Device, ip, new HashSet<string> { device.Id });
                    if (found != null)
                    {
                        string m = "MAC:" + found.Id;
                        device.ArpTable.Set(ip, m);
                        AddStep(new SimStep { Title = $"ARP 请求", Detail = $"谁有 {ip}？{found.Name} 回应: {m}", Device = device, Layer = "ARP" });
                        return m;
                    }
                }
            }

            // Fallback: if this is a gateway IP, search for any router on the path
            if (allowRouterFallback)
            {
                foreach (var port in device.Ports)
                {
                    if (port.ConnectedTo != null)
                    {
                        var foundRouter = FindRouterOnPath(port.ConnectedTo.Device, new HashSet<string> { device.Id });
                        if (foundRouter != null)
                        {
                            string m = "MAC:" + foundRouter.Id;
                            device.ArpTable.Set(ip, m);
                            AddStep(new SimStep { Title = $"ARP 请求", Detail = $"谁有 {ip}？路由器 {foundRouter.Name} 回应: {m}", Device = device, Layer = "ARP" });
                            return m;
                        }
                    }
                }
            }

            return null;
        }

        private Device FindRouterOnPath(Device start, HashSet<string> visited)
        {
            if (visited.Contains(start.Id)) return null;
            visited.Add(start.Id);
            if (start.Type == DeviceType.Router) return start;
            foreach (var port in start.Ports)
                if (port.ConnectedTo != null && !visited.Contains(port.ConnectedTo.Device.Id))
                {
                    var r = FindRouterOnPath(port.ConnectedTo.Device, visited);
                    if (r != null) return r;
                }
            return null;
        }

        private Device FindPCByIP(Device start, string ip, HashSet<string> visited)
        {
            if (visited.Contains(start.Id)) return null;
            visited.Add(start.Id);

            if ((start.Type == DeviceType.PC || start.Type == DeviceType.WebSite) && start.IP == ip) return start;

            foreach (var port in start.Ports)
            {
                if (port.ConnectedTo != null && !visited.Contains(port.ConnectedTo.Device.Id))
                {
                    var r = FindPCByIP(port.ConnectedTo.Device, ip, visited);
                    if (r != null) return r;
                }
            }
            return null;
        }

        private List<Device> GetConnectedDevices(Device device)
        {
            var list = new List<Device>();
            foreach (var port in device.Ports)
            {
                if (port.ConnectedTo != null) list.Add(port.ConnectedTo.Device);
            }
            return list;
        }

        private Port FindConnectedPort(Device device, Device other)
        {
            return device.Ports.FirstOrDefault(p => p.ConnectedTo?.Device == other);
        }

        private Device FindDeviceByMac(string mac)
        {
            foreach (var d in _devices)
            {
                if (d.Type == DeviceType.PC && ("MAC:" + d.Id) == mac) return d;
                if (d.Type == DeviceType.Router && mac.StartsWith("MAC:" + d.Id)) return d;
                if (d.Type == DeviceType.Switch && mac == "MAC:" + d.Id) return d;
            }
            return null;
        }

        private Device FindNextDevice(Device current, string targetMac)
        {
            // First try exact MAC match
            var d = FindDeviceByMac(targetMac);
            if (d != null && d != current) return d;

            // Try to find via port connections: flood to all connected devices
            foreach (var port in current.Ports)
            {
                if (port.ConnectedTo != null)
                {
                    var next = port.ConnectedTo.Device;
                    if (next.Type == DeviceType.PC && ("MAC:" + next.Id) == targetMac) return next;
                    if (next.Type == DeviceType.Router && targetMac.StartsWith("MAC:" + next.Id)) return next;
                    // For switch, follow any port
                    if (next.Type == DeviceType.Switch) return next;
                }
            }

            // Follow through switch flood
            foreach (var port in current.Ports)
            {
                if (port.ConnectedTo?.Device?.Type == DeviceType.Switch)
                {
                    var sw = port.ConnectedTo.Device;
                    foreach (var sp in sw.Ports)
                    {
                        if (sp.ConnectedTo?.Device != null && sp.ConnectedTo.Device != current)
                        {
                            var next = sp.ConnectedTo.Device;
                            if (next.Type == DeviceType.PC && ("MAC:" + next.Id) == targetMac) return next;
                            if (next.Type == DeviceType.Router && targetMac.StartsWith("MAC:" + next.Id)) return next;
                            if (next.Type == DeviceType.PC) return next;
                        }
                    }
                }
            }

            return null;
        }

        private void AddStep(SimStep step)
        {
            step.Index = Steps.Count;
            Steps.Add(step);
            if (step.Device != null && !step.IsError)
                step.Device.Logs.Add(new LogEntry { Time = DateTime.Now, Layer = step.Layer, Message = step.Detail });
        }

        private bool IsPrivateIP(string ip)
        {
            try
            {
                var b = System.Net.IPAddress.Parse(ip).GetAddressBytes();
                return (b[0] == 10) || (b[0] == 172 && b[1] >= 16 && b[1] <= 31) || (b[0] == 192 && b[1] == 168);
            }
            catch { return false; }
        }

        private void PreLearnMacs()
        {
            // Pre-populate MAC tables on switches and ARP tables on routers
            foreach (var dev in _devices)
            {
                if (dev.Type == DeviceType.PC && !string.IsNullOrEmpty(dev.IP))
                {
                    // Walk from PC through switches, learning MAC on each switch
                    var visited = new HashSet<string> { dev.Id };
                    var mac = "MAC:" + dev.Id;
                    PropagateMac(dev, mac, visited, 2);
                }
                if (dev.Type == DeviceType.Router)
                {
                    // Learn router MAC on connected switches
                    var visited = new HashSet<string> { dev.Id };
                    var mac = "MAC:" + dev.Id;
                    PropagateMac(dev, mac, visited, 2);
                }
            }

            // Pre-populate ARP on routers from connected PCs
            foreach (var router in _devices.Where(d => d.Type == DeviceType.Router))
            {
                foreach (var port in router.Ports)
                {
                    if (!port.IsConnected) continue;
                    var neighbor = port.ConnectedTo.Device;
                    WalkForArp(router, neighbor, new HashSet<string> { router.Id });
                }
            }

            // Pre-populate ARP on PCs from connected routers
            foreach (var pc in _devices.Where(d => d.Type == DeviceType.PC))
            {
                WalkForArp(pc, pc, new HashSet<string>());
            }
        }

        private void PropagateMac(Device from, string mac, HashSet<string> visited, int depth)
        {
            if (depth <= 0) return;
            foreach (var port in from.Ports)
            {
                if (!port.IsConnected) continue;
                var neighbor = port.ConnectedTo.Device;
                if (visited.Contains(neighbor.Id)) continue;
                visited.Add(neighbor.Id);

                if (neighbor.Type == DeviceType.Switch)
                {
                    var inPort = neighbor.Ports.FirstOrDefault(p => p.ConnectedTo?.Device == from);
                    if (inPort != null)
                    {
                        neighbor.MacTable.Learn(mac, inPort.Name);
                    }
                    PropagateMac(neighbor, mac, visited, depth - 1);
                }
            }
        }

        private void WalkForArp(Device requester, Device device, HashSet<string> visited)
        {
            if (visited.Contains(device.Id)) return;
            visited.Add(device.Id);

            if ((device.Type == DeviceType.PC || device.Type == DeviceType.WebSite) && !string.IsNullOrEmpty(device.IP))
                requester.ArpTable.Set(device.IP, "MAC:" + device.Id);

            if (device.Type == DeviceType.Router)
            {
                foreach (var p in device.Ports)
                {
                    if (!string.IsNullOrEmpty(p.Description))
                    {
                        var parts = p.Description.Split('/');
                        if (parts.Length > 0 && !string.IsNullOrEmpty(parts[0]))
                            requester.ArpTable.Set(parts[0], "MAC:" + device.Id);
                    }
                }
            }

            foreach (var port in device.Ports)
            {
                if (!port.IsConnected) continue;
                var neighbor = port.ConnectedTo.Device;
                if (neighbor.Type != DeviceType.DNSServer)
                    WalkForArp(requester, neighbor, visited);
            }
        }
    }

    public class SimStep
    {
        public int Index { get; set; }
        public string Title { get; set; }
        public string Detail { get; set; }
        public Device Device { get; set; }
        public string Layer { get; set; }
        public bool IsError { get; set; }
    }
}