using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MewPewServerNew
{
    class MewPewSocket
    {
        private NetworkStream Stream { get; }
        public string IP { get; }
        public int Port { get; set; }
        public bool Listening { get; set; }
        public User UserData { get; set; }
        private string LastMessage { get; set; }
        public bool Ping { get; set; }
        public bool ReadyToSend { get; set; }
        public MewPewSocket(TcpClient client)
        {
            Stream = client.GetStream();
            IP = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
            Listening = true;
            LastMessage = "";
            Console.WriteLine($"New TCP Client {IP}");
            ReadyToSend = true;
            Task.Run(Poll);
            new Thread(() => Task.Run(Listen)).Start();
        }

        public async void Disconnect()
        {
            Stream.Close();
            Listening = false;
            Program.sockets.Remove(this);

            if(Program.activeUsers.Find(element => element == UserData) != null)
                Program.activeUsers.Remove(UserData);

            if (UserData != null && UserData.Location != null)
            {
                Map map = Program.maps.Find(element => element.Name == UserData.Location);
                if (map != null)
                    map.Players.Remove(UserData);
            }

            Console.WriteLine($"Socket disconnected: {IP}");
        }

        public async Task Listen()
        {
            while (Listening)
            {
                if (Stream.DataAvailable || LastMessage != "")
                    await Task.Run(GetDataTCP);

                await Task.Delay(10);
            }
            return;
        }

        public async Task GetDataTCP()
        {
            string message = LastMessage;
            while (true)
            {
                if (message.Contains(";") && message != "")
                {
                    int charidx = message.IndexOf(';');
                    LastMessage = message.Substring(charidx + 1);
                    message = message.Substring(0, charidx);
                    Console.WriteLine($"Data Recieved: {message} from {IP} via TCP");
                    TCPRouter.Route(this, message);
                    break;
                }

                Byte[] bytes = new Byte[512];
                int idx = -1;
                try
                {
                    idx = Stream.Read(bytes, 0, bytes.Length);
                } catch(Exception e)
                {
                    Console.WriteLine($"Error on TCP socket {IP} - {e}");
                    Disconnect();
                    break;
                }
                string msg = Encoding.UTF8.GetString(bytes, 0, idx);
                message += msg;

                if (message == "")
                    break;

                await Task.Delay(10);
            }
        }

        public async void SendData(string msg, bool log = false)
        {
            while (!ReadyToSend)
                await Task.Delay(10);

            ReadyToSend = false;
            Byte[] bytes = Encoding.UTF8.GetBytes(msg + ";");

            try
            {
                if (msg == string.Empty)
                    return;

                Stream.Write(bytes, 0, bytes.Length);
                ReadyToSend = true;

                if(log)
                    Console.WriteLine($"Data Sent: {msg} to {IP} via TCP");
            }
            catch(Exception e)
            {
                Console.WriteLine($"Error on TCP socket {IP} - {e}");
                Disconnect();
            }
        }

        public void SendDataUDP(string msg, bool log = false)
        {
            Byte[] buffer = Encoding.UTF8.GetBytes(msg);

            try
            {
                if (msg == string.Empty)
                    return;

                Program.udp.Send(buffer, buffer.Length);
                if(log)
                    Console.WriteLine($"Data Sent: {msg} to {IP}:{Port} via UDP");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error on UDP socket {IP}:{Port} - {e}");
            }
        }

        async Task Poll()
        {
            while(true)
            {
                Ping = false;
                SendData("poll?", true);
                await Task.Delay(5000);
                SendData("poll?", true);
                await Task.Delay(5000);

                if (!Listening)
                    break;
                
                if (!Ping)
                {
                    Disconnect();
                    break;
                }
            }
        }
    }
}
