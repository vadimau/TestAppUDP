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
        //Таймер для отсчета времени работы
        private static Timer timerPeriod;
        //Таймер для отсчета времени паузы в работе
        private static Timer timerLenght;
        //Таймер для определение неполадок в сети
        private static Timer workTimeout;

        private static int pausePeriod;
        private static int pauseLenght;
        private static IPAddress ipAdr;
        private static Socket s;
        private static IPEndPoint ipep;
        private static UdpData oldUdpData=new UdpData();
        private static UdpData udpData;
        private static string ip;
        private static Task receiving;
        private static bool taskEnabled;
        private static UdpClient udpClient;


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
            workTimeout = new Timer();
            timerPeriod.Interval = pausePeriod;
            timerLenght.Interval = pauseLenght;
            workTimeout.Interval = 6000;
            timerPeriod.Elapsed += TimerPeriod_Elapsed;
            timerLenght.Elapsed += TimerLenght_Elapsed;
            workTimeout.Elapsed += WorkTimeout_Elapsed;
            CreateSok();
            receiving = new Task(Receive);
            receiving.Start();
            timerPeriod.Start();


            Console.ReadKey();
            s.Close();
        }

        private static void WorkTimeout_Elapsed(object sender, ElapsedEventArgs e)
        {
            if(!s.Connected)
            {
                Console.WriteLine("Что-то с сетью");
                CreateSok();
            }
                
        }

        private static void TimerLenght_Elapsed(object sender, ElapsedEventArgs e)
        {
            taskEnabled = true;
            receiving = new Task(Receive);
            timerLenght.Stop();
            timerPeriod.Start();
            if (receiving.Status!=TaskStatus.Running)
                receiving.Start();
        }

        private static void TimerPeriod_Elapsed(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine("TimerPeriod_Elapsed");
            taskEnabled = false;
            receiving.Dispose();
            if (receiving.IsCompleted)
            {
                Console.WriteLine(DateTime.Now.ToString() + " task paused");
                timerPeriod.Stop();
                timerLenght.Start();
            }
        }


        private static void CreateSok()
        {
            //s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //ipAdr = IPAddress.Parse(ip);
            //ipep = new IPEndPoint(IPAddress.Any, 4567);
            //s.Bind(ipep);

            //s.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(ipAdr, IPAddress.Any));
            udpClient = new UdpClient(8088);
            udpClient.JoinMulticastGroup(IPAddress.Parse("224.100.0.1"), 50);

        }

        private static UdpData Deserialyse(byte[] serializedAsBytes)
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            stream.Write(serializedAsBytes, 0, serializedAsBytes.Length);
            stream.Seek(0, SeekOrigin.Begin);
            return (UdpData)formatter.Deserialize(stream);
        }

        public void Start()
        {
            udpClient = new UdpClient(8088);
            udpClient.JoinMulticastGroup(IPAddress.Parse("224.100.0.1"), 50);
        }



        private static void Receive()
        {
            Console.WriteLine(DateTime.Now.ToString() + " task started");
            do
            {
                Console.WriteLine("receiving...");
                byte[] b = new byte[10240];
                var ipEndPoint = new IPEndPoint(IPAddress.Any, 0);
                //s.Receive(b);
                b= udpClient.Receive(ref ipEndPoint);
                //перезапуск таймера для определения неполадок в сети
                workTimeout.Stop();
                workTimeout.Start();
                udpData = Deserialyse(b);
                if(oldUdpData.HasValue&& oldUdpData.count< udpData.count)
                {
                    if(udpData.dateTime.Subtract(oldUdpData.dateTime).TotalMinutes<1)
                    {
                        udpData.lostPackages = udpData.count - oldUdpData.count - 1;
                    }
                
                    
                    Console.WriteLine(udpData.ToString());
                    using (ApplicationContext db = new ApplicationContext())
                    {
                        db.udpData.Add(udpData);
                        db.SaveChanges();
                    }

                }
                oldUdpData = udpData;

            } while (taskEnabled);


        }
    }
}
