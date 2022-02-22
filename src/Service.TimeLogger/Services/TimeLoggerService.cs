using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Service.Core.Client.Extensions;
using Service.Core.Client.Models;
using Service.Grpc;
using Service.ServerKeyValue.Grpc;
using Service.ServerKeyValue.Grpc.Models;
using Service.TimeLogger.Domain.Models;
using Service.TimeLogger.Grpc;
using Service.TimeLogger.Grpc.Models;
using Service.TimeLogger.Models;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Service.TimeLogger.Services
{
	public class TimeLoggerService : ITimeLoggerService
	{
		private static Func<string> KeyUserTime => Program.ReloadedSettings(model => model.KeyUserTime);
		private static Func<string> KeyUserDayTime => Program.ReloadedSettings(model => model.KeyUserDayTime);

		private readonly ILogger<TimeLoggerService> _logger;
		private readonly IGrpcServiceProxy<IServerKeyValueService> _serverKeyValueService;
		private readonly ITimeLogHashService _timeLogHashService;

		public TimeLoggerService(ILogger<TimeLoggerService> logger, IGrpcServiceProxy<IServerKeyValueService> serverKeyValueService, ITimeLogHashService timeLogHashService)
		{
			_logger = logger;
			_serverKeyValueService = serverKeyValueService;
			_timeLogHashService = timeLogHashService;
		}

		public async void ProcessRequests(TimeLogGrpcRequest[] requests)
		{
			foreach (TimeLogGrpcRequest request in requests)
				_timeLogHashService.UpdateNew(request.UserId, request.StartDate);

			await SaveTimeValues();
		}

		private async Task SaveTimeValues()
		{
			TimeLogHashRecord[] hashRecords = _timeLogHashService.GetExpired();
			if (hashRecords.IsNullOrEmpty())
				return;

			_logger.LogDebug("Retrieved {count} expired items, processing...", hashRecords.Length);

			foreach (TimeLogHashRecord info in hashRecords)
			{
				Guid? userId = info.UserId;

				ItemsGrpcResponse response = await GetData(userId);
				if (response == null)
					continue;

				KeyValueGrpcModel[] existingGrpcModels = response.Items;
				var timeDto = GetDto<TimeLogDto>(KeyUserTime, existingGrpcModels);
				var dayTimeDto = GetDto<DayTimeLogDto>(KeyUserDayTime, existingGrpcModels);

				UpdateTimeDto(timeDto, info);
				UpdateDayTimeDto(dayTimeDto, info);

				_logger.LogDebug($"Update timeDto: {JsonConvert.SerializeObject(timeDto)}, dayTimeDto: {JsonConvert.SerializeObject(dayTimeDto)}");

				await SetData(userId, timeDto, dayTimeDto);
			}
		}

		private static void UpdateTimeDto(TimeLogDto dayTimeDto, TimeLogHashRecord hashRecord) => dayTimeDto.Value += hashRecord.GetValue();

		private static void UpdateDayTimeDto(DayTimeLogDto dayTimeDto, TimeLogHashRecord hashRecord)
		{
			if (dayTimeDto.Date != hashRecord.EndDateTime.Date)
			{
				dayTimeDto.Date = hashRecord.EndDateTime.Date;
				dayTimeDto.Value = TimeSpan.Zero;
			}

			dayTimeDto.Value += hashRecord.GetTodayValue();
		}

		private static TDto GetDto<TDto>(Func<string> keyFunc, IEnumerable<KeyValueGrpcModel> grpcModels) where TDto : new()
		{
			string value = grpcModels?.FirstOrDefault(model => model.Key == keyFunc.Invoke())?.Value;

			if (value != null)
			{
				var dto = JsonSerializer.Deserialize<TDto>(value);
				if (dto != null)
					return dto;
			}

			return new TDto();
		}

		private async Task<ItemsGrpcResponse> GetData(Guid? userId)
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

		private async Task SetData(Guid? userId, TimeLogDto timeDto, DayTimeLogDto dayTimeDto)
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