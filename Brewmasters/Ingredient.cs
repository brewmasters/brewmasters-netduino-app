using System;
using Microsoft.SPOT;

namespace Brewmasters
{
    public class Ingredient
    {
        
        public String name { get; set; }
        public bool hasAdded { get; set; }
        public bool hasPrompted { get; set; }
        public int add_time { get; set; }

        public Ingredient(String name, int addTime)
        {
            this.hasAdded = false;
            this.hasPrompted = false;
            this.name = name;
            this.add_time = addTime;
        }

    }
}
