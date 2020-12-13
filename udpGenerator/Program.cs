using System;
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
        static void Main(string[] args)
        {
            Sender sender = new Sender();
        }
    }
}
