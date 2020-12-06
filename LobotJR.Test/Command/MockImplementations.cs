using LobotJR.Command;
using LobotJR.Data;
using LobotJR.Modules;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LobotJR.Test.Command
{
    public class TestRepository<T> : IRepository<T>
    {
        private List<T> data;

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
        }

        public void Commit()
        {
        }

        public T Create(T entry)
        {
            data.Add(entry);
            return entry;
        }

        public T Delete(T entry)
        {
            data.Remove(entry);
            return entry;
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

    public class CommandModule : ICommandModule
    {
        public string Name => "Command";

        public List<string> Calls { get; set; } = new List<string>();

        public IEnumerable<CommandHandler> Commands { get; private set; }

        public IEnumerable<ICommandModule> SubModules { get; private set; }

        public CommandModule()
        {
            Commands = new CommandHandler[] {
                new CommandHandler("Foo", (data, user) => { Calls.Add("Command.Foo"); return new CommandResult(""); }, "Foo"),
                new CommandHandler("Bar", (data, user) => { Calls.Add("Command.Bar"); return new CommandResult(""); }, "Bar"),
                new CommandHandler("Unrestricted", (data, user) => { Calls.Add("Command.Unrestricted"); return new CommandResult(""); }, "Unrestricted")
            };
            SubModules = new ICommandModule[]
            {
                new SubCommandModule()
            };
        }
    }

    public class SubCommandModule : ICommandModule
    {
        public string Name => "Sub";
        public List<string> Calls { get; set; } = new List<string>();

        public IEnumerable<CommandHandler> Commands { get; private set; }

        public IEnumerable<ICommandModule> SubModules => null;

        public SubCommandModule()
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
}