using System.Runtime.Serialization;
using Service.Bitgo.PendingApprovals.Domain.Models;
using Service.BitGo.SignTransaction.Domain.Models;

namespace Service.Bitgo.PendingApprovals.Grpc.Models
{
    [DataContract]
    public class ResolvePendingApprovalResponse
    {
        [DataMember(Order = 1)] public bool Success { get; set; }
        [DataMember(Order = 2)] public string Error { get; set; }
        [DataMember(Order = 3)] public PendingApproval PendingApproval { get; set; }
    }
}