using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Net;
using System.Globalization;
using Steam.Query;
using System.IO;
using System.Windows.Controls;

namespace ServerWatch
{
    class ServerViewModel
    {
        private IList<Server> m_Servers;
        private ServerInfoList m_Results;

        private ICommand clickAdd;

        private string savepath;
        private string savefile;

        public ServerViewModel()
        {
            m_Servers = new List<Server>();
            m_Results = new ServerInfoList();
            clickAdd = new ButtonCommand(this);

            savepath = Environment.ExpandEnvironmentVariables("%AppData%") + "\\ServerWatch";
            savefile = savepath + "\\serverlist.txt";

            // load saved servers
            try
            {
                Directory.CreateDirectory(savepath);
                var serverAddresses = File.ReadAllLines(savefile);
                foreach (var address in serverAddresses)
                {
                    Servers.Add(new Server(CreateIPEndPoint(address)));
                }
            }
            catch
            {

            }

            // query them...
            QueryServers();
        }

        public IList<Server> Servers
        {
            get { return m_Servers; }
            set { m_Servers = value; }
        }

        public ServerInfoList Results
        {
            get { return m_Results; }
            set { m_Results = value; }
        }

        public ICommand ClickAdd
        {
            get { return clickAdd; }
        }

        public void AddServer(string address)
        {
            Servers.Add(new Server(CreateIPEndPoint(address)));
        }

        public void AddNewServer(string address)
        {
            // ignore if address already exists
            if (Servers.Any(s => s.EndPoint.ToString() == address))
            {
                return;
            }
            
            AddServer(address);
            QueryServers();

            // save to file
            var addresses = Servers.Select(x=>x.EndPoint.ToString()).ToArray();
            File.WriteAllLines(savefile, addresses);
        }

        public async void QueryServers()
        {
            Results.Clear();
            foreach (var server in Servers)
            {
                Results.Add(await server.GetServerInfo());
            }
        }

        public async Task<ServerInfoResult> FetchServerAsync(string addr)
        {
            var server = new Server(CreateIPEndPoint(addr));
            var info = await server.GetServerInfo();
            return info;
        }

        public static IPEndPoint CreateIPEndPoint(string endPoint)
        {
            string[] ep = endPoint.Split(':');
            if (ep.Length != 2) throw new FormatException("Invalid endpoint format");
            IPAddress ip;
            if (!IPAddress.TryParse(ep[0], out ip))
            {
                throw new FormatException("Invalid ip-adress");
            }
            int port;
            if (!int.TryParse(ep[1], NumberStyles.None, NumberFormatInfo.CurrentInfo, out port))
            {
                throw new FormatException("Invalid port");
            }
            return new IPEndPoint(ip, port);
        }

        public void TextBoxGotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            (sender as TextBox).Clear();
        }

        public void TextBoxLostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            (sender as TextBox).Text = "Add Server...";
        }

        private class ButtonCommand : ICommand
        {
            private ServerViewModel vm;

            public ButtonCommand(ServerViewModel viewmodel)
            {
                vm = viewmodel;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public event EventHandler CanExecuteChanged;

            public void Execute(object parameter)
            {
                var input = parameter.ToString();
                if (!input.Contains(":"))
                {
                    input += ":27015";
                }

                try
                {
                    vm.AddNewServer(input);
                }
                catch
                {
                   System.Windows.MessageBox.Show("Please enter a valid IP address. URLs are not yet supported.");
                }
            }
        }
    }
}
