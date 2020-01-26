using LobotJR.Modules.Classes;
using LobotJR.Modules.Wolfcoins;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LobotJR.Modules.GroupFinder
{
    public class GroupFinderQueue
    {
        const int DUNGEON_MAX = 3;
        const int RAID_MAX = 5;

        public int priority = 0;
        public List<CharClass> queue;
        private readonly Dictionary<int, string> dungeonList;
        public DateTime lastFormed = DateTime.Now;

        public GroupFinderQueue(Dictionary<int, string> dungeonList)
        {
            queue = new List<CharClass>();
            this.dungeonList = dungeonList;
        }

        public Party Add(CharClass player)
        {
            queue.Add(player);

            return CheckForMatch(player);
        }

        public Party CheckForMatch(CharClass player)
        {
            Party newParty = new Party();
            bool partyFilled = false;
            newParty.AddMember(player);
            List<int> membersByPriority = new List<int>();
            List<CharClass> eligibileMembers = new List<CharClass>();
            List<int> matchedDungeons = player.queueDungeons;

            foreach (var member in queue)
            {
                if (member.name == player.name)
                    continue;

                if (member.classType == player.classType)
                {
                    continue;
                }
                else
                {
                    membersByPriority.Add(member.queuePriority);
                }

                //for(int k = 0; k < matchedDungeons.Count; k++)
                //{
                //    if(!currentMatches.Contains(matchedDungeons.ElementAt(k)))
                //    {
                //        matchedDungeons.Remove(matchedDungeons.ElementAt(k));
                //        k--;
                //    }
                //}
            }

            if (membersByPriority.Count == 0)
                return newParty;

            membersByPriority.Sort();

            for (int i = 0; i < membersByPriority.Count; i++)
            {
                foreach (var member in queue)
                {
                    if (member.queuePriority == membersByPriority.ElementAt(i))
                    {
                        eligibileMembers.Add(member);
                        break;
                    }
                }
            }

            foreach (var member in eligibileMembers)
            {
                List<int> currentMatches = new List<int>();
                foreach (var dungeonID in member.queueDungeons)
                {
                    if (!matchedDungeons.Contains(dungeonID))
                        continue;

                    currentMatches.Add(dungeonID);
                }

                if (currentMatches.Count == 0)
                    continue;

                for (int k = 0; k < matchedDungeons.Count; k++)
                {
                    if (!currentMatches.Contains(matchedDungeons.ElementAt(k)))
                    {
                        matchedDungeons.Remove(matchedDungeons.ElementAt(k));
                        k--;
                    }
                }
                bool isDuplicateClass = false;
                foreach (var partyMember in newParty.members)
                {
                    if (member.classType == partyMember.classType)
                    {
                        isDuplicateClass = true;
                        break;
                    }
                }
                if (isDuplicateClass)
                    continue;

                newParty.AddMember(member);
                if (newParty.NumMembers() == DUNGEON_MAX)
                {
                    foreach (var dude in newParty.members)
                    {
                        dude.queueDungeons = matchedDungeons;
                    }
                    partyFilled = true;
                    lastFormed = DateTime.Now;
                    break;
                }
            }


            if (partyFilled)
                RemoveMembers(newParty);

            return newParty;
        }

        private void RemoveMembers(Party myParty)
        {
            for (int i = 0; i < queue.Count; i++)
            {
                if (queue.ElementAt(i).Equals(myParty.members.ElementAt(0)) || queue.ElementAt(i).Equals(myParty.members.ElementAt(1)) || queue.ElementAt(i).Equals(myParty.members.ElementAt(2)))
                {
                    queue.Remove(queue.ElementAt(i));
                    i--;
                }
            }
        }

        public void RemoveMember(string user)
        {
            int iter = 0;
            foreach (var member in queue)
            {
                if (member.name == user)
                {
                    queue.Remove(queue.ElementAt(iter));
                    break;
                }
                iter++;
            }
        }

    }
}
