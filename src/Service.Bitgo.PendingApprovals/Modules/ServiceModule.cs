using Autofac;
using Microsoft.Extensions.Logging;
using MyJetWallet.BitGo.Settings.Ioc;
using MyJetWallet.Sdk.Service;
using MyNoSqlServer.Abstractions;
using MyNoSqlServer.DataReader;
using MyNoSqlServer.DataWriter;
using MyServiceBus.Abstractions;
using MyServiceBus.TcpClient;
using Service.Bitgo.PendingApprovals.NoSql;
using Service.Bitgo.PendingApprovals.ServiceBus;
using Service.BitGo.SignTransaction.Client;
using Service.BitGo.SignTransaction.Domain.Models.NoSql;
using Service.Bitgo.Webhooks.Client;
// ReSharper disable TemplateIsNotCompileTimeConstantProblem

namespace Service.Bitgo.PendingApprovals.Modules
{
    public class ServiceModule : Module
    {
        public static ILogger ServiceBusLogger { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            ServiceBusLogger = Program.LogFactory.CreateLogger(nameof(MyServiceBusTcpClient));

            var serviceBusClient = new MyServiceBusTcpClient(Program.ReloadedSettings(e => e.SpotServiceBusHostPort),
                ApplicationEnvironment.HostName);
            serviceBusClient.Log.AddLogException(ex =>
                ServiceBusLogger.LogInformation(ex, "Exception in MyServiceBusTcpClient"));
            serviceBusClient.Log.AddLogInfo(info => ServiceBusLogger.LogDebug($"MyServiceBusTcpClient[info]: {info}"));
            serviceBusClient.SocketLogs.AddLogInfo((context, msg) =>
                ServiceBusLogger.LogInformation(
                    $"MyServiceBusTcpClient[Socket {context?.Id}|{context?.ContextName}|{context?.Inited}][Info] {msg}"));
            serviceBusClient.SocketLogs.AddLogException((context, exception) =>
                ServiceBusLogger.LogInformation(exception,
                    $"MyServiceBusTcpClient[Socket {context?.Id}|{context?.ContextName}|{context?.Inited}][Exception] {exception.Message}"));
            builder.RegisterInstance(serviceBusClient).AsSelf().SingleInstance();

            builder.RegisterSignalBitGoApprovalSubscriber(serviceBusClient, "Bitgo-PendingApprovals",
                TopicQueueType.Permanent);

            var myNoSqlClient = new MyNoSqlTcpClient(
                Program.ReloadedSettings(e => e.MyNoSqlReaderHostPort),
                ApplicationEnvironment.HostName ??
                $"{ApplicationEnvironment.AppName}:{ApplicationEnvironment.AppVersion}");

            builder
                .RegisterInstance(myNoSqlClient)
                .AsSelf()
                .SingleInstance();

            builder
                .RegisterInstance(
                    new MyNoSqlReadRepository<BitGoUserNoSqlEntity>(myNoSqlClient, BitGoUserNoSqlEntity.TableName))
                .As<IMyNoSqlServerDataReader<BitGoUserNoSqlEntity>>()
                .SingleInstance();
            
            builder.RegisterBitgoSettingsReader(myNoSqlClient);

            builder
                .RegisterInstance(new MyNoSqlServerDataWriter<PendingApprovalNoSqlEntity>(
                    Program.ReloadedSettings(e => e.MyNoSqlWriterUrl), PendingApprovalNoSqlEntity.TableName, true))
                .As<IMyNoSqlServerDataWriter<PendingApprovalNoSqlEntity>>()
                .SingleInstance()
                .AutoActivate();

            builder
                .RegisterType<SignalBitGoPendingApprovalJob>()
                .AutoActivate()
                .SingleInstance();
            
            builder.RegisterPendingApprovalsClient(Program.Settings.BitgoSignTransactionGrpcServiceUrl);
        }
    }
}