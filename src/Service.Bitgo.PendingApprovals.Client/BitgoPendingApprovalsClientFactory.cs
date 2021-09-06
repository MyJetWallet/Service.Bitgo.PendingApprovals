using JetBrains.Annotations;
using MyJetWallet.Sdk.Grpc;
using Service.Bitgo.PendingApprovals.Grpc;

namespace Service.Bitgo.PendingApprovals.Client
{
    [UsedImplicitly]
    public class BitgoPendingApprovalsClientFactory : MyGrpcClientFactory
    {
        public BitgoPendingApprovalsClientFactory(string grpcServiceUrl) : base(grpcServiceUrl)
        {
        }

        public IPendingApprovalsManageService GetPendingApprovalsManageService =>
            CreateGrpcService<IPendingApprovalsManageService>();
    }
}