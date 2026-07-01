using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("NetSim.Tests")]

namespace NetSim
{
    public enum DeviceType { PC, Switch, Router, DNSServer, WebSite }
    public enum RouterStatus { Normal, Congested }
    public enum RoutingAlgo { RIP, OSPF }

    public class Device
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DeviceType Type { get; set; }
        public Point Location { get; set; }
        public Size Size { get; set; }
        public List<Port> Ports { get; set; } = new List<Port>();
        public ArpTable ArpTable { get; set; } = new ArpTable();
        public MacTable MacTable { get; set; } = new MacTable();
        public RoutingTable RoutingTable { get; set; } = new RoutingTable();
        public NatTable NatTable { get; set; } = new NatTable();
        public List<DnsEntry> DnsEntries { get; set; } = new List<DnsEntry>();
        public List<LogEntry> Logs { get; set; } = new List<LogEntry>();
        public List<Packet> Packets { get; set; } = new List<Packet>();
        public bool Selected { get; set; }
        public RouterStatus Status { get; set; } = RouterStatus.Normal;
        public RoutingAlgo Algorithm { get; set; } = RoutingAlgo.RIP;
        public int NextPortNum { get; set; } = 1;

        // PC specific
        public string IP { get; set; } = "";
        public string Mask { get; set; } = "255.255.255.0";
        public List<string> Gateways { get; set; } = new List<string>();
        public string DnsServer { get; set; } = "";

        public Device(DeviceType type, string name)
        {
            Type = type; Name = name; Id = Guid.NewGuid().ToString("N").Substring(0, 6);
            if (type == DeviceType.PC)
            {
                Size = new Size(80, 70);
                Ports.Add(new Port("eth0", "", this));
            }
            else if (type == DeviceType.Switch)
            {
                Size = new Size(120, 180);
                for (int i = 1; i <= 12; i++) Ports.Add(new Port("P" + i, "", this, PortSide.Left));
                for (int i = 13; i <= 24; i++) Ports.Add(new Port("P" + i, "", this, PortSide.Right));
            }
            else if (type == DeviceType.Router)
            {
                Size = new Size(100, 80);
                Ports.Add(new Port("eth0", "LAN", this, PortSide.Left));
                Ports.Add(new Port("eth1", "LAN", this, PortSide.Left));
                Ports.Add(new Port("eth2", "WAN", this, PortSide.Right));
                Ports.Add(new Port("eth3", "WAN", this, PortSide.Right));
                NatTable.Enabled = true;
            }
            else if (type == DeviceType.DNSServer)
            {
                Size = new Size(80, 60);
                IP = "8.8.8.8"; Name = name;
                Ports.Add(new Port("eth0", "", this));
                DnsEntries.Add(new DnsEntry { Domain = "www.gov.cn", IP = "203.0.113.10" });
                DnsEntries.Add(new DnsEntry { Domain = "www.bank.com", IP = "203.0.113.20" });
            }
            else if (type == DeviceType.WebSite)
            {
                Size = new Size(80, 50);
                IP = "203.0.113.10"; Name = name;
                Ports.Add(new Port("eth0", "", this));
            }
        }

        public Port AddPort(string name, string desc = "", PortSide side = PortSide.Right)
        {
            var p = new Port(name, desc, this, side);
            Ports.Add(p);
            return p;
        }

        public string LogText => string.Join("\r\n", Logs.Select(l => l.ToString()));
        public void ClearLogs() => Logs.Clear();
    }

    public enum PortSide { Left, Right }

    public class Port
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Device Device { get; set; }
        public Port ConnectedTo { get; set; }
        public PortSide Side { get; set; } = PortSide.Right;
        public int Index => Device.Ports.IndexOf(this);

        public Point Anchor
        {
            get
            {
                var d = Device;
                int total = d.Ports.Count(p => p.Side == Side);
                int idx = d.Ports.Where(p => p.Side == Side).ToList().IndexOf(this);
                if (idx < 0) idx = Index;
                int spacing = Math.Min(12, d.Size.Height / Math.Max(1, total));
                int x = Side == PortSide.Right ? d.Location.X + d.Size.Width : d.Location.X;
                int y = d.Location.Y + 3 + idx * spacing;
                return new Point(x, y);
            }
        }

        public Port(string name, string desc, Device device, PortSide side = PortSide.Right)
        {
            Name = name; Description = desc; Device = device; Side = side;
        }

        public bool IsConnected => ConnectedTo != null;
        public string ConnectedDeviceName => ConnectedTo?.Device?.Name ?? "-";
        public string ConnectedPortName => ConnectedTo?.Name ?? "-";
    }

    public class ArpEntry
    {
        public string IP { get; set; }
        public string MAC { get; set; }
        public DateTime Time { get; set; } = DateTime.Now;
    }

    public class ArpTable
    {
        public List<ArpEntry> Entries { get; set; } = new List<ArpEntry>();
        public string GetMac(string ip) => Entries.FirstOrDefault(e => e.IP == ip)?.MAC;
        public void Set(string ip, string mac)
        {
            var e = Entries.FirstOrDefault(x => x.IP == ip);
            if (e != null) { e.MAC = mac; e.Time = DateTime.Now; }
            else Entries.Add(new ArpEntry { IP = ip, MAC = mac });
        }
    }

    public class MacEntry
    {
        public string MAC { get; set; }
        public string PortName { get; set; }
        public DateTime Time { get; set; } = DateTime.Now;
    }

    public class MacTable
    {
        public List<MacEntry> Entries { get; set; } = new List<MacEntry>();
        public string GetPort(string mac) => Entries.FirstOrDefault(e => e.MAC == mac)?.PortName;
        public void Learn(string mac, string port)
        {
            var e = Entries.FirstOrDefault(x => x.MAC == mac);
            if (e != null) { e.PortName = port; e.Time = DateTime.Now; }
            else Entries.Add(new MacEntry { MAC = mac, PortName = port });
        }
    }

    public class RouteEntry
    {
        public string Network { get; set; }
        public string Mask { get; set; }
        public string NextHop { get; set; }
        public string OutPort { get; set; }
        public int Cost { get; set; }
    }

    public class NatEntry
    {
        public string InternalIP { get; set; }
        public string ExternalIP { get; set; }
        public string InternalPort { get; set; }
        public string ExternalPort { get; set; }
        public DateTime Time { get; set; } = DateTime.Now;
    }

    public class NatTable
    {
        public List<NatEntry> Entries { get; set; } = new List<NatEntry>();
        public bool Enabled { get; set; } = false;
        public string ExternalIP { get; set; } = "203.0.113.1";
    }

    public class DnsEntry
    {
        public string Domain { get; set; }
        public string IP { get; set; }
    }

    public class RoutingTable
    {
        public List<RouteEntry> Entries { get; set; } = new List<RouteEntry>();
        public RouteEntry FindRoute(string destIP)
        {
            RouteEntry best = null; int bestLen = -1;
            foreach (var r in Entries)
            {
                if (Match(destIP, r.Network, r.Mask))
                {
                    int len = PrefixLen(r.Mask);
                    if (len > bestLen && r.OutPort != null) { best = r; bestLen = len; }
                }
            }
            return best;
        }
        private bool Match(string ip, string net, string mask)
        {
            try
            {
                var ib = System.Net.IPAddress.Parse(ip).GetAddressBytes();
                var nb = System.Net.IPAddress.Parse(net).GetAddressBytes();
                var mb = System.Net.IPAddress.Parse(mask).GetAddressBytes();
                for (int i = 0; i < 4; i++) if ((ib[i] & mb[i]) != (nb[i] & mb[i])) return false;
                return true;
            }
            catch { return false; }
        }
        private int PrefixLen(string mask)
        {
            try
            {
                int count = 0;
                foreach (var b in System.Net.IPAddress.Parse(mask).GetAddressBytes()) count += CountBits(b);
                return count;
            }
            catch { return 0; }
        }
        private int CountBits(byte b) { int n = 0; while (b > 0) { n++; b &= (byte)(b - 1); } return n; }
    }

    public class Packet
    {
        // Layer 2 - Ethernet
        public string SrcMAC { get; set; }
        public string DstMAC { get; set; }
        public string EtherType { get; set; } = "0x0800(IP)";
        public int FrameSize { get; set; }

        // Layer 3 - IP
        public string SrcIP { get; set; }
        public string DstIP { get; set; }
        public int TTL { get; set; } = 64;
        public string Protocol { get; set; } = "TCP";
        public int TotalLength { get; set; }
        public string Flags { get; set; } = "DF";

        // Layer 4 - TCP/UDP
        public int SrcPort { get; set; } = 54321;
        public int DstPort { get; set; } = 80;
        public int SeqNum { get; set; }
        public int AckNum { get; set; }
        public string TcpFlags { get; set; } = "PSH ACK";

        // Application
        public string AppProtocol { get; set; } = "HTTP";
        public string AppData { get; set; } = "GET / HTTP/1.1";

        public string GetL2View()
        {
            return $"╔══════════ Ethernet Frame ══════════\n" +
                   $"║ 目的MAC: {DstMAC}\n" +
                   $"║ 源MAC:   {SrcMAC}\n" +
                   $"║ 类型:    {EtherType}\n" +
                   $"║ 帧大小:  {FrameSize} bytes\n" +
                   $"╚════════════════════════════════";
        }

        public string GetL3View()
        {
            return $"╔══════════ IP Datagram ═══════════\n" +
                   $"║ 版本:    IPv4\n" +
                   $"║ 总长度:  {TotalLength} bytes\n" +
                   $"║ 标识:    0x{Math.Abs(GetHashCode()) & 0xFFFF:X4}\n" +
                   $"║ 标志:    {Flags}\n" +
                   $"║ TTL:     {TTL}\n" +
                   $"║ 协议:    {Protocol}\n" +
                   $"║ 源IP:    {SrcIP}\n" +
                   $"║ 目的IP:  {DstIP}\n" +
                   $"╠══════════ 封装 ═══════════════\n" +
                   $"║ [{SrcMAC}] → [{DstMAC}]\n" +
                   $"╚════════════════════════════════";
        }

        public string GetL4View()
        {
            return $"╔══════════ TCP Segment ═══════════\n" +
                   $"║ 源端口:  {SrcPort}\n" +
                   $"║ 目的端口:{DstPort}\n" +
                   $"║ 序号:    {SeqNum}\n" +
                   $"║ 确认号:  {AckNum}\n" +
                   $"║ 标志:    {TcpFlags}\n" +
                   $"╠══════════ 封装 ═══════════════\n" +
                   $"║ {SrcIP}:{SrcPort} → {DstIP}:{DstPort}\n" +
                   $"║ [{SrcMAC}] → [{DstMAC}]\n" +
                   $"╚════════════════════════════════";
        }

        public string GetAppView()
        {
            return $"╔══════════ {AppProtocol} ══════════════\n" +
                   $"║ {AppData}\n" +
                   $"╠══════════ 封装 ═══════════════\n" +
                   $"║ TCP: {SrcIP}:{SrcPort} → {DstIP}:{DstPort}\n" +
                   $"║ MAC: [{SrcMAC}] → [{DstMAC}]\n" +
                   $"╚════════════════════════════════";
        }
    }

    public class LogEntry
    {
        public DateTime Time { get; set; }
        public string Layer { get; set; }  // ARP / MAC / IP / Route
        public string Message { get; set; }
        public override string ToString() => $"[{Time:HH:mm:ss}][{Layer}] {Message}";
    }

    public enum SimStatus { Idle, Sending, ARPing, Forwarding, ForwardingRoute, Arrived, Failed }
}
