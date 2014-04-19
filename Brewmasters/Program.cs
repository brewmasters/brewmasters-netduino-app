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
        public static ProcessStep step = ProcessStep.Idle;
        public static bool isBrewing = false;

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

            while (true)
            {
                if (server.getCurrentRecipe() != null && !isBrewing)
                {
                    isBrewing = true;
                }
                if (isBrewing)
                {
                    if (step.Equals(ProcessStep.Cleaning))
                    {
                        RunCleaning();
                    }
                    else if (step.Equals(ProcessStep.Mashing))
                    {
                        RunMashing();
                    }
                    else if (step.Equals(ProcessStep.Sparging))
                    {
                        RunSparging();
                    }
                    else if (step.Equals(ProcessStep.Boiling))
                    {
                        RunBoiling();
                    }
                    else if (step.Equals(ProcessStep.Cooling))
                    {
                        RunCooling();
                    }
                    else if (step.Equals(ProcessStep.Reset))
                    {
                        server.resetRecipe();
                        isBrewing = false;
                    }
                    else
                    {
                        server.SendErrorResponse();
                    }
                }
            }
           

            //float temp1 = t1.ConvertAndReadTemperature();
            //float temp2 = t2.ConvertAndReadTemperature();
            //temp = temp / 5 * 9 + 32;
            //Debug.Print("Temp1 sensor: " + temp1.ToString() + "\n" + "Temp2 sensor: " + temp2.ToString());
            //Thread.Sleep(1000);
              


        }
        private static void RunCleaning()
        {
            while(true)
            {

            }
            step++;
        }
        private static void RunMashing()
        {
            while (true)
            {

            }
            step++;
        }
        private static void RunSparging()
        {
            while (true)
            {

            }
            step++;
        }
        private static void RunBoiling()
        {
            while (true)
            {

            }
            step++;
        }
        private static void RunCooling()
        {
            while (true)
            {

            }
            step++;
        }

    }
}
