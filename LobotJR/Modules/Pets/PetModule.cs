using LobotJR.Client;
using LobotJR.Modules.Wolfcoins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobotJR.Modules.Pets
{
    public class PetModule
    {
        public static Pet GrantPet(string playerName, Currency wolfcoins, Dictionary<int, Pet> petDatabase, IrcClient irc, IrcClient group)
        {
            List<Pet> toAward = new List<Pet>();
            // figure out the rarity of pet to give and build a list of non-duplicate pets to award
            int rarity = wolfcoins.classList[playerName].petEarned;
            foreach (var basePet in petDatabase)
            {
                if (basePet.Value.petRarity != rarity)
                    continue;

                bool alreadyOwned = false;


                foreach (var pet in wolfcoins.classList[playerName].myPets)
                {
                    if (pet.ID == basePet.Value.ID)
                    {
                        alreadyOwned = true;
                        break;
                    }

                }

                if (!alreadyOwned)
                {
                    toAward.Add(basePet.Value);
                }
            }
            // now that we have a list of eligible pets, randomly choose one from the list to award
            Pet newPet;

            if (toAward.Count > 0)
            {
                string toSend = "";
                Random RNG = new Random();
                int petToAward = RNG.Next(1, toAward.Count + 1);
                newPet = new Pet(toAward[petToAward - 1]);
                int sparklyCheck = RNG.Next(1, 101);
                bool firstPet = false;
                if (wolfcoins.classList[playerName].myPets.Count == 0)
                    firstPet = true;

                if (sparklyCheck == 1)
                    newPet.isSparkly = true;

                if (firstPet)
                {
                    newPet.isActive = true;
                    toSend = "You found your first pet! You now have a pet " + newPet.type + ". Whisper me !pethelp for more info.";
                }
                else
                {
                    toSend = "You found a new pet buddy! You earned a " + newPet.type + " pet!";
                }

                if (newPet.isSparkly)
                {
                    toSend += " WOW! And it's a sparkly version! Luck you!";
                }

                newPet.stableID = wolfcoins.classList[playerName].myPets.Count + 1;
                wolfcoins.classList[playerName].myPets.Add(newPet);

                group.Whisper(playerName, toSend);
                if (newPet.isSparkly)
                {
                    Console.WriteLine(DateTime.Now.ToString() + "WOW! " + ": " + playerName + " just found a SPARKLY pet " + newPet.name + "!");
                    irc.SendChatMessage("WOW! " + playerName + " just found a SPARKLY pet " + newPet.name + "! What luck!");
                }
                else
                {
                    Console.WriteLine(DateTime.Now.ToString() + ": " + playerName + " just found a pet " + newPet.name + "!");
                    irc.SendChatMessage(playerName + " just found a pet " + newPet.name + "!");
                }

                if (wolfcoins.classList[playerName].myPets.Count == petDatabase.Count)
                {
                    group.Whisper(playerName, "You've collected all of the available pets! Congratulations!");
                }

                wolfcoins.classList[playerName].petEarned = -1;

                return newPet;
            }

            return new Pet();
        }

        public static void UpdatePets(string petListPath, ref Dictionary<int, string> petList, ref Dictionary<int, Pet> petDatabase)
        {
            IEnumerable<string> fileText;
            if (File.Exists(petListPath))
            {
                fileText = File.ReadLines(petListPath, UTF8Encoding.Default);
            }
            else
            {
                fileText = new List<string>();
                Console.WriteLine($"Failed to load item list file, {petListPath} not found.");
            }
            petDatabase = new Dictionary<int, Pet>();
            petList = new Dictionary<int, string>();
            int petIter = 1;
            foreach (var line in fileText)
            {
                string[] temp = line.Split(',');
                int id = -1;
                int.TryParse(temp[0], out id);
                if (id != -1)
                    petList.Add(id, "content/pets/" + temp[1]);
                else
                    Console.WriteLine("Invalid pet read on line " + petIter);
                petIter++;
            }

            petIter = 0;
            foreach (var pet in petList)
            {
                Pet mypet = new Pet();
                int parsedInt = -1;
                int line = 0;
                string[] temp = { "" };
                fileText = System.IO.File.ReadLines(petList.ElementAt(petIter).Value, UTF8Encoding.Default);
                // pet ID
                mypet.ID = petList.ElementAt(petIter).Key;
                // pet name
                temp = fileText.ElementAt(line).Split('=');
                mypet.name = temp[1];
                line++;
                // pet type (string ex: Fox, Cat, Dog)
                temp = fileText.ElementAt(line).Split('=');
                mypet.type = temp[1];
                line++;
                // pet size (string ex: tiny, small, large)
                temp = fileText.ElementAt(line).Split('=');
                mypet.size = temp[1];
                line++;
                // pet rarity (1=Common,2=Uncommon,3=Rare,4=Epic,5=Artifact)
                temp = fileText.ElementAt(line).Split('=');
                int.TryParse(temp[1], out parsedInt);
                mypet.petRarity = parsedInt;
                line++;
                // pet description
                temp = fileText.ElementAt(line).Split('=');
                mypet.description = temp[1];
                line++;
                // pet emote
                temp = fileText.ElementAt(line).Split('=');
                mypet.emote = temp[1];
                line++;
                int numLines = fileText.Count();
                if (numLines <= line)
                {
                    petDatabase.Add(petIter, mypet);
                    petIter++;
                    continue;
                }
                // success boost (%)
                temp = fileText.ElementAt(line).Split('=');
                int.TryParse(temp[1], out parsedInt);
                mypet.successChance = parsedInt;
                line++;
                // item find (%)
                temp = fileText.ElementAt(line).Split('=');
                int.TryParse(temp[1], out parsedInt);
                mypet.itemFind = parsedInt;
                line++;
                // coin boost (%)
                temp = fileText.ElementAt(line).Split('=');
                int.TryParse(temp[1], out parsedInt);
                mypet.coinBonus = parsedInt;
                line++;
                // xp boost (%)
                temp = fileText.ElementAt(line).Split('=');
                int.TryParse(temp[1], out parsedInt);
                mypet.xpBonus = parsedInt;
                line++;
                // prevent death boost (%)
                temp = fileText.ElementAt(line).Split('=');
                int.TryParse(temp[1], out parsedInt);
                mypet.preventDeathBonus = parsedInt;
                line++;


                petDatabase.Add(petIter, mypet);

                petIter++;
            }
        }

        public static void WhisperPet(string user, Pet pet, IrcClient whisperClient, int detail)
        {
            const int HIGH_DETAIL = 1;

            string name = pet.name;
            int stableID = pet.stableID;
            string rarity = "";
            switch (pet.petRarity)
            {
                case (Pet.QUALITY_COMMON):
                    {
                        rarity = "Common";
                    }
                    break;

                case (Pet.QUALITY_UNCOMMON):
                    {
                        rarity = "Uncommon";
                    }
                    break;

                case (Pet.QUALITY_RARE):
                    {
                        rarity = "Rare";
                    }
                    break;

                case (Pet.QUALITY_EPIC):
                    {
                        rarity = "Epic";
                    }
                    break;

                case (Pet.QUALITY_ARTIFACT):
                    {
                        rarity = "Legendary";
                    }
                    break;

                default:
                    {
                        rarity = "Error";
                    }
                    break;
            }

            List<string> stats = new List<string>();
            if (detail == HIGH_DETAIL)
                stats.Add("Level: " + pet.level + " | Affection: " + pet.affection + " | Energy: " + pet.hunger);

            bool active = pet.isActive;
            string status = "";
            string myStableID = "";
            if (active)
            {
                status = "Active";
                myStableID = "<[" + pet.stableID + "]> ";
            }
            else
            {
                status = "In the Stable";
                myStableID = "[" + pet.stableID + "] ";
            }

            whisperClient.Whisper(user, myStableID + name + " the " + pet.type + " (" + rarity + ") ");
            string sparkly = "";
            if (pet.isSparkly)
                sparkly = "Yes!";
            else
                sparkly = "No";
            if (detail == HIGH_DETAIL)
                whisperClient.Whisper(user, "Status: " + status + " | Sparkly? " + sparkly);

            foreach (var stat in stats)
            {
                whisperClient.Whisper(user, stat);
            }
        }
    }
}
