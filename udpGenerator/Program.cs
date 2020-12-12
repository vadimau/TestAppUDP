using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Timers;
using TestAppUDP;

namespace udpGenerator
{
    
    class Program
    {
        private static Timer genImpulse = new Timer();
        private static string ip;
        private static long count=0;
        private static IPAddress ipAdr;
        private static Socket s;
        private static IPEndPoint ipep;

        static void Main(string[] args)
        {
            int genPeriod = 0;

            Settings settings = new Settings();

            if (settings.parameters.ContainsKey("genPeriod"))
                genPeriod = int.Parse(settings.parameters["genPeriod"].ToString());
            if (settings.parameters.ContainsKey("ip"))
                ip = settings.parameters["ip"].ToString();

            CreateSok();

            genImpulse.Interval = genPeriod;
            genImpulse.Elapsed += GenImpulse_Elapsed;
            genImpulse.Start();

            Console.ReadKey();
            s.Close();
        }

        private static byte[] Serialyse(udpData udpData)
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
        private static void GenImpulse_Elapsed(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine(count);
            count = count==long.MaxValue ? 0 : count+1;
            var randomInt = new Random(int.MaxValue);
            var value1 = randomInt.Next();
            var value2 = randomInt.Next();
            var value3 = randomInt.Next();
            var value4 = randomInt.Next();
            var value5 = randomInt.Next();
            var udpData = new udpData(count, value1, value2, value3, value4, value5);
            var bytes = Serialyse(udpData);


            
            byte[] b = Serialyse(udpData);
            s.Send(b, b.Length, SocketFlags.None);
            
        }
    }
}
