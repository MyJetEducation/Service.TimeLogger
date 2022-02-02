using System.Runtime.Serialization;

namespace Service.TimeLogger.Grpc.Models
{
	[DataContract]
	public class LogTimeGrpcResponse
	{
		[DataMember(Order = 1)]
		public bool Expired { get; set; }
	}
}