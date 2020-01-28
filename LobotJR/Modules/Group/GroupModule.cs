using LobotJR.Client;
using LobotJR.Modules.Items;
using LobotJR.Modules.Pets;
using LobotJR.Modules.Wolfcoins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobotJR.Modules.Group
{
    public class GroupModule
    {
        public const int DUNGEON_MAX = 3;
        public const int PARTY_FORMING = 1;
        public const int PARTY_FULL = 2;
        public const int PARTY_STARTED = 3;
        public const int PARTY_COMPLETE = 4;
        public const int PARTY_READY = 2;

        public static List<int> Process(Dictionary<int, Party> parties, Dictionary<int, Item> itemDatabase, Dictionary<int, Pet> petDatabase,
            Currency wolfcoins, IrcClient publicClient, IrcClient whisperClient)
        {
            List<int> partiesToRemove = new List<int>();

            foreach (var party in parties)
            {
                if (party.Value.status == PARTY_STARTED && party.Value.myDungeon.messenger.messageQueue.Count() > 0)
                {

                    if (party.Value.myDungeon.messenger.ProcessQueue() == 1)
                    {
                        // grant rewards here
                        foreach (var member in party.Value.members)
                        {

                            // if player had an active pet, lower its hunger and affection
                            if (member.myPets.Count > 0)
                            {
                                bool petUpdated = false;
                                foreach (var pet in wolfcoins.classList[member.name].myPets)
                                {
                                    // if we updated the active pet already (should only be one), we're done
                                    if (petUpdated)
                                        break;

                                    // check for active pet
                                    if (pet.isActive)
                                    {
                                        Random RNG = new Random();
                                        int hungerToLose = RNG.Next(Pet.DUNGEON_HUNGER, Pet.DUNGEON_HUNGER + 6);

                                        pet.affection -= Pet.DUNGEON_AFFECTION;

                                        if (pet.hunger <= 0)
                                        {
                                            // PET DIES HERE
                                            whisperClient.Whisper(member.name, pet.name + " starved to death.");
                                            wolfcoins.classList[member.name].ReleasePet(pet.stableID);
                                            wolfcoins.SaveClassData();
                                            break;
                                        }
                                        else if (pet.hunger <= 10)
                                        {
                                            whisperClient.Whisper(member.name, pet.name + " is very hungry and will die if you don't feed it soon!");
                                        }
                                        else if (pet.hunger <= 25)
                                        {
                                            whisperClient.Whisper(member.name, pet.name + " is hungry! Be sure to !feed them!");
                                        }



                                        if (pet.affection < 0)
                                            pet.affection = 0;

                                        petUpdated = true;
                                    }

                                }
                            }

                            if (member.xpEarned == 0 || member.coinsEarned == 0)
                                continue;

                            if ((member.xpEarned + member.coinsEarned) > 0 && member.usedGroupFinder && (DateTime.Now - member.lastDailyGroupFinder).TotalDays >= 1)
                            {
                                member.lastDailyGroupFinder = DateTime.Now;
                                member.xpEarned *= 2;
                                member.coinsEarned *= 2;
                                whisperClient.Whisper(member.name, "You earned double rewards for completing a daily Group Finder dungeon! Queue up again in 24 hours to receive the 2x bonus again! (You can whisper me '!daily' for a status.)");
                            }

                            wolfcoins.AwardXP(member.xpEarned, member.name, whisperClient);
                            wolfcoins.AwardCoins(member.coinsEarned, member.name);
                            if (member.xpEarned > 0 && member.coinsEarned > 0)
                                whisperClient.Whisper(member.name, member.name + ", you've earned " + member.xpEarned + " XP and " + member.coinsEarned + " Wolfcoins for completing the dungeon!");

                            if (wolfcoins.classList[member.name].itemEarned != -1)
                            {
                                int itemID = ItemModule.GrantItem(wolfcoins.classList[member.name].itemEarned, wolfcoins, member.name, itemDatabase);
                                whisperClient.Whisper(member.name, "You looted " + itemDatabase[(itemID - 1)].itemName + "!");
                            }
                            // if a pet is waiting to be awarded
                            if (wolfcoins.classList[member.name].petEarned != -1)
                            {

                                Dictionary<int, Pet> allPets = new Dictionary<int, Pet>(petDatabase);
                                Pet newPet = PetModule.GrantPet(member.name, wolfcoins, allPets, publicClient, whisperClient);
                                if (newPet.stableID != -1)
                                {
                                    string logPath = "petlog.txt";
                                    string timestamp = DateTime.Now.ToString();
                                    if (newPet.isSparkly)
                                    {
                                        System.IO.File.AppendAllText(logPath, timestamp + ": " + member.name + " found a SPARKLY pet " + newPet.name + "." + Environment.NewLine);
                                    }
                                    else
                                    {
                                        System.IO.File.AppendAllText(logPath, timestamp + ": " + member.name + " found a pet " + newPet.name + "." + Environment.NewLine);
                                    }
                                }
                                //if (wolfcoins.classList[member.name].petEarned != -1)
                                //{
                                //    List<Pet> toAward = new List<Pet>();
                                //    bool hasActivePet = false;
                                //    // figure out the rarity of pet to give and build a list of non-duplicate pets to award
                                //    int rarity = wolfcoins.classList[member.name].petEarned;
                                //    foreach (var basePet in petDatabase)
                                //    {
                                //        if (basePet.Value.petRarity != rarity)
                                //            continue;

                                //        bool alreadyOwned = false;

                                //        foreach(var pet in wolfcoins.classList[member.name].myPets)
                                //        {
                                //            if (pet.isActive)
                                //                hasActivePet = true;

                                //            if (pet.ID == basePet.Value.ID)
                                //                alreadyOwned = true;
                                //        }

                                //        if(!alreadyOwned)
                                //        {
                                //            toAward.Add(basePet.Value);
                                //        }
                                //    }
                                //    // now that we have a list of eligible pets, randomly choose one from the list to award
                                //    Pet newPet = new Pet();

                                //    if(toAward.Count > 0)
                                //    {
                                //        string toSend = "";
                                //        Random RNG = new Random();
                                //        int petToAward = RNG.Next(1, toAward.Count);
                                //        newPet = toAward[petToAward - 1];
                                //        int sparklyCheck = RNG.Next(1, 100);

                                //        if (sparklyCheck == 1)
                                //            newPet.isSparkly = true;

                                //        newPet.stableID = wolfcoins.classList[member.name].myPets.Count;
                                //        wolfcoins.classList[member.name].myPets.Count = wolfcoins.classList[member.name].myPets.Count;

                                //        if (!hasActivePet)
                                //        {
                                //            newPet.isActive = true;
                                //            toSend = "You found your first pet! You now have a pet " + newPet.name + ". Whisper me !pethelp for more info.";
                                //        }
                                //        else
                                //        {
                                //            toSend = "You found a new pet buddy! You earned a " + newPet.name + " pet!";
                                //        }

                                //        if(newPet.isSparkly)
                                //        {
                                //            toSend += " WOW! And it's a sparkly version! Luck you!";
                                //        }

                                //        wolfcoins.classList[member.name].myPets.Add(newPet);

                                //        wolfcoins.classList[member.name].petEarned = -1;
                                //        group.Whisper(member.name, toSend);
                                //        if (newPet.isSparkly)
                                //        {
                                //            Console.WriteLine(DateTime.Now.ToString() + "WOW! " + ": " + member.name + " just found a SPARKLY pet " + newPet.name + "!");
                                //            irc.sendChatMessage("WOW! " + member.name + " just found a SPARKLY pet " + newPet.name + "! What luck!");
                                //        }
                                //        else
                                //        {
                                //            Console.WriteLine(DateTime.Now.ToString() + ": " + member.name + " just found a pet " + newPet.name + "!");
                                //            irc.sendChatMessage(member.name + " just found a pet " + newPet.name + "!");
                                //        }
                                //    }
                                //}
                            }

                            if (wolfcoins.classList[member.name].queueDungeons.Count > 0)
                                wolfcoins.classList[member.name].ClearQueue();

                        }

                        party.Value.PostDungeon(wolfcoins);
                        wolfcoins.SaveClassData();
                        wolfcoins.SaveXP();
                        wolfcoins.SaveCoins();
                        party.Value.status = PARTY_READY;

                        if (party.Value.usedDungeonFinder)
                        {
                            partiesToRemove.Add(party.Key);
                        }
                    }
                }
            }

            return partiesToRemove;
        }

        public static void RemoveParties(List<int> partiesToRemove, Dictionary<int, Party> parties, Currency wolfcoins, IrcClient whisperClient)
        {
            for (int i = 0; i < partiesToRemove.Count; i++)
            {
                int Key = partiesToRemove[i];
                foreach (var member in parties[Key].members)
                {
                    whisperClient.Whisper(member.name, "You completed a group finder dungeon. Type !queue to join another group!");
                    wolfcoins.classList[member.name].groupID = -1;
                    wolfcoins.classList[member.name].numInvitesSent = 0;
                    wolfcoins.classList[member.name].isPartyLeader = false;
                    wolfcoins.classList[member.name].ClearQueue();
                }
                parties.Remove(Key);
            }
        }
    }
}
