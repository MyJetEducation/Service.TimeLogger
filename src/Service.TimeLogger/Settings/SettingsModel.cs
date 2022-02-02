using MyJetWallet.Sdk.Service;
using MyYamlParser;

namespace Service.TimeLogger.Settings
{
	public class SettingsModel
	{
		[YamlProperty("TimeLogger.SeqServiceUrl")]
		public string SeqServiceUrl { get; set; }

		[YamlProperty("TimeLogger.ZipkinUrl")]
		public string ZipkinUrl { get; set; }

		[YamlProperty("TimeLogger.ElkLogs")]
		public LogElkSettings ElkLogs { get; set; }

		[YamlProperty("TimeLogger.ServerKeyValueServiceUrl")]
		public string ServerKeyValueServiceUrl { get; set; }

		[YamlProperty("TimeLogger.KeyUserTime")]
		public string KeyUserTime { get; set; }

		[YamlProperty("TimeLogger.KeyUserDayTime")]
		public string KeyUserDayTime { get; set; }

		[YamlProperty("TimeLogger.CheckHasDurationMinutes")]
		public int CheckHasDurationMinutes { get; set; }

		[YamlProperty("TimeLogger.HashExpiresMinutes")]
		public int HashExpiresMinutes { get; set; }
	}
}