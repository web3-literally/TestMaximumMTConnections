using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using nj4x.Net;

namespace TestMaximumMTConnections
{
    [SuppressMessage("ReSharper", "LocalizableElement")]
    public class Configs
    {
        private static DateTime _tsConfigModifiedAt;
        private static Timer _guiTimer;
        private static Dashboard _gui;
        private static string _localIP;

        public static Dashboard Gui
        {
            get { return _gui; }
            set
            {
                _gui = value;
                //
                // init gui change actions
                //
                EventHandler onSetupModified = (sender, args) =>
                {
                    _tsConfigModifiedAt = DateTime.Now;
                    _gui.buttonStart.Enabled = false;
                };
                _gui.textBoxTSPort.TextChanged += onSetupModified;
                _gui.textBoxAccountNo.TextChanged += onSetupModified;
                _gui.textBoxLocalIP.TextChanged += onSetupModified;
                _gui.textBoxBroker.TextChanged += onSetupModified;
                _gui.textBoxNumTerms.TextChanged += onSetupModified;
                _gui.textBoxPassword.TextChanged += onSetupModified;
                _gui.textBoxTSHost.TextChanged += onSetupModified;
                _gui.buttonStart.TextChanged += onSetupModified;
                //
                _gui.radioButtonMT4.CheckedChanged += (sender, args) =>
                {
                    if (_gui.radioButtonMT4.Checked)
                    {
                        _gui.textBoxBroker.Text = "demo1-amsterdam.fxpro.com";
                        _gui.textBoxAccountNo.Text = "6796186";
                        _gui.textBoxPassword.Text = "dpkk4hg";
                    }
                    else
                    {
//                        _gui.textBoxBroker.Text = "178.255.202.119";
//                        _gui.textBoxAccountNo.Text = "50131257";
//                        _gui.textBoxPassword.Text = "ljaxqw2r";
                        _gui.textBoxBroker.Text = "88.212.234.128:1952";
                        _gui.textBoxAccountNo.Text = "6857379";//6857498
                        _gui.textBoxPassword.Text = "p5gyafxp";//2vjvwxwp
                    }
                };
                //
                // set default values
                //
                _gui.textBoxTSHost.Text = "127.0.0.1";
                _gui.textBoxTSPort.Text = "7788";
                _gui.textBoxLocalIP.Text = "127.0.0.1";
                _gui.radioButtonMT4.Checked = false;
                _gui.radioButtonMT4.Checked = true;
                //
                if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                {
                    var host = Dns.GetHostEntry(Dns.GetHostName());
                    foreach (var ip in host.AddressList)
                    {
                        if (ip.AddressFamily == AddressFamily.InterNetwork)
                        {
                            _gui.textBoxLocalIP.Text = ip.ToString();
                        }
                    }
                }
                //
                // init timer job
                //
                _guiTimer = new Timer {Interval = 200, Enabled = true};
                _guiTimer.Tick += (sender, args) =>
                {
                    var guiChangedSeconds = (DateTime.Now - _tsConfigModifiedAt).Seconds;
                    //
                    var tsHost = _gui.textBoxTSHost.Text;
                    if (TsHost == null || TsHost != tsHost)
                    {
                        TsHost = tsHost;
                    }
                    int tsPort;
                    if (int.TryParse(_gui.textBoxTSPort.Text, out tsPort) && TsPort == 0 || TsPort != tsPort)
                    {
                        TsPort = tsPort;
                    }
                    if ((LocalIP == null || _gui.textBoxLocalIP.Text != LocalIP) && guiChangedSeconds > 1)
                    {
                        LocalIP = _gui.textBoxLocalIP.Text;
                    }
                    //
                    if (guiChangedSeconds > 1)
                    {
                        bool isTsAvailable = false;
                        ulong tsBoxid = 0;
                        if (TsPort > 0 && !String.IsNullOrEmpty(TsHost))
                        {
                            try
                            {
                                tsBoxid = TerminalClient.GetBoxID(TsHost, TsPort);
                                isTsAvailable = true;
                                _gui.textBoxTSInfo.Text = tsBoxid > 0
                                    ? $"Connected to Personal TS @ {TsHost}:{TsPort}, number of terminals is limited by 32"
                                    : $"Connected to Professional/Premium TS @ {TsHost}:{TsPort}";
                            }
                            catch (Exception e)
                            {
                                isTsAvailable = false;
                                _gui.textBoxTSInfo.Text = "Error connecting to TS: " + e.Message;
                            }
                        }
                        else
                        {
                            _gui.textBoxTSInfo.Text = "Waiting for Terminal Server parameters entry...";
                        }
                        //
                        int numTerms;
                        if (!int.TryParse(_gui.textBoxNumTerms.Text, out numTerms))
                        {
                            numTerms = 0;
                            _gui.textBoxNumTerms.Text = "0";
                        }
                        if (numTerms > 32 && tsBoxid > 0)
                        {
                            numTerms = 32;
                            _gui.textBoxNumTerms.Text = "32";
                        }
                        if (numTerms > 500)
                        {
                            numTerms = 500;
                            _gui.textBoxNumTerms.Text = "500";
                        }
                        //
                        if (numTerms > 0 && isTsAvailable)
                        {
                            _gui.buttonStart.Enabled = true;
                        }
                        //
                        var b = _gui.buttonStart.Text == "Start";
                        _gui.textBoxTSHost.Enabled = b;
                        _gui.textBoxTSPort.Enabled = b;
                        _gui.textBoxLocalIP.Enabled = b;
                        _gui.textBoxBroker.Enabled = b;
                        _gui.textBoxAccountNo.Enabled = b;
                        _gui.textBoxPassword.Enabled = b;
                        _gui.textBoxNumTerms.Enabled = b;
                        _gui.radioButtonMT4.Enabled = b;
                        _gui.radioButtonMT5.Enabled = b;
                        //
                        _tsConfigModifiedAt = DateTime.Now + TimeSpan.FromSeconds(30); // set next check in 30 seconds
                    }
                };
            }
        }

        public static String TsHost { set; get; }
        public static int TsPort { set; get; }

        public static String Broker => _gui.textBoxBroker.Text;
        public static String Account => _gui.textBoxAccountNo.Text;
        public static String Password => _gui.textBoxPassword.Text;
        public static int NumTerms => int.Parse(_gui.textBoxNumTerms.Text);
        public static bool IsMT5 => _gui.radioButtonMT5.Checked;

        public static String LocalIP
        {
            set
            {
                _localIP = value;
                System.Configuration.ConfigurationManager.AppSettings.Set("nj4x_server_host", _localIP);
            }
            get { return _localIP; }
        }
    }
}