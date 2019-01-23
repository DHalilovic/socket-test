﻿using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace AsyncTcp_01
{
    class Program
    {
        static IPAddress ipa;
        static IPEndPoint ipe;
        static bool server;
        static Socket serverSocket, clientSocket;
        static BufferedStream stream; // Wraps socket for data retrieval
        static readonly byte[] readBuffer = new byte[5000];

        static void Main(string[] args)
        {
            ipa = IPAddress.Parse("127.0.0.1");
            ipe = new IPEndPoint(ipa, 8080);

            if (Console.ReadKey().Key.Equals(ConsoleKey.F))
                server = true;

            if (server)
            {
                serverSocket = new Socket(ipa.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.Bind(ipe);
                serverSocket.Listen(100);
                serverSocket.BeginAccept(OnEndAccept, null);
            }
            else
            {
                clientSocket = new Socket(ipa.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                clientSocket.BeginConnect(ipe, OnEndConnect, null);
            }

            short m = 2;

            while (true)
            {
                if (Console.ReadKey().Key.Equals(ConsoleKey.F))
                {
                    Send(m);
                }
                else
                    break;
            }

            if (serverSocket != null)
                serverSocket.Dispose();
            if (clientSocket != null)
                clientSocket.Dispose();
        }

        static void OnEndAccept(IAsyncResult ar)
        {
            clientSocket = serverSocket.EndAccept(ar);
            clientSocket.NoDelay = true; // Improve performance

            stream = new BufferedStream(new NetworkStream(clientSocket));
            stream.BeginRead(readBuffer, 0, readBuffer.Length, OnRead, null);

            Console.WriteLine("Accepted client");
        }

        static void OnEndConnect(IAsyncResult ar)
        {
            clientSocket.EndConnect(ar);
            clientSocket.NoDelay = true; // Improve performance

            stream = new BufferedStream(new NetworkStream(clientSocket));
            stream.BeginRead(readBuffer, 0, readBuffer.Length, OnRead, null);

            Console.WriteLine("Connected to server");
        }

        static void OnRead(IAsyncResult ar)
        {
            Console.WriteLine("Received");
            int length = stream.EndRead(ar);

            if (length <= 0)
            {
                OnDisconnect();
            }
            else
            {
                Console.WriteLine(BitConverter.ToInt16(readBuffer, 0));
            }

            stream.BeginRead(readBuffer, 0, readBuffer.Length, OnRead, null);
        }

        static void OnDisconnect()
        {
            stream.Dispose();
            clientSocket = null;
        }


        static void Send(short message)
        {
            clientSocket.BeginSend(BitConverter.GetBytes(message), 0, 2, 0, null, null);
            Console.WriteLine("Sent");
        }
    }
}
