using Npgsql;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MewPewServerNew
{
    class Program
    {
        
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
            IPAddress ipAddress = IPAddress.Parse("172.31.18.142");
            int port = 2500;
            

            tcpListener = new TcpListener(ipAddress, port);
            tcpListener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, false);
            tcpListener.Start();

            listening = true;
            udp = new UdpClient(2701);
            generalEndpoint = new IPEndPoint(IPAddress.Any, 2701);
            sockets = new List<MewPewSocket>();
            activeUsers = new List<User>();
            maps = new List<Map>();
            await DBController.InitDb();

            maps.Add(new Map("Alpha Centauri"));

            Console.WriteLine($"Server listening on {ipAddress}:{port} in {env} mode");

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
