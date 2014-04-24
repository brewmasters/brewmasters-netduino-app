using System;
using Microsoft.SPOT;

namespace Brewmasters
{
    public class ResponseObject
    {
        public bool status { get; set; }
        public ProcessStep step { get; set; }
        public TimeSpan timeLeft { get; set; }
        public double MashTemp { get; set; }
        public double BoilTemp { get; set; }
        public bool DoesRequireUser { get; set; }
        public string Message { get; set; }
        public int ConfirmationNumber { get; set; }
        public int SpargeNumber { get; set; }

        public ResponseObject(bool isBrewing, ProcessStep currentStep, TimeSpan timeleft, double MashTemp, double BoilTemp, bool reqUser, string Message, int connum)
        {
            this.status = isBrewing;
            this.step = currentStep;
            this.timeLeft = timeleft;
            this.BoilTemp = BoilTemp;
            this.MashTemp = MashTemp;
            this.DoesRequireUser = reqUser;
            this.Message = Message;
            this.ConfirmationNumber = connum;
        }
        public ResponseObject()
        {
            this.status = Program.isBrewing;
            this.step = Program.step;
            this.timeLeft = Program.timeLeft;
            this.MashTemp = Program.mashTemp;
            this.BoilTemp = Program.boilTemp;
            this.DoesRequireUser = Program.isWaiting;
            this.Message = Program.roMessage;
            this.ConfirmationNumber = Program.ConfirmationNumber;
            this.SpargeNumber = Program.spargeCounter;
            

        }
        public ResponseObject(bool requser)
        {
            this.status = Program.isBrewing;
            this.step = Program.step;
            this.timeLeft = Program.timeLeft;
            this.MashTemp = Program.mashTemp;
            this.BoilTemp = Program.boilTemp;
            this.DoesRequireUser = requser;
            this.Message = Program.roMessage;
            this.ConfirmationNumber = Program.ConfirmationNumber;
            this.SpargeNumber = Program.spargeCounter;


        }
      
    }
}
