using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Service.Core.Client.Services;
using Service.TimeLogger.Models;

namespace Service.TimeLogger.Services
{
	public class TimeLogHashService : ITimeLogHashService
	{
		private readonly ISystemClock _systemClock;

		private static readonly ConcurrentDictionary<string, TimeLogHashRecord> Dictionary;

		static TimeLogHashService() => Dictionary = new ConcurrentDictionary<string, TimeLogHashRecord>();

		public TimeLogHashService(ISystemClock systemClock) => _systemClock = systemClock;

		public void UpdateNew(string userId, DateTime startDate)
		{
			DateTime endDateTime = _systemClock.Now;

			TimeLogHashRecord record = Dictionary.GetOrAdd(userId, _ => new TimeLogHashRecord(userId, endDateTime));

			record.EndDateTime = endDateTime;
		}

		public TimeLogHashRecord[] GetExpired()
		{
			int expiredMinutes = Program.ReloadedSettings(model => model.HashExpiresMinutes).Invoke();

			KeyValuePair<string, TimeLogHashRecord>[] expiredPairs = Dictionary
				.Where(pair => pair.Value.StartDateTime.AddMinutes(expiredMinutes) < _systemClock.Now)
				.ToArray();

			foreach (KeyValuePair<string, TimeLogHashRecord> pair in expiredPairs)
				Dictionary.TryRemove(pair);

			return expiredPairs.Select(pair => pair.Value).ToArray();
		}
	}
}