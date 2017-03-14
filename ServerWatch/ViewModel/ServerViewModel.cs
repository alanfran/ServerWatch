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

        private IDictionary<ServerInfoResult, Server> serverMap;

        private ICommand clickAdd;

        private string savepath;
        private string savefile;

        public ServerViewModel()
        {
            m_Servers = new List<Server>();
            m_Results = new ServerInfoList();

            serverMap = new Dictionary<ServerInfoResult, Server>();

            clickAdd = new ButtonCommand(this);
            Row_DoubleClick = new DoubleClickCommand(this);

            savepath = Environment.ExpandEnvironmentVariables("%AppData%") + "\\ServerWatch";
            savefile = savepath + "\\serverlist.txt";

            // load saved servers
            try
            {
                Directory.CreateDirectory(savepath);
                var serverAddresses = File.ReadAllLines(savefile);
                foreach (var address in serverAddresses)
                {
                    //Servers.Add(new Server(CreateIPEndPoint(address)));
                    AddServer(address);
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

        public ICommand Row_DoubleClick
        {
            get;
            set;
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
                var result = await server.GetServerInfo();
                Results.Add(result);
                serverMap.Add(result, server);
            }
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


        // UI Code

        public void TextBoxGotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            (sender as TextBox).Clear();
        }

        public void TextBoxLostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            (sender as TextBox).Text = "Add Server...";
        }


        // Click Command Handlers

        private class DoubleClickCommand : ICommand
        {
            private ServerViewModel vm;

            public DoubleClickCommand(ServerViewModel viewmodel)
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
                var result = parameter as ServerInfoResult;

                var server = vm.serverMap[result];
                var address = "steam://connect/" + server.EndPoint.ToString();

                System.Diagnostics.Process.Start(address);
            }
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
