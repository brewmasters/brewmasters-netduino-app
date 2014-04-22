using System;
using Microsoft.SPOT;
using System.Collections;

namespace Brewmasters
{
    public class Recipe
    {
       
        //consists of ingredients
	    public String name {get;set;}
	    public String beer_type{get;set;}
        public String description { get; set; }
        public long id { get; set; }

        public float water_grain_ratio { get; set; }
        public int mash_temperature { get; set; }

        public int boil_duration { get; set; }
        public int mash_duration { get; set; }

        public Ingredient[] ingredients { get; set; }


        public Recipe()
        {

        }
        public Recipe(long id,float waterGrainRatio, int mashTemp, int boilDuration, int mashDuration,Ingredient[] Ingredients)
        {
            this.id = id;
            this.boil_duration = boilDuration;
            this.mash_temperature = mashTemp;
            this.water_grain_ratio = waterGrainRatio;
            this.mash_duration = mashDuration;
            this.ingredients = Ingredients;
        }
    }
}
