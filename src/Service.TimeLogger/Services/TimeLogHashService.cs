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

		private static readonly ConcurrentDictionary<Guid, TimeLogHashRecord> Dictionary;
		private static int _hashLiveTime;

		static TimeLogHashService() => Dictionary = new ConcurrentDictionary<Guid, TimeLogHashRecord>();

		public TimeLogHashService(ISystemClock systemClock) => _systemClock = systemClock;

		public void SetTimeOut(int timeoutMinutes) => _hashLiveTime = timeoutMinutes;

		public void Update(Guid userId, DateTime startDate)
		{
			TimeLogHashRecord record = Dictionary.GetOrAdd(userId, _ => new TimeLogHashRecord(userId, startDate));

			record.EndDateTime = _systemClock.Now;
		}

		public TimeLogHashRecord[] CutExpired()
		{
			KeyValuePair<Guid, TimeLogHashRecord>[] pairs = Dictionary
				.Where(pair => pair.Value.StartDateTime.AddMinutes(_hashLiveTime) < _systemClock.Now)
				.ToArray();

			foreach (KeyValuePair<Guid, TimeLogHashRecord> pair in pairs)
				Dictionary.TryRemove(pair);

			return pairs.Select(pair => pair.Value).ToArray();
		}

		public TimeLogHashRecord[] CutAll()
		{
			TimeLogHashRecord[] pairs = Dictionary.Values.ToArray();

			Dictionary.Clear();

			return pairs;
		}
	}
}