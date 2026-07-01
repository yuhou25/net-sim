using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;

namespace NetSim.Tests
{
    [TestFixture]
    public class SimEngineUtilityTests
    {
        private List<Device> _devices;
        private SimEngine _engine;

        [SetUp]
        public void Setup()
        {
            _devices = new List<Device>();
            _engine = new SimEngine(_devices, null, null);
        }

        [Test]
        public void IsSameNetwork_SameSubnet_ReturnsTrue()
        {
            Assert.That(_engine.IsSameNetwork("192.168.1.10", "192.168.1.20", "255.255.255.0"), Is.True);
        }

        [Test]
        public void IsSameNetwork_DifferentSubnet_ReturnsFalse()
        {
            Assert.That(_engine.IsSameNetwork("192.168.1.10", "192.168.2.20", "255.255.255.0"), Is.False);
        }

        [Test]
        public void IsSameNetwork_SameSubnetSlash16_ReturnsTrue()
        {
            Assert.That(_engine.IsSameNetwork("172.16.1.1", "172.16.100.1", "255.255.0.0"), Is.True);
        }

        [Test]
        public void IsSameNetwork_DifferentSubnetSlash16_ReturnsFalse()
        {
            Assert.That(_engine.IsSameNetwork("172.16.1.1", "172.17.1.1", "255.255.0.0"), Is.False);
        }

        [Test]
        public void IsSameNetwork_SameNetwork10_ReturnsTrue()
        {
            Assert.That(_engine.IsSameNetwork("10.0.0.1", "10.255.255.254", "255.0.0.0"), Is.True);
        }

        [Test]
        public void IsSameNetwork_InvalidIP_ReturnsFalse()
        {
            Assert.That(_engine.IsSameNetwork("invalid", "192.168.1.1", "255.255.255.0"), Is.False);
        }

        [Test]
        public void IsSameNetwork_InvalidMask_ReturnsFalse()
        {
            Assert.That(_engine.IsSameNetwork("192.168.1.1", "192.168.1.2", "bad-mask"), Is.False);
        }

        [Test]
        public void IsSameNetwork_NullInputs_ReturnsFalse()
        {
            Assert.That(_engine.IsSameNetwork(null, "192.168.1.1", "255.255.255.0"), Is.False);
            Assert.That(_engine.IsSameNetwork("192.168.1.1", null, "255.255.255.0"), Is.False);
            Assert.That(_engine.IsSameNetwork("192.168.1.1", "192.168.1.2", null), Is.False);
        }

        [Test]
        public void IsSameNetwork_BroadcastAddress_SameSubnet_ReturnsTrue()
        {
            Assert.That(_engine.IsSameNetwork("192.168.1.255", "192.168.1.1", "255.255.255.0"), Is.True);
        }

        [Test]
        public void IsSameNetwork_NetworkAddress_SameSubnet_ReturnsTrue()
        {
            Assert.That(_engine.IsSameNetwork("192.168.1.0", "192.168.1.1", "255.255.255.0"), Is.True);
        }

        [Test]
        public void IsPrivateIP_192_168_x_x_ReturnsTrue()
        {
            Assert.That(_engine.IsPrivateIP("192.168.0.1"), Is.True);
            Assert.That(_engine.IsPrivateIP("192.168.1.1"), Is.True);
            Assert.That(_engine.IsPrivateIP("192.168.255.254"), Is.True);
        }

        [Test]
        public void IsPrivateIP_10_x_x_x_ReturnsTrue()
        {
            Assert.That(_engine.IsPrivateIP("10.0.0.1"), Is.True);
            Assert.That(_engine.IsPrivateIP("10.255.255.254"), Is.True);
        }

        [Test]
        public void IsPrivateIP_172_16_x_x_Through_172_31_x_x_ReturnsTrue()
        {
            Assert.That(_engine.IsPrivateIP("172.16.0.1"), Is.True);
            Assert.That(_engine.IsPrivateIP("172.20.0.1"), Is.True);
            Assert.That(_engine.IsPrivateIP("172.31.255.254"), Is.True);
        }

        [Test]
        public void IsPrivateIP_172_15_x_x_ReturnsFalse()
        {
            Assert.That(_engine.IsPrivateIP("172.15.0.1"), Is.False);
        }

        [Test]
        public void IsPrivateIP_172_32_x_x_ReturnsFalse()
        {
            Assert.That(_engine.IsPrivateIP("172.32.0.1"), Is.False);
        }

        [Test]
        public void IsPrivateIP_PublicIPs_ReturnsFalse()
        {
            Assert.That(_engine.IsPrivateIP("8.8.8.8"), Is.False);
            Assert.That(_engine.IsPrivateIP("1.1.1.1"), Is.False);
            Assert.That(_engine.IsPrivateIP("203.0.113.10"), Is.False);
            Assert.That(_engine.IsPrivateIP("100.64.0.1"), Is.False);
        }

        [Test]
        public void IsPrivateIP_InvalidFormat_ReturnsFalse()
        {
            Assert.That(_engine.IsPrivateIP("not-an-ip"), Is.False);
            Assert.That(_engine.IsPrivateIP(""), Is.False);
            Assert.That(_engine.IsPrivateIP(null), Is.False);
        }

        [Test]
        public void IsPrivateIP_Loopback_ReturnsFalse()
        {
            Assert.That(_engine.IsPrivateIP("127.0.0.1"), Is.False);
        }

        [Test]
        public void Constructor_ShouldInitializeSteps()
        {
            Assert.That(_engine.Steps, Is.Not.Null);
            Assert.That(_engine.Steps, Is.Empty);
        }

        [Test]
        public void Constructor_ShouldInitializeCurrentStepToNegative()
        {
            Assert.That(_engine.CurrentStep, Is.EqualTo(-1));
        }

        [Test]
        public void IsSimulating_ShouldReturnFalse_WhenNoSteps()
        {
            Assert.That(_engine.IsSimulating, Is.False);
        }

        [Test]
        public void Reset_ShouldClearStepsAndCurrentStep()
        {
            _engine.Steps.Add(new SimStep { Title = "Test" });
            _engine.CurrentStep = 0;
            _engine.Reset();
            Assert.That(_engine.Steps, Is.Empty);
            Assert.That(_engine.CurrentStep, Is.EqualTo(-1));
            Assert.That(_engine.StatusMessage, Is.EqualTo(""));
        }

        [Test]
        public void Reset_ShouldClearAllDeviceLogsAndPackets()
        {
            var pc = new Device(DeviceType.PC, "PC1");
            pc.Logs.Add(new LogEntry { Layer = "IP", Message = "test" });
            pc.Packets.Add(new Packet { SrcIP = "1.1.1.1", DstIP = "2.2.2.2" });
            _devices.Add(pc);
            _engine.Reset();
            Assert.That(pc.Logs, Is.Empty);
            Assert.That(pc.Packets, Is.Empty);
        }

        [Test]
        public void StatusMessage_DefaultIsEmpty()
        {
            Assert.That(_engine.StatusMessage, Is.EqualTo(""));
        }
    }

    [TestFixture]
    public class SimEngineFindPCByIPTests
    {
        private List<Device> _devices;
        private SimEngine _engine;

        [SetUp]
        public void Setup()
        {
            _devices = new List<Device>();
            _engine = new SimEngine(_devices, null, null);
        }

        [Test]
        public void FindPCByIP_DirectlyConnected_ReturnsPC()
        {
            var pc1 = new Device(DeviceType.PC, "PC1") { IP = "192.168.1.1" };
            var pc2 = new Device(DeviceType.PC, "PC2") { IP = "192.168.1.2" };
            pc1.Ports[0].ConnectedTo = pc2.Ports[0];
            pc2.Ports[0].ConnectedTo = pc1.Ports[0];
            _devices.Add(pc1);
            _devices.Add(pc2);

            var found = _engine.FindPCByIP(pc1, "192.168.1.2", new HashSet<string>());
            Assert.That(found, Is.Not.Null);
            Assert.That(found.Name, Is.EqualTo("PC2"));
        }

        [Test]
        public void FindPCByIP_ThroughSwitch_ReturnsPC()
        {
            var pc1 = new Device(DeviceType.PC, "PC1") { IP = "192.168.1.1" };
            var sw = new Device(DeviceType.Switch, "SW1");
            var pc2 = new Device(DeviceType.PC, "PC2") { IP = "192.168.1.2" };
            pc1.Ports[0].ConnectedTo = sw.Ports[0];
            sw.Ports[0].ConnectedTo = pc1.Ports[0];
            sw.Ports[1].ConnectedTo = pc2.Ports[0];
            pc2.Ports[0].ConnectedTo = sw.Ports[1];
            _devices.Add(pc1);
            _devices.Add(sw);
            _devices.Add(pc2);

            var found = _engine.FindPCByIP(pc1, "192.168.1.2", new HashSet<string>());
            Assert.That(found, Is.Not.Null);
            Assert.That(found.Name, Is.EqualTo("PC2"));
        }

        [Test]
        public void FindPCByIP_NotFound_ReturnsNull()
        {
            var pc1 = new Device(DeviceType.PC, "PC1") { IP = "192.168.1.1" };
            _devices.Add(pc1);

            var found = _engine.FindPCByIP(pc1, "192.168.1.99", new HashSet<string>());
            Assert.That(found, Is.Null);
        }

        [Test]
        public void FindPCByIP_VisitedPreventsLoop()
        {
            var pc1 = new Device(DeviceType.PC, "PC1") { IP = "192.168.1.1" };
            var pc2 = new Device(DeviceType.PC, "PC2") { IP = "192.168.1.2" };
            pc1.Ports[0].ConnectedTo = pc2.Ports[0];
            pc2.Ports[0].ConnectedTo = pc1.Ports[0];
            _devices.Add(pc1);
            _devices.Add(pc2);

            var visited = new HashSet<string> { pc1.Id };
            var found = _engine.FindPCByIP(pc2, "192.168.1.1", visited);
            // pc1 is in visited, so should not be reached from pc2
            Assert.That(found, Is.Null);
        }
    }

    [TestFixture]
    public class SimEngineFindRouterOnPathTests
    {
        [Test]
        public void FindRouterOnPath_ThroughSwitch_FindsRouter()
        {
            var devices = new List<Device>();
            var engine = new SimEngine(devices, null, null);
            var pc1 = new Device(DeviceType.PC, "PC1") { IP = "192.168.1.1" };
            var sw = new Device(DeviceType.Switch, "SW1");
            var router = new Device(DeviceType.Router, "R1");
            pc1.Ports[0].ConnectedTo = sw.Ports[0];
            sw.Ports[0].ConnectedTo = pc1.Ports[0];
            sw.Ports[1].ConnectedTo = router.Ports[0];
            router.Ports[0].ConnectedTo = sw.Ports[1];

            var found = engine.FindRouterOnPath(pc1.Ports[0].ConnectedTo.Device, new HashSet<string>());
            Assert.That(found, Is.Not.Null);
            Assert.That(found.Type, Is.EqualTo(DeviceType.Router));
        }

        [Test]
        public void FindRouterOnPath_NoRouter_ReturnsNull()
        {
            var devices = new List<Device>();
            var engine = new SimEngine(devices, null, null);
            var pc1 = new Device(DeviceType.PC, "PC1") { IP = "192.168.1.1" };
            var pc2 = new Device(DeviceType.PC, "PC2") { IP = "192.168.1.2" };
            pc1.Ports[0].ConnectedTo = pc2.Ports[0];
            pc2.Ports[0].ConnectedTo = pc1.Ports[0];

            var found = engine.FindRouterOnPath(pc1.Ports[0].ConnectedTo.Device, new HashSet<string>());
            Assert.That(found, Is.Null);
        }
    }

    [TestFixture]
    public class SimEnginePreLearnMacsTests
    {
        [Test]
        public void PreLearnMacs_PCToSwitch_PopulatesSwitchMacTable()
        {
            var pc1 = new Device(DeviceType.PC, "PC1") { IP = "192.168.1.1" };
            var sw = new Device(DeviceType.Switch, "SW1");
            var pc2 = new Device(DeviceType.PC, "PC2") { IP = "192.168.1.2" };
            pc1.Ports[0].ConnectedTo = sw.Ports[0];
            sw.Ports[0].ConnectedTo = pc1.Ports[0];
            sw.Ports[12].ConnectedTo = pc2.Ports[0];
            pc2.Ports[0].ConnectedTo = sw.Ports[12];

            var devices = new List<Device> { pc1, sw, pc2 };
            var engine = new SimEngine(devices, null, null);
            engine.PreLearnMacs();

            Assert.That(sw.MacTable.GetPort("MAC:" + pc1.Id), Is.EqualTo("P1"));
            Assert.That(sw.MacTable.GetPort("MAC:" + pc2.Id), Is.Not.Null);
        }

        [Test]
        public void PreLearnMacs_PCSwitchRouter_PopulatesARPTables()
        {
            var pc1 = new Device(DeviceType.PC, "PC1") { IP = "192.168.1.1", Mask = "255.255.255.0" };
            var sw = new Device(DeviceType.Switch, "SW1");
            var router = new Device(DeviceType.Router, "R1");
            pc1.Ports[0].ConnectedTo = sw.Ports[0];
            sw.Ports[0].ConnectedTo = pc1.Ports[0];
            sw.Ports[12].ConnectedTo = router.Ports[0];
            router.Ports[0].ConnectedTo = sw.Ports[12];

            var devices = new List<Device> { pc1, sw, router };
            var engine = new SimEngine(devices, null, null);
            engine.PreLearnMacs();

            // Router MAC should be learned on the switch
            Assert.That(sw.MacTable.GetPort("MAC:" + router.Id), Is.Not.Null);
            // PC IP should be in router's ARP table (via WalkForArp)
            Assert.That(router.ArpTable.GetMac("192.168.1.1"), Is.Not.Null);
        }
    }

    [TestFixture]
    public class SimEngineArpResolveTests
    {
        private List<Device> _devices;
        private SimEngine _engine;

        [SetUp]
        public void Setup()
        {
            _devices = new List<Device>();
            _engine = new SimEngine(_devices, null, null);
        }

        [Test]
        public void ArpResolve_CacheHit_ReturnsMac()
        {
            var pc = new Device(DeviceType.PC, "PC1") { IP = "192.168.1.1" };
            pc.ArpTable.Set("192.168.1.2", "AA:BB:CC:DD:EE:FF");
            _devices.Add(pc);

            var mac = _engine.ArpResolve(pc, "192.168.1.2");
            Assert.That(mac, Is.EqualTo("AA:BB:CC:DD:EE:FF"));
        }

        [Test]
        public void ArpResolve_DirectConnection_FindsPC()
        {
            var pc1 = new Device(DeviceType.PC, "PC1") { IP = "192.168.1.1" };
            var pc2 = new Device(DeviceType.PC, "PC2") { IP = "192.168.1.2" };
            pc1.Ports[0].ConnectedTo = pc2.Ports[0];
            pc2.Ports[0].ConnectedTo = pc1.Ports[0];
            _devices.Add(pc1);
            _devices.Add(pc2);

            var mac = _engine.ArpResolve(pc1, "192.168.1.2");
            Assert.That(mac, Is.Not.Null);
            Assert.That(mac, Is.EqualTo("MAC:" + pc2.Id));
        }

        [Test]
        public void ArpResolve_ThroughSwitch_FindsPC()
        {
            var pc1 = new Device(DeviceType.PC, "PC1") { IP = "192.168.1.1" };
            var sw = new Device(DeviceType.Switch, "SW1");
            var pc2 = new Device(DeviceType.PC, "PC2") { IP = "192.168.1.2" };
            pc1.Ports[0].ConnectedTo = sw.Ports[0];
            sw.Ports[0].ConnectedTo = pc1.Ports[0];
            sw.Ports[1].ConnectedTo = pc2.Ports[0];
            pc2.Ports[0].ConnectedTo = sw.Ports[1];
            _devices.Add(pc1);
            _devices.Add(sw);
            _devices.Add(pc2);

            var mac = _engine.ArpResolve(pc1, "192.168.1.2");
            Assert.That(mac, Is.Not.Null);
            Assert.That(mac, Is.EqualTo("MAC:" + pc2.Id));
        }

        [Test]
        public void ArpResolve_NotFound_ReturnsNull()
        {
            var pc = new Device(DeviceType.PC, "PC1") { IP = "192.168.1.1" };
            _devices.Add(pc);

            var mac = _engine.ArpResolve(pc, "10.0.0.1");
            Assert.That(mac, Is.Null);
        }
    }

    [TestFixture]
    public class SimEngineSimulateValidationTests
    {
        [Test]
        public void Simulate_SourceNull_Fails()
        {
            var engine = new SimEngine(new List<Device>(), null, null);
            engine.Simulate(null, new Device(DeviceType.PC, "DST"));
            Assert.That(engine.StatusMessage, Is.EqualTo("请选择源和目标设备"));
        }

        [Test]
        public void Simulate_DestinationNull_Fails()
        {
            var engine = new SimEngine(new List<Device>(), null, null);
            engine.Simulate(new Device(DeviceType.PC, "SRC"), null);
            Assert.That(engine.StatusMessage, Is.EqualTo("请选择源和目标设备"));
        }

        [Test]
        public void Simulate_SourceNotPC_Fails()
        {
            var src = new Device(DeviceType.Switch, "SW1");
            var dst = new Device(DeviceType.PC, "PC1");
            var engine = new SimEngine(new List<Device> { src, dst }, null, null);
            engine.Simulate(src, dst);
            Assert.That(engine.StatusMessage, Is.EqualTo("源必须是PC"));
        }

        [Test]
        public void Simulate_SourceNoIP_Fails()
        {
            var src = new Device(DeviceType.PC, "PC1");
            var dst = new Device(DeviceType.PC, "PC2") { IP = "192.168.1.2" };
            var engine = new SimEngine(new List<Device> { src, dst }, null, null);
            engine.Simulate(src, dst);
            Assert.That(engine.StatusMessage, Is.EqualTo("源PC需配置IP地址"));
        }

        [Test]
        public void Simulate_SourceNotConnected_Fails()
        {
            var src = new Device(DeviceType.PC, "PC1") { IP = "192.168.1.1" };
            var dst = new Device(DeviceType.PC, "PC2") { IP = "192.168.1.2" };
            var engine = new SimEngine(new List<Device> { src, dst }, null, null);
            engine.Simulate(src, dst);
            Assert.That(engine.StatusMessage, Does.Contain("失败：PC未连接线缆"));
        }

        [Test]
        public void Simulate_DestinationNoIP_Fails()
        {
            var src = new Device(DeviceType.PC, "PC1") { IP = "192.168.1.1" };
            var dst = new Device(DeviceType.WebSite, "Site1");
            dst.IP = "";
            src.Ports[0].ConnectedTo = dst.Ports[0];
            dst.Ports[0].ConnectedTo = src.Ports[0];
            var engine = new SimEngine(new List<Device> { src, dst }, null, null);
            engine.Simulate(src, dst);
            Assert.That(engine.StatusMessage, Is.EqualTo("目标未配置IP"));
        }
    }

    [TestFixture]
    public class SimEngineSimulateSameSubnetTests
    {
        private void ConnectPorts(Device a, int portIndexA, Device b, int portIndexB)
        {
            a.Ports[portIndexA].ConnectedTo = b.Ports[portIndexB];
            b.Ports[portIndexB].ConnectedTo = a.Ports[portIndexA];
        }

        [Test]
        public void Simulate_PCToPCSameSubnetViaSwitch_ShouldSucceed()
        {
            var pc1 = new Device(DeviceType.PC, "PC1") { IP = "192.168.1.1", Mask = "255.255.255.0" };
            var sw = new Device(DeviceType.Switch, "SW1");
            var pc2 = new Device(DeviceType.PC, "PC2") { IP = "192.168.1.2", Mask = "255.255.255.0" };
            ConnectPorts(pc1, 0, sw, 0);
            ConnectPorts(pc2, 0, sw, 12);

            var devices = new List<Device> { pc1, sw, pc2 };
            var engine = new SimEngine(devices, null, null);
            engine.Simulate(pc1, pc2);

            Assert.That(engine.StatusMessage, Does.Contain("成功"));
            Assert.That(engine.Steps.Count, Is.GreaterThanOrEqualTo(2));
            Assert.That(pc2.Logs.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Simulate_PCToPCDirect_ShouldSucceed()
        {
            var pc1 = new Device(DeviceType.PC, "PC1") { IP = "192.168.1.1", Mask = "255.255.255.0" };
            var pc2 = new Device(DeviceType.PC, "PC2") { IP = "192.168.1.2", Mask = "255.255.255.0" };
            ConnectPorts(pc1, 0, pc2, 0);

            var devices = new List<Device> { pc1, pc2 };
            var engine = new SimEngine(devices, null, null);
            engine.Simulate(pc1, pc2);

            Assert.That(engine.StatusMessage, Does.Contain("成功"));
        }

        [Test]
        public void Simulate_PCToPCSameSubnet_CreatesPacketOnSource()
        {
            var pc1 = new Device(DeviceType.PC, "PC1") { IP = "192.168.1.1", Mask = "255.255.255.0" };
            var pc2 = new Device(DeviceType.PC, "PC2") { IP = "192.168.1.2", Mask = "255.255.255.0" };
            ConnectPorts(pc1, 0, pc2, 0);

            var devices = new List<Device> { pc1, pc2 };
            var engine = new SimEngine(devices, null, null);
            engine.Simulate(pc1, pc2);

            Assert.That(pc1.Packets.Count, Is.GreaterThan(0));
            Assert.That(pc1.Packets[0].SrcIP, Is.EqualTo("192.168.1.1"));
            Assert.That(pc1.Packets[0].DstIP, Is.EqualTo("192.168.1.2"));
        }

        [Test]
        public void Simulate_PCToPCSameSubnet_HasCorrectStepSequence()
        {
            var pc1 = new Device(DeviceType.PC, "PC1") { IP = "192.168.1.1", Mask = "255.255.255.0" };
            var pc2 = new Device(DeviceType.PC, "PC2") { IP = "192.168.1.2", Mask = "255.255.255.0" };
            ConnectPorts(pc1, 0, pc2, 0);

            var devices = new List<Device> { pc1, pc2 };
            var engine = new SimEngine(devices, null, null);
            engine.Simulate(pc1, pc2);

            Assert.That(engine.Steps.Any(s => s.Title.Contains("检查目的地址")), Is.True);
            Assert.That(engine.Steps.Any(s => s.Layer == "ARP"), Is.True);
            Assert.That(engine.Steps.Any(s => s.Title.Contains("发送帧")), Is.True);
        }

        [Test]
        public void Simulate_PCToSameSubnet_ShouldLogIPLayerSteps()
        {
            var pc1 = new Device(DeviceType.PC, "PC1") { IP = "192.168.1.1", Mask = "255.255.255.0" };
            var pc2 = new Device(DeviceType.PC, "PC2") { IP = "192.168.1.2", Mask = "255.255.255.0" };
            ConnectPorts(pc1, 0, pc2, 0);

            var devices = new List<Device> { pc1, pc2 };
            var engine = new SimEngine(devices, null, null);
            engine.Simulate(pc1, pc2);

            Assert.That(engine.Steps.Any(s => s.Layer == "IP"), Is.True);
            Assert.That(engine.Steps.Any(s => s.Layer == "MAC"), Is.True);
        }
    }

    [TestFixture]
    public class SimEngineSimulateDifferentSubnetTests
    {
        private void ConnectPorts(Device a, int portIndexA, Device b, int portIndexB)
        {
            a.Ports[portIndexA].ConnectedTo = b.Ports[portIndexB];
            b.Ports[portIndexB].ConnectedTo = a.Ports[portIndexA];
        }

        [Test]
        public void Simulate_DifferentSubnetNoGateway_Fails()
        {
            var pc1 = new Device(DeviceType.PC, "PC1") { IP = "192.168.1.1", Mask = "255.255.255.0" };
            var pc2 = new Device(DeviceType.PC, "PC2") { IP = "10.0.0.1", Mask = "255.255.255.0" };
            ConnectPorts(pc1, 0, pc2, 0);

            var devices = new List<Device> { pc1, pc2 };
            var engine = new SimEngine(devices, null, null);
            engine.Simulate(pc1, pc2);

            Assert.That(engine.StatusMessage, Does.Contain("失败：未配网关"));
        }

        [Test]
        public void Simulate_DifferentSubnetViaRouter_ShouldSucceed()
        {
            var pc1 = new Device(DeviceType.PC, "PC1") { IP = "192.168.1.10", Mask = "255.255.255.0" };
            pc1.Gateways.Add("192.168.1.1");
            var router = new Device(DeviceType.Router, "R1");
            router.RoutingTable.Entries.Add(new RouteEntry { Network = "10.0.0.0", Mask = "255.255.255.0", NextHop = "10.0.0.10", OutPort = "eth1", Cost = 1 });
            var pc2 = new Device(DeviceType.PC, "PC2") { IP = "10.0.0.10", Mask = "255.255.255.0" };

            ConnectPorts(pc1, 0, router, 0);
            ConnectPorts(router, 1, pc2, 0);

            var devices = new List<Device> { pc1, router, pc2 };
            var engine = new SimEngine(devices, null, null);
            engine.Simulate(pc1, pc2);

            Assert.That(engine.StatusMessage, Does.Contain("成功"));
        }

        [Test]
        public void Simulate_DifferentSubnet_ShouldHaveRouteStep()
        {
            var pc1 = new Device(DeviceType.PC, "PC1") { IP = "192.168.1.10", Mask = "255.255.255.0" };
            pc1.Gateways.Add("192.168.1.1");
            var router = new Device(DeviceType.Router, "R1");
            router.RoutingTable.Entries.Add(new RouteEntry { Network = "10.0.0.0", Mask = "255.255.255.0", NextHop = "10.0.0.10", OutPort = "eth1", Cost = 1 });
            var pc2 = new Device(DeviceType.PC, "PC2") { IP = "10.0.0.10", Mask = "255.255.255.0" };

            ConnectPorts(pc1, 0, router, 0);
            ConnectPorts(router, 1, pc2, 0);

            var devices = new List<Device> { pc1, router, pc2 };
            var engine = new SimEngine(devices, null, null);
            engine.Simulate(pc1, pc2);

            Assert.That(engine.Steps.Any(s => s.Layer == "Route"), Is.True);
            Assert.That(engine.Steps.Any(s => s.Title.Contains("查路由表")), Is.True);
        }

        [Test]
        public void Simulate_RouterNoRoute_Fails()
        {
            var pc1 = new Device(DeviceType.PC, "PC1") { IP = "192.168.1.10", Mask = "255.255.255.0" };
            pc1.Gateways.Add("192.168.1.1");
            var router = new Device(DeviceType.Router, "R1");
            var pc2 = new Device(DeviceType.PC, "PC2") { IP = "10.0.0.10", Mask = "255.255.255.0" };

            ConnectPorts(pc1, 0, router, 0);
            ConnectPorts(router, 1, pc2, 0);

            var devices = new List<Device> { pc1, router, pc2 };
            var engine = new SimEngine(devices, null, null);
            engine.Simulate(pc1, pc2);

            Assert.That(engine.StatusMessage, Does.Contain("失败：无路由"));
        }
    }

    [TestFixture]
    public class SimEngineNATTests
    {
        private void ConnectPorts(Device a, int portIndexA, Device b, int portIndexB)
        {
            a.Ports[portIndexA].ConnectedTo = b.Ports[portIndexB];
            b.Ports[portIndexB].ConnectedTo = a.Ports[portIndexA];
        }

        [Test]
        public void Simulate_PrivateToPublic_ShouldHaveNATStep()
        {
            var pc1 = new Device(DeviceType.PC, "PC1") { IP = "192.168.1.10", Mask = "255.255.255.0" };
            pc1.Gateways.Add("192.168.1.1");
            var router = new Device(DeviceType.Router, "R1");
            router.NatTable.Enabled = true;
            router.NatTable.ExternalIP = "203.0.113.50";
            router.RoutingTable.Entries.Add(new RouteEntry { Network = "8.8.8.0", Mask = "255.255.255.0", NextHop = "8.8.8.8", OutPort = "eth2", Cost = 1 });
            var webServer = new Device(DeviceType.WebSite, "www.example.com") { IP = "8.8.8.8" };

            ConnectPorts(pc1, 0, router, 0);
            ConnectPorts(router, 2, webServer, 0);

            var devices = new List<Device> { pc1, router, webServer };
            var engine = new SimEngine(devices, null, null);
            engine.Simulate(pc1, webServer);

            Assert.That(engine.Steps.Any(s => s.Layer == "NAT"), Is.True);
        }

        [Test]
        public void Simulate_PrivateToPublic_ShouldAddNatEntry()
        {
            var pc1 = new Device(DeviceType.PC, "PC1") { IP = "192.168.1.10", Mask = "255.255.255.0" };
            pc1.Gateways.Add("192.168.1.1");
            var router = new Device(DeviceType.Router, "R1");
            router.NatTable.Enabled = true;
            router.NatTable.ExternalIP = "203.0.113.50";
            router.RoutingTable.Entries.Add(new RouteEntry { Network = "8.8.8.0", Mask = "255.255.255.0", NextHop = "8.8.8.8", OutPort = "eth2", Cost = 1 });
            var webServer = new Device(DeviceType.WebSite, "www.example.com") { IP = "8.8.8.8" };

            ConnectPorts(pc1, 0, router, 0);
            ConnectPorts(router, 2, webServer, 0);

            var devices = new List<Device> { pc1, router, webServer };
            var engine = new SimEngine(devices, null, null);
            engine.Simulate(pc1, webServer);

            Assert.That(router.NatTable.Entries.Count, Is.GreaterThan(0));
            Assert.That(router.NatTable.Entries[0].InternalIP, Is.EqualTo("192.168.1.10"));
            Assert.That(router.NatTable.Entries[0].ExternalIP, Is.EqualTo("203.0.113.50"));
        }
    }

    [TestFixture]
    public class SimEngineDNSTests
    {
        private void ConnectPorts(Device a, int portIndexA, Device b, int portIndexB)
        {
            a.Ports[portIndexA].ConnectedTo = b.Ports[portIndexB];
            b.Ports[portIndexB].ConnectedTo = a.Ports[portIndexA];
        }

        [Test]
        public void Simulate_PCWithDNSToWebSite_ShouldHaveDNSStep()
        {
            var pc1 = new Device(DeviceType.PC, "PC1") { IP = "192.168.1.10", Mask = "255.255.255.0", DnsServer = "8.8.8.8" };
            var dnsServer = new Device(DeviceType.DNSServer, "DNS");
            var website = new Device(DeviceType.WebSite, "www.gov.cn") { IP = "203.0.113.10" };

            pc1.Ports[0].ConnectedTo = dnsServer.Ports[0];
            dnsServer.Ports[0].ConnectedTo = pc1.Ports[0];

            var devices = new List<Device> { pc1, dnsServer, website };
            var engine = new SimEngine(devices, null, null);
            engine.Simulate(pc1, website);

            Assert.That(engine.Steps.Any(s => s.Layer == "DNS"), Is.True);
        }

        [Test]
        public void Simulate_PCWithoutDNSToWebSite_ShouldHaveNoDnsStep()
        {
            var pc1 = new Device(DeviceType.PC, "PC1") { IP = "192.168.1.10", Mask = "255.255.255.0" };
            var website = new Device(DeviceType.WebSite, "www.test.com") { IP = "203.0.113.10" };

            pc1.Ports[0].ConnectedTo = website.Ports[0];
            website.Ports[0].ConnectedTo = pc1.Ports[0];

            var devices = new List<Device> { pc1, website };
            var engine = new SimEngine(devices, null, null);
            engine.Simulate(pc1, website);

            Assert.That(engine.Steps.Any(s => s.Title.Contains("无DNS配置")), Is.True);
        }

        [Test]
        public void Simulate_PCWithDNSToPC_ShouldHaveNoDnsStep()
        {
            var pc1 = new Device(DeviceType.PC, "PC1") { IP = "192.168.1.10", Mask = "255.255.255.0", DnsServer = "8.8.8.8" };
            var pc2 = new Device(DeviceType.PC, "PC2") { IP = "192.168.1.20", Mask = "255.255.255.0" };
            ConnectPorts(pc1, 0, pc2, 0);

            var devices = new List<Device> { pc1, pc2 };
            var engine = new SimEngine(devices, null, null);
            engine.Simulate(pc1, pc2);

            Assert.That(engine.Steps.Any(s => s.Title.Contains("DNS")), Is.False);
        }
    }

    [TestFixture]
    public class SimEnginePublicAPITests
    {
        [Test]
        public void StartSimulation_ShouldResetAndRun()
        {
            var pc1 = new Device(DeviceType.PC, "PC1") { IP = "192.168.1.1", Mask = "255.255.255.0" };
            var pc2 = new Device(DeviceType.PC, "PC2") { IP = "192.168.1.2", Mask = "255.255.255.0" };
            pc1.Ports[0].ConnectedTo = pc2.Ports[0];
            pc2.Ports[0].ConnectedTo = pc1.Ports[0];

            var devices = new List<Device> { pc1, pc2 };
            string completeMsg = null;
            bool refreshCalled = false;

            var engine = new SimEngine(devices, () => refreshCalled = true, (msg) => completeMsg = msg);

            // Async void, need to wait
            engine.StartSimulation(pc1, pc2);

            // Poll for completion (StartSimulation runs on threadpool)
            System.Threading.Thread.Sleep(500);

            Assert.That(completeMsg, Does.Contain("成功"));
            Assert.That(refreshCalled, Is.True);
        }
    }
}
