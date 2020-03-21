using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;


namespace SocketClient
{
    public static class Client
    {
        private const int Port = 11000;

        private static readonly ManualResetEvent ConnectReset = new ManualResetEvent(false);
        private static readonly ManualResetEvent SendReset = new ManualResetEvent(false);

        private static void Start()
        {
            try
            {
                IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint remoteEp = new IPEndPoint(ipAddress, Port);

                Socket client = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                client.BeginConnect(remoteEp, ConnectCallback, client);
                ConnectReset.WaitOne();

                var iteration = 0;
                while (true)
                {
                    Send(client, $"Hello! This is packet {iteration}");
                    SendReset.WaitOne();
                    iteration++;
                    Thread.Sleep(5000);
                }

                //client.Shutdown(SocketShutdown.Both);
                //client.Close();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
            }
        }

        private static void ConnectCallback(IAsyncResult asyncResult)
        {
            try
            {
                Socket client = (Socket) asyncResult.AsyncState;

                client.EndConnect(asyncResult);
                Console.WriteLine("Socket connected to {0}", client.RemoteEndPoint);

                ConnectReset.Set();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
            }
        }

        private static void Send(Socket client, string message)
        {
            byte[] byteData = Encoding.ASCII.GetBytes(message);

            Console.WriteLine($"Sent message : {message}");
            client.BeginSend(byteData, 0, byteData.Length, 0, SendCallback, client);
        }


        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket) ar.AsyncState;

                int bytesSent = client.EndSend(ar);
                Console.WriteLine($"Size : {bytesSent} bytes \n");

                SendReset.Set();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
            }
        }

        public static int Main()
        {
            Start();
            return 0;
        }
    }
}