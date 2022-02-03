using System;
using Service.TimeLogger.Models;

namespace Service.TimeLogger.Services
{
	public interface ITimeLogHashService
	{
		void UpdateNew(Guid userId, DateTime startDate);

		TimeLogHashRecord[] GetExpired();
	}
}