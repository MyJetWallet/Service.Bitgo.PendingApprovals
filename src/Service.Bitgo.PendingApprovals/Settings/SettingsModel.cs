using MyJetWallet.Sdk.Service;
using MyYamlParser;

namespace Service.Bitgo.PendingApprovals.Settings
{
    public class SettingsModel
    {
        [YamlProperty("BitgoPendingApprovals.SeqServiceUrl")]
        public string SeqServiceUrl { get; set; }

        [YamlProperty("BitgoPendingApprovals.ZipkinUrl")]
        public string ZipkinUrl { get; set; }

        [YamlProperty("BitgoPendingApprovals.ElkLogs")]
        public LogElkSettings ElkLogs { get; set; }

        [YamlProperty("BitgoPendingApprovals.SpotServiceBusHostPort")]
        public string SpotServiceBusHostPort { get; set; }

        [YamlProperty("BitgoPendingApprovals.MyNoSqlWriterUrl")]
        public string MyNoSqlWriterUrl { get; set; }

        [YamlProperty("BitgoPendingApprovals.MyNoSqlReaderHostPort")]
        public string MyNoSqlReaderHostPort { get; set; }

        [YamlProperty("BitgoPendingApprovals.BitgoSignTransactionGrpcServiceUrl")]
        public string BitgoSignTransactionGrpcServiceUrl { get; set; }
    }
}
