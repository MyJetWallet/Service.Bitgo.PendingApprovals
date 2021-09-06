using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Service.Bitgo.PendingApprovals.Domain.Models
{
    [DataContract]
    public class PendingApproval
    {
        [DataMember(Order = 1)] public string BrokerId { get; set; }
        [DataMember(Order = 2)] public string Id { get; set; }
        [DataMember(Order = 3)] public DateTime CreatedDate { get; set; }
        [DataMember(Order = 4)] public string Asset { get; set; }
        [DataMember(Order = 5)] public Double Amount { get; set; }
        [DataMember(Order = 6)] public string DestinationAddress { get; set; }
        [DataMember(Order = 7)] public string CreatedBy { get; set; }
        [DataMember(Order = 8)] public string OperationId { get; set; }
        [DataMember(Order = 9)] public int ApprovalsCount { get; set; }
        [DataMember(Order = 10)] public List<string> Approvers { get; set; }
        [DataMember(Order = 11)] public List<string> ApprovedBy { get; set; }
        [DataMember(Order = 12)] public string State { get; set; }
    }
}