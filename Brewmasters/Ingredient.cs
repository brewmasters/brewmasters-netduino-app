using System;
using Microsoft.SPOT;

namespace Brewmasters
{
    public class Ingredient
    {
        public long id { get; set; }
        public long recipe_id { get; set; }
        public String name { get; set; }
        public String type { get; set; }
        public String description { get; set; }
        public int amount { get; set; }
        public String unit { get; set; }
        public int add_time { get; set; }


    }
}
