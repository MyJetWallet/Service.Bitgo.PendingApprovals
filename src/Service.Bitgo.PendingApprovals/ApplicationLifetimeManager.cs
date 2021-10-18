using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
using MyJetWallet.Sdk.ServiceBus;
using MyNoSqlServer.DataReader;
using MyServiceBus.TcpClient;

namespace Service.Bitgo.PendingApprovals
{
    public class ApplicationLifetimeManager : ApplicationLifetimeManagerBase
    {
        private readonly ILogger<ApplicationLifetimeManager> _logger;
        private readonly ServiceBusLifeTime _busTcpClient;
        private readonly MyNoSqlTcpClient _myNoSqlClient;

        public ApplicationLifetimeManager(IHostApplicationLifetime appLifetime,
            ILogger<ApplicationLifetimeManager> logger, ServiceBusLifeTime myServiceBusTcpClient,
            MyNoSqlTcpClient myNoSqlClient) : base(appLifetime)
        {
            _logger = logger;
            _busTcpClient = myServiceBusTcpClient;
            _myNoSqlClient = myNoSqlClient;
        }

        protected override void OnStarted()
        {
            _logger.LogInformation("OnStarted has been called");
            _myNoSqlClient.Start();
            _logger.LogInformation("MyNoSqlTcpClient is started");
            _busTcpClient.Start();
            _logger.LogInformation("MyServiceBusTcpClient is started");
        }

        protected override void OnStopping()
        {
            _logger.LogInformation("OnStopping has been called");
            _myNoSqlClient.Stop();
            _logger.LogInformation("MyNoSqlTcpClient is stopped");
            _busTcpClient.Stop();
            _logger.LogInformation("MyServiceBusTcpClient is stopped");
        }

        protected override void OnStopped()
        {
            _logger.LogInformation("OnStopped has been called");
        }
    }
}