using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NetSim
{
    public class MainForm : Form
    {
        private List<Device> _devices = new List<Device>();
        private SimEngine _engine;
        private Device _dragged;
        private Point _dragOffset;
        private Point _rightClick;
        private Device _rightClickDevice;

        private Panel _canvas;
        private ToolStrip _toolbar;
        private TextBox _logBox;
        private ToolStripComboBox _cbSrc, _cbDst;
        private Label _lblStatus;

        private bool _connecting;
        private Port _connectFrom;
        private Point _connectMouse;

        public MainForm()
        {
            Text = "网络协议模拟器";
            Size = new Size(1400, 850);
            BackColor = Color.FromArgb(245, 245, 245);
        _engine = new SimEngine(_devices, () => _canvas.Invalidate(), (status) =>
        {
            Invoke(new Action(() =>
            {
                _logBox.Text = string.Join("\r\n", _engine.Steps.Select(s => (s.IsError ? "✗ " : "✓ ") + s.Title + "\r\n  " + s.Detail));
                _logBox.SelectionStart = 0;
                _logBox.ScrollToCaret();
                _lblStatus.Text = status;
                _canvas.Invalidate();
            }));
        });
            BuildUI();
            AddSampleTopo();
        }

        private void BuildUI()
        {
            _toolbar = new ToolStrip { Dock = DockStyle.Top, BackColor = Color.FromArgb(240, 240, 240) };
            _toolbar.Items.Add(new ToolStripButton("添加PC", null, (s, e) => AddDevice(DeviceType.PC)));
            _toolbar.Items.Add(new ToolStripButton("交换机", null, (s, e) => AddDevice(DeviceType.Switch)));
            _toolbar.Items.Add(new ToolStripButton("路由器", null, (s, e) => AddDevice(DeviceType.Router)));
            _toolbar.Items.Add(new ToolStripButton("DNS", null, (s, e) => AddDevice(DeviceType.DNSServer)));
            _toolbar.Items.Add(new ToolStripButton("网站", null, (s, e) => AddDevice(DeviceType.WebSite)));
            _toolbar.Items.Add(new ToolStripSeparator());
            _toolbar.Items.Add(new ToolStripButton("清空", null, (s, e) => { _devices.Clear(); _canvas.Invalidate(); }));
            _toolbar.Items.Add(new ToolStripSeparator());
            _toolbar.Items.Add(new ToolStripLabel("源:"));
            _cbSrc = new ToolStripComboBox { Width = 100 };
            _cbSrc.DropDown += (s, e) => LoadCombo(_cbSrc);
            _toolbar.Items.Add(_cbSrc);
            _toolbar.Items.Add(new ToolStripLabel("目的:"));
            _cbDst = new ToolStripComboBox { Width = 100 };
            _cbDst.DropDown += (s, e) => LoadCombo(_cbDst);
            _toolbar.Items.Add(_cbDst);
            _toolbar.Items.Add(new ToolStripButton("开始模拟", null, (s, e) => StartSim()));
            _toolbar.Items.Add(new ToolStripButton("路由交换", null, (s, e) => ExchangeRoutes()));
            _toolbar.Items.Add(new ToolStripButton("重置", null, (s, e) => { _engine.Reset(); _logBox.Text = ""; _lblStatus.Text = ""; _canvas.Invalidate(); }));
            Controls.Add(_toolbar);

            var split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 950, SplitterWidth = 2, Orientation = Orientation.Vertical };
            _canvas = new Panel { BackColor = Color.White, Dock = DockStyle.Fill, AllowDrop = true };
            _canvas.DragEnter += (s, e) => e.Effect = DragDropEffects.Move;
            _canvas.DragDrop += (s, e) => { if (_dragged != null) { _dragged.Location = _canvas.PointToClient(new Point(e.X, e.Y)); _canvas.Invalidate(); } };
            _canvas.MouseDown += Canvas_MouseDown;
            _canvas.MouseMove += Canvas_MouseMove;
            _canvas.MouseUp += Canvas_MouseUp;
            _canvas.DoubleClick += Canvas_DoubleClick;
            _canvas.Paint += Canvas_Paint;
            split.Panel1.Controls.Add(_canvas);

            var rightPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(4, 30, 4, 4) };
            _logBox = new TextBox { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, Font = new Font("Consolas", 9), BackColor = Color.FromArgb(30, 30, 30), ForeColor = Color.FromArgb(200, 255, 200), ScrollBars = ScrollBars.Both, BorderStyle = BorderStyle.None, Margin = new Padding(4) };
            _lblStatus = new Label { Dock = DockStyle.Bottom, Text = "", Height = 24, BackColor = Color.FromArgb(60, 60, 60), ForeColor = Color.White };
            rightPanel.Controls.Add(_logBox);
            rightPanel.Controls.Add(_lblStatus);
            split.Panel2.Controls.Add(rightPanel);
            Controls.Add(split);
        }

        private void AddSampleTopo()
        {
            var pc1 = new Device(DeviceType.PC, "PC-A") { Location = new Point(50, 250), IP = "192.168.1.10", Mask = "255.255.255.0" };
            pc1.Gateways.Add("192.168.1.1");
            _devices.Add(pc1);

            var pc2 = new Device(DeviceType.PC, "PC-B") { Location = new Point(680, 250), IP = "10.0.0.20", Mask = "255.255.255.0" };
            pc2.Gateways.Add("10.0.0.1");
            _devices.Add(pc2);

            var sw1 = new Device(DeviceType.Switch, "SW1") { Location = new Point(180, 120) };
            _devices.Add(sw1);

            var sw2 = new Device(DeviceType.Switch, "SW2") { Location = new Point(550, 120) };
            _devices.Add(sw2);

            var r1 = new Device(DeviceType.Router, "R1") { Location = new Point(370, 230) };
            r1.Ports[0].Description = "192.168.1.1/24";
            r1.Ports[1].Description = "";
            r1.Ports[2].Description = "10.0.0.1/24";
            r1.Ports[3].Description = "";
            r1.RoutingTable.Entries.Add(new RouteEntry { Network = "192.168.1.0", Mask = "255.255.255.0", NextHop = "0.0.0.0", OutPort = "eth0", Cost = 1 });
            r1.RoutingTable.Entries.Add(new RouteEntry { Network = "10.0.0.0", Mask = "255.255.255.0", NextHop = "0.0.0.0", OutPort = "eth2", Cost = 1 });
            _devices.Add(r1);

            // Connect cables
            pc1.Ports[0].ConnectedTo = sw1.Ports[0]; sw1.Ports[0].ConnectedTo = pc1.Ports[0];
            sw1.Ports[12].ConnectedTo = r1.Ports[0]; r1.Ports[0].ConnectedTo = sw1.Ports[12];
            r1.Ports[2].ConnectedTo = sw2.Ports[12]; sw2.Ports[12].ConnectedTo = r1.Ports[2];
            sw2.Ports[0].ConnectedTo = pc2.Ports[0]; pc2.Ports[0].ConnectedTo = sw2.Ports[0];

            // Add DNS server and website
            var dns = new Device(DeviceType.DNSServer, "DNS") { Location = new Point(300, 350), IP = "192.168.1.2" };
            dns.DnsEntries.Clear();
            dns.DnsEntries.Add(new DnsEntry { Domain = "www.gov.cn", IP = "203.0.113.10" });
            dns.Ports[0].ConnectedTo = sw1.Ports[1]; sw1.Ports[1].ConnectedTo = dns.Ports[0];
            _devices.Add(dns);

            var site = new Device(DeviceType.WebSite, "www.gov.cn") { Location = new Point(680, 100), IP = "203.0.113.10" };
            site.Ports[0].ConnectedTo = r1.Ports[3]; r1.Ports[3].ConnectedTo = site.Ports[0];
            _devices.Add(site);

            // Set PC-A DNS
            pc1.DnsServer = "192.168.1.2";
            // Set PC-B DNS - same DNS server, different network
            pc2.DnsServer = "192.168.1.2";

            // Update R1 routing for internet
            r1.RoutingTable.Entries.Add(new RouteEntry { Network = "203.0.0.0", Mask = "255.0.0.0", NextHop = "0.0.0.0", OutPort = "eth3", Cost = 1 });
        }

        private void LoadCombo(ToolStripComboBox cb)
        {
            cb.Items.Clear();
            foreach (var d in _devices) if (d.Type == DeviceType.PC || d.Type == DeviceType.WebSite) cb.Items.Add(d.Name);
            if (cb.Items.Count > 0 && cb.SelectedIndex < 0) cb.SelectedIndex = 0;
        }

        private void AddDevice(DeviceType type)
        {
            string name = type.ToString() + (_devices.Count(x => x.Type == type) + 1);
            var d = new Device(type, name)
            {
                Location = new Point(new Random().Next(50, 700), new Random().Next(50, 400))
            };
            if (type == DeviceType.PC) d.Gateways.Add("");
            if (type == DeviceType.WebSite) d.Name = "www.site-" + (_devices.Count(x => x.Type == DeviceType.WebSite) + 1) + ".com";
            _devices.Add(d);
            _canvas.Invalidate();
        }

        private void Canvas_MouseDown(object sender, MouseEventArgs e)
        {
            var hit = HitTest(e.Location);
            if (e.Button == MouseButtons.Right)
            {
                // Cancel connection drag
                if (_connecting) { _connecting = false; _connectFrom = null; _canvas.Invalidate(); return; }
                // Check if clicking on a connection line
                var port = HitConnection(e.Location);
                if (port != null)
                {
                    var other = port.ConnectedTo;
                    port.ConnectedTo = null;
                    other.ConnectedTo = null;
                    _canvas.Invalidate();
                    return;
                }
                if (hit != null) { ShowContextMenu(hit, e.Location); return; }
                return;
            }
            if (e.Button == MouseButtons.Left)
            {
                // Check if clicking on a port anchor
                if (hit != null)
                {
                    var port = HitPort(hit, e.Location);
                    if (port != null)
                    {
                        if (_connecting)
                        {
                            if (hit != _connectFrom.Device)
                            {
                                // Complete connection
                                if (_connectFrom.ConnectedTo != null) _connectFrom.ConnectedTo.ConnectedTo = null;
                                if (port.ConnectedTo != null) port.ConnectedTo.ConnectedTo = null;
                                _connectFrom.ConnectedTo = port;
                                port.ConnectedTo = _connectFrom;
                            }
                            _connecting = false; _connectFrom = null;
                            _canvas.Invalidate();
                            return;
                        }
                        else
                        {
                            // Start connection
                            _connecting = true; _connectFrom = port;
                            _connectMouse = e.Location;
                            _canvas.Invalidate();
                            return;
                        }
                    }
                }
                // Start dragging device
                if (hit != null && !_connecting) { _dragged = hit; _dragOffset = new Point(e.X - hit.Location.X, e.Y - hit.Location.Y); }
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_connecting) { _connectMouse = e.Location; _canvas.Invalidate(); return; }
            if (_dragged != null)
            {
                _dragged.Location = new Point(e.X - _dragOffset.X, e.Y - _dragOffset.Y);
                _canvas.Invalidate();
            }
        }

        private void Canvas_MouseUp(object sender, MouseEventArgs e)
        {
            _dragged = null;
        }

        private Device HitTest(Point pt)
        {
            foreach (var d in _devices)
            {
                var r = new Rectangle(d.Location, d.Size);
                if (r.Contains(pt)) return d;
            }
            return null;
        }

        private Port HitPort(Device device, Point pt)
        {
            foreach (var port in device.Ports)
            {
                var anchor = port.Anchor;
                if (Math.Abs(pt.X - anchor.X) < 8 && Math.Abs(pt.Y - anchor.Y) < 8)
                    return port;
            }
            return null;
        }

        private Port HitConnection(Point pt)
        {
            foreach (var d in _devices)
                foreach (var p in d.Ports)
                    if (p.ConnectedTo != null && string.Compare(d.Id, p.ConnectedTo.Device.Id) < 0)
                        if (DistToLine(pt, p.Anchor, p.ConnectedTo.Anchor) < 5) return p;
            return null;
        }

        private static double DistToLine(Point pt, Point a, Point b)
        {
            double dx = b.X - a.X, dy = b.Y - a.Y;
            double len = dx * dx + dy * dy;
            if (len < 1) return Math.Sqrt((pt.X - a.X) * (pt.X - a.X) + (pt.Y - a.Y) * (pt.Y - a.Y));
            double t = Math.Max(0, Math.Min(1, ((pt.X - a.X) * dx + (pt.Y - a.Y) * dy) / len));
            double cx = a.X + t * dx, cy = a.Y + t * dy;
            return Math.Sqrt((pt.X - cx) * (pt.X - cx) + (pt.Y - cy) * (pt.Y - cy));
        }

        private void ShowContextMenu(Device device, Point pt)
        {
            var menu = new ContextMenuStrip();
            menu.Items.Add("配置", null, (s, e) => OpenConfig(device));
            if (device.Type == DeviceType.PC)
            {
                menu.Items.Add("配置IP", null, (s, e) => ConfigPC(device));
                menu.Items.Add("查看ARP表", null, (s, e) => ShowTable("ARP表 - " + device.Name, device.ArpTable.Entries.Select(ae => $"{ae.IP} → {ae.MAC}").ToList()));
            }
            else if (device.Type == DeviceType.Router)
            {
                menu.Items.Add("配置路由表", null, (s, e) => ConfigRouter(device));
                menu.Items.Add("查看路由表", null, (s, e) => ShowTable("路由表 - " + device.Name, device.RoutingTable.Entries.Select(re => $"{re.Network}/{re.Mask} → {re.NextHop} (端口{re.OutPort})").ToList()));
                menu.Items.Add("查看ARP表", null, (s, e) => ShowTable("ARP表 - " + device.Name, device.ArpTable.Entries.Select(ae => $"{ae.IP} → {ae.MAC}").ToList()));
                menu.Items.Add("查看NAT表", null, (s, e) => ShowTable("NAT表 - " + device.Name, device.NatTable.Entries.Select(ne => $"{ne.InternalIP} → {ne.ExternalIP}").ToList()));
                menu.Items.Add(new ToolStripSeparator());
                var statusItem = new ToolStripMenuItem("状态: " + (device.Status == RouterStatus.Normal ? "正常" : "拥塞"));
                statusItem.Click += (s, e) => { device.Status = device.Status == RouterStatus.Normal ? RouterStatus.Congested : RouterStatus.Normal; _canvas.Invalidate(); };
                menu.Items.Add(statusItem);
                var algoItem = new ToolStripMenuItem("算法: " + (device.Algorithm == RoutingAlgo.RIP ? "RIP" : "OSPF"));
                algoItem.Click += (s, e) => { device.Algorithm = device.Algorithm == RoutingAlgo.RIP ? RoutingAlgo.OSPF : RoutingAlgo.RIP; _canvas.Invalidate(); };
                menu.Items.Add(algoItem);
            }
            else if (device.Type == DeviceType.Switch)
            {
                menu.Items.Add("查看MAC表", null, (s, e) => ShowTable("MAC表 - " + device.Name, device.MacTable.Entries.Select(me => $"{me.MAC} → 端口{me.PortName}").ToList()));
            }
            else if (device.Type == DeviceType.DNSServer)
            {
                menu.Items.Add("配置DNS记录", null, (s, e) => ConfigDNS(device));
                menu.Items.Add("查看DNS记录", null, (s, e) => ShowTable("DNS记录 - " + device.Name, device.DnsEntries.Select(de => $"{de.Domain} → {de.IP}").ToList()));
            }
            else if (device.Type == DeviceType.WebSite)
            {
                menu.Items.Add("配置网站", null, (s, e) => ConfigWebSite(device));
            }
            menu.Items.Add("查看日志", null, (s, e) => ShowTable("通信日志 - " + device.Name, device.Logs.Select(l => l.ToString()).ToList()));
            menu.Items.Add("查看数据包", null, (s, e) => ViewPackets(device));
            menu.Items.Add("删除设备", null, (s, e) => { RemoveDevice(device); });
            menu.Show(_canvas, pt);
        }

        private void ConfigPC(Device pc)
        {
            using (var f = new Form { Text = "配置PC - " + pc.Name, Size = new Size(400, 300), StartPosition = FormStartPosition.CenterParent })
            {
                var lblIP = new Label { Text = "IP地址:", Location = new Point(20, 20), Width = 80 };
                var txtIP = new TextBox { Text = pc.IP, Location = new Point(100, 18), Width = 200 };
                var lblMask = new Label { Text = "掩码:", Location = new Point(20, 50), Width = 80 };
                var txtMask = new TextBox { Text = pc.Mask, Location = new Point(100, 48), Width = 200 };
                var lblGw = new Label { Text = "网关(逗号分隔):", Location = new Point(20, 80), Width = 120 };
                var txtGw = new TextBox { Text = string.Join(",", pc.Gateways), Location = new Point(20, 105), Width = 280 };
                var lblDns = new Label { Text = "DNS服务器:", Location = new Point(20, 140), Width = 80 };
                var txtDns = new TextBox { Text = pc.DnsServer, Location = new Point(110, 138), Width = 250 };
                var btn = new Button { Text = "保存", Location = new Point(120, 185) };
                btn.Click += (s, e) =>
                {
                    pc.IP = txtIP.Text; pc.Mask = txtMask.Text;
                    pc.DnsServer = txtDns.Text;
                    pc.Gateways = txtGw.Text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
                    f.Close(); _canvas.Invalidate();
                };
                f.Controls.AddRange(new Control[] { lblIP, txtIP, lblMask, txtMask, lblGw, txtGw, lblDns, txtDns, btn });
                f.ShowDialog();
            }
        }

        private void ConfigRouter(Device router)
        {
            using (var f = new Form { Text = "配置路由表 - " + router.Name, Size = new Size(600, 400), StartPosition = FormStartPosition.CenterParent })
            {
                var dgv = new DataGridView { Location = new Point(10, 10), Size = new Size(560, 250), AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, AllowUserToAddRows = true };
                dgv.Columns.Add("Network", "目的网络");
                dgv.Columns.Add("Mask", "掩码");
                dgv.Columns.Add("NextHop", "下一跳");
                dgv.Columns.Add("OutPort", "出端口");
                dgv.Columns.Add("Cost", "代价");
                foreach (var re in router.RoutingTable.Entries)
                    dgv.Rows.Add(re.Network, re.Mask, re.NextHop, re.OutPort, re.Cost);
                var btn = new Button { Text = "保存", Location = new Point(10, 270) };
                btn.Click += (s, e) =>
                {
                    router.RoutingTable.Entries.Clear();
                    foreach (DataGridViewRow row in dgv.Rows)
                    {
                        if (row.IsNewRow) continue;
                        var vals = row.Cells.Cast<DataGridViewCell>().Select(c => c.Value?.ToString() ?? "").ToArray();
                        if (!string.IsNullOrEmpty(vals[0]))
                            router.RoutingTable.Entries.Add(new RouteEntry
                            {
                                Network = vals[0], Mask = vals[1], NextHop = vals[2],
                                OutPort = vals[3], Cost = int.TryParse(vals[4], out int c) ? c : 1
                            });
                    }
                    f.Close(); _canvas.Invalidate();
                };
                f.Controls.AddRange(new Control[] { dgv, btn });
                f.ShowDialog();
            }
        }

        private void Canvas_DoubleClick(object sender, EventArgs e)
        {
            var pt = _canvas.PointToClient(Cursor.Position);
            var hit = HitTest(pt);
            if (hit != null) OpenConfig(hit);
        }

        private void OpenConfig(Device device)
        {
            if (device.Type == DeviceType.PC) ConfigPC(device);
            else if (device.Type == DeviceType.Router) ConfigRouter(device);
            else if (device.Type == DeviceType.Switch) ConfigSwitch(device);
            else if (device.Type == DeviceType.DNSServer) ConfigDNS(device);
            else if (device.Type == DeviceType.WebSite) ConfigWebSite(device);
        }

        private void ConfigDNS(Device dns)
        {
            using (var f = new Form { Text = "配置DNS - " + dns.Name, Size = new Size(450, 350), StartPosition = FormStartPosition.CenterParent })
            {
                var lblIP = new Label { Text = "DNS服务器IP:", Location = new Point(20, 15), Width = 100 };
                var txtIP = new TextBox { Text = dns.IP, Location = new Point(120, 13), Width = 120 };
                var lbl = new Label { Text = "域名解析记录 (域名=IP，每行一条):", Location = new Point(20, 45), Width = 400 };
                var txt = new TextBox { Location = new Point(20, 65), Size = new Size(400, 200), Multiline = true };
                txt.Text = string.Join("\r\n", dns.DnsEntries.Select(e => $"{e.Domain}={e.IP}"));
                var btn = new Button { Text = "保存", Location = new Point(180, 275) };
                btn.Click += (s, _) =>
                {
                    dns.IP = txtIP.Text;
                    dns.DnsEntries.Clear();
                    foreach (var line in txt.Lines)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        var kv = line.Split('=');
                        if (kv.Length >= 2)
                            dns.DnsEntries.Add(new DnsEntry { Domain = kv[0].Trim(), IP = kv[1].Trim() });
                    }
                    f.Close(); _canvas.Invalidate();
                };
                f.Controls.AddRange(new Control[] { lblIP, txtIP, lbl, txt, btn });
                f.ShowDialog();
            }
        }

        private void ConfigWebSite(Device site)
        {
            using (var f = new Form { Text = "配置网站 - " + site.Name, Size = new Size(300, 200), StartPosition = FormStartPosition.CenterParent })
            {
                var lblIP = new Label { Text = "IP地址:", Location = new Point(20, 20), Width = 80 };
                var txtIP = new TextBox { Text = site.IP, Location = new Point(100, 18), Width = 150 };
                var lblName = new Label { Text = "网站名称:", Location = new Point(20, 60), Width = 80 };
                var txtName = new TextBox { Text = site.Name, Location = new Point(100, 58), Width = 150 };
                var btn = new Button { Text = "保存", Location = new Point(100, 100) };
                btn.Click += (s, _) => { site.IP = txtIP.Text; site.Name = txtName.Text; f.Close(); _canvas.Invalidate(); };
                f.Controls.AddRange(new Control[] { lblIP, txtIP, lblName, txtName, btn });
                f.ShowDialog();
            }
        }

        private void ConfigSwitch(Device sw)
        {
            using (var f = new Form { Text = "配置交换机 - " + sw.Name, Size = new Size(400, 300), StartPosition = FormStartPosition.CenterParent })
            {
                var lbl = new Label { Text = "端口列表 (名=desc/side): L=左 R=右", Location = new Point(10, 10), Width = 360 };
                var txt = new TextBox { Location = new Point(10, 35), Size = new Size(360, 150), Multiline = true };
                txt.Text = string.Join("\r\n", sw.Ports.Select(p => $"{p.Name}={p.Description}|{(p.Side == PortSide.Left ? "L" : "R")}"));
                var btn = new Button { Text = "保存", Location = new Point(150, 200) };
                btn.Click += (s, ev) =>
                {
                    var oldCons = sw.Ports.Select(p => p.ConnectedTo).ToList();
                    sw.Ports.Clear();
                    foreach (var line in txt.Lines)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        var parts = line.Split('|');
                        var nameDesc = parts[0].Split('=');
                        var side = parts.Length > 1 && parts[1].Trim() == "L" ? PortSide.Left : PortSide.Right;
                        var p = new Port(nameDesc[0], nameDesc.Length > 1 ? nameDesc[1] : "", sw, side);
                        sw.Ports.Add(p);
                    }
                    for (int i = 0; i < Math.Min(oldCons.Count, sw.Ports.Count); i++)
                    {
                        if (oldCons[i] != null) { sw.Ports[i].ConnectedTo = oldCons[i]; oldCons[i].ConnectedTo = sw.Ports[i]; }
                    }
                    f.Close(); _canvas.Invalidate();
                };
                f.Controls.AddRange(new Control[] { lbl, txt, btn });
                f.ShowDialog();
            }
        }

        private void ShowTable(string title, List<string> items)
        {
            MessageBox.Show(string.Join("\r\n", items.Count > 0 ? items.ToArray() : new[] { "（空）" }), title);
        }

        private void ViewPackets(Device device)
        {
            if (device.Packets.Count == 0) { MessageBox.Show("暂无数据包"); return; }
            var pkt = device.Packets.Last();
            string view;
            if (device.Type == DeviceType.Switch)
                view = pkt.GetL2View() + "\n\n交换机仅解析到 L2(以太网帧)";
            else if (device.Type == DeviceType.Router)
                view = pkt.GetL3View() + "\n\n路由器解析到 L3(IP数据报)";
            else
                view = pkt.GetAppView();

            using (var f = new Form { Text = device.Name + " - 数据包查看", Size = new Size(450, 420), StartPosition = FormStartPosition.CenterParent })
            {
                var txt = new TextBox { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, Font = new Font("Consolas", 10), BackColor = Color.FromArgb(30, 30, 30), ForeColor = Color.FromArgb(0, 255, 100), BorderStyle = BorderStyle.None, Text = view };
                f.Controls.Add(txt);
                f.ShowDialog();
            }
        }

        private void RemoveDevice(Device d)
        {
            foreach (var port in d.Ports) if (port.ConnectedTo != null) port.ConnectedTo.ConnectedTo = null;
            _devices.Remove(d);
            _canvas.Invalidate();
        }

        private void StartSim()
        {
            var srcName = _cbSrc.SelectedItem?.ToString();
            var dstName = _cbDst.SelectedItem?.ToString();
            var src = _devices.FirstOrDefault(d => d.Name == srcName);
            var dst = _devices.FirstOrDefault(d => d.Name == dstName);
            if (src == null || dst == null) { MessageBox.Show("请选择源和目标"); return; }
            _engine.StartSimulation(src, dst);
        }

        private void ExchangeRoutes()
        {
            var routers = _devices.Where(d => d.Type == DeviceType.Router).ToList();
            if (routers.Count < 2) { MessageBox.Show("至少需要2台路由器才能交换路由"); return; }

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("=== 动态路由交换 (RIP模式) ===\r\n");

            foreach (var router in routers)
            {
                // Collect directly connected networks from this router's interfaces
                foreach (var port in router.Ports)
                {
                    if (string.IsNullOrEmpty(port.Description)) continue;
                    // Parse "192.168.1.1/24" from port description
                    var parts = port.Description.Split('/');
                    if (parts.Length < 2) continue;
                    string ip = parts[0];
                    string maskBits = parts[1];
                    string mask = MaskFromBits(int.TryParse(maskBits, out int bits) ? bits : 24);
                    string network = GetNetworkAddr(ip, mask);

                    // Send this network to all neighbor routers
                    foreach (var other in routers)
                    {
                        if (other == router) continue;
                        if (!AreConnected(router, other)) continue;

                        string nextHopIp = "";
                        var conn = router.Ports.FirstOrDefault(p => p.ConnectedTo?.Device == other);
                        if (conn != null && !string.IsNullOrEmpty(conn.Description))
                            nextHopIp = conn.Description.Split('/')[0];

                        if (!other.RoutingTable.Entries.Any(re => re.Network == network && re.Mask == mask))
                        {
                            int cost = 1;
                            if (router.Status == RouterStatus.Congested) cost = 10;
                            if (router.Algorithm == RoutingAlgo.OSPF) cost *= 100; // OSPF uses 100M/带宽

                            other.RoutingTable.Entries.Add(new RouteEntry
                            {
                                Network = network, Mask = mask, NextHop = nextHopIp,
                                OutPort = other.Ports.FirstOrDefault(p => p.ConnectedTo?.Device == router)?.Name ?? "",
                                Cost = cost
                            });
                            string algo = router.Algorithm == RoutingAlgo.RIP ? "RIP" : "OSPF";
                            string st = router.Status == RouterStatus.Congested ? "[拥塞]" : "";
                            sb.AppendLine($"  {algo}{st} {other.Name} 学习: {network}/{mask} via {nextHopIp} cost={cost}");
                        }
                    }
                }
            }

            _logBox.Text = sb.ToString();
            _canvas.Invalidate();
            MessageBox.Show("路由交换完成", "动态路由", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private bool AreConnected(Device a, Device b)
        {
            return a.Ports.Any(p => p.ConnectedTo?.Device == b);
        }

        private string GetNetworkAddr(string ip, string mask)
        {
            try
            {
                var ib = System.Net.IPAddress.Parse(ip).GetAddressBytes();
                var mb = System.Net.IPAddress.Parse(mask).GetAddressBytes();
                for (int i = 0; i < 4; i++) ib[i] &= mb[i];
                return string.Join(".", ib);
            }
            catch { return ip; }
        }

        private string MaskFromBits(int bits)
        {
            uint mask = 0xFFFFFFFF << (32 - bits);
            return $"{(mask >> 24) & 0xFF}.{(mask >> 16) & 0xFF}.{(mask >> 8) & 0xFF}.{mask & 0xFF}";
        }

        private void Canvas_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.White);

            // Draw connections
            var drawn = new HashSet<string>();
            foreach (var d in _devices)
            {
                foreach (var port in d.Ports)
                {
                    if (port.ConnectedTo != null)
                    {
                        var key = d.Id + "_" + port.ConnectedTo.Device.Id;
                        if (drawn.Contains(key)) continue;
                        drawn.Add(key);
                        drawn.Add(port.ConnectedTo.Device.Id + "_" + d.Id);
                        var p1 = port.Anchor;
                        var p2 = port.ConnectedTo.Anchor;
                        using (var pen = new Pen(Color.FromArgb(100, 100, 100), 2))
                            g.DrawLine(pen, p1, p2);
                    }
                }
            }

            // Draw temporary connection line
            if (_connecting && _connectFrom != null)
                using (var pen = new Pen(Color.Red, 2)) { pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash; g.DrawLine(pen, _connectFrom.Anchor, _connectMouse); }

            // Draw devices
            foreach (var d in _devices)
            {
                var r = new Rectangle(d.Location, d.Size);
                var brush = d.Type == DeviceType.PC ? new SolidBrush(Color.FromArgb(200, 230, 255)) :
                            d.Type == DeviceType.Switch ? new SolidBrush(Color.FromArgb(200, 255, 200)) :
                            new SolidBrush(Color.FromArgb(255, 230, 200));
                var border = d.Selected || _connecting && _connectFrom?.Device == d ? Color.Red : Color.FromArgb(80, 80, 80);
                if (_dragged == d) border = Color.Blue;

                g.FillRectangle(brush, r);
                g.DrawRectangle(new Pen(border, 2), r);

                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                using (var font = new Font("Microsoft YaHei", 9, FontStyle.Bold))
                    g.DrawString(d.Name, font, Brushes.Black, new RectangleF(r.X, r.Y + 5, r.Width, 18), sf);

                using (var font = new Font("Consolas", 7))
                {
                    if (d.Type == DeviceType.PC && !string.IsNullOrEmpty(d.IP))
                    {
                        g.DrawString(d.IP, font, Brushes.DarkBlue, new RectangleF(r.X, r.Y + 22, r.Width, 14), sf);
                        if (!string.IsNullOrEmpty(d.DnsServer))
                            g.DrawString("DNS:" + d.DnsServer, font, Brushes.DarkGray, new RectangleF(r.X, r.Y + 36, r.Width, 12), sf);
                        if (d.Gateways.Count > 0 && !string.IsNullOrEmpty(d.Gateways[0]))
                            g.DrawString("GW:" + d.Gateways[0], font, Brushes.DarkGray, new RectangleF(r.X, r.Y + 48, r.Width, 12), sf);
                    }
                    else if (d.Type == DeviceType.Router)
                    {
                        int y = r.Y + 24;
                        foreach (var p in d.Ports)
                            g.DrawString(p.Description, font, Brushes.DarkGreen, new RectangleF(r.X, y += 14, r.Width, 12), sf);
                    }
                }

                // Draw port anchors
                if (d.Type != DeviceType.PC || d.Ports.Count > 0)
                {
                    foreach (var port in d.Ports)
                    {
                        var anchor = port.Anchor;
                        var portColor = port.IsConnected ? Color.Green : Color.Gray;
                        g.FillEllipse(new SolidBrush(portColor), anchor.X - 4, anchor.Y - 4, 8, 8);
                        g.DrawString(port.Name, new Font("Consolas", 6), Brushes.Black, anchor.X - 10, anchor.Y + 8);
                    }
                }

                brush.Dispose();
            }
        }
    }
}