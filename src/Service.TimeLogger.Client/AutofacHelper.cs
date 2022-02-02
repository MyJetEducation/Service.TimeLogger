using Autofac;
using Service.TimeLogger.Grpc;

// ReSharper disable UnusedMember.Global

namespace Service.TimeLogger.Client
{
    public static class AutofacHelper
    {
        public static void RegisterTimeLoggerClient(this ContainerBuilder builder, string grpcServiceUrl)
        {
            var factory = new TimeLoggerClientFactory(grpcServiceUrl);

            builder.RegisterInstance(factory.GetTimeLoggerService()).As<ITimeLoggerService>().SingleInstance();
        }
    }
}
