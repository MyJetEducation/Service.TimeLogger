using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service.Tools;
using Service.Core.Client.Models;
using Service.ServerKeyValue.Grpc;
using Service.ServerKeyValue.Grpc.Models;
using Service.TimeLogger.Domain.Models;
using Service.TimeLogger.Grpc;
using Service.TimeLogger.Grpc.Models;
using Service.TimeLogger.Models;

namespace Service.TimeLogger.Services
{
	public class TimeLoggerService : ITimeLoggerService, IDisposable
	{
		private static Func<string> KeyUserTime => Program.ReloadedSettings(model => model.KeyUserTime);
		private static Func<string> KeyUserDayTime => Program.ReloadedSettings(model => model.KeyUserDayTime);

		private readonly ILogger<TimeLoggerService> _logger;
		private readonly IServerKeyValueService _serverKeyValueService;
		private readonly ITimeLogHashService _timeLogHashService;
		private readonly MyTaskTimer _timer;

		public TimeLoggerService(ILogger<TimeLoggerService> logger, IServerKeyValueService serverKeyValueService, ITimeLogHashService timeLogHashService)
		{
			_logger = logger;
			_serverKeyValueService = serverKeyValueService;
			_timeLogHashService = timeLogHashService;
			_timer = new MyTaskTimer(typeof (TimeLoggerService), GetDuration(), logger, TimerAction);
			_timer.Start();
		}

		private async Task TimerAction()
		{
			TimeLogHashRecord[] hashRecords = _timeLogHashService.CutExpired();

			await SaveTimeValues(hashRecords);

			_timer.ChangeInterval(GetDuration());
		}

		private async Task SaveTimeValues(IEnumerable<TimeLogHashRecord> hashRecords)
		{
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
			string value = grpcModels.FirstOrDefault(model => model.Key == keyFunc.Invoke())?.Value;
			if (value == null)
				return new TDto();

			return JsonSerializer.Deserialize<TDto>(value) ?? new TDto();
		}

		private async Task<ItemsGrpcResponse> GetData(Guid? userId)
		{
			ItemsGrpcResponse response = await _serverKeyValueService.Get(new ItemsGetGrpcRequest
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
			CommonGrpcResponse response = await _serverKeyValueService.Put(new ItemsPutGrpcRequest
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
			});

			if (response?.IsSuccess != true)
				_logger.LogError("Can't set server key values for user {user}", userId);
		}

		public void ProcessRequests(TimeLogGrpcRequest[] requests)
		{
			foreach (TimeLogGrpcRequest request in requests)
				_timeLogHashService.Update(request.UserId, request.StartDate);
		}

		public async void Dispose()
		{
			_timer?.Dispose();

			TimeLogHashRecord[] hashRecords = _timeLogHashService.CutAll();

			await SaveTimeValues(hashRecords);
		}

		private static TimeSpan GetDuration() => TimeSpan.FromSeconds(Program.ReloadedSettings(model => model.CheckHasDurationMinutes).Invoke());
	}
}