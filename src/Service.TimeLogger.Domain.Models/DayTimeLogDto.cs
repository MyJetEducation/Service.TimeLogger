using System;

namespace Service.TimeLogger.Domain.Models
{
	public class DayTimeLogDto
	{
		public DayTimeLogDto()
		{
			Date = DateTime.UtcNow;
			Value = TimeSpan.Zero;
		}

		public DateTime Date { get; set; }

		public TimeSpan Value { get; set; }
	}
}