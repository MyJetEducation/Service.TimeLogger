using Autofac;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.ServiceBus;
using MyServiceBus.TcpClient;
using Service.Core.Client.Services;
using Service.ServerKeyValue.Client;
using Service.ServiceBus.Models;
using Service.TimeLogger.Services;

namespace Service.TimeLogger.Modules
{
	public class ServiceModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterServerKeyValueClient(Program.Settings.ServerKeyValueServiceUrl, Program.LogFactory.CreateLogger(typeof (ServerKeyValueClientFactory)));

			builder.RegisterType<SystemClock>().AsImplementedInterfaces().SingleInstance();
			builder.RegisterType<TimeLogHashService>().AsImplementedInterfaces().SingleInstance();
			builder.RegisterType<DtoRepository>().AsImplementedInterfaces().SingleInstance();

			var tcpServiceBus = new MyServiceBusTcpClient(() => Program.Settings.ServiceBusWriter, "MyJetEducation Service.TimeLogger");

			builder
				.Register(_ => new MyServiceBusPublisher<UserTimeChangedServiceBusModel>(tcpServiceBus, UserTimeChangedServiceBusModel.TopicName, false))
				.As<IServiceBusPublisher<UserTimeChangedServiceBusModel>>()
				.SingleInstance();

			tcpServiceBus.Start();
		}
	}
}