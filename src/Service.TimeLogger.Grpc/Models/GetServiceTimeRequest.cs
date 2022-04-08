using System.Runtime.Serialization;

namespace Service.TimeLogger.Grpc.Models
{
	[DataContract]
	public class GetServiceTimeRequest
	{
		[DataMember(Order = 1)]
		public string UserId { get; set; }
	}
}