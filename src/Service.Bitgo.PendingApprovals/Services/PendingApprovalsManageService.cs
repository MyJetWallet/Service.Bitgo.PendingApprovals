using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
using MyNoSqlServer.Abstractions;
using Newtonsoft.Json;
using Service.Bitgo.PendingApprovals.Domain.Models;
using Service.Bitgo.PendingApprovals.Grpc;
using Service.Bitgo.PendingApprovals.Grpc.Models;
using Service.Bitgo.PendingApprovals.NoSql;
using Service.Bitgo.PendingApprovals.Utils;
using Service.BitGo.SignTransaction.Domain.Models;
using Service.BitGo.SignTransaction.Grpc;
using Service.BitGo.SignTransaction.Grpc.Models;

// ReSharper disable InconsistentLogPropertyNaming

namespace Service.Bitgo.PendingApprovals.Services
{
    public class PendingApprovalsManageService : IPendingApprovalsManageService
    {
        private readonly ILogger<PendingApprovalsManageService> _logger;
        private readonly IPendingApprovalsService _pendingApprovalsService;
        private readonly IMyNoSqlServerDataWriter<PendingApprovalNoSqlEntity> _myNoSqlServerDataWriter;

        public PendingApprovalsManageService(ILogger<PendingApprovalsManageService> logger,
            IPendingApprovalsService pendingApprovalsService,
            IMyNoSqlServerDataWriter<PendingApprovalNoSqlEntity> myNoSqlServerDataWriter)
        {
            _logger = logger;
            _pendingApprovalsService = pendingApprovalsService;
            _myNoSqlServerDataWriter = myNoSqlServerDataWriter;
        }

        public async Task<List<PendingApproval>> GetPendingApprovals()
        {
            using var action = MyTelemetry.StartActivity("Get all pending approvals");
            var entities = await _myNoSqlServerDataWriter.GetAsync();
            return entities.Select(e => e.PendingApproval).Where(e => e.State != "rejected" && e.State != "approved")
                .ToList();
        }

        public async Task<PendingApproval> GetPendingApproval(GetPendingApprovalRequest request)
        {
            using var action = MyTelemetry.StartActivity("Get pending approval");
            request.AddToActivityAsJsonTag("request");
            var entity = await _myNoSqlServerDataWriter.GetAsync(
                PendingApprovalNoSqlEntity.GeneratePartitionKey(request.BrokerId),
                PendingApprovalNoSqlEntity.GenerateRowKey(request.PendingApprovalId));

            if (entity == null)
            {
                _logger.LogInformation("Unable to find pending approval with id {id}", request.PendingApprovalId);
                return null;
            }

            return entity.PendingApproval;
        }

        public async Task<ResolvePendingApprovalResponse> ResolvePendingApproval(ResolvePendingApprovalRequest request)
        {
            using var action = MyTelemetry.StartActivity("Get pending approval resolve request");
            JsonConvert.SerializeObject(request, new OtpHiddenJsonConverter(typeof(ResolvePendingApprovalRequest)))
                .AddToActivityAsTag("request");
            _logger.LogInformation("Get pending approval resolve request: {request}",
                JsonConvert.SerializeObject(request,
                    new OtpHiddenJsonConverter(typeof(ResolvePendingApprovalRequest))));
            var pendingApproval = await _myNoSqlServerDataWriter.GetAsync(
                PendingApprovalNoSqlEntity.GeneratePartitionKey(request.BrokerId),
                PendingApprovalNoSqlEntity.GenerateRowKey(request.PendingApprovalId));
            if (pendingApproval == null)
            {
                _logger.LogInformation("Unable to find pending approval with id {id}", request.PendingApprovalId);
                return new ResolvePendingApprovalResponse
                {
                    Success = false,
                    Error = "Unable to find pending approval"
                };
            }

            if (pendingApproval.PendingApproval.Approvers.Contains(request.ResolvedBy))
            {
                _logger.LogInformation("BitGo user {user} already approved pending approval {id}, ignoring",
                    request.ResolvedBy, request.PendingApprovalId);
                return new ResolvePendingApprovalResponse
                {
                    Success = false,
                    Error = "BitGo user already approved pending approval"
                };
            }

            var resolveResult = await _pendingApprovalsService.UpdatePendingApproval(new UpdatePendingApprovalRequest
            {
                BrokerId = request.BrokerId,
                UserId = request.ResolvedBy,
                PendingApprovalId = request.PendingApprovalId,
                Otp = request.Otp,
                State = request.State,
                UpdatedBy = request.UpdatedBy
            });

            if (resolveResult.Error != null)
            {
                _logger.LogInformation("Unable to resolve pending approval {id} due to {error}",
                    request.PendingApprovalId, resolveResult.Error.Message);
                if (resolveResult.Error.Message.Equals("request was already rejected"))
                {
                    pendingApproval.PendingApproval.State = PendingApprovalUpdatedState.Rejected.ToString().ToLower();
                    await _myNoSqlServerDataWriter.InsertOrReplaceAsync(pendingApproval);
                }
                return new ResolvePendingApprovalResponse
                {
                    Success = false,
                    Error = resolveResult.Error.Message
                };
            }

            pendingApproval.PendingApproval.ApprovedBy ??= new List<string>();
            pendingApproval.PendingApproval.ApprovedBy.Add(request.ResolvedBy);
            pendingApproval.PendingApproval.State = resolveResult.PendingApprovalInfo.State;
            switch (pendingApproval.PendingApproval.State)
            {
                case "approved":
                    _logger.LogInformation("Pending approval {id} is finally approved", request.PendingApprovalId);
                    break;
                case "rejected":
                    _logger.LogInformation("Pending approval {id} is finally rejected", request.PendingApprovalId);
                    break;
            }

            await _myNoSqlServerDataWriter.InsertOrReplaceAsync(pendingApproval);

            return new ResolvePendingApprovalResponse
            {
                Success = true,
                PendingApproval = pendingApproval.PendingApproval
            };
        }
    }
}