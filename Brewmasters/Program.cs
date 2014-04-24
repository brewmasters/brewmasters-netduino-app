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
        public static bool ingredientAdded = false;

        private static bool boilFloat = false;
        private static bool mashFloat = false;

        public static Recipe currentRecipe = null;
        private static bool isHeating = false;
        private static bool isPumping = false;
        private static bool isWaitingToHeat = false;
        private static DateTime waitTime = DateTime.Now;

        private static float targetTemp = 0;
        public static int spargeCounter = 1;
        public static bool resetFlag = false;
        public static string roMessage = "";

        private static DateTime startTime = DateTime.Now;
        public static ResponseObject ResponseObj = new ResponseObject();

        public static DS18B20 MashTempSensor = new DS18B20(Pins.GPIO_PIN_D2);
        public static DS18B20 BoilingTempSensor = new DS18B20(Pins.GPIO_PIN_D3);

        public static OutputPort HeatingElement = new OutputPort(Pins.GPIO_PIN_D8, false);
        public static OutputPort Pump = new OutputPort(Pins.GPIO_PIN_D6, true);
        public static InputPort MashFloatSensor = new InputPort(Pins.GPIO_PIN_D4, false, Port.ResistorMode.Disabled);
        public static InputPort BoilFloatSensor = new InputPort(Pins.GPIO_PIN_D5, false, Port.ResistorMode.Disabled);
        public static OutputPort Fan = new OutputPort(Pins.GPIO_PIN_D7, true);

        public static void Main()
        {
            //Ingredient[] ingredients = new Ingredient[1]{new Ingredient()};
            //Recipe reciple = new Recipe(1, 1.2f, 85, 5, 5, ingredients);
            //currentRecipe = reciple;
            //NextStep = true;
            //Microsoft.SPOT.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()[0].EnableDhcp();
            Microsoft.SPOT.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()[0].EnableStaticIP("192.168.0.100", "255.255.255.0", "192.168.2.1");
            //Fan.Write(true);
            //Pump.Write(true);
            //HeatingElement.Write(false);
            WebServer server = new WebServer();
            while (true)
            {
                ResponseObj = new ResponseObject();
                if (resetFlag)
                {
                    RunReset();
                }
                else
                {
                    //Fan.Write(false);
                    if (step.Equals(ProcessStep.Mashing))
                    {
                        timeLeft = startTime.AddMinutes(currentRecipe.mash_duration).Subtract(DateTime.Now);
                    }
                    else if (step.Equals(ProcessStep.Boiling))
                    {
                        timeLeft = startTime.AddMinutes(currentRecipe.boil_duration).Subtract(DateTime.Now);
                    }
                    else
                    {
                        timeLeft = TimeSpan.Zero;

                    }
                    mashFloat = MashFloatSensor.Read();
                    boilFloat = BoilFloatSensor.Read();
                    mashTemp = DS18B20.ToFahrenheit(MashTempSensor.ConvertAndReadTemperature());
                    boilTemp = DS18B20.ToFahrenheit(BoilingTempSensor.ConvertAndReadTemperature());
                    Debug.Print("MASHTEMP:" + mashTemp.ToString());
                    Debug.Print("BOILTEMP:" + boilTemp.ToString());
                    Debug.Print("MASHFLOAT:" + mashFloat.ToString());
                    Debug.Print("BOILFLOAT:" + boilFloat.ToString());
                    Debug.Print("HEATING:" + HeatingElement.Read());
                    Debug.Print("FAN:" + Fan.Read());
                    Debug.Print("PUMP:" + Pump.Read() + "\n");
                    if (isHeating && boilFloat)
                    {
                        if (step.Equals(ProcessStep.PreMash) || step.Equals(ProcessStep.Mashing) || step.Equals(ProcessStep.Sparging) || step.Equals(ProcessStep.Boiling))
                        {
                            if (boilTemp < targetTemp)
                            {
                                HeatingElement.Write(true);
                                Fan.Write(false);
                            }
                            else
                            {
                                HeatingElement.Write(false);
                                Fan.Write(true);
                                if (isWaitingToHeat)
                                {
                                    isWaitingToHeat = false;
                                }
                            }
                        }
                    }
                    else if (boilFloat)
                    {
                        HeatingElement.Write(false);
                        Fan.Write(true);
                        
                    }
                    else
                    {
                        HeatingElement.Write(false);
                        Fan.Write(true);
                    }
                    if (isPumping)
                    {
                        
                        if (step.Equals(ProcessStep.Sparging))
                        {

                            if (spargeCounter != 1)
                            {
                                //TEST 30 secs
                                if (startTime.AddMinutes(2).CompareTo(DateTime.Now) < 0)
                                {
                                    isHeating = false;
                                    if (!mashFloat && boilFloat)
                                    {
                                        Pump.Write(false);
                                    }
                                    else
                                    {
                                        Pump.Write(true);
                                        startTime = DateTime.Now;
                                        spargeCounter++;

                                    }

                                }
                                else
                                {
                                    Pump.Write(true);
                                    if (boilFloat && boilTemp < 170)
                                    {
                                        isHeating = true;
                                    }
                                    else
                                    {
                                        isHeating = false;
                                    }
                                }
                            }
                            else
                            {
                                if (!mashFloat && boilFloat)
                                {
                                    Pump.Write(false);
                                }
                                else
                                {
                                    Pump.Write(true);
                                    startTime = DateTime.Now;
                                    spargeCounter++;
                                }
                            }
                        }
                        else if (step.Equals(ProcessStep.PreMash))
                        {
                            if (!mashFloat && boilFloat)
                            {
                                Pump.Write(false);
                            }
                            else
                            {
                                Pump.Write(true);
                                isPumping = false;
                            }

                        }
                    }
                    else
                    {
                        Pump.Write(true);
                            
                    }
                    if (NextStep)
                    {
                        if (NextStep && step.Equals(ProcessStep.Idle))
                        {
                            NextStep = false;

                            //currentRecipe = server.getCurrentRecipe(); 
                            //Thread.Sleep(5000);
                            Thread preMashThread = new Thread(new ThreadStart(RunPreMashing));
                            preMashThread.Start();

                            //RunPreMashing();
                        }
                        if (NextStep && step.Equals(ProcessStep.PreMash))
                        {
                            NextStep = false;
                            //Thread.Sleep(5000);
                            Thread mashThread = new Thread(new ThreadStart(RunMashing));
                            mashThread.Start();
                            //RunMashing();
                        }
                        if (NextStep && step.Equals(ProcessStep.Mashing))
                        {
                            NextStep = false;
                            //Thread.Sleep(5000);
                            Thread spargeThread = new Thread(new ThreadStart(RunSparging));
                            spargeThread.Start();
                            //RunSparging();
                        }
                        if (NextStep && step.Equals(ProcessStep.Sparging))
                        {
                            NextStep = false;
                            //Thread.Sleep(5000);
                            Thread boilThread = new Thread(new ThreadStart(RunBoiling));
                            boilThread.Start();
                            //RunBoiling();
                        }
                        if (NextStep && step.Equals(ProcessStep.Boiling))
                        {
                            NextStep = false;
                            //Thread.Sleep(5000);
                            RunCooling();
                        }
                        if (NextStep && step.Equals(ProcessStep.Cooling))
                        {
                            NextStep = false;
                            //Thread.Sleep(5000);
                            RunReset();
                        }


                    }
                    //Thread.Sleep(700);
                }
                // server.ListenForRequest();




                //float temp1 = t1.ConvertAndReadTemperature();
                //float temp2 = t2.ConvertAndReadTemperature();
                //temp = temp / 5 * 9 + 32;
                //Debug.Print("Temp1 sensor: " + temp1.ToString() + "\n" + "Temp2 sensor: " + temp2.ToString());
                //Thread.Sleep(1000);

                
            }
        }

        private static void RunPreMashing()
        {
            step = ProcessStep.PreMash;
            isWaiting = true;
            ConfirmationNumber++;
            roMessage = "Please Add Mash Water.";
            ResponseObj = new ResponseObject();
            waitTime = DateTime.Now;
            while (isWaiting)
            {
                if (waitTime.AddMinutes(10) < DateTime.Now)
                {
                    RunReset();
                }
            }
            roMessage = "Heating. Please wait.";
            ResponseObj = new ResponseObject();
            targetTemp = currentRecipe.mash_temperature;
            isHeating = true;
            isWaitingToHeat = true;
            while (isWaitingToHeat)
            {

            }
            isWaiting = true;
            ConfirmationNumber++;
            roMessage = "Please Open Valve.";
            ResponseObj = new ResponseObject();
            waitTime = DateTime.Now;
            while (isWaiting)
            {
                if (waitTime.AddMinutes(10) < DateTime.Now)
                {
                    RunReset();
                }
            }
            roMessage = "Pumping. Please wait.";
            ResponseObj = new ResponseObject();
            isHeating = false;
            isPumping = true;
            startTime = DateTime.Now;
            while(isPumping)
            {
                
            }
            isPumping = false;
            isWaiting = true;
            ConfirmationNumber++;
            roMessage = "Please Close Valve and add Grain then Mix mash and put on Lid.";
            ResponseObj = new ResponseObject();
            waitTime = DateTime.Now;
            while (isWaiting)
            {
                if (waitTime.AddMinutes(10) < DateTime.Now)
                {
                    RunReset();
                }
            }
            roMessage = "";
            ResponseObj = new ResponseObject();
            NextStep = true;
        }
        private static void RunMashing()
        {
            step = ProcessStep.Mashing;
            startTime = DateTime.Now;
            isWaiting = true;
            ConfirmationNumber++;
            roMessage = "Please add Sparge water to boiling kettle.";
            ResponseObj = new ResponseObject();
            waitTime = DateTime.Now;
            //TEST 80
            targetTemp = 170;
            while (isWaiting)
            {
                if (waitTime.AddMinutes(10) < DateTime.Now)
                {
                    RunReset();
                }
            }
            roMessage = "Mashing/Heating. Please Wait.";
            ResponseObj = new ResponseObject();
            isHeating = true;
            isWaitingToHeat = true;
            while (isWaitingToHeat || startTime.AddMinutes(currentRecipe.mash_duration).CompareTo(DateTime.Now)>0)
            {

            }
             
            NextStep = true;
        }
        private static void RunSparging()
        {
            step = ProcessStep.Sparging;
            isWaiting = true;
            ConfirmationNumber++;
            roMessage = "Please Open Both Valves.";
            ResponseObj = new ResponseObject();
            waitTime = DateTime.Now;
            while (isWaiting)
            {
                if (waitTime.AddMinutes(10) < DateTime.Now)
                {
                    RunReset();
                }
            }
            roMessage = "Sparging. Please Wait.";
            ResponseObj = new ResponseObject();
            startTime = DateTime.Now;
            
            isPumping = true;
            while (isPumping)
            {

                if (spargeCounter == 6)
                {
                    isPumping = false;
                }
                else
                {
                    ResponseObj = new ResponseObject();
                }
            }

            
            
            isWaiting = true;
            ConfirmationNumber++;
            roMessage = "Please Close Both Valves.";
            ResponseObj = new ResponseObject();
            waitTime = DateTime.Now;
            while (isWaiting)
            {
                if (waitTime.AddMinutes(10) < DateTime.Now)
                {
                    RunReset();
                }
            }
            roMessage = "";
            ResponseObj = new ResponseObject();
            NextStep = true;
        }
        private static void RunBoiling()
        {
            spargeCounter = 1;
            step = ProcessStep.Boiling;
            //TEST 80
            targetTemp = 206;
            isWaitingToHeat = true;
            isHeating = true;
            roMessage = "Heating. Please Wait.";
            ResponseObj = new ResponseObject();
            while(isWaitingToHeat)
            {

            }
            //isWaiting = true;
            ConfirmationNumber++;
            //roMessage = "Wort Has Reached Boiling Please Add Hops.";
            //ResponseObj = new ResponseObject();
            //waitTime = DateTime.Now;
            //while (isWaiting)
            //{
            //    if (waitTime.AddMinutes(10) < DateTime.Now)
            //    {
            //        RunReset();
            //    }
            //}
            roMessage = "Heating. Please Wait.";
            ResponseObj = new ResponseObject();
            startTime = DateTime.Now;
            while (startTime.AddMinutes(currentRecipe.boil_duration).CompareTo(DateTime.Now) > 0)
            {
                for (int i = 0; i < currentRecipe.ingredients.Length; i++)
                {

                    if (DateTime.Now.Minute - startTime.Minute == currentRecipe.ingredients[i].add_time && !currentRecipe.ingredients[i].hasPrompted)
                    {
                        currentRecipe.ingredients[i].hasPrompted = true;
                        isWaiting = true;
                        roMessage = "Please Add " + currentRecipe.ingredients[i].name;
                        ResponseObj = new ResponseObject();
                    }
                    else if (ingredientAdded)
                    {
                        isWaiting = false;
                        ingredientAdded = false;
                        ResponseObj = new ResponseObject();
                    }
                }

            }
            isHeating = false;
            isWaiting = true;
            ConfirmationNumber++;
            roMessage = "Boiling has completed Please Add Ice.";
            ResponseObj = new ResponseObject();
            waitTime = DateTime.Now;
            while (isWaiting)
            {
                if (waitTime.AddMinutes(10) < DateTime.Now)
                {
                    RunReset();
                }
            }
            roMessage = "";
            ResponseObj = new ResponseObject();
            NextStep = true;
        }
        private static void RunCooling()
        {
            step = ProcessStep.Cooling;
            roMessage = "Cooling. Please Wait.";
            ResponseObj = new ResponseObject();
            while (boilTemp > 70)
            {
                Fan.Write(false);
            }
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
            resetFlag = false;
            Pump.Write(true);
            HeatingElement.Write(false);
            Fan.Write(true); 
            ConfirmationNumber = 0;
            spargeCounter = 0;
            ResponseObj = new ResponseObject();
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
