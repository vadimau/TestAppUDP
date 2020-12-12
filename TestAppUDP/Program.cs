using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Timers;

namespace TestAppUDP
{
    class Program
    {
        private static Timer timerPeriod;
        private static Timer timerLenght;
        private static int pausePeriod;
        private static int pauseLenght;
        private static IPAddress ipAdr;
        private static Socket s;
        private static IPEndPoint ipep;
        private static udpData oldUdpData;
        private static udpData udpData;
        private static string ip;
        private static Task receiving;
        private static bool taskEnabled;

        static void Main(string[] args)
        {
            taskEnabled = true;
            Settings settings = new Settings();
            if(settings.parameters.ContainsKey("ip"))
                ip = settings.parameters["ip"].ToString();
            if (settings.parameters.ContainsKey("pausePeriod"))
                pausePeriod=int.Parse(settings.parameters["pausePeriod"].ToString());
            if (settings.parameters.ContainsKey("pauseLenght"))
                pauseLenght = int.Parse(settings.parameters["pauseLenght"].ToString());
            timerPeriod = new Timer();
            timerLenght = new Timer();
            timerPeriod.Interval = pausePeriod;
            timerLenght.Interval = pauseLenght;
            timerPeriod.Elapsed += TimerPeriod_Elapsed;
            timerLenght.Elapsed += TimerLenght_Elapsed;
            CreateSok();
            receiving = new Task(Receive);
            receiving.Start();
            timerPeriod.Start();


            Console.ReadKey();
            s.Close();
        }

        private static void TimerLenght_Elapsed(object sender, ElapsedEventArgs e)
        {
            taskEnabled = true;
            receiving.Start();
            timerLenght.Stop();
            timerPeriod.Start();
            
        }

        private static void TimerPeriod_Elapsed(object sender, ElapsedEventArgs e)
        {
            taskEnabled = false;
            receiving.Wait();
            timerPeriod.Stop();
            timerLenght.Start();
        }

        private static void CreateSok()
        {
            s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            ipAdr = IPAddress.Parse(ip);
            ipep = new IPEndPoint(IPAddress.Any, 4567);
            s.Bind(ipep);

            s.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(ipAdr, IPAddress.Any));
           
        }

        private static udpData Deserialyse(byte[] serializedAsBytes)
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            stream.Write(serializedAsBytes, 0, serializedAsBytes.Length);
            stream.Seek(0, SeekOrigin.Begin);
            return (udpData)formatter.Deserialize(stream);
        }

        private static void Receive()
        {
            do
            {
                Console.WriteLine("receiving...");
                byte[] b = new byte[10240];
                s.Receive(b);
                udpData = Deserialyse(b);
                Console.WriteLine(udpData.ToString());
                using (ApplicationContext db = new ApplicationContext())
                {
                    db.udpData.Add(new udpData());
                    udpData udpData = new udpData();
                    db.SaveChanges();
                }

            } while (true);//taskEnabled


        }
    }
}
