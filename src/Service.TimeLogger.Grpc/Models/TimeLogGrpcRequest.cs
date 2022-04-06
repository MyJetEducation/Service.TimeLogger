using System;
using System.Runtime.Serialization;

namespace Service.TimeLogger.Grpc.Models
{
    [DataContract]
    public class TimeLogGrpcRequest
    {
        [DataMember(Order = 1)]
        public string UserId { get; set; }

        [DataMember(Order = 2)]
        public DateTime StartDate { get; set; }
    }
}
