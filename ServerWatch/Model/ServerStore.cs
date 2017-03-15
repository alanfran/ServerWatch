using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Steam.Query;
using System.Net;
using System.Globalization;

namespace ServerWatch
{
    public class ServerStore
    {
        // server info query results
        public ObservableCollection<ServerInfoResult> Results { set; get; }
        // server IP addresses
        public IList<Server> Servers { set; get; }
        // map serverinfo -> server
        public IDictionary<ServerInfoResult, Server> Map { set; get; }

        public ServerStore() : base()
        {
            Results = new ObservableCollection<ServerInfoResult>();
            Servers = new List<Server>();
            Map = new Dictionary<ServerInfoResult, Server>();

        }

        public void AddServer(string address)
        {
            Servers.Add(new Server(CreateIPEndPoint(address)));
        }

        public void RemoveServer(string address)
        {
            var server = Servers.Where(x => x.EndPoint.ToString() == address).Single();
            try
            {
                var result = Map.Where(x => x.Value.EndPoint == server.EndPoint).Select(p => p.Key).Single();
                Results.Remove(result);
            }
            catch { }
            Servers.Remove(server);
        }

        public async Task QueryServers()
        {
            Results.Clear();
            foreach (var server in Servers)
            {
                var result = await server.GetServerInfo();
                Results.Add(result);
                Map.Add(result, server);
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

    }
}
