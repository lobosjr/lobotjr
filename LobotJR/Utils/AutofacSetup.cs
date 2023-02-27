using Autofac;
using LobotJR.Command;
using LobotJR.Command.Module;
using LobotJR.Command.Module.AccessControl;
using LobotJR.Command.Module.Fishing;
using LobotJR.Command.Module.Gloat;
using LobotJR.Command.System;
using LobotJR.Command.System.Fishing;
using LobotJR.Command.System.Gloat;
using LobotJR.Data;
using LobotJR.Data.User;
using System.Data.Entity;
using Wolfcoins;

namespace LobotJR.Utils
{
    public class AutofacSetup
    {
        public IContainer Setup(Currency wolfCoins)
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<SqliteContext>().AsSelf().As<DbContext>().As<IStartable>().SingleInstance();
            builder.RegisterType<SqliteRepositoryManager>().AsSelf().AsImplementedInterfaces().SingleInstance();
            //Figure out how to register the IRepository properties of the SqliteRepositoryManager instance
            // Or give up and just change all of the constructors to get the manager and resolve the repositories in their constructors

            builder.RegisterType<FishingSystem>().AsSelf().As<ISystem>().InstancePerLifetimeScope();
            builder.RegisterType<LeaderboardSystem>().AsSelf().As<ISystem>().InstancePerLifetimeScope();
            builder.RegisterType<TournamentSystem>().AsSelf().As<ISystem>().InstancePerLifetimeScope();
            builder.RegisterType<GloatSystem>().AsSelf().As<ISystem>().InstancePerLifetimeScope().WithParameter("wolfcoins", wolfCoins);

            builder.RegisterType<UserLookup>().AsSelf().SingleInstance();

            builder.RegisterType<AccessControlModule>().AsSelf().As<ICommandModule>().InstancePerLifetimeScope();
            builder.RegisterType<AccessControlAdmin>().AsSelf().As<ICommandModule>().InstancePerLifetimeScope();
            builder.RegisterType<FishingModule>().AsSelf().As<ICommandModule>().InstancePerLifetimeScope();
            builder.RegisterType<FishingAdmin>().AsSelf().As<ICommandModule>().InstancePerLifetimeScope();
            builder.RegisterType<TournamentModule>().AsSelf().As<ICommandModule>().InstancePerLifetimeScope();
            builder.RegisterType<LeaderboardModule>().AsSelf().As<ICommandModule>().InstancePerLifetimeScope();
            builder.RegisterType<GloatModule>().AsSelf().As<ICommandModule>().InstancePerLifetimeScope();

            builder.RegisterType<SystemManager>().AsSelf().As<ISystemManager>().SingleInstance();
            builder.RegisterType<CommandManager>().AsSelf().As<ICommandManager>().SingleInstance();

            return builder.Build();
            /*
            var scope = container.BeginLifetimeScope();
            scope.Resolve<ICommandModule>();
            scope.Dispose();
            */
        }
    }
}
