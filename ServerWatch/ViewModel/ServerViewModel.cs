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
    public class ServerViewModel
    {
        public ServerStore ServerModel { set; get; }

        public ICommand ClickAdd { get; set; }
        public ICommand Row_DoubleClick { get; set; }
        public ICommand ClickDelete { get; set; }

        private string savepath;
        private string savefile;

        public ServerViewModel()
        {
            ServerModel = new ServerStore();

            ClickAdd = new ButtonCommand(this);
            Row_DoubleClick = new DoubleClickCommand(this);
            ClickDelete = new DeleteCommand(this);

            savepath = Environment.ExpandEnvironmentVariables("%AppData%") + "\\ServerWatch";
            savefile = savepath + "\\serverlist.txt";

            // load saved servers
            try
            {
                Directory.CreateDirectory(savepath);
                var serverAddresses = File.ReadAllLines(savefile);
                foreach (var address in serverAddresses)
                {
                    ServerModel.AddServer(address);
                }
            }
            catch
            {

            }

            // query them...
            ServerModel.QueryServers();
        }

        public void AddNewServer(string address)
        {
            // ignore if address already exists
            if (ServerModel.Servers.Any(s => s.EndPoint.ToString() == address))
            {
                return;
            }

            ServerModel.AddServer(address);
            ServerModel.QueryServers();

            // save to file
            var addresses = ServerModel.Servers.Select(x => x.EndPoint.ToString()).ToArray();
            File.WriteAllLines(savefile, addresses);
        }

        public void RemoveServer(Server s)
        {
            // filter out the server from our list of servers
            var filtered = ServerModel.Servers.Where(x => x.EndPoint != s.EndPoint);

            // write addresses to file
            var addresses = filtered.Select(x => x.EndPoint.ToString()).ToArray();
            File.WriteAllLines(savefile, addresses);

            ServerModel.RemoveServer(s.EndPoint.ToString());
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

                var server = vm.ServerModel.Map[result];
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

        private class DeleteCommand : ICommand
        {
            private ServerViewModel vm;

            public DeleteCommand(ServerViewModel viewmodel)
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

                var server = vm.ServerModel.Map[result];

                vm.RemoveServer(server);
            }
        }
    }
}
