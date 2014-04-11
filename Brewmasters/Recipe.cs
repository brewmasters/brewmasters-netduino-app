using System;
using Microsoft.SPOT;

namespace Brewmasters
{
    public class Recipe
    {
       
        //consists of ingredients
	    public String name {get;set;}
	    public String beerType{get;set;}
        public String description { get; set; }
        public long id { get; set; }

        public float waterGrainRatio { get; set; }
        private int mashTemp { get; set; }

        private int boilDuration { get; set; }
        private int mashDuration { get; set; }

        public Recipe(long id,float waterGrainRatio, int mashTemp, int boilDuration, int mashDuration)
        {
            this.id = id;
            this.boilDuration = boilDuration;
            this.mashTemp = mashTemp;
            this.waterGrainRatio = waterGrainRatio;
            this.mashDuration = mashDuration;
        }
    }
}
