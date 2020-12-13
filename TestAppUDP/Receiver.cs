using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Timers;

namespace TestAppUDP
{
    class Receiver
    {
        //Таймер для отсчета времени работы
        private Timer timerPeriod;
        //Таймер для отсчета времени паузы в работе
        private Timer timerLenght;
        //Таймер для определение неполадок в сети
        private Timer workTimeout;

        private int pausePeriod;
        private int pauseLenght;
        private IPAddress ipAdr;
        private Socket s;
        private IPEndPoint ipep;
        private UdpData oldUdpData = new UdpData();
        private UdpData udpData;
        private string ip;
        private Task receiving;
        private bool taskEnabled;
        private bool firstStart = true;
        public Receiver()
        {
            taskEnabled = true;
            Settings settings = new Settings();
            if (settings.parameters.ContainsKey("ip"))
                ip = settings.parameters["ip"].ToString();
            if (settings.parameters.ContainsKey("pausePeriod"))
                pausePeriod = int.Parse(settings.parameters["pausePeriod"].ToString());
            if (settings.parameters.ContainsKey("pauseLenght"))
                pauseLenght = int.Parse(settings.parameters["pauseLenght"].ToString());
            timerPeriod = new Timer();
            timerLenght = new Timer();
            workTimeout = new Timer();
            timerPeriod.Interval = pausePeriod;
            timerLenght.Interval = pauseLenght;
            workTimeout.Interval = 60000;
            timerPeriod.Elapsed += TimerPeriod_Elapsed;
            timerLenght.Elapsed += TimerLenght_Elapsed;
            workTimeout.Elapsed += WorkTimeout_Elapsed;
            CreateSok();
            receiving = new Task(Receive);
            receiving.Start();
            timerPeriod.Start();

            while (true)
            {
                var s = Console.ReadLine();
                GetStat();
                if (s == "exit") break;
            }

            s.Close();
        }

        private void WorkTimeout_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!s.Connected)
            {
                Console.WriteLine("Что-то с сетью");
                CreateSok();
            }

        }
        /// <summary>
        /// Во время работы этого таймера сообщения принимаются
        /// как только таймер отработал, он выключается и начинает работать таймер,
        /// во время которого сообщения не принимаются
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerLenght_Elapsed(object sender, ElapsedEventArgs e)
        {
            taskEnabled = true;
            receiving = new Task(Receive);
            timerLenght.Stop();
            timerPeriod.Start();
            if (receiving.Status != TaskStatus.Running)
                receiving.Start();
        }
        /// <summary>
        /// Во время раоты этого тайера сообщения не принимаются
        /// как только таймер отработал, он выключается и начинает работать таймер,
        /// во время которого сообщения принимаются
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerPeriod_Elapsed(object sender, ElapsedEventArgs e)
        {
            //Console.WriteLine("TimerPeriod_Elapsed");
            taskEnabled = false;
            // receiving.Dispose();
            if (receiving.IsCompleted)
            {
                //Console.WriteLine(DateTime.Now.ToString() + " task paused");
                timerPeriod.Stop();
                timerLenght.Start();
            }
        }
        /// <summary>
        /// Производим подсчет статистики
        /// </summary>
        public void GetStat()
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                var AVGvalue1 = db.udpData.Average(x => x.value1).ToString();
                var AVGvalue2 = db.udpData.Average(x => x.value1).ToString();
                var AVGvalue3 = db.udpData.Average(x => x.value1).ToString();
                var AVGvalue4 = db.udpData.Average(x => x.value1).ToString();
                var AVGvalue5 = db.udpData.Average(x => x.value1).ToString();

                var stdDev1 = db.udpData.Select(x => x.value1).StdDev();
                var stdDev2 = db.udpData.Select(x => x.value2).StdDev();
                var stdDev3 = db.udpData.Select(x => x.value3).StdDev();
                var stdDev4 = db.udpData.Select(x => x.value4).StdDev();
                var stdDev5 = db.udpData.Select(x => x.value5).StdDev();

                var Mode1 = db.udpData.Select(x => x.value1).Mode();
                var Mode2 = db.udpData.Select(x => x.value2).Mode();
                var Mode3 = db.udpData.Select(x => x.value3).Mode();
                var Mode4 = db.udpData.Select(x => x.value4).Mode();
                var Mode5 = db.udpData.Select(x => x.value5).Mode();

                var Mediane1 = db.udpData.Select(x => x.value1).Median();
                var Mediane2 = db.udpData.Select(x => x.value2).Median();
                var Mediane3 = db.udpData.Select(x => x.value3).Median();
                var Mediane4 = db.udpData.Select(x => x.value4).Median();
                var Mediane5 = db.udpData.Select(x => x.value5).Median();

                var received = db.udpData.Where(x => x.lostPackages == 0).Count();
                var notReceived = db.udpData.Where(x => x.lostPackages != 0).Count();


                Console.WriteLine("\r\nСредние значения");
                Console.WriteLine(AVGvalue1 + " | " + AVGvalue2 + " | " + AVGvalue3 + " | " + AVGvalue4 + " | " + AVGvalue5);
                Console.WriteLine("Средние отклонения");
                Console.WriteLine(stdDev1 + " | " + stdDev2 + " | " + stdDev3 + " | " + stdDev4 + " | " + stdDev5);
                Console.WriteLine("Мода (что бы это ни значило)");
                Console.WriteLine(Mode1 + " | " + Mode2 + " | " + Mode3 + " | " + Mode4 + " | " + Mode5);
                Console.WriteLine("Медиана");
                Console.WriteLine(Mediane1 + " | " + Mediane2 + " | " + Mediane3 + " | " + Mediane4 + " | " + Mediane5);
                Console.WriteLine("Количество принятх пакетов / кол-во пропущенных пакетов / %");
                Console.WriteLine(received + " | " + notReceived + " | " + 100 * notReceived / received);



            }
        }


        private void CreateSok()
        {
            s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            ipAdr = IPAddress.Parse(ip);
            ipep = new IPEndPoint(IPAddress.Any, 4567);
            s.Bind(ipep);

            s.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(ipAdr, IPAddress.Any));

        }

        private UdpData Deserialyse(byte[] serializedAsBytes)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                stream.Write(serializedAsBytes, 0, serializedAsBytes.Length);
                stream.Seek(0, SeekOrigin.Begin);
                return (UdpData)formatter.Deserialize(stream);
            }
        }

        private void Receive()
        {
            //Console.WriteLine(DateTime.Now.ToString() + " task started");
            do
            {
                Console.Write("\r " + "receiving...");
                byte[] b = new byte[10240];
                s.Receive(b);
                //перезапуск таймера для определения неполадок в сети
                workTimeout.Stop();
                workTimeout.Start();
                udpData = Deserialyse(b);
                if (oldUdpData.HasValue && oldUdpData.count < udpData.count)
                {
                    if (udpData.dateTime.Subtract(oldUdpData.dateTime).TotalMinutes < 1)
                    {
                        udpData.lostPackages = udpData.count - oldUdpData.count - 1;
                    }


                    Console.Write("\r " + udpData.ToString());
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
