using Autofac;
using Service.Core.Client.Services;
using Service.ServerKeyValue.Client;
using Service.TimeLogger.Services;

namespace Service.TimeLogger.Modules
{
	public class ServiceModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterServerKeyValueClient(Program.Settings.ServerKeyValueServiceUrl);
			builder.RegisterType<SystemClock>().AsImplementedInterfaces().SingleInstance();
			builder.RegisterType<TimeLogHashService>().AsImplementedInterfaces().SingleInstance();
		}
	}
}