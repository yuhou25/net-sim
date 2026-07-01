using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;

namespace NetSim.Tests
{
    [TestFixture]
    public class DeviceTests
    {
        [Test]
        public void PC_Creation_ShouldHaveSinglePort()
        {
            var d = new Device(DeviceType.PC, "TestPC");
            Assert.That(d.Ports.Count, Is.EqualTo(1));
            Assert.That(d.Ports[0].Name, Is.EqualTo("eth0"));
        }

        [Test]
        public void PC_Creation_ShouldHaveSize80x70()
        {
            var d = new Device(DeviceType.PC, "TestPC");
            Assert.That(d.Size, Is.EqualTo(new Size(80, 70)));
        }

        [Test]
        public void Switch_Creation_ShouldHave24Ports()
        {
            var d = new Device(DeviceType.Switch, "TestSW");
            Assert.That(d.Ports.Count, Is.EqualTo(24));
        }

        [Test]
        public void Switch_Creation_ShouldHavePorts1To12OnLeft()
        {
            var d = new Device(DeviceType.Switch, "TestSW");
            for (int i = 0; i < 12; i++)
            {
                Assert.That(d.Ports[i].Side, Is.EqualTo(PortSide.Left));
                Assert.That(d.Ports[i].Name, Is.EqualTo("P" + (i + 1)));
            }
        }

        [Test]
        public void Switch_Creation_ShouldHavePorts13To24OnRight()
        {
            var d = new Device(DeviceType.Switch, "TestSW");
            for (int i = 12; i < 24; i++)
            {
                Assert.That(d.Ports[i].Side, Is.EqualTo(PortSide.Right));
                Assert.That(d.Ports[i].Name, Is.EqualTo("P" + (i + 1)));
            }
        }

        [Test]
        public void Switch_Creation_ShouldHaveSize120x180()
        {
            var d = new Device(DeviceType.Switch, "TestSW");
            Assert.That(d.Size, Is.EqualTo(new Size(120, 180)));
        }

        [Test]
        public void Router_Creation_ShouldHave4Ports()
        {
            var d = new Device(DeviceType.Router, "TestRouter");
            Assert.That(d.Ports.Count, Is.EqualTo(4));
            Assert.That(d.Ports[0].Name, Is.EqualTo("eth0"));
            Assert.That(d.Ports[1].Name, Is.EqualTo("eth1"));
            Assert.That(d.Ports[2].Name, Is.EqualTo("eth2"));
            Assert.That(d.Ports[3].Name, Is.EqualTo("eth3"));
        }

        [Test]
        public void Router_Creation_ShouldHaveEth0Eth1Left_Eth2Eth3Right()
        {
            var d = new Device(DeviceType.Router, "TestRouter");
            Assert.That(d.Ports[0].Side, Is.EqualTo(PortSide.Left));
            Assert.That(d.Ports[1].Side, Is.EqualTo(PortSide.Left));
            Assert.That(d.Ports[2].Side, Is.EqualTo(PortSide.Right));
            Assert.That(d.Ports[3].Side, Is.EqualTo(PortSide.Right));
        }

        [Test]
        public void Router_Creation_ShouldHaveNatEnabled()
        {
            var d = new Device(DeviceType.Router, "TestRouter");
            Assert.That(d.NatTable.Enabled, Is.True);
        }

        [Test]
        public void Router_Creation_ShouldHaveSize100x80()
        {
            var d = new Device(DeviceType.Router, "TestRouter");
            Assert.That(d.Size, Is.EqualTo(new Size(100, 80)));
        }

        [Test]
        public void DNSServer_Creation_ShouldHaveDefaultDnsEntries()
        {
            var d = new Device(DeviceType.DNSServer, "DNS1");
            Assert.That(d.DnsEntries.Count, Is.EqualTo(2));
            Assert.That(d.DnsEntries[0].Domain, Is.EqualTo("www.gov.cn"));
            Assert.That(d.DnsEntries[0].IP, Is.EqualTo("203.0.113.10"));
            Assert.That(d.DnsEntries[1].Domain, Is.EqualTo("www.bank.com"));
            Assert.That(d.DnsEntries[1].IP, Is.EqualTo("203.0.113.20"));
        }

        [Test]
        public void DNSServer_Creation_ShouldDefaultIP()
        {
            var d = new Device(DeviceType.DNSServer, "DNS1");
            Assert.That(d.IP, Is.EqualTo("8.8.8.8"));
        }

        [Test]
        public void DNSServer_Creation_ShouldHaveSize80x60()
        {
            var d = new Device(DeviceType.DNSServer, "DNS1");
            Assert.That(d.Size, Is.EqualTo(new Size(80, 60)));
        }

        [Test]
        public void WebSite_Creation_ShouldDefaultIP()
        {
            var d = new Device(DeviceType.WebSite, "www.test.com");
            Assert.That(d.IP, Is.EqualTo("203.0.113.10"));
        }

        [Test]
        public void WebSite_Creation_ShouldHaveSize80x50()
        {
            var d = new Device(DeviceType.WebSite, "www.test.com");
            Assert.That(d.Size, Is.EqualTo(new Size(80, 50)));
        }

        [Test]
        public void WebSite_Creation_ShouldHaveSinglePort()
        {
            var d = new Device(DeviceType.WebSite, "www.test.com");
            Assert.That(d.Ports.Count, Is.EqualTo(1));
            Assert.That(d.Ports[0].Name, Is.EqualTo("eth0"));
        }

        [Test]
        public void Device_Creation_ShouldGenerateId_Length6()
        {
            var d = new Device(DeviceType.PC, "TestPC");
            Assert.That(d.Id, Is.Not.Null);
            Assert.That(d.Id.Length, Is.EqualTo(6));
        }

        [Test]
        public void Device_Creation_ShouldHaveUniqueIds()
        {
            var d1 = new Device(DeviceType.PC, "A");
            var d2 = new Device(DeviceType.PC, "B");
            Assert.That(d1.Id, Is.Not.EqualTo(d2.Id));
        }

        [Test]
        public void Device_Creation_ShouldSetName()
        {
            var d = new Device(DeviceType.PC, "MyPC");
            Assert.That(d.Name, Is.EqualTo("MyPC"));
        }

        [Test]
        public void Device_Creation_ShouldSetType()
        {
            Assert.That(new Device(DeviceType.PC, "A").Type, Is.EqualTo(DeviceType.PC));
            Assert.That(new Device(DeviceType.Switch, "A").Type, Is.EqualTo(DeviceType.Switch));
            Assert.That(new Device(DeviceType.Router, "A").Type, Is.EqualTo(DeviceType.Router));
            Assert.That(new Device(DeviceType.DNSServer, "A").Type, Is.EqualTo(DeviceType.DNSServer));
            Assert.That(new Device(DeviceType.WebSite, "A").Type, Is.EqualTo(DeviceType.WebSite));
        }

        [Test]
        public void PC_DefaultMask_ShouldBe255_255_255_0()
        {
            var d = new Device(DeviceType.PC, "PC1");
            Assert.That(d.Mask, Is.EqualTo("255.255.255.0"));
        }

        [Test]
        public void PC_DefaultIP_ShouldBeEmpty()
        {
            var d = new Device(DeviceType.PC, "PC1");
            Assert.That(d.IP, Is.EqualTo(""));
        }

        [Test]
        public void PC_DefaultGateways_ShouldBeEmpty()
        {
            var d = new Device(DeviceType.PC, "PC1");
            Assert.That(d.Gateways, Is.Empty);
        }

        [Test]
        public void Device_DefaultSelected_ShouldBeFalse()
        {
            var d = new Device(DeviceType.PC, "PC1");
            Assert.That(d.Selected, Is.False);
        }

        [Test]
        public void Device_DefaultStatus_ShouldBeNormal()
        {
            var d = new Device(DeviceType.PC, "PC1");
            Assert.That(d.Status, Is.EqualTo(RouterStatus.Normal));
        }

        [Test]
        public void Device_DefaultAlgorithm_ShouldBeRIP()
        {
            var d = new Device(DeviceType.PC, "PC1");
            Assert.That(d.Algorithm, Is.EqualTo(RoutingAlgo.RIP));
        }

        [Test]
        public void Device_DefaultPackets_ShouldBeEmpty()
        {
            var d = new Device(DeviceType.PC, "PC1");
            Assert.That(d.Packets, Is.Empty);
        }

        [Test]
        public void Device_DefaultLogs_ShouldBeEmpty()
        {
            var d = new Device(DeviceType.PC, "PC1");
            Assert.That(d.Logs, Is.Empty);
        }

        [Test]
        public void AddPort_ShouldAddPortWithCorrectProperties()
        {
            var d = new Device(DeviceType.PC, "TestPC");
            var p = d.AddPort("eth1", "test desc", PortSide.Left);
            Assert.That(d.Ports.Count, Is.EqualTo(2));
            Assert.That(p.Name, Is.EqualTo("eth1"));
            Assert.That(p.Description, Is.EqualTo("test desc"));
            Assert.That(p.Side, Is.EqualTo(PortSide.Left));
            Assert.That(p.Device, Is.SameAs(d));
        }

        [Test]
        public void AddPort_DefaultParams_ShouldUseRightSide()
        {
            var d = new Device(DeviceType.PC, "TestPC");
            var p = d.AddPort("eth1");
            Assert.That(p.Side, Is.EqualTo(PortSide.Right));
        }

        [Test]
        public void LogText_WithNoLogs_ShouldBeEmpty()
        {
            var d = new Device(DeviceType.PC, "PC1");
            Assert.That(d.LogText, Is.EqualTo(""));
        }

        [Test]
        public void LogText_WithLogs_ShouldJoinWithCrLf()
        {
            var d = new Device(DeviceType.PC, "PC1");
            d.Logs.Add(new LogEntry { Time = DateTime.Now, Layer = "IP", Message = "Msg1" });
            d.Logs.Add(new LogEntry { Time = DateTime.Now, Layer = "MAC", Message = "Msg2" });
            var text = d.LogText;
            Assert.That(text, Does.Contain("Msg1"));
            Assert.That(text, Does.Contain("Msg2"));
            Assert.That(text, Does.Contain("\r\n"));
        }

        [Test]
        public void ClearLogs_ShouldEmptyList()
        {
            var d = new Device(DeviceType.PC, "PC1");
            d.Logs.Add(new LogEntry { Time = DateTime.Now, Layer = "IP", Message = "Test" });
            d.ClearLogs();
            Assert.That(d.Logs, Is.Empty);
        }
    }

    [TestFixture]
    public class PortTests
    {
        private Device _device;
        private Port _port;

        [SetUp]
        public void Setup()
        {
            _device = new Device(DeviceType.PC, "TestPC") { Location = new Point(100, 100) };
            _port = _device.Ports[0];
        }

        [Test]
        public void Constructor_ShouldSetProperties()
        {
            var d = new Device(DeviceType.PC, "PC");
            var p = new Port("eth1", "desc", d, PortSide.Left);
            Assert.That(p.Name, Is.EqualTo("eth1"));
            Assert.That(p.Description, Is.EqualTo("desc"));
            Assert.That(p.Device, Is.SameAs(d));
            Assert.That(p.Side, Is.EqualTo(PortSide.Left));
        }

        [Test]
        public void Anchor_RightSide_ShouldCalculateCorrectly()
        {
            _device.Location = new Point(100, 200);
            _device.Size = new Size(80, 70);
            var anchor = _port.Anchor;
            Assert.That(anchor.X, Is.EqualTo(180)); // 100 + 80
            Assert.That(anchor.Y, Is.GreaterThanOrEqualTo(200));
        }

        [Test]
        public void Anchor_LeftSide_ShouldCalculateCorrectly()
        {
            _device.Location = new Point(100, 200);
            var d = new Device(DeviceType.Router, "R1") { Location = new Point(100, 200) };
            var p = d.Ports[0]; // eth0, Left side
            var anchor = p.Anchor;
            Assert.That(anchor.X, Is.EqualTo(100));
        }

        [Test]
        public void IsConnected_WhenNull_ShouldReturnFalse()
        {
            Assert.That(_port.IsConnected, Is.False);
        }

        [Test]
        public void IsConnected_WhenConnected_ShouldReturnTrue()
        {
            var other = new Device(DeviceType.PC, "PC2");
            _port.ConnectedTo = other.Ports[0];
            other.Ports[0].ConnectedTo = _port;
            Assert.That(_port.IsConnected, Is.True);
        }

        [Test]
        public void ConnectedDeviceName_WhenNotConnected_ShouldReturnDash()
        {
            Assert.That(_port.ConnectedDeviceName, Is.EqualTo("-"));
        }

        [Test]
        public void ConnectedDeviceName_WhenConnected_ShouldReturnDeviceName()
        {
            var other = new Device(DeviceType.PC, "PC2");
            _port.ConnectedTo = other.Ports[0];
            Assert.That(_port.ConnectedDeviceName, Is.EqualTo("PC2"));
        }

        [Test]
        public void ConnectedPortName_WhenNotConnected_ShouldReturnDash()
        {
            Assert.That(_port.ConnectedPortName, Is.EqualTo("-"));
        }

        [Test]
        public void ConnectedPortName_WhenConnected_ShouldReturnPortName()
        {
            var other = new Device(DeviceType.PC, "PC2");
            _port.ConnectedTo = other.Ports[0];
            Assert.That(_port.ConnectedPortName, Is.EqualTo("eth0"));
        }

        [Test]
        public void Index_ShouldReturnCorrectPosition()
        {
            var d = new Device(DeviceType.Switch, "SW1");
            Assert.That(d.Ports[0].Index, Is.EqualTo(0));
            Assert.That(d.Ports[12].Index, Is.EqualTo(12));
            Assert.That(d.Ports[23].Index, Is.EqualTo(23));
        }

        [Test]
        public void ConnectedTo_DefaultShouldBeNull()
        {
            Assert.That(_port.ConnectedTo, Is.Null);
        }
    }

    [TestFixture]
    public class ArpTableTests
    {
        [Test]
        public void Set_NewEntry_ShouldAdd()
        {
            var table = new ArpTable();
            table.Set("192.168.1.1", "AA:BB:CC:DD:EE:FF");
            Assert.That(table.Entries.Count, Is.EqualTo(1));
            Assert.That(table.Entries[0].IP, Is.EqualTo("192.168.1.1"));
            Assert.That(table.Entries[0].MAC, Is.EqualTo("AA:BB:CC:DD:EE:FF"));
        }

        [Test]
        public void Set_ExistingEntry_ShouldUpdateMacAndTime()
        {
            var table = new ArpTable();
            table.Set("192.168.1.1", "AA:BB:CC:DD:EE:FF");
            var oldTime = table.Entries[0].Time;
            System.Threading.Thread.Sleep(10);
            table.Set("192.168.1.1", "11:22:33:44:55:66");
            Assert.That(table.Entries.Count, Is.EqualTo(1));
            Assert.That(table.Entries[0].MAC, Is.EqualTo("11:22:33:44:55:66"));
            Assert.That(table.Entries[0].Time, Is.GreaterThan(oldTime));
        }

        [Test]
        public void GetMac_ExistingIP_ShouldReturnMac()
        {
            var table = new ArpTable();
            table.Set("192.168.1.1", "AA:BB:CC:DD:EE:FF");
            Assert.That(table.GetMac("192.168.1.1"), Is.EqualTo("AA:BB:CC:DD:EE:FF"));
        }

        [Test]
        public void GetMac_NonExistingIP_ShouldReturnNull()
        {
            var table = new ArpTable();
            table.Set("192.168.1.1", "AA:BB:CC:DD:EE:FF");
            Assert.That(table.GetMac("10.0.0.1"), Is.Null);
        }

        [Test]
        public void Entries_DefaultShouldBeEmpty()
        {
            var table = new ArpTable();
            Assert.That(table.Entries, Is.Empty);
        }

        [Test]
        public void Set_MultipleEntries_ShouldNotOverwriteUnrelated()
        {
            var table = new ArpTable();
            table.Set("192.168.1.1", "AA:BB:CC:DD:EE:01");
            table.Set("192.168.1.2", "AA:BB:CC:DD:EE:02");
            table.Set("192.168.1.3", "AA:BB:CC:DD:EE:03");
            Assert.That(table.Entries.Count, Is.EqualTo(3));
            Assert.That(table.GetMac("192.168.1.2"), Is.EqualTo("AA:BB:CC:DD:EE:02"));
        }
    }

    [TestFixture]
    public class MacTableTests
    {
        [Test]
        public void Learn_NewEntry_ShouldAdd()
        {
            var table = new MacTable();
            table.Learn("AA:BB:CC:DD:EE:FF", "P1");
            Assert.That(table.Entries.Count, Is.EqualTo(1));
            Assert.That(table.Entries[0].MAC, Is.EqualTo("AA:BB:CC:DD:EE:FF"));
            Assert.That(table.Entries[0].PortName, Is.EqualTo("P1"));
        }

        [Test]
        public void Learn_ExistingEntry_ShouldUpdatePortAndTime()
        {
            var table = new MacTable();
            table.Learn("AA:BB:CC:DD:EE:FF", "P1");
            var oldTime = table.Entries[0].Time;
            System.Threading.Thread.Sleep(10);
            table.Learn("AA:BB:CC:DD:EE:FF", "P5");
            Assert.That(table.Entries.Count, Is.EqualTo(1));
            Assert.That(table.Entries[0].PortName, Is.EqualTo("P5"));
            Assert.That(table.Entries[0].Time, Is.GreaterThan(oldTime));
        }

        [Test]
        public void GetPort_ExistingMAC_ShouldReturnPort()
        {
            var table = new MacTable();
            table.Learn("AA:BB:CC:DD:EE:FF", "P1");
            Assert.That(table.GetPort("AA:BB:CC:DD:EE:FF"), Is.EqualTo("P1"));
        }

        [Test]
        public void GetPort_NonExistingMAC_ShouldReturnNull()
        {
            var table = new MacTable();
            table.Learn("AA:BB:CC:DD:EE:FF", "P1");
            Assert.That(table.GetPort("11:22:33:44:55:66"), Is.Null);
        }

        [Test]
        public void Entries_DefaultShouldBeEmpty()
        {
            var table = new MacTable();
            Assert.That(table.Entries, Is.Empty);
        }

        [Test]
        public void Learn_MultipleEntries_ShouldTrackCorrectly()
        {
            var table = new MacTable();
            table.Learn("AA:AA:AA:AA:AA:01", "P1");
            table.Learn("AA:AA:AA:AA:AA:02", "P2");
            table.Learn("AA:AA:AA:AA:AA:03", "P3");
            Assert.That(table.Entries.Count, Is.EqualTo(3));
            Assert.That(table.GetPort("AA:AA:AA:AA:AA:02"), Is.EqualTo("P2"));
        }
    }

    [TestFixture]
    public class RoutingTableTests
    {
        [Test]
        public void FindRoute_ExactMatch_ShouldReturnRoute()
        {
            var table = new RoutingTable();
            table.Entries.Add(new RouteEntry { Network = "192.168.1.0", Mask = "255.255.255.0", NextHop = "192.168.1.1", OutPort = "eth0", Cost = 1 });
            var route = table.FindRoute("192.168.1.100");
            Assert.That(route, Is.Not.Null);
            Assert.That(route.Network, Is.EqualTo("192.168.1.0"));
            Assert.That(route.NextHop, Is.EqualTo("192.168.1.1"));
            Assert.That(route.OutPort, Is.EqualTo("eth0"));
        }

        [Test]
        public void FindRoute_LongestPrefixMatch_ShouldReturnMoreSpecific()
        {
            var table = new RoutingTable();
            table.Entries.Add(new RouteEntry { Network = "10.0.0.0", Mask = "255.0.0.0", NextHop = "10.0.0.1", OutPort = "eth0", Cost = 10 });
            table.Entries.Add(new RouteEntry { Network = "10.1.0.0", Mask = "255.255.0.0", NextHop = "10.1.0.1", OutPort = "eth1", Cost = 5 });
            var route = table.FindRoute("10.1.2.3");
            Assert.That(route, Is.Not.Null);
            Assert.That(route.Mask, Is.EqualTo("255.255.0.0"));
            Assert.That(route.OutPort, Is.EqualTo("eth1"));
        }

        [Test]
        public void FindRoute_DefaultRoute_ShouldMatch()
        {
            var table = new RoutingTable();
            table.Entries.Add(new RouteEntry { Network = "0.0.0.0", Mask = "0.0.0.0", NextHop = "192.168.1.1", OutPort = "eth0", Cost = 1 });
            var route = table.FindRoute("203.0.113.50");
            Assert.That(route, Is.Not.Null);
            Assert.That(route.OutPort, Is.EqualTo("eth0"));
        }

        [Test]
        public void FindRoute_NoMatch_ShouldReturnNull()
        {
            var table = new RoutingTable();
            table.Entries.Add(new RouteEntry { Network = "192.168.1.0", Mask = "255.255.255.0", NextHop = "192.168.1.1", OutPort = "eth0", Cost = 1 });
            var route = table.FindRoute("10.0.0.1");
            Assert.That(route, Is.Null);
        }

        [Test]
        public void FindRoute_OutPortNull_ShouldNotMatch()
        {
            var table = new RoutingTable();
            table.Entries.Add(new RouteEntry { Network = "0.0.0.0", Mask = "0.0.0.0", NextHop = "192.168.1.1", OutPort = null, Cost = 1 });
            var route = table.FindRoute("203.0.113.50");
            Assert.That(route, Is.Null);
        }

        [Test]
        public void FindRoute_EmptyTable_ShouldReturnNull()
        {
            var table = new RoutingTable();
            Assert.That(table.FindRoute("192.168.1.1"), Is.Null);
        }

        [Test]
        public void FindRoute_Mask24Match_ShouldWork()
        {
            var table = new RoutingTable();
            table.Entries.Add(new RouteEntry { Network = "172.16.0.0", Mask = "255.255.255.0", NextHop = "172.16.0.1", OutPort = "eth0", Cost = 1 });
            Assert.That(table.FindRoute("172.16.0.50"), Is.Not.Null);
            Assert.That(table.FindRoute("172.16.1.50"), Is.Null);
        }

        [Test]
        public void FindRoute_InvalidIP_ShouldNotThrow()
        {
            var table = new RoutingTable();
            table.Entries.Add(new RouteEntry { Network = "192.168.1.0", Mask = "255.255.255.0", NextHop = "192.168.1.1", OutPort = "eth0", Cost = 1 });
            Assert.That(table.FindRoute("not-an-ip"), Is.Null);
        }

        [Test]
        public void FindRoute_TieBreakByPrefixLen_ShouldPickLongest()
        {
            var table = new RoutingTable();
            table.Entries.Add(new RouteEntry { Network = "192.168.0.0", Mask = "255.255.0.0", NextHop = "192.168.0.1", OutPort = "eth0", Cost = 1 });
            table.Entries.Add(new RouteEntry { Network = "192.168.1.0", Mask = "255.255.255.0", NextHop = "192.168.1.1", OutPort = "eth1", Cost = 1 });
            var route = table.FindRoute("192.168.1.100");
            Assert.That(route.OutPort, Is.EqualTo("eth1"));
        }
    }

    [TestFixture]
    public class PacketTests
    {
        [Test]
        public void DefaultValues_ShouldBeSet()
        {
            var p = new Packet();
            Assert.That(p.EtherType, Is.EqualTo("0x0800(IP)"));
            Assert.That(p.TTL, Is.EqualTo(64));
            Assert.That(p.Protocol, Is.EqualTo("TCP"));
            Assert.That(p.Flags, Is.EqualTo("DF"));
            Assert.That(p.SrcPort, Is.EqualTo(54321));
            Assert.That(p.DstPort, Is.EqualTo(80));
            Assert.That(p.TcpFlags, Is.EqualTo("PSH ACK"));
            Assert.That(p.AppProtocol, Is.EqualTo("HTTP"));
            Assert.That(p.AppData, Is.EqualTo("GET / HTTP/1.1"));
        }

        [Test]
        public void GetL2View_ShouldContainSourceAndDestinationMacs()
        {
            var p = new Packet { SrcMAC = "MAC:SRC01", DstMAC = "MAC:DST01", FrameSize = 1500 };
            var view = p.GetL2View();
            Assert.That(view, Does.Contain("MAC:SRC01"));
            Assert.That(view, Does.Contain("MAC:DST01"));
            Assert.That(view, Does.Contain("1500"));
            Assert.That(view, Does.Contain("Ethernet Frame"));
        }

        [Test]
        public void GetL3View_ShouldContainIPInfo()
        {
            var p = new Packet { SrcIP = "192.168.1.1", DstIP = "10.0.0.1", SrcMAC = "M1", DstMAC = "M2", TotalLength = 1460 };
            var view = p.GetL3View();
            Assert.That(view, Does.Contain("192.168.1.1"));
            Assert.That(view, Does.Contain("10.0.0.1"));
            Assert.That(view, Does.Contain("1460"));
            Assert.That(view, Does.Contain("IPv4"));
            Assert.That(view, Does.Contain("IP Datagram"));
        }

        [Test]
        public void GetL4View_ShouldContainPortInfo()
        {
            var p = new Packet { SrcIP = "192.168.1.1", DstIP = "10.0.0.1", SrcMAC = "M1", DstMAC = "M2", SrcPort = 12345, DstPort = 443, SeqNum = 100, AckNum = 200 };
            var view = p.GetL4View();
            Assert.That(view, Does.Contain("12345"));
            Assert.That(view, Does.Contain("443"));
            Assert.That(view, Does.Contain("100"));
            Assert.That(view, Does.Contain("200"));
            Assert.That(view, Does.Contain("TCP Segment"));
        }

        [Test]
        public void GetAppView_ShouldContainAppData()
        {
            var p = new Packet { AppProtocol = "HTTPS", AppData = "POST /api/data", SrcIP = "192.168.1.1", DstIP = "10.0.0.1", SrcMAC = "M1", DstMAC = "M2", SrcPort = 12345, DstPort = 443 };
            var view = p.GetAppView();
            Assert.That(view, Does.Contain("HTTPS"));
            Assert.That(view, Does.Contain("POST /api/data"));
            Assert.That(view, Does.Contain("192.168.1.1"));
            Assert.That(view, Does.Contain("10.0.0.1"));
        }

        [Test]
        public void GetL2View_ShouldContainEtherType()
        {
            var p = new Packet { EtherType = "0x86DD(IPv6)", SrcMAC = "M1", DstMAC = "M2" };
            Assert.That(p.GetL2View(), Does.Contain("0x86DD(IPv6)"));
        }

        [Test]
        public void GetL3View_ShouldContainTtlAndProtocol()
        {
            var p = new Packet { TTL = 128, Protocol = "UDP", SrcMAC = "M1", DstMAC = "M2", SrcIP = "10.0.0.1", DstIP = "10.0.0.2" };
            var view = p.GetL3View();
            Assert.That(view, Does.Contain("128"));
            Assert.That(view, Does.Contain("UDP"));
        }

        [Test]
        public void GetL4View_ShouldContainTcpFlags()
        {
            var p = new Packet { TcpFlags = "SYN", SrcIP = "1.1.1.1", DstIP = "2.2.2.2", SrcMAC = "M1", DstMAC = "M2" };
            Assert.That(p.GetL4View(), Does.Contain("SYN"));
        }
    }

    [TestFixture]
    public class LogEntryTests
    {
        [Test]
        public void ToString_ShouldFormatCorrectly()
        {
            var entry = new LogEntry
            {
                Time = new DateTime(2024, 1, 15, 14, 30, 45),
                Layer = "ARP",
                Message = "Request sent"
            };
            var result = entry.ToString();
            Assert.That(result, Is.EqualTo("[14:30:45][ARP] Request sent"));
        }

        [Test]
        public void ToString_ShouldHandleAllLayers()
        {
            var layers = new[] { "ARP", "MAC", "IP", "Route", "DNS", "NAT" };
            foreach (var layer in layers)
            {
                var entry = new LogEntry { Time = DateTime.Now, Layer = layer, Message = "msg" };
                Assert.That(entry.ToString(), Does.Contain("[" + layer + "]"));
            }
        }

        [Test]
        public void Time_CanBeSet()
        {
            var expected = new DateTime(2024, 6, 1, 10, 0, 0);
            var entry = new LogEntry { Time = expected };
            Assert.That(entry.Time, Is.EqualTo(expected));
        }
    }

    [TestFixture]
    public class NatTableTests
    {
        [Test]
        public void Default_Enabled_ShouldBeFalse()
        {
            Assert.That(new NatTable().Enabled, Is.False);
        }

        [Test]
        public void Default_ExternalIP_ShouldBe203_0_113_1()
        {
            Assert.That(new NatTable().ExternalIP, Is.EqualTo("203.0.113.1"));
        }

        [Test]
        public void Entries_DefaultShouldBeEmpty()
        {
            Assert.That(new NatTable().Entries, Is.Empty);
        }
    }

    [TestFixture]
    public class DnsEntryTests
    {
        [Test]
        public void ShouldStoreDomainAndIP()
        {
            var entry = new DnsEntry { Domain = "example.com", IP = "1.2.3.4" };
            Assert.That(entry.Domain, Is.EqualTo("example.com"));
            Assert.That(entry.IP, Is.EqualTo("1.2.3.4"));
        }
    }

    [TestFixture]
    public class SimStepTests
    {
        [Test]
        public void DefaultValues_ShouldBeSet()
        {
            var step = new SimStep();
            Assert.That(step.Index, Is.EqualTo(0));
            Assert.That(step.IsError, Is.False);
            Assert.That(step.Title, Is.Null);
            Assert.That(step.Detail, Is.Null);
            Assert.That(step.Device, Is.Null);
            Assert.That(step.Layer, Is.Null);
        }
    }
}
