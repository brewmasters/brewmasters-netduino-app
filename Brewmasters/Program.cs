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
using System.Text;





namespace Brewmasters
{
    public class Program
    {
       
        public static ProcessStep step = ProcessStep.Idle;
        public static bool isBrewing = false;
       
        public static bool NextStep = false;
        public static bool isWaiting = false;
        public static TimeSpan timeLeft = TimeSpan.Zero;
        public static double boilTemp = 0;
        public static double mashTemp = 0;
        public static int ConfirmationNumber = 0;

        private static bool boilFloat = false;
        private static bool mashFloat = false;

        private static Recipe currentRecipe = null;
        private static bool isHeating = false;
        private static bool isPumping = false;
        private static bool isWaitingToHeat = false;
        private static float targetTemp = 0;
        private static int spargeCounter = 0;

        private static DateTime startTime = DateTime.Now;
        public static ResponseObject ResponseObj = new ResponseObject();

        public static DS18B20 MashTempSensor = new DS18B20(Pins.GPIO_PIN_D2);
        public static DS18B20 BoilingTempSensor = new DS18B20(Pins.GPIO_PIN_D3);

        public static OutputPort HeatingElement = new OutputPort(Pins.GPIO_PIN_D7, false);
        public static OutputPort Pump = new OutputPort(Pins.GPIO_PIN_D8, false);
        public static InputPort MashFloatSensor = new InputPort(Pins.GPIO_PIN_D4, false, Port.ResistorMode.Disabled);
        public static InputPort BoilFloatSensor = new InputPort(Pins.GPIO_PIN_D5, false, Port.ResistorMode.Disabled);
        public static OutputPort Fan = new OutputPort(Pins.GPIO_PIN_D6, false);

        public static void Main()
        {
            Ingredient[] ingredients = new Ingredient[1]{new Ingredient()};
            Recipe reciple = new Recipe(1, 1.2f, 50, 5, 5, ingredients);
            currentRecipe = reciple;
            NextStep = false;
            Microsoft.SPOT.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()[0].EnableDhcp();
            //Microsoft.SPOT.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()[0].EnableStaticIP("192.168.2.75", "255.255.255.0", "192.168.2.1");
            
            WebServer server = new WebServer();
            while (true)
            {
                if (mashTemp > 80 || boilTemp > 80)
                {
                    isWaiting = true;
                }
                else
                {
                    isWaiting = false;
                }
                //Fan.Write(false);
                if (step.Equals(ProcessStep.Mashing))
                {
                    timeLeft = startTime.AddMinutes(currentRecipe.mash_duration).Subtract(DateTime.Now);
                }
                else if(step.Equals(ProcessStep.Boiling))
                {
                    timeLeft = startTime.AddMinutes(currentRecipe.boil_duration).Subtract(DateTime.Now);
                }
                else{
                    timeLeft = TimeSpan.Zero;
                
                }
                mashFloat = MashFloatSensor.Read();
                boilFloat = BoilFloatSensor.Read();
                mashTemp = DS18B20.ToFahrenheit(MashTempSensor.ConvertAndReadTemperature());
                boilTemp = DS18B20.ToFahrenheit(BoilingTempSensor.ConvertAndReadTemperature());
                //Debug.Print(mashTemp.ToString());
                //Debug.Print(boilTemp.ToString());
                if (isHeating)
                {
                    if (step.Equals(ProcessStep.PreMash) || step.Equals(ProcessStep.Mashing) || step.Equals(ProcessStep.Boiling))
                    {
                        if (boilTemp < targetTemp)
                        {
                            HeatingElement.Write(true);
                            Fan.Write(true);
                        }
                        else
                        {
                            HeatingElement.Write(false);
                            Fan.Write(false);
                            isWaitingToHeat = false;
                        }
                    }
                }
                if (isPumping)
                {
                    if (step.Equals(ProcessStep.Sparging))
                    {
                        //IMPLEMENT FLOAT SENSOR HERE
                        if (spargeCounter != 0)
                        {
                            if (startTime.AddMinutes(2).CompareTo(DateTime.Now) < 0)
                            {
                                if (mashFloat && !boilFloat)
                                {
                                    Pump.Write(true);
                                }
                                else
                                {
                                    Pump.Write(false);
                                    startTime = DateTime.Now;
                                    spargeCounter++;
                                }

                            }
                        }
                        else
                        {
                            if (mashFloat && !boilFloat)
                            {
                                Pump.Write(true);
                            }
                            else
                            {
                                Pump.Write(false);
                                startTime = DateTime.Now;
                                spargeCounter++;
                            }
                        }
                    }
                    else if (step.Equals(ProcessStep.PreMash))
                    {
                        if (mashFloat && !boilFloat)
                        {
                            Pump.Write(true);
                        }
                        else
                        {
                            Pump.Write(false);
                            isPumping = false;
                        }

                    }
                }
                if (NextStep)
                {
                    if(step.Equals(ProcessStep.Idle)){
                        NextStep = false;
                        isBrewing = true;
                        currentRecipe = server.getCurrentRecipe(); 
                        Thread.Sleep(10000);
                        RunPreMashing();
                    }
                    if (step.Equals(ProcessStep.PreMash))
                    {
                        NextStep = false;
                        Thread.Sleep(10000);
                        RunMashing();
                    }
                    if (step.Equals(ProcessStep.Mashing))
                    {
                        NextStep = false;
                        Thread.Sleep(10000);
                        RunSparging();
                    }
                    if (step.Equals(ProcessStep.Sparging))
                    {
                        NextStep = false;
                        Thread.Sleep(10000);
                        RunBoiling();
                    }
                    if (step.Equals(ProcessStep.Boiling))
                    {
                        NextStep = false;
                        Thread.Sleep(10000);
                        RunReset();
                        
                        
                    }

                }

            }
           // server.ListenForRequest();

            
           

            //float temp1 = t1.ConvertAndReadTemperature();
            //float temp2 = t2.ConvertAndReadTemperature();
            //temp = temp / 5 * 9 + 32;
            //Debug.Print("Temp1 sensor: " + temp1.ToString() + "\n" + "Temp2 sensor: " + temp2.ToString());
            //Thread.Sleep(1000);
              


        }

        private static void RunPreMashing()
        {
            step = ProcessStep.PreMash;
            //isWaiting = true;
            ConfirmationNumber++;
            ResponseObj = new ResponseObject("Please Add Mash Water");
            while (isWaiting)
            {

            }

            ResponseObj = new ResponseObject("");
            targetTemp = currentRecipe.mash_temperature;
            isHeating = true;
            isWaitingToHeat = true;
            while (isWaitingToHeat)
            {

            }
            //isWaiting = true;
            ConfirmationNumber++;
            ResponseObj = new ResponseObject("Please Open Valve");
            while (isWaiting)
            {

            }
            ResponseObj = new ResponseObject("");
            isHeating = false;
            isPumping = true;
            startTime = DateTime.Now;
            while(isPumping)
            {
                
            }
            isPumping = false;
            //isWaiting = true;
            ConfirmationNumber++;
            ResponseObj = new ResponseObject("Please Close Valve and add Grain then Mix mash and put on Lid");
            while (isWaiting)
            {
                
            }
            ResponseObj = new ResponseObject("");
            NextStep = true;
        }
        private static void RunMashing()
        {
            step = ProcessStep.Mashing;
            startTime = DateTime.Now;
            //isWaiting = true;
            ConfirmationNumber++;
            ResponseObj = new ResponseObject("Please add Sparge water to boiling kettle");
            while (isWaiting)
            {

            }
            ResponseObj = new ResponseObject("");
            isHeating = true;
            isWaitingToHeat = true;
            while (isWaitingToHeat && startTime.AddMinutes(currentRecipe.mash_duration).CompareTo(DateTime.Now)>0)
            {

            }
             
            NextStep = true;
        }
        private static void RunSparging()
        {
            step = ProcessStep.Sparging;
            //isWaiting = true;
            ConfirmationNumber++;
            ResponseObj = new ResponseObject("Please Open Both Valves");
            while (isWaiting)
            {

            }
            ResponseObj = new ResponseObject("");
            startTime = DateTime.Now;
            isPumping = true;
            while (isPumping)
            {
                if (spargeCounter == 5)
                {
                    isPumping = false;
                }
            }

            
            
            //isWaiting = true;
            ConfirmationNumber++;
            ResponseObj = new ResponseObject("Please Close Both Valves");
            while (isWaiting)
            {

            }
            ResponseObj = new ResponseObject("");
            NextStep = true;
        }
        private static void RunBoiling()
        {
            step = ProcessStep.Boiling;
            targetTemp = 212;
            isWaitingToHeat = true;
            while(isWaitingToHeat)
            {

            }
            //isWaiting = true;
            ConfirmationNumber++;
            ResponseObj = new ResponseObject("Wort Has Reached Boiling Please Add Hops");
            while (isWaiting)
            {

            }
            ResponseObj = new ResponseObject("");
            startTime = DateTime.Now;
            while (startTime.AddMinutes(currentRecipe.boil_duration).CompareTo(DateTime.Now) > 0)
            {

            }
            isHeating = false;
            //isWaiting = true;
            ConfirmationNumber++;
            ResponseObj = new ResponseObject("Boiling has completed Please Add Ice");
            while (isWaiting)
            {

            }
            ResponseObj = new ResponseObject("");
            NextStep = true;
        }
        private static void RunReset()
        {
            step = ProcessStep.Idle;
            currentRecipe = null;
            isHeating = false;
            isPumping = false;
            isWaiting = false;
            isBrewing = false;
            ConfirmationNumber = 0;
            spargeCounter = 0;
        }
        //private static void RunCooling()
        //{
        //    step = ProcessStep.Cooling;
        //    while (true)
        //    {

        //    }
        //    step++;
        //}

    }
}
