using Autofac;
using Service.Bitgo.PendingApprovals.Grpc;

// ReSharper disable UnusedMember.Global

namespace Service.Bitgo.PendingApprovals.Client
{
    public static class AutofacHelper
    {
        public static void RegisterBitgoPendingApprovalsClient(this ContainerBuilder builder, string grpcServiceUrl)
        {
            var factory = new BitgoPendingApprovalsClientFactory(grpcServiceUrl);
            builder.RegisterInstance(factory.GetPendingApprovalsManageService).As<IPendingApprovalsManageService>().SingleInstance();
        }
    }
}
