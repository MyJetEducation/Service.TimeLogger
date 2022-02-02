using System;

namespace Service.TimeLogger.Domain.Models
{
	public class TimeLogDto
	{
		public TimeLogDto() => Value = TimeSpan.Zero;

		public TimeSpan Value { get; set; }
	}
}