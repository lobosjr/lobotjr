using System.Collections.Generic;
using System.Linq;

namespace LobotJR.Modules.Fishing
{
    public class TournamentResult
    {
        public IList<TournamentEntry> Entries { get; private set; }

        public TournamentEntry Winner
        {
            get
            {
                return Entries.FirstOrDefault();
            }
        }

        public TournamentResult(IEnumerable<TournamentEntry> entries)
        {
            if (entries == null)
            {
                Entries = new List<TournamentEntry>();
            }
            Entries = entries.OrderByDescending(x => x.Points).ToList();
        }

        public int GetRankByName(string name)
        {
            return Entries.IndexOf(Entries.FirstOrDefault(x => x.Name.Equals(name)));
        }
    }

    public class TournamentEntry
    {
        public string Name { get; set; }
        public int Points { get; set; }
    }
}
