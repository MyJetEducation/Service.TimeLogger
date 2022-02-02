using System;

namespace Service.TimeLogger.Models
{
	public class TimeLogHashRecord
	{
		public TimeLogHashRecord(Guid userId, DateTime startDateTime)
		{
			UserId = userId;
			StartDateTime = startDateTime;
		}

		public Guid UserId { get; set; }

		public DateTime StartDateTime { get; set; }

		public DateTime EndDateTime { get; set; }

		public TimeSpan GetValue() => EndDateTime.Subtract(StartDateTime);

		public TimeSpan GetTodayValue() => EndDateTime.Subtract(StartDateTime.Date < EndDateTime.Date ? EndDateTime.Date : StartDateTime);
	}
}