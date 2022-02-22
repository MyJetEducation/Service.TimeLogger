using Autofac;
using Microsoft.Extensions.Logging;
using Service.Core.Client.Services;
using Service.ServerKeyValue.Client;
using Service.TimeLogger.Services;

namespace Service.TimeLogger.Modules
{
	public class ServiceModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterServerKeyValueClient(Program.Settings.ServerKeyValueServiceUrl, Program.LogFactory.CreateLogger(typeof(ServerKeyValueClientFactory)));

			builder.RegisterType<SystemClock>().AsImplementedInterfaces().SingleInstance();
			builder.RegisterType<TimeLogHashService>().AsImplementedInterfaces().SingleInstance();
		}
	}
}