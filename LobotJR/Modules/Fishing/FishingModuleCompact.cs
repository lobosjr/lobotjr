using LobotJR.Command;

namespace LobotJR.Modules.Fishing
{
    /// <summary>
    /// Contains the compact methods for the fishing module.
    /// </summary>
    public class FishingModuleCompact
    {
        public ICompactResponse PlayerLeaderboard(string data, string user)
        {
            /*
            else if (whisperMessage == "!fish")
            {
                if (wolfcoins.Exists(wolfcoins.fishingList, whisperSender))
                {
                    if (wolfcoins.fishingList[whisperSender].biggestFish.Count > 0)
                    {
                        Whisper(whisperSender, "You've caught " + wolfcoins.fishingList[whisperSender].biggestFish.Count + " different types of fish: ", group);
                        WhisperFish(whisperSender, wolfcoins.fishingList[whisperSender].biggestFish, group);

                    }
                    else
                    {
                        Whisper(whisperSender, "You haven't caught any fish yet!", group);
                    }
                }
            }
            else if (whisperMessage == "!fish -c")
            {
                if (wolfcoins.Exists(wolfcoins.fishingList, whisperSender))
                {
                    var compact = wolfcoins.fishingList[whisperSender].biggestFish.Select(x => $"{x.name}|{x.length}|{x.weight};");
                    var prefix = "fish: ";
                    var toSend = prefix;
                    foreach (var entry in compact)
                    {
                        if (toSend.Length + entry.Length > 450)
                        {
                            Whisper(whisperSender, toSend, group);
                            toSend = prefix;
                        }
                        toSend += entry;
                    }
                    Whisper(whisperSender, toSend, group);
                }
            }
            else if (whisperMessage.StartsWith("!fish"))
            {
                if (wolfcoins.Exists(wolfcoins.fishingList, whisperSender))
                {
                    if (wolfcoins.fishingList[whisperSender].biggestFish.Count > 0)
                    {
                        string[] msgData = whispers[1].Split(' ');
                        if (msgData.Count() != 2)
                        {
                            Whisper(whisperSender, "Invalid number of parameters. Syntax: !fish <Fish #>", group);
                            continue;
                        }
                        int fishID = -1;
                        if (int.TryParse(msgData[1], out fishID))
                        {
                            if (fishID <= wolfcoins.fishingList[whisperSender].biggestFish.Count && fishID > 0)
                            {
                                WhisperFish(whisperSender, wolfcoins.fishingList, fishID, group);
                            }
                        }
                    }
                    else
                    {
                        Whisper(whisperSender, "You don't have any fish! Type !cast to try and fish for some!", group);
                    }
                }
            }
            */
            return null;
        }

        public ICompactResponse GlobalLeaderboard(string data, string user)
        {
            /*
            else if (whisperMessage == "!fishleaders" || whisperMessage == "!leaderboards")
            {
                foreach (var fish in wolfcoins.fishingLeaderboard)
                {
                    Whisper(whisperSender, "Largest " + fish.name + " caught by " + fish.caughtBy + " at " + fish.weight + " lbs.", group);
                }
            }
            else if (whisperMessage == "!fishleaders -c" || whisperMessage == "!leaderboards -c")
            {
                var compact = wolfcoins.fishingLeaderboard.Select(x => $"{x.name}|{x.length}|{x.weight}|{x.caughtBy};");
                var prefix = "fishleaders: ";
                var toSend = prefix;
                foreach (var entry in compact)
                {
                    if (toSend.Length + entry.Length > 450)
                    {
                        Whisper(whisperSender, toSend, group);
                        toSend = prefix;
                    }
                    toSend += entry;
                }
                Whisper(whisperSender, toSend, group);
            }
            */
            return null;
        }

        public ICompactResponse ReleaseFish(string data, string user)
        {
            /*
            else if (whisperMessage.StartsWith("!releasefish"))
            {
                if (wolfcoins.Exists(wolfcoins.fishingList, whisperSender))
                {
                    if (wolfcoins.fishingList[whisperSender].biggestFish.Count > 0)
                    {
                        string[] msgData = whispers[1].Split(' ');
                        if (msgData.Count() != 2)
                        {
                            Whisper(whisperSender, "Invalid number of parameters. Syntax: !fish <Fish #>", group);
                            continue;
                        }
                        int fishID = -1;
                        if (int.TryParse(msgData[1], out fishID))
                        {
                            if (fishID <= wolfcoins.fishingList[whisperSender].biggestFish.Count && fishID > 0)
                            {
                                string fishName = wolfcoins.fishingList[whisperSender].biggestFish[fishID - 1].name;
                                wolfcoins.fishingList[whisperSender].biggestFish.RemoveAt(fishID - 1);

                                Whisper(whisperSender, "You released your " + fishName + ". Bye bye!", group);
                            }
                        }
                    }
                    else
                    {
                        Whisper(whisperSender, "You don't have any fish! Type !cast to try and fish for some!", group);
                    }
                }
            }
             */
            return null;
        }

        public ICompactResponse CancelCast(string data, string user)
        {
            /*
            else if (whisperMessage.StartsWith("!cancelcast"))
            {
                if ((wolfcoins.Exists(wolfcoins.fishingList, whisperSender)))
                {
                    if (wolfcoins.fishingList[whisperSender].isFishing)
                    {
                        wolfcoins.fishingList[whisperSender].isFishing = false;
                        wolfcoins.fishingList[whisperSender].fishHooked = false;
                        wolfcoins.fishingList[whisperSender].hookedFishID = -1;
                        Whisper(whisperSender, "You reel in the empty line.", group);
                    }
                }
            }
             */
            return null;
        }

        public ICompactResponse CatchFish(string data, string user)
        {
            /*
            else if (whisperMessage.StartsWith("!catch") || whisperMessage.StartsWith("!reel"))
            {
                if ((wolfcoins.Exists(wolfcoins.fishingList, whisperSender)))
                {
                    if (wolfcoins.fishingList[whisperSender].isFishing && (!wolfcoins.fishingList[whisperSender].fishHooked))
                    {
                        Whisper(whisperSender, "Nothing is biting yet! To reset your cast, use !cancelcast", group);
                    }

                    if (wolfcoins.fishingList[whisperSender].fishHooked && wolfcoins.fishingList[whisperSender].hookedFishID != -1)
                    {
                        Fish myCatch = new Fish();

                        foreach (var fish in fishDatabase)
                        {
                            if (fish.ID == wolfcoins.fishingList[whisperSender].hookedFishID)
                            {
                                myCatch = (wolfcoins.fishingList[whisperSender].Catch(new Fish(fish), group, fishingTournamentActive, wolfcoins.fishingList));
                            }
                        }

                        if (fishingTournamentActive)
                        {
                            // update leaderboard
                            bool matchFound = false;
                            for (int i = 0; i < wolfcoins.fishingLeaderboard.Count; i++)
                            {
                                if (wolfcoins.fishingLeaderboard.ElementAt(i).ID == myCatch.ID)
                                {
                                    matchFound = true;
                                    if (myCatch.weight > wolfcoins.fishingLeaderboard.ElementAt(i).weight)
                                    {
                                        wolfcoins.fishingLeaderboard[i] = new Fish(myCatch);
                                        irc.sendChatMessage(whisperSender + " just caught the heaviest " + myCatch.name + " ever! It weighs " + myCatch.weight + " pounds!");
                                        break;
                                    }
                                }
                            }
                            if (!matchFound)
                            {
                                wolfcoins.fishingLeaderboard.Add(new Fish(myCatch));
                                irc.sendChatMessage(whisperSender + " just caught the heaviest " + myCatch.name + " ever! It weighs " + myCatch.weight + " pounds!");

                            }

                            wolfcoins.SaveFishingList();
                        }
                        if (!fishingTournamentActive)
                        {
                            Whisper(whisperSender, "Congratulations! You caught a " + myCatch.length + " inch, " +
                                myCatch.weight + " pound " + myCatch.name + "!", group);
                        }

                        wolfcoins.fishingList[whisperSender].fishHooked = false;
                        wolfcoins.fishingList[whisperSender].hookedFishID = -1;



                    }
                }
            }
             */
            return null;
        }

        public ICompactResponse Cast(string data, string user)
        {
            /*
            else if (whisperMessage.StartsWith("!cast"))
            {
                if (!(wolfcoins.Exists(wolfcoins.fishingList, whisperSender)))
                {
                    // first time fishing! initialize all necessary values
                    Fisherman temp = new Fisherman();
                    temp.username = whisperSender;
                    temp.level = 1;
                    temp.XP = 0;
                    temp.lure = 0;

                    // add new fisherman and save
                    wolfcoins.fishingList.Add(whisperSender, temp);
                    wolfcoins.SaveFishingList();

                }
                if (wolfcoins.fishingList[whisperSender].isFishing)
                {
                    Whisper(whisperSender, "Your line is already cast! I'm sure a fish'll be along soon...", group);
                    continue;
                }

                if (wolfcoins.fishingList[whisperSender].fishHooked)
                {
                    Whisper(whisperSender, "Something's already bit your line! Quick, type !catch to snag it!", group);
                    continue;
                }

                // min/max time, in seconds, before a fish will bite
                int minimumCastTime = 60;
                int maximumCastTime = 600;

                if (fishingTournamentActive)
                {
                    minimumCastTime = tournamentCastTimeMax / 2;
                    maximumCastTime = tournamentCastTimeMax;
                }
                // determine when a fish will bite
                Random rng = new Random();
                int elapsedTime = rng.Next(minimumCastTime, maximumCastTime);

                wolfcoins.fishingList[whisperSender].timeOfCatch = DateTime.Now.AddSeconds(elapsedTime);
                wolfcoins.fishingList[whisperSender].isFishing = true;

                Whisper(whisperSender, "You cast your line out into the water.", group);
            }
            else if (whisperMessage == "!debugcast")
            {
                // min/max time, in seconds, before a fish will bite
                if (whisperSender == tokenData.BroadcastUser || whisperSender == tokenData.ChatUser)
                {
                    wolfcoins.fishingList[whisperSender].timeOfCatch = DateTime.Now.AddSeconds(2);
                    wolfcoins.fishingList[whisperSender].isFishing = true;

                    Whisper(whisperSender, "You cast your line out into the water.", group);
                }
            }
             */
            return null;
        }

        public ICompactResponse NextTournament(string data, string user)
        {
            /*
            else if (whisperMessage == "!nexttournament")
            {
                if (!broadcasting)
                {
                    Whisper(whisperSender, "Stream is offline. Next fishing tournament will begin 15m after the beginning of next stream.", group);
                }
                else
                {
                    if (fishingTournamentActive)
                    {
                        Whisper(whisperSender, "A fishing tournament is active now! Go catch fish at: https://tinyurl.com/PlayWolfpackRPG !", group);
                    }
                    string tourneyTime = "";
                    tourneyTime += (nextTournament - DateTime.Now).Minutes;
                    Whisper(whisperSender, "Next fishing tournament begins in " + tourneyTime + " minutes.", group);
                }
            }
            else if (whisperMessage == "!nexttournament -c")
            {
                if (broadcasting)
                {
                    if (fishingTournamentActive)
                    {
                        var maxDuration = new TimeSpan(0, tournamentDuration, 0); // value is in minutes, so convert to seconds to compare against time elapsed
                        var currentDuration = DateTime.Now - tournamentStart;
                        var left = maxDuration - currentDuration;
                        Whisper(whisperSender, $"nexttournament: -{left.ToString("c")}", group);
                    }
                    else
                    {
                        var toNext = nextTournament - DateTime.Now;
                        Whisper(whisperSender, $"nexttournament: {toNext.ToString("c")}", group);
                    }
                }
            }
             */
            return null;
        }
    }
}
