using LobotJR.Modules.Classes;
using LobotJR.Modules.Dungeons;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LobotJR.Modules.Wolfcoins
{
    public class Party
    {
        public Dungeon myDungeon;
        public HashSet<CharClass> members = new HashSet<CharClass>();
        public string partyLeader;
        public int status = 0;
        public int myID = -1;
        public DateTime lastTime = DateTime.Now;
        public bool usedDungeonFinder = false;

        public void PostDungeon(Currency wolfcoins)
        {
            foreach (var member in members)
            {
                wolfcoins.classList[member.name].xpEarned = 0;
                wolfcoins.classList[member.name].coinsEarned = 0;

                wolfcoins.classList[member.name].numInvitesSent = 0;
                wolfcoins.classList[member.name].pendingInvite = false;
            }
        }

        public void ResetTime()
        {
            this.lastTime = DateTime.Now;
        }

        public void AddMember(CharClass member)
        {
            members.Add(member);
        }

        public bool RemoveMember(string user)
        {
            for (int i = 0; i < members.Count(); i++)
            {
                if (members.ElementAt(i).name == user)
                {
                    members.Remove(members.ElementAt(i));
                    return true;
                }
            }
            return false;
        }

        public int NumMembers()
        {
            int num = 0;
            for (int i = 0; i < members.Count(); i++)
            {
                if (!members.ElementAt(i).pendingInvite)
                    num++;
            }
            return num;
        }
    }
}
