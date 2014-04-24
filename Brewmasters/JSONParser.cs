using System;
using Microsoft.SPOT;


namespace Brewmasters
{
    public class JSONParser
    {
       // string JSONString = null;
       // ArrayList ingredients = new ArrayList();
        Recipe JSONRecipe = new Recipe();
        
        string currentKey = null;
        string currentValue = null;
        Ingredient[] ingredientList;

        public Recipe Deserialize(string JSON)
        {
            //String ingredientString = JSON.Split('[')[1].Split(']')[0];
            string[] firstSplit = JSON.Split('{')[2].Split('}');
            string[] secondSplit = firstSplit[0].Split(',');
            int index = 0;
            foreach (string s in secondSplit)
            {
                currentKey = s.Split('=')[0];
                currentValue = s.Split('=')[1];
                //if (currentKey.Trim().Equals("\\\"id\\\""))
                //{
                //    JSONRecipe.id = Convert.ToInt16(currentValue.Trim());
                //}
                //else if (currentKey.Trim().Equals("\\\"name\\\""))
                //{
                //    JSONRecipe.name = currentValue.Split('"')[1];
                //}
                //else if (currentKey.Trim().Equals("\\\"description\\\""))
                //{
                //    JSONRecipe.description = currentValue.Split('"')[1];
                //}
                //else if (currentKey.Trim().Equals("\\\"waterGrainRatio\\\""))
                //{
                //    JSONRecipe.water_grain_ratio = (float)Convert.ToDouble(currentValue.Trim());
                //}
                //else if (currentKey.Trim().Equals("\\\"beerType\\\""))
                //{
                //    JSONRecipe.beer_type = currentValue.Split('"')[1];
                //}
                if (currentKey.Trim().Equals("mash_temperature"))
                {
                    JSONRecipe.mash_temperature = Convert.ToInt32(currentValue);
                }
                else if (currentKey.Trim().Equals("boil_duration"))
                {
                    JSONRecipe.boil_duration = Convert.ToInt32(currentValue);
                }
                else if (currentKey.Trim().Equals("mash_duration"))
                {
                    JSONRecipe.mash_duration = Convert.ToInt32(currentValue);
                }
                else if (currentKey.Trim().Equals("ingredient_length"))
                {
                    ingredientList = new Ingredient[Convert.ToInt32(currentValue)];

                }
                else
                {
                    ingredientList[index] = new Ingredient(currentKey, Convert.ToInt32(currentValue));
                    index++;
                }
                //else if (currentKey.Trim().Equals("\"ingredients\""))
                //{
                //    JSONRecipe.ingredients = DeserializeIngredients(ingredientString);
                    
                //}
            }
            JSONRecipe.ingredients = ingredientList;
            return JSONRecipe;
        }

        public string Serialize(ResponseObject ro)
        {
            int minutes = ro.timeLeft.Minutes;
            int spargeNum = ro.SpargeNumber;
            if (spargeNum > 5)
            {
                spargeNum = 5;
            }
            if(minutes==0 && ro.timeLeft.Seconds>0)
            {
                minutes = 1;

            }
            else if(minutes<0){
                minutes = 0;
            }
            string JSON = "{\"ResponseObject\":{\"status\":" + "\"" + ro.status + "\"" + ",\"step\":" + "\"" + ro.step + "\"" + ",\"timeLeft\":" + "\"" + minutes+ "\"" + ",\"MashTemp\":" + "\"" + ro.MashTemp + "\"" + ",\"BoilTemp\":" + "\"" + ro.BoilTemp + "\"" + ",\"DoesRequireUser\":" + "\"" + ro.DoesRequireUser + "\"" + ",\"Message\":" + "\"" + ro.Message + "\"" + ",\"ConfirmationNumber\":" + "\"" + ro.ConfirmationNumber + "\"" + ",\"SpargeNumber\":" + "\"" + spargeNum + "\"" + "}}";
            return JSON;

        }

        //public Ingredient[] DeserializeIngredients(string ingredientString)
        //{
            
        //    String[] ingredientList = ingredientString.Split('}');
        //    String[] DelimetedList = new String[ingredientList.Length - 1];
        //    Ingredient[] finalList = new Ingredient[DelimetedList.Length];
        //    Ingredient currentIngredient = new Ingredient();
        //    for(int i=0;i<ingredientList.Length-1;i++)
        //    {
                
        //        DelimetedList[i] = ingredientList[i].Split('{')[1];
                
        //    }
        //    int counter = 0;
        //    foreach (String s in DelimetedList)
        //    {
        //        currentIngredient = new Ingredient();
        //        String[] split = s.Split(',');
        //        foreach (String p in split)
        //        {
        //            currentKey = p.Split(':')[0];
        //            currentValue = p.Split(':')[1];
        //            if (currentKey.Trim().Equals("\"id\""))
        //            {
        //                currentIngredient.id = Convert.ToInt32(currentValue.Trim());
        //            }
        //            else if (currentKey.Trim().Equals("\"recipeId\""))
        //            {
        //                currentIngredient.recipe_id = Convert.ToInt32(currentValue.Trim());
        //            }
        //            else if (currentKey.Trim().Equals("\"name\""))
        //            {
        //                currentIngredient.name = currentValue.Split('"')[1];
        //            }
        //            else if (currentKey.Trim().Equals("\"type\""))
        //            {
        //                currentIngredient.type = currentValue.Split('"')[1];
        //            }
        //            else if (currentKey.Trim().Equals("\"description\""))
        //            {
        //                currentIngredient.description = currentValue.Split('"')[1];
        //            }
        //            else if (currentKey.Trim().Equals("\"amount\""))
        //            {
                        
        //                currentIngredient.amount = Convert.ToInt32(currentValue.Trim());
                            
        //            }
        //            else if (currentKey.Trim().Equals("\"unit\""))
        //            {
        //                currentIngredient.unit = currentValue.Split('"')[1];
        //            }
        //            else if (currentKey.Trim().Equals("\"addTime\""))
        //            {
        //                currentIngredient.add_time = Convert.ToInt32(currentValue.Trim());
        //            }



                           
        //        }
        //        finalList[counter] = currentIngredient;
        //        counter++;
        //    }
        //    return finalList;

        //}
        

    }
}
