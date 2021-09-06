using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using Service.Bitgo.PendingApprovals.Domain.Models;
using Service.Bitgo.PendingApprovals.Grpc.Models;
using Service.BitGo.SignTransaction.Grpc.Models;

namespace Service.Bitgo.PendingApprovals.Grpc
{
    [ServiceContract]
    public interface IPendingApprovalsManageService
    {
        [OperationContract]
        public Task<List<PendingApproval>> GetPendingApprovals();
        
        [OperationContract]
        public Task<PendingApproval> GetPendingApproval(GetPendingApprovalRequest request);
        
        [OperationContract]
        public Task<ResolvePendingApprovalResponse> ResolvePendingApproval(ResolvePendingApprovalRequest request);
    }
}