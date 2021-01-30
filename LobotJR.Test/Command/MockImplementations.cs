using LobotJR.Command;
using LobotJR.Data;
using LobotJR.Data.User;
using LobotJR.Modules;
using LobotJR.Modules.Fishing.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LobotJR.Test.Command
{
    public class TestRepository<T> : IRepository<T>
    {
        private readonly List<T> data;
        private readonly List<T> toRemove;

        public TestRepository(IEnumerable<T> content = null)
        {
            if (content != null)
            {
                data = new List<T>(content);
            }
            else
            {
                data = new List<T>();
            }
            toRemove = new List<T>();
        }

        public void Commit()
        {
            foreach (var entry in toRemove)
            {
                data.Remove(entry);
            }
            toRemove.Clear();
        }

        public T Create(T entry)
        {
            data.Add(entry);
            return entry;
        }

        public T Delete(T entry)
        {
            if (data.Contains(entry))
            {
                toRemove.Add(entry);
                return entry;
            }
            return default(T);
        }

        public T DeleteById(int id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> Read()
        {
            return data;
        }

        public IEnumerable<T> Read(Func<T, bool> filter)
        {
            return data.Where(filter);
        }

        public T Read(T entry)
        {
            return data.Where(x => x.Equals(entry)).FirstOrDefault();
        }

        public T ReadById(int id)
        {
            throw new NotImplementedException();
        }

        public T Update(T entry)
        {
            return entry;
        }
    }

    public class TestRepositoryManager : IRepositoryManager
    {
        public IRepository<UserRole> UserRoles { get; private set; }

        public IRepository<TournamentResult> TournamentResults { get; private set; }

        public IRepository<AppSettings> AppSettings { get; private set; }

        public IRepository<UserMap> Users { get; private set; }

        public IRepository<Fisher> Fishers { get; private set; }

        public IRepository<Catch> FishingLeaderboard { get; private set; }

        public TestRepositoryManager()
        {
            UserRoles = new TestRepository<UserRole>();
            TournamentResults = new TestRepository<TournamentResult>();
        }

        public TestRepositoryManager(IEnumerable<UserRole> roles)
        {
            UserRoles = new TestRepository<UserRole>(roles);
            TournamentResults = new TestRepository<TournamentResult>();
        }

        public TestRepositoryManager(IEnumerable<TournamentResult> results)
        {
            UserRoles = new TestRepository<UserRole>();
            TournamentResults = new TestRepository<TournamentResult>(results);
        }

        public TestRepositoryManager(IEnumerable<UserRole> roles, IEnumerable<TournamentResult> results)
        {
            UserRoles = new TestRepository<UserRole>(roles);
            TournamentResults = new TestRepository<TournamentResult>(results);
        }
    }

    public class TestCommandModule : ICommandModule
    {
        public string Name => "Command";

        public List<string> Calls { get; set; } = new List<string>();

        public IEnumerable<CommandHandler> Commands { get; private set; }

        public IEnumerable<ICommandModule> SubModules { get; private set; }

        public TestCommandModule()
        {
            Commands = new CommandHandler[] {
                new CommandHandler("Foo", (data, user) =>
                {
                    Calls.Add("Command.Foo");
                    return new CommandResult("");
                }, (data, user) =>
                {
                    Calls.Add("Command.Foo -c");
                    var output = new TestCompactResponse();
                    output.data.Add("Foo", string.IsNullOrWhiteSpace(data) ? "Bar" : data);
                    return output;
                }, "Foo"),
                new CommandHandler("Bar", (data, user) => { Calls.Add("Command.Bar"); return new CommandResult(""); }, "Bar"),
                new CommandHandler("Unrestricted", (data, user) => { Calls.Add("Command.Unrestricted"); return new CommandResult(""); }, "Unrestricted")
            };
            SubModules = new ICommandModule[]
            {
                new TestSubCommandModule()
            };
        }
    }

    public class TestSubCommandModule : ICommandModule
    {
        public string Name => "Sub";
        public List<string> Calls { get; set; } = new List<string>();

        public IEnumerable<CommandHandler> Commands { get; private set; }

        public IEnumerable<ICommandModule> SubModules => null;

        public TestSubCommandModule()
        {
            Commands = new CommandHandler[]
            {
                new CommandHandler("Foobar", (data, user) => { Calls.Add("Command.Sub.Foobar"); return new CommandResult(""); }, "Foobar"),
            };
        }
    }

    public class TestModule : ICommandModule
    {
        public string Name => "Test";

        public List<string> Calls { get; set; } = new List<string>();

        public IEnumerable<CommandHandler> Commands => new CommandHandler[]
        {
            new CommandHandler("Test", (data, user) => { Calls.Add("Test.Test"); return null; }, "Test")
        };

        public IEnumerable<ICommandModule> SubModules => null;
    }

    public class TestCompactResponse : ICompactResponse
    {
        public Dictionary<string, string> data = new Dictionary<string, string>();
        public IEnumerable<string> ToCompact()
        {
            return data.Select(x => $"{x.Key}|{x.Value};");
        }
    }
}