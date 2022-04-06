using System;
using System.Runtime.Serialization;
using Service.Education.Structure;

namespace Service.TimeLogger.Grpc.Models
{
	[DataContract]
	public class TaskTimeLogGrpcRequest
	{
		[DataMember(Order = 1)]
		public string UserId { get; set; }

		[DataMember(Order = 2)]
		public DateTime StartDate { get; set; }

		[DataMember(Order = 3)]
		public EducationTutorial Tutorial { get; set; }

		[DataMember(Order = 4)]
		public int Unit { get; set; }

		[DataMember(Order = 5)]
		public int Task { get; set; }
	}
}