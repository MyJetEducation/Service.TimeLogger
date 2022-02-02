using Autofac;
using Service.Core.Client.Services;
using Service.ServerKeyValue.Client;

namespace Service.TimeLogger.Modules
{
	public class ServiceModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterKeyValueClient(Program.Settings.ServerKeyValueServiceUrl);
			builder.RegisterType<SystemClock>().AsImplementedInterfaces().SingleInstance();
		}
	}
}