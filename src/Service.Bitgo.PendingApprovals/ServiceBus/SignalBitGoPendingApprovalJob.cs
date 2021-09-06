using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DotNetCoreDecorators;
using Microsoft.Extensions.Logging;
using MyJetWallet.BitGo.Settings.Services;
using MyJetWallet.Sdk.Service;
using MyNoSqlServer.Abstractions;
using Newtonsoft.Json;
using OpenTelemetry.Trace;
using Service.Bitgo.PendingApprovals.Domain.Models;
using Service.Bitgo.PendingApprovals.NoSql;
using Service.BitGo.SignTransaction.Domain.Models.NoSql;
using Service.BitGo.SignTransaction.Grpc;
using Service.BitGo.SignTransaction.Grpc.Models;
using Service.Bitgo.Webhooks.Domain.Models;

// ReSharper disable InconsistentLogPropertyNaming

namespace Service.Bitgo.PendingApprovals.ServiceBus
{
    public class SignalBitGoPendingApprovalJob
    {
        private readonly ILogger<SignalBitGoPendingApprovalJob> _logger;
        private readonly IAssetMapper _assetMapper;
        private readonly IPendingApprovalsService _pendingApprovalsService;
        private readonly IMyNoSqlServerDataReader<BitGoUserNoSqlEntity> _myNoSqlServerUserDataReader;
        private readonly IMyNoSqlServerDataWriter<PendingApprovalNoSqlEntity> _myNoSqlServerDataWriter;

        public SignalBitGoPendingApprovalJob(ISubscriber<SignalBitGoPendingApproval> subscriber,
            ILogger<SignalBitGoPendingApprovalJob> logger, IAssetMapper assetMapper, IWalletMapper walletMapper,
            IPendingApprovalsService pendingApprovalsService,
            IMyNoSqlServerDataReader<BitGoUserNoSqlEntity> myNoSqlServerUserDataReader,
            IMyNoSqlServerDataWriter<PendingApprovalNoSqlEntity> myNoSqlServerDataWriter)
        {
            _logger = logger;
            _assetMapper = assetMapper;
            _pendingApprovalsService = pendingApprovalsService;
            _myNoSqlServerUserDataReader = myNoSqlServerUserDataReader;
            _myNoSqlServerDataWriter = myNoSqlServerDataWriter;
            
            subscriber.Subscribe(HandleSignal);
        }

        private async ValueTask HandleSignal(SignalBitGoPendingApproval signal)
        {
            using var activity = MyTelemetry.StartActivity("Handle Event SignalBitGoPendingApproval");
            try
            {
                signal.AddToActivityAsJsonTag("bitgo pending approval signal");

                _logger.LogInformation("Request to handle pending approval from BitGo: {approvalJson}",
                    JsonConvert.SerializeObject(signal));

                var approval =
                    await _pendingApprovalsService.GetPendingApprovalDetails(new GetPendingApprovalRequest()
                        { BrokerId = "jetwallet", PendingApprovalId = signal.PendingApprovalId });

                if (approval == null)
                {
                    _logger.LogInformation("Cannot handle BitGo pending approval, do not found {transferJson}",
                        JsonConvert.SerializeObject(signal));
                    Activity.Current?.SetStatus(Status.Error);
                    return;
                }

                approval.AddToActivityAsJsonTag("bitgo-pending-approval");

                var (brokerId, coin) = _assetMapper.BitgoCoinToAsset(approval.Coin, approval.WalletId);

                var entity = PendingApprovalNoSqlEntity.Create(new PendingApproval
                {
                    BrokerId = brokerId,
                    Id = approval.Id,
                    Asset = coin,
                    Amount = _assetMapper.ConvertAmountFromBitgo(approval.Coin,
                        long.Parse(approval.Info.TransactionRequest.Recipients[0].Amount)),
                    Approvers = approval.WalletId.Split(",").ToList(),
                    ApprovalsCount = approval.ApprovalsRequired,
                    CreatedBy = _myNoSqlServerUserDataReader.Get(BitGoUserNoSqlEntity.GeneratePartitionKey(brokerId))
                        .FirstOrDefault(e => e.User.BitGoId == approval.Creator)?.User?.Id,
                    CreatedDate = approval.CreateDate,
                    DestinationAddress = approval.Info.TransactionRequest.Recipients[0].Address,
                    OperationId = approval.Info.TransactionRequest.BuildParams.SequenceId,
                    State = approval.State
                });

                var existingEntity = await _myNoSqlServerDataWriter.GetAsync(entity.PartitionKey, entity.RowKey);

                if (existingEntity != null)
                {
                    _logger.LogInformation("Pending approval notification {id} already processed, ignoring", approval.Id);
                    return;
                }

                await _myNoSqlServerDataWriter.InsertOrReplaceAsync(entity);

                _logger.LogInformation("Pending approval from BitGo: {approvalJson}",
                    JsonConvert.SerializeObject(entity.PendingApproval));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ex.FailActivity();
                throw;
            }
        }
    }
}