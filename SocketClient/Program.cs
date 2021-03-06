﻿using System;
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

        private static void StartClient()
        {
            try
            {
                var hostName = Dns.GetHostEntry(Dns.GetHostName());
                var ipAddress = hostName.AddressList[0];
                var ipAddressAndPort = new IPEndPoint(ipAddress, Port);

                var clientSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                clientSocket.BeginConnect(ipAddressAndPort, ConnectCallback, clientSocket);
                ConnectReset.WaitOne();

                var iteration = 0;
                while (true)
                {
                    SendMessage(clientSocket, $"Hello! This is packet {iteration}");
                    SendReset.WaitOne();
                    iteration++;
                    Thread.Sleep(5000);
                }
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
                var clientSocket = (Socket) asyncResult.AsyncState;

                clientSocket.EndConnect(asyncResult);
                Console.WriteLine("Socket connected to {0}", clientSocket.RemoteEndPoint);

                ConnectReset.Set();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
            }
        }

        private static void SendMessage(Socket client, string message)
        {
            var byteData = Encoding.ASCII.GetBytes(message);

            Console.WriteLine($"Sent message : {message}");
            client.BeginSend(byteData, 0, byteData.Length, 0, SendCallback, client);
        }


        private static void SendCallback(IAsyncResult asyncResult)
        {
            try
            {
                var client = (Socket) asyncResult.AsyncState;
                var messageByteSize = client.EndSend(asyncResult);
                Console.WriteLine($"Size : {messageByteSize} bytes \n");

                SendReset.Set();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
            }
        }

        public static int Main()
        {
            StartClient();
            return 0;
        }
    }
}