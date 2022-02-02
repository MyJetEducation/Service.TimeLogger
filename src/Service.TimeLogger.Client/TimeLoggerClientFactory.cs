using JetBrains.Annotations;
using MyJetWallet.Sdk.Grpc;
using Service.TimeLogger.Grpc;

namespace Service.TimeLogger.Client
{
    [UsedImplicitly]
    public class TimeLoggerClientFactory : MyGrpcClientFactory
    {
        public TimeLoggerClientFactory(string grpcServiceUrl) : base(grpcServiceUrl)
        {
        }

        public ITimeLoggerService GetTimeLoggerService() => CreateGrpcService<ITimeLoggerService>();
    }
}
