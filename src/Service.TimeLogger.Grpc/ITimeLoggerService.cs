using System.ServiceModel;
using Service.TimeLogger.Grpc.Models;

namespace Service.TimeLogger.Grpc
{
	[ServiceContract]
	public interface ITimeLoggerService
	{
		[OperationContract]
		void ProcessRequests(TimeLogGrpcRequest[] requests);
	}
}