using System.Runtime.Serialization;
using Service.BitGo.SignTransaction.Domain.Models;

namespace Service.Bitgo.PendingApprovals.Grpc.Models
{
    [DataContract]
    public class ResolvePendingApprovalRequest
    {
        [DataMember(Order = 1)] public string BrokerId { get; set; }
        [DataMember(Order = 2)] public string PendingApprovalId { get; set; }
        [DataMember(Order = 3)] public PendingApprovalUpdatedState State { get; set; }
        [DataMember(Order = 4)] public string Otp { get; set; }
        [DataMember(Order = 5)] public string ResolvedBy { get; set; }
        [DataMember(Order = 6)] public string UpdatedBy { get; set; }
        [DataMember(Order = 7)] public string UpdatedTime { get; set; }
    }
}