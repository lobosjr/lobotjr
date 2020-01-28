using LobotJR.Client;
using LobotJR.Modules.Wolfcoins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LobotJR.Modules.Fishing
{
    public class FishingModule
    {
        public static void Process(Currency wolfcoins, IrcClient client, List<Fish> fishDatabase)
        {
            // iterate through fishing players, see if anyone's got a bite
            foreach (var player in wolfcoins.fishingList)
            {
                if (player.Value.fishHooked)
                {
                    if ((DateTime.Now - player.Value.timeSinceHook).Seconds > 30)
                    {
                        player.Value.fishHooked = false;
                        player.Value.hookedFishID = -1;

                        client.Whisper(player.Value.username, "Heck! The fish got away. Maybe next time...");
                    }
                }
                if (player.Value.isFishing)
                {
                    if (player.Value.timeOfCatch < DateTime.Now)
                    {
                        player.Value.isFishing = false;
                        player.Value.fishHooked = true;

                        // pick a fish to bite

                        Random rng = new Random();
                        int randFish = WeightedRandomFish(ref fishDatabase).ID;
                        Fish temp = new Fish();
                        foreach (var fish in fishDatabase)
                        {
                            if (fish.ID == randFish)
                            {
                                temp = new Fish(fish);
                            }
                        }
                        player.Value.hookedFishID = randFish;
                        string toSend = "";

                        switch (temp.sizeCategory)
                        {
                            case Fish.SIZE_TINY:
                                {
                                    toSend = "You feel a light tug at your line! Type !catch to reel it in!";
                                }
                                break;

                            case Fish.SIZE_SMALL:
                                {
                                    toSend = "Something nibbles at your bait! Type !catch to reel it in!";
                                }
                                break;

                            case Fish.SIZE_MEDIUM:
                                {
                                    toSend = "A strong tug snags your bait! Type !catch to reel it in!";
                                }
                                break;

                            case Fish.SIZE_LARGE:
                                {
                                    toSend = "Whoa! Something big grabs your line! Type !catch to reel it in!";
                                }
                                break;

                            case Fish.SIZE_HUGE:
                                {
                                    toSend = "You're almost pulled into the water! Something HUGE is hooked! Type !catch to reel it in!";
                                }
                                break;

                            default: break;
                        }

                        client.Whisper(player.Value.username, toSend);
                        player.Value.timeSinceHook = DateTime.Now;
                    }
                }
            }
        }

        public static Fish WeightedRandomFish(ref List<Fish> myFishDatabase)
        {
            if (myFishDatabase.Count <= 1)
                return new Fish();

            const float totalChance = 100;
            int numRarities = myFishDatabase.Last().rarity;
            float dividingFactor = totalChance / numRarities;
            List<float> chances = new List<float>();
            float updatedChance = totalChance;
            // algorithm to generate rarity %'s for each rarity (i.e., if there are 3 rarities, common/uncommon/epic,
            // assign each one a decreasing chance of it being picked ex: 66%/17%/8.5% based on this algorithm
            while (numRarities > 0)
            {
                chances.Add(dividingFactor * 2);
                updatedChance -= (dividingFactor * 2);
                numRarities--;
                if (numRarities == 2)
                {
                    // if last 2 rarities, assign 3/4 of remaining chance to 2nd to last and 1/4 to the last then break
                    chances.Add((3 * dividingFactor) / 4);
                    chances.Add(dividingFactor / 4);
                    break;
                }
                dividingFactor = (updatedChance / numRarities);
            }

            Random rng = new Random();
            // get a decimal rng value between 0 and 1.0 and scale it up to be 1 - 100
            float roll = ((float)rng.NextDouble() * 100);

            // using the assigned % for each rarity, equally distribute that % to each fish of that rarity
            // first find out the rarity to choose from (result)
            int result = 0;
            foreach (var chance in chances)
            {
                float check = totalChance - chance;
                result++;
                if (roll < check)
                    break;
            }

            // create a list of fish that have that rarity
            List<Fish> fishToChooseFrom = new List<Fish>();

            foreach (var fish in myFishDatabase)
            {
                if (fish.rarity == result)
                    fishToChooseFrom.Add(new Fish(fish));
            }

            // now that we have a list of fish of that rarity, equally random roll one
            int choice = rng.Next(0, fishToChooseFrom.Count);

            return fishToChooseFrom.ElementAt(choice);
        }

        public static void WhisperFish(string user, List<Fish> myFish, IrcClient whisperClient)
        {
            int iterator = 0;
            foreach (var fish in myFish)
            {
                iterator++;
                whisperClient.Whisper(user, iterator + ": " + fish.name);
            }

            whisperClient.Whisper(user, "Type !fish # for more information on the particular type of fish.");
        }

        public static void WhisperFish(string user, Dictionary<string, Fisherman> fishingList, int fishID, IrcClient whisperClient)
        {
            Fish myFish = fishingList[user].biggestFish[fishID - 1];
            string mySize = "";
            switch (myFish.sizeCategory)
            {
                case Fish.SIZE_TINY:
                    {
                        mySize = "Tiny";
                    }
                    break;

                case Fish.SIZE_SMALL:
                    {
                        mySize = "Small";
                    }
                    break;

                case Fish.SIZE_MEDIUM:
                    {
                        mySize = "Medium";
                    }
                    break;

                case Fish.SIZE_LARGE:
                    {
                        mySize = "Large";
                    }
                    break;

                case Fish.SIZE_HUGE:
                    {
                        mySize = "Huge";
                    }
                    break;
                default: break;
            }

            whisperClient.Whisper(user, "Name - " + myFish.name);
            whisperClient.Whisper(user, "Length - " + myFish.length + " in.");
            whisperClient.Whisper(user, "Weight - " + myFish.weight + " lbs.");
            whisperClient.Whisper(user, "Size Category - " + mySize);
            whisperClient.Whisper(user, myFish.flavorText);
        }

        public static void UpdateFish(string fishListPath, ref Dictionary<int, string> fishList, ref List<Fish> fishDatabase)
        {
            IEnumerable<string> fileText;
            if (File.Exists(fishListPath))
            {
                fileText = File.ReadLines(fishListPath, UTF8Encoding.Default);
            }
            else
            {
                fileText = new List<string>();
                Console.WriteLine($"Failed to load item list file, {fishListPath} not found.");
            }
            fishDatabase = new List<Fish>();
            fishList = new Dictionary<int, string>();
            int fishIter = 0;
            foreach (var line in fileText)
            {
                fishIter++;
                fishList.Add(fishIter, "content/fishing/" + line);
            }

            fishIter = 0;
            foreach (var fish in fishList)
            {
                Fish myfish = new Fish();
                int parsedInt = -1;
                float parsedFloat = -1;
                int line = 0;
                string[] temp = { "" };
                fileText = System.IO.File.ReadLines(fishList.ElementAt(fishIter).Value, UTF8Encoding.Default);
                // fish ID
                myfish.ID = fishList.ElementAt(fishIter).Key;
                // fish name
                temp = fileText.ElementAt(line).Split('=');
                myfish.name = temp[1];
                line++;
                // fish size category
                temp = fileText.ElementAt(line).Split('=');
                int.TryParse(temp[1], out parsedInt);
                myfish.sizeCategory = parsedInt;
                line++;
                // fish minimum length
                temp = fileText.ElementAt(line).Split('=');
                float.TryParse(temp[1], out parsedFloat);
                myfish.lengthRange[0] = parsedFloat;
                line++;
                // fish maximum length
                temp = fileText.ElementAt(line).Split('=');
                float.TryParse(temp[1], out parsedFloat);
                myfish.lengthRange[1] = parsedFloat;
                line++;
                // fish minimum weight
                temp = fileText.ElementAt(line).Split('=');
                float.TryParse(temp[1], out parsedFloat);
                myfish.weightRange[0] = parsedFloat;
                line++;
                // fish maximum weight
                temp = fileText.ElementAt(line).Split('=');
                float.TryParse(temp[1], out parsedFloat);
                myfish.weightRange[1] = parsedFloat;
                line++;
                // rarity
                temp = fileText.ElementAt(line).Split('=');
                int.TryParse(temp[1], out parsedInt);
                myfish.rarity = parsedInt;
                line++;
                // fish description
                temp = fileText.ElementAt(line).Split('=');
                myfish.flavorText = temp[1];

                fishDatabase.Add(myfish);

                fishIter++;
            }

            fishDatabase.Sort((x, y) => x.rarity.CompareTo(y.rarity));
        }
    }
}
