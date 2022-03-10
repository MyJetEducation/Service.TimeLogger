using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Service.Core.Client.Extensions;
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
		private readonly ILogger<TimeLoggerService> _logger;
		private readonly ITimeLogHashService _timeLogHashService;
		private readonly IDtoRepository _dtoRepository;

		public TimeLoggerService(ILogger<TimeLoggerService> logger, ITimeLogHashService timeLogHashService, IDtoRepository dtoRepository)
		{
			_logger = logger;
			_timeLogHashService = timeLogHashService;
			_dtoRepository = dtoRepository;
		}

		public async void ProcessRequests(TimeLogGrpcRequest[] requests)
		{
			foreach (TimeLogGrpcRequest request in requests)
				_timeLogHashService.UpdateNew(request.UserId, request.StartDate);

			await SaveTimeValues();
		}

		public async ValueTask<ServiceTimeResponse> GetUserTime(GetServiceTimeRequest request)
		{
			Guid? userId = request.UserId;
			
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
				Guid? userId = info.UserId;

				ItemsGrpcResponse response = await _dtoRepository.GetTime(userId);
				if (response == null)
					continue;

				KeyValueGrpcModel[] existingGrpcModels = response.Items;
				var timeDto = GetDto<TimeLogDto>(Program.ReloadedSettings(model => model.KeyUserTime), existingGrpcModels);
				var dayTimeDto = GetDto<DayTimeLogDto>(Program.ReloadedSettings(model => model.KeyUserDayTime), existingGrpcModels);

				UpdateTimeDto(timeDto, info);
				UpdateDayTimeDto(dayTimeDto, info);

				_logger.LogDebug($"Update timeDto: {JsonConvert.SerializeObject(timeDto)}, dayTimeDto: {JsonConvert.SerializeObject(dayTimeDto)}");

				await _dtoRepository.SetTime(userId, timeDto, dayTimeDto);
			}
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