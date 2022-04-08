using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Service.Core.Client.Models;
using Service.Grpc;
using Service.ServerKeyValue.Grpc;
using Service.ServerKeyValue.Grpc.Models;
using Service.TimeLogger.Domain.Models;

namespace Service.TimeLogger.Services
{
	public class DtoRepository : IDtoRepository
	{
		private static Func<string> KeyUserTime => Program.ReloadedSettings(model => model.KeyUserTime);
		private static Func<string> KeyUserDayTime => Program.ReloadedSettings(model => model.KeyUserDayTime);

		private readonly IGrpcServiceProxy<IServerKeyValueService> _serverKeyValueService;
		private readonly ILogger<DtoRepository> _logger;

		public DtoRepository(IGrpcServiceProxy<IServerKeyValueService> serverKeyValueService, ILogger<DtoRepository> logger)
		{
			_serverKeyValueService = serverKeyValueService;
			_logger = logger;
		}

		public async ValueTask<ItemsGrpcResponse> GetTime(string userId)
		{
			ItemsGrpcResponse response = await _serverKeyValueService.Service.Get(new ItemsGetGrpcRequest
			{
				UserId = userId,
				Keys = new[]
				{
					KeyUserTime.Invoke(),
					KeyUserDayTime.Invoke()
				}
			});

			if (response == null)
				_logger.LogError("Can't get server key values for user {user}", userId);

			return response;
		}

		public async Task SetTime(string userId, TimeLogDto timeDto, DayTimeLogDto dayTimeDto)
		{
			CommonGrpcResponse response = await _serverKeyValueService.TryCall(service => service.Put(new ItemsPutGrpcRequest
			{
				UserId = userId,
				Items = new[]
				{
					new KeyValueGrpcModel
					{
						Key = KeyUserTime.Invoke(),
						Value = JsonSerializer.Serialize(timeDto)
					},
					new KeyValueGrpcModel
					{
						Key = KeyUserDayTime.Invoke(),
						Value = JsonSerializer.Serialize(dayTimeDto)
					}
				}
			}));

			if (response?.IsSuccess != true)
				_logger.LogError("Can't set server key values for user {user}", userId);
		}
	}
}