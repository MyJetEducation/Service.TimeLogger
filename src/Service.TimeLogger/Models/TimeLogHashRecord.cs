using System;

namespace Service.TimeLogger.Models
{
	public class TimeLogHashRecord
	{
		public TimeLogHashRecord(string userId, DateTime startDateTime)
		{
			UserId = userId;
			StartDateTime = startDateTime;
		}

		public string UserId { get; set; }

		public DateTime StartDateTime { get; set; }

		public DateTime EndDateTime { get; set; }

		public TimeSpan GetValue() => EndDateTime.Subtract(StartDateTime);

		public TimeSpan GetTodayValue() => EndDateTime.Subtract(StartDateTime.Date < EndDateTime.Date ? EndDateTime.Date : StartDateTime);
	}
}