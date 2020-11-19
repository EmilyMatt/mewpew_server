using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;


namespace MewPewServerNew
{
    class Program
    {
        public static IConfiguration config;
        public static List<MewPewSocket> sockets;
        public static List<User> activeUsers;
        public static List<Map> maps;

        public static UdpClient udp;
        public static bool listening;
        public static string appVersion;
        public static string env = "prod";

        private static TcpListener tcpListener;
        private static IPEndPoint generalEndpoint;

        static async Task Main(string[] args)
        {
            config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            IPAddress ipAddress = IPAddress.Parse(config["ip"]);
            int tcPort = int.Parse(config["tcPort"]);
            int udPort = int.Parse(config["udPort"]);

            tcpListener = new TcpListener(ipAddress, tcPort);
            tcpListener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, false);
            tcpListener.Start();

            listening = true;
            udp = new UdpClient(udPort);
            generalEndpoint = new IPEndPoint(IPAddress.Any, udPort);
            sockets = new List<MewPewSocket>();
            activeUsers = new List<User>();
            maps = new List<Map>();
            await DBController.InitDb();

            maps.Add(new Map("Alpha Centauri"));

            Console.WriteLine($"Server listening on {ipAddress}:{tcPort} in {env} mode");

            await Task.Run( async () => {
                while (listening)
                {
                    if (tcpListener.Pending())
                    {
                        MewPewSocket socket = new MewPewSocket(tcpListener.AcceptTcpClient());
                        sockets.Add(socket);
                    }

                    if (udp.Available > 0)
                        await Task.Run(GetDataUdp);

                    Task.Delay(10);
                }
            });

            tcpListener.Stop();
        }

        public async static Task GetDataUdp()
        {
            string ip = "unknown";
            int port = -1;
            Byte[] res = new Byte[256];
            try
            {
                res = udp.Receive(ref generalEndpoint);
                string msg = Encoding.UTF8.GetString(res);
                ip = generalEndpoint.Address.ToString();
                port = generalEndpoint.Port;
                MewPewSocket socket = sockets.Find(element => element.IP == ip);
                if (socket == null)
                    return;

                socket.Port = port;
                Console.WriteLine($"Data Recieved: {msg} from {ip}:{port} via UDP");
                Task.Run(() => UDPRouter.Route(socket, msg));
                
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error on UDP socket {ip}:{port} - {e}");
            }
        }
    }
}
