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


namespace Brewmasters
{
    public class Program
    {
        private const string WebsiteFilePath = @"\SD\";

        public static void Main()
        {
            
            Microsoft.SPOT.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()[0].EnableDhcp();
            //Microsoft.SPOT.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()[0].EnableStaticIP("192.168.2.75", "255.255.255.0", "192.168.2.1");
            //DS18B20 t1 = new DS18B20(Pins.GPIO_PIN_D0);
            //DS18B20 t2 = new DS18B20(Pins.GPIO_PIN_D1);
            //OutputPort HeatingElement = new OutputPort(Pins.GPIO_PIN_D2, true);
            //OutputPort Pump = new OutputPort(Pins.GPIO_PIN_D3, true);
            WebServer server = new WebServer();
            
            server.ListenForRequest();


           

            //float temp1 = t1.ConvertAndReadTemperature();
            //float temp2 = t2.ConvertAndReadTemperature();
            //temp = temp / 5 * 9 + 32;
            //Debug.Print("Temp1 sensor: " + temp1.ToString() + "\n" + "Temp2 sensor: " + temp2.ToString());
            //Thread.Sleep(1000);
              


        }

    }
}
