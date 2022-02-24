using System;
using System.Threading.Tasks;
using Service.ServerKeyValue.Grpc.Models;
using Service.TimeLogger.Domain.Models;

namespace Service.TimeLogger.Services
{
	public interface IDtoRepository
	{
		ValueTask<ItemsGrpcResponse> GetTime(Guid? userId);

		Task SetTime(Guid? userId, TimeLogDto timeDto, DayTimeLogDto dayTimeDto);
	}
}