using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using ThreelnDotOrg.NETMF.Hardware;
using System.IO;
using SecretLabs.NETMF.IO;
using EmbeddedWebserver.Core;
using EmbeddedWebserver.Core.Configuration;
using EmbeddedWebserver.Core.Helpers;

namespace Brewmasters
{
    public class Program
    {
        public static DS18B20 t1;
        private const string WebsiteFilePath = @"\SD\";
        int step = 1;



        public static double getTemp(DS18B20 tempsensor) {
            double temp = tempsensor.ConvertAndReadTemperature();
            Debug.Print("getting temp value from sensor: " + temp.ToString());
            return temp;

        }

        public static double getTemp1()
        {
            double temp = t1.ConvertAndReadTemperature();
            Debug.Print("PID GETTING TEMP SENSOR VALUE: " + temp.ToString() + "\n");
            return temp;
        }

        public static double getTempSetPoint()
        {
            double setpoint = 30.00;
            Debug.Print("getting temperature set point: " + setpoint.ToString());
            return setpoint;
        }

        public static void pidOutput(double value)
        {
            Debug.Print("PID OUTPUT VALUE: " + value.ToString() + "\n");
        }

        public static void Main()
        {
            
            //Microsoft.SPOT.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()[0].EnableDhcp();
            //Microsoft.SPOT.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()[0].EnableStaticIP("192.168.2.75", "255.255.255.0", "192.168.2.1");
            //t1 = new DS18B20(Pins.GPIO_PIN_D0);
            //DS18B20 t2 = new DS18B20(Pins.GPIO_PIN_D1);
            //PID pid = new PID(100.00, 50.00, 0.00, 110.00, 10.00, 100, 0, getTemp1, getTempSetPoint, pidOutput);
            //pid.Enable();
            //Microsoft.SPOT.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()[0].EnableStaticIP("192.168.1.100", "255.255.255.0", "192.168.0.1");
            Microsoft.SPOT.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()[0].EnableDhcp();
            EmbeddedWebapplicationConfiguration config = new EmbeddedWebapplicationConfiguration(@"SD\webroot");
            config.ReadConfiguration();

            using (Webserver server = new Webserver(config))
            {
                server.RegisterHandler("hello", typeof(TestHandler));
                server.RegisterHandler("random", typeof(RandomHandler));
                server.RegisterThreadSafeHandler("rnd", new RandomHandler());
                server.RegisterThreadSafeHandler("post", new PostHandler());
                server.StartListening();
                Thread.Sleep(System.Threading.Timeout.Infinite);
            }

            //OutputPort HeatingElement = new OutputPort(Pins.GPIO_PIN_D2, true);
            //OutputPort Pump = new OutputPort(Pins.GPIO_PIN_D3, true);
            //WebServer server = new WebServer();
            
            //server.ListenForRequest();

            //while (server.getCurrentRecipe() != null)
            //{

            //}
           

            //float temp1 = t1.ConvertAndReadTemperature();
            //float temp2 = t2.ConvertAndReadTemperature();
            //temp = temp / 5 * 9 + 32;
            //Debug.Print("Temp1 sensor: " + temp1.ToString() + "\n" + "Temp2 sensor: " + temp2.ToString());
            //Thread.Sleep(1000);
              


        }

    }
}
