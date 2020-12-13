﻿using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using TestAppUDP;
using Timer = System.Timers.Timer;

namespace udpGenerator
{
    
    class Program
    {
        //периодичность отправки сгенерированных данных
        private static Timer genImpulse = new Timer();
        private static Timer workTimeout = new Timer();
        private static string ip;
        private static long count=0;
        private static IPAddress ipAdr;
        private static Socket s;
        private static IPEndPoint ipep;
        private static Task generate;
        private static Random random;
        private static UdpData udpData;

        static void Main(string[] args)
        {
            int genPeriod = 0;

            Settings settings = new Settings();

            if (settings.parameters.ContainsKey("genPeriod"))
                genPeriod = int.Parse(settings.parameters["genPeriod"].ToString());
            if (settings.parameters.ContainsKey("ip"))
                ip = settings.parameters["ip"].ToString();

            random = new Random();

            CreateSok();

            genImpulse.Interval = genPeriod;
            genImpulse.Elapsed += GenImpulse_Elapsed;
            genImpulse.Start();

            workTimeout.Interval = 6000;
            workTimeout.Elapsed += WorkTimeout_Elapsed;
            workTimeout.Start();

            Console.ReadKey();
            s.Close();
        }

        private static void WorkTimeout_Elapsed(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine("Что-то с сетью");
            generate.Dispose();
            CreateSok();
        }

        public static void SendMessage(byte[] data)
        {
            using (var udpClient = new UdpClient(AddressFamily.InterNetwork))
            {
                var address = IPAddress.Parse("224.100.0.1");
                var ipEndPoint = new IPEndPoint(address, 8088);
                udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, 8088));
                udpClient.JoinMulticastGroup(8088, address);
                udpClient.MulticastLoopback = true;
                udpClient.Send(data, data.Length, ipEndPoint);
                udpClient.Close();
            }
        }

        private static byte[] Serialyse(UdpData udpData)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            formatter.Serialize(stream, udpData);
            return stream.ToArray();
        }

        private static void CreateSok()
        {
            s = new Socket(AddressFamily.InterNetwork,
    SocketType.Dgram, ProtocolType.Udp);

            ipAdr = IPAddress.Parse(ip);
            s.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(ipAdr));
            s.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 2);
            ipep = new IPEndPoint(ipAdr, 4567);
            s.Connect(ipep);
        }

        private static void GenerateData()
        {
            int value1, value2, value3, value4, value5;
            
            count = count == long.MaxValue ? 0 : count + 1;
            value1 = random.Next(0,int.MaxValue);
            value2 = random.Next(0, int.MaxValue);
            value3 = random.Next(0, int.MaxValue);
            value4 = random.Next(0, int.MaxValue);
            value5 = random.Next(0, int.MaxValue);
            udpData = new UdpData(count, value1, value2, value3, value4, value5);
            Console.WriteLine(udpData);
            var bytes = Serialyse(udpData);



            byte[] b = Serialyse(udpData);
            SendMessage(b);
            //s.Send(b, b.Length, SocketFlags.None);
            workTimeout.Stop();
            workTimeout.Start();
        }
        private static void GenImpulse_Elapsed(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine("генерируем и отправляем данные");
            generate?.Wait();
            generate = new Task(GenerateData);
            generate.Start();
        }
    }
}
