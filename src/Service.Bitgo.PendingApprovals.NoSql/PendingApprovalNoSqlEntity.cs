using MyNoSqlServer.Abstractions;
using Service.Bitgo.PendingApprovals.Domain.Models;

namespace Service.Bitgo.PendingApprovals.NoSql
{
    public class PendingApprovalNoSqlEntity : MyNoSqlDbEntity
    {
        public const string TableName = "myjetwallet-bitgo-wallet-pending-approvals";

        public static string GeneratePartitionKey(string brokerId) => $"{brokerId}";
        public static string GenerateRowKey(string id) => $"{id}";
        
        public PendingApproval PendingApproval { get; set; }

        public static PendingApprovalNoSqlEntity Create(PendingApproval approval)
        {
            return new PendingApprovalNoSqlEntity()
            {
                PartitionKey = GeneratePartitionKey(approval.BrokerId),
                RowKey = GenerateRowKey(approval.Id),
                PendingApproval = approval
            };
        }
    }
}