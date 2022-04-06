using System.Threading.Tasks;
using Service.ServerKeyValue.Grpc.Models;
using Service.TimeLogger.Domain.Models;

namespace Service.TimeLogger.Services
{
	public interface IDtoRepository
	{
		ValueTask<ItemsGrpcResponse> GetTime(string userId);

		Task SetTime(string userId, TimeLogDto timeDto, DayTimeLogDto dayTimeDto);
	}
}