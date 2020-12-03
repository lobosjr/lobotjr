using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace LobotJR.Modules.Fishing
{
    public class TournamentResult
    {
        public int Id { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        
        public virtual List<TournamentEntry> Entries { get; set; } = new List<TournamentEntry>();

        public TournamentEntry Winner
        {
            get
            {
                return Entries.FirstOrDefault();
            }
        }
        public TournamentResult()
        {

        }

        public TournamentResult(IEnumerable<TournamentEntry> entries)
        {
            Entries = entries?.OrderByDescending(x => x.Points).ToList();
        }

        public int GetRankByName(string name)
        {
            return Entries.IndexOf(Entries.FirstOrDefault(x => x.Name.Equals(name)));
        }
    }

    public class TournamentEntry
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Points { get; set; }

        public int ResultId { get; set; }
        public virtual TournamentResult Result { get; set; }
    }
}
