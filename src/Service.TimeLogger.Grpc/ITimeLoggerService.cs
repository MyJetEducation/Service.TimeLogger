using System.ServiceModel;
using System.Threading.Tasks;
using Service.TimeLogger.Grpc.Models;

namespace Service.TimeLogger.Grpc
{
	[ServiceContract]
	public interface ITimeLoggerService
	{
		[OperationContract]
		void ProcessRequests(TimeLogGrpcRequest[] requests);

		[OperationContract]
		ValueTask<ServiceTimeResponse> GetUserTime(GetServiceTimeRequest request);
	}
}