using System;
using System.Runtime.Serialization;

namespace Service.TimeLogger.Grpc.Models
{
	[DataContract]
	public class ServiceTimeResponse
	{
		[DataMember(Order = 1)]
		public TimeSpan? Interval { get; set; }
	}
}