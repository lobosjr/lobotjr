using System;
using System.Collections.Generic;
using System.Linq;
using TwitchBot;
using System.Text;
using System.Threading.Tasks;

namespace Fishing
{
    class Fish
    {
        public const int SIZE_TINY = 1;
        public const int SIZE_SMALL = 2;
        public const int SIZE_MEDIUM = 3;
        public const int SIZE_LARGE = 4;
        public const int SIZE_HUGE = 5;

        public int sizeCategory = -1;
        public int ID = -1;
        public int rarity = -1;
        public string name = "";
        public float[] lengthRange = {-1, -1};
        public float[] weightRange = {-1, -1};
        public float length = -1;
        public float weight = -1;
        public string flavorText = "";
        public string caughtBy = "";

        public Fish()
        {

        }

        public Fish (Fish toCopy)
        {
            sizeCategory = toCopy.sizeCategory;
            ID = toCopy.ID;
            name = toCopy.name;
            lengthRange = toCopy.lengthRange;
            weightRange = toCopy.weightRange;
            length = toCopy.length;
            weight = toCopy.weight;
            flavorText = toCopy.flavorText;
            rarity = toCopy.rarity;
            caughtBy = toCopy.caughtBy;
        }
    }

    class Fisherman
    {
        // category based on random rolling lower than the value
        public const int TINY_CHANCE = 40;
        public const int SMALL_CHANCE = 70;
        public const int MEDIUM_CHANCE = 95;
        public const int LARGE_CHANCE = 99;
        public const int HUGE_CHANCE = 100;

        public const int LURE_COMMON = 1;
        public const int LURE_UNCOMMON = 2;
        public const int LURE_RARE = 3;
        public const int LURE_EPIC = 4;
        public const int LURE_LEGENDARY = 5;

        public string username = "";
        public int level = -1;
        public int XP = -1;
        public List<Fish> biggestFish = new List<Fish>();
        public int lure = -1;
        public bool isFishing = false;
        public bool fishHooked = false;
        public int hookedFishID = -1;
        public DateTime timeOfCatch;
        public DateTime timeSinceHook;

        public Fish Catch(Fish tempFish, IrcClient whisperClient)
        {
            // get fish data out of table
            Fish myCatch = new Fish(tempFish);

            myCatch.caughtBy = username;
            // determine weight & length range, then segment it equally. Weighted RNG will roll one of 5 categories
            float weightRange = myCatch.weightRange[1] - myCatch.weightRange[0];
            float weightFactor = weightRange / 5;
            float lengthRange = myCatch.lengthRange[1] - myCatch.lengthRange[0];
            float lengthFactor = lengthRange / 5;

            Random rng = new Random();

            // roll to determine size category
            float size = (float)rng.NextDouble() * 100;

            float minLength = myCatch.lengthRange[0];
            float minWeight = myCatch.weightRange[0];
            // dependant on size category, set up lengthFactor & roll between the adjusted ranges for weight/length 
            if ( size <= TINY_CHANCE)
            {
                myCatch.length = minLength + (lengthFactor * (float)rng.NextDouble());
                myCatch.weight = minWeight + (weightFactor * (float)rng.NextDouble());
            }
            else if (size <= SMALL_CHANCE)
            {
                minLength += lengthFactor;
                minWeight += weightFactor;
                myCatch.length = minLength + (lengthFactor * (float)rng.NextDouble());
                myCatch.weight = minWeight + (weightFactor * (float)rng.NextDouble());

            }
            else if (size <= MEDIUM_CHANCE)
            {
                minLength += (lengthFactor * 2);
                minWeight += (weightFactor * 2);
                myCatch.length = minLength + (lengthRange * (float)rng.NextDouble());
                myCatch.weight = minWeight + (weightFactor * (float)rng.NextDouble());
            }
            else if (size <= LARGE_CHANCE)
            {
                minLength += (lengthFactor * 3);
                minWeight += (weightFactor * 3);
                myCatch.length = minLength + (lengthFactor * (float)rng.NextDouble());
                myCatch.weight = minWeight + (weightFactor * (float)rng.NextDouble());
            }
            else if(size ==  HUGE_CHANCE)
            {
                minLength += (lengthFactor * 4);
                minWeight += (weightFactor * 4);
                myCatch.length = minLength + (lengthFactor * (float)rng.NextDouble());
                myCatch.weight = minWeight + (weightFactor * (float)rng.NextDouble());
            }

            myCatch.length = (float)Math.Round((double)myCatch.length, 2);
            myCatch.weight = (float)Math.Round((double)myCatch.weight, 2);
            // see if this is the biggest fish of its type you've caught and update if necessary
            bool matchFound = false;
            for (int i = 0; i < biggestFish.Count; i++)
            {
                if(biggestFish[i].ID == myCatch.ID)
                {
                    matchFound = true;
                    if(myCatch.weight > biggestFish[i].weight)
                    {
                        biggestFish[i] = new Fish(myCatch);
                        whisperClient.sendChatMessage(".w " + username + " This is the biggest " + myCatch.name + " you've ever caught!");
                        break;
                    }
                }
            }

            if(!matchFound)
            {
                biggestFish.Add(myCatch);
                whisperClient.sendChatMessage(".w " + username + " This is the biggest " + myCatch.name + " you've ever caught!");
            }
            Console.WriteLine(username + " caught a " + myCatch.weight + " pound, " + myCatch.length + " inch " + myCatch.name + "!");

            myCatch.caughtBy = username;
            
            return myCatch;
        }
    }
}
