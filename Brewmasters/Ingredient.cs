using System;
using Microsoft.SPOT;

namespace Brewmasters
{
    public class Ingredient
    {
        private long id { get; set; }
        private long recipeId { get; set; }
        private String name { get; set; }
        private String type { get; set; }
        private String description { get; set; }
        private int amount { get; set; }
        private String unit { get; set; }
        private int addTime { get; set; }
    }
}
