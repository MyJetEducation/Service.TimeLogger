using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.ServiceBus;
using Service.Core.Client.Extensions;
using Service.ServerKeyValue.Grpc.Models;
using Service.ServiceBus.Models;
using Service.TimeLogger.Domain.Models;
using Service.TimeLogger.Grpc;
using Service.TimeLogger.Grpc.Models;
using Service.TimeLogger.Models;

namespace Service.TimeLogger.Services
{
	public class TimeLoggerService : ITimeLoggerService
	{
		private readonly ILogger<TimeLoggerService> _logger;
		private readonly ITimeLogHashService _timeLogHashService;
		private readonly IDtoRepository _dtoRepository;
		private readonly IServiceBusPublisher<UserTimeChangedServiceBusModel> _publisher;

		public TimeLoggerService(ILogger<TimeLoggerService> logger,
			ITimeLogHashService timeLogHashService,
			IDtoRepository dtoRepository,
			IServiceBusPublisher<UserTimeChangedServiceBusModel> publisher)
		{
			_logger = logger;
			_timeLogHashService = timeLogHashService;
			_dtoRepository = dtoRepository;
			_publisher = publisher;
		}

		public async void ProcessRequests(TimeLogGrpcRequest[] requests)
		{
			foreach (TimeLogGrpcRequest request in requests)
				_timeLogHashService.UpdateNew(request.UserId, request.StartDate);

			await SaveTimeValues();
		}

		public async ValueTask<ServiceTimeResponse> GetUserTime(GetServiceTimeRequest request)
		{
			string userId = request.UserId;

			var result = new ServiceTimeResponse();

			ItemsGrpcResponse response = await _dtoRepository.GetTime(userId);
			if (response != null)
			{
				KeyValueGrpcModel[] existingGrpcModels = response.Items;

				var timeDto = GetDto<TimeLogDto>(Program.ReloadedSettings(model => model.KeyUserTime), existingGrpcModels);

				result.Interval = timeDto?.Value;
			}

			return result;
		}

		private async Task SaveTimeValues()
		{
			TimeLogHashRecord[] hashRecords = _timeLogHashService.GetExpired();
			if (hashRecords.IsNullOrEmpty())
				return;

			_logger.LogDebug("Retrieved {count} expired items, processing...", hashRecords.Length);

			foreach (TimeLogHashRecord info in hashRecords)
			{
				string userId = info.UserId;

				ItemsGrpcResponse response = await _dtoRepository.GetTime(userId);
				if (response == null)
					continue;

				KeyValueGrpcModel[] existingGrpcModels = response.Items;
				var timeDto = GetDto<TimeLogDto>(Program.ReloadedSettings(model => model.KeyUserTime), existingGrpcModels);
				var dayTimeDto = GetDto<DayTimeLogDto>(Program.ReloadedSettings(model => model.KeyUserDayTime), existingGrpcModels);

				UpdateTimeDto(timeDto, info);
				UpdateDayTimeDto(dayTimeDto, info);

				await Save(userId, timeDto, dayTimeDto);
			}
		}

		private async Task Save(string userId, TimeLogDto timeDto, DayTimeLogDto dayTimeDto)
		{
			_logger.LogDebug("Update timeDto: {@dto}, dayTimeDto: {@dayTimeDto}", timeDto, dayTimeDto);

			await _dtoRepository.SetTime(userId, timeDto, dayTimeDto);

			var userTimeServiceBusModel = new UserTimeChangedServiceBusModel
			{
				UserId = userId,
				TotalSpan = timeDto.Value,
				TodaySpan = dayTimeDto.Value
			};

			_logger.LogDebug("Publish user time info to service bus: {@model}", userTimeServiceBusModel);

			await _publisher.PublishAsync(userTimeServiceBusModel);
		}

		private static void UpdateTimeDto(TimeLogDto dayTimeDto, TimeLogHashRecord hashRecord)
		{
			dayTimeDto.Value += hashRecord.GetValue();
		}

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
	}
}