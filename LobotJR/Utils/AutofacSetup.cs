using Autofac;
using Autofac.Core;
using LobotJR.Command;
using LobotJR.Command.Module;
using LobotJR.Command.Module.AccessControl;
using LobotJR.Command.Module.Fishing;
using LobotJR.Command.Module.Gloat;
using LobotJR.Command.System;
using LobotJR.Command.System.Fishing;
using LobotJR.Command.System.Gloat;
using LobotJR.Data;
using LobotJR.Data.Migration;
using LobotJR.Data.User;
using LobotJR.Shared.Authentication;
using LobotJR.Shared.Client;
using LobotJR.Trigger;
using LobotJR.Trigger.Responder;
using LobotJR.Twitch;
using System.Data.Entity;
using Wolfcoins;

namespace LobotJR.Utils
{
    public static class AutofacSetup
    {
        public static IContainer SetupUpdater(ClientData clientData, TokenData tokenData)
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<DatabaseUpdate_Null_1_0_0>().As<IDatabaseUpdate>().InstancePerLifetimeScope()
                .WithParameters(new Parameter[] { new TypedParameter(typeof(ClientData), clientData), new TypedParameter(typeof(TokenData), tokenData) });
            builder.RegisterType<DatabaseUpdate_1_0_0_1_0_1>().As<IDatabaseUpdate>().InstancePerLifetimeScope();
            builder.RegisterType<DatabaseUpdate_1_0_1_1_0_2>().As<IDatabaseUpdate>().InstancePerLifetimeScope();

            builder.RegisterType<SqliteDatabaseUpdater>().AsSelf().InstancePerLifetimeScope();

            return builder.Build();
        }

        private static void RegisterDatabase(ContainerBuilder builder, ClientData clientData, TokenData tokenData)
        {
            builder.RegisterType<SqliteContext>().AsSelf().As<DbContext>().As<IStartable>().InstancePerLifetimeScope();

            builder.RegisterType<SqliteRepositoryManager>().AsSelf().AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<UserLookup>().AsSelf().InstancePerLifetimeScope();
        }

        private static void RegisterRpg(ContainerBuilder builder, ClientData clientData, TokenData tokenData)
        {
            builder.RegisterType<Currency>().AsSelf().SingleInstance()
                .WithParameters(new Parameter[] { new TypedParameter(typeof(ClientData), clientData), new TypedParameter(typeof(TokenData), tokenData) });
        }

        private static void RegisterSystems(ContainerBuilder builder)
        {
            builder.RegisterType<FishingSystem>().AsSelf().As<ISystem>().InstancePerLifetimeScope();
            builder.RegisterType<LeaderboardSystem>().AsSelf().As<ISystem>().InstancePerLifetimeScope();
            builder.RegisterType<TournamentSystem>().AsSelf().As<ISystem>().InstancePerLifetimeScope();
            builder.RegisterType<GloatSystem>().AsSelf().As<ISystem>().InstancePerLifetimeScope();
        }

        private static void RegisterModules(ContainerBuilder builder)
        {
            builder.RegisterType<AccessControlModule>().AsSelf().As<ICommandModule>().InstancePerLifetimeScope();
            builder.RegisterType<AccessControlAdmin>().AsSelf().As<ICommandModule>().InstancePerLifetimeScope();
            builder.RegisterType<FishingModule>().AsSelf().As<ICommandModule>().InstancePerLifetimeScope();
            builder.RegisterType<FishingAdmin>().AsSelf().As<ICommandModule>().InstancePerLifetimeScope();
            builder.RegisterType<TournamentModule>().AsSelf().As<ICommandModule>().InstancePerLifetimeScope();
            builder.RegisterType<LeaderboardModule>().AsSelf().As<ICommandModule>().InstancePerLifetimeScope();
            builder.RegisterType<GloatModule>().AsSelf().As<ICommandModule>().InstancePerLifetimeScope();
        }

        private static void RegisterTriggers(ContainerBuilder builder)
        {
            builder.RegisterType<BlockLinks>().AsSelf().As<ITriggerResponder>().InstancePerLifetimeScope();
            builder.RegisterType<NoceanMan>().AsSelf().As<ITriggerResponder>().InstancePerLifetimeScope();
        }

        private static void RegisterManagers(ContainerBuilder builder, ClientData clientData, TokenData tokenData)
        {
            builder.RegisterType<TwitchClient>().AsSelf().InstancePerLifetimeScope()
                .WithParameters(new Parameter[] { new TypedParameter(typeof(ClientData), clientData), new TypedParameter(typeof(TokenData), tokenData) });
            builder.RegisterType<SystemManager>().AsSelf().As<ISystemManager>().InstancePerLifetimeScope();
            builder.RegisterType<CommandManager>().AsSelf().As<ICommandManager>().InstancePerLifetimeScope();
            builder.RegisterType<TriggerManager>().AsSelf().InstancePerLifetimeScope();
        }

        public static IContainer Setup(ClientData clientData, TokenData tokenData)
        {
            var builder = new ContainerBuilder();

            RegisterDatabase(builder, clientData, tokenData);
            RegisterRpg(builder, clientData, tokenData);
            RegisterSystems(builder);
            RegisterModules(builder);
            RegisterTriggers(builder);
            RegisterManagers(builder, clientData, tokenData);

            return builder.Build();
        }
    }
}
