using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ProtoBuf.Grpc.Client;
using Service.Bitgo.PendingApprovals.Client;
using Service.BitGo.SignTransaction.Grpc.Models;

namespace TestApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            GrpcClientFactory.AllowUnencryptedHttp2 = true;


            var factory = new BitgoPendingApprovalsClientFactory("http://localhost:99");
            var client = factory.GetPendingApprovalsManageService;
            var approvals = await client.GetPendingApprovals();
            Console.WriteLine(JsonConvert.SerializeObject(approvals));
            var approval = await client.GetPendingApproval(new GetPendingApprovalRequest() {BrokerId = "jetwallet", PendingApprovalId = "61321dd4f9cf330006362c31f8cb076b"
            });
            Console.WriteLine(JsonConvert.SerializeObject(approval));
            //
            // var resp = await  client.SayHelloAsync(new HelloRequest(){Name = "Alex"});
            // Console.WriteLine(resp?.Message);

            Console.WriteLine("End");
            Console.ReadLine();
        }
    }
}