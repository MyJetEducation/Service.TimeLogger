using System;
using Service.TimeLogger.Models;

namespace Service.TimeLogger.Services
{
	public interface ITimeLogHashService
	{
		void SetTimeOut(int timeoutMinutes);

		void Update(Guid userId, DateTime startDate);

		TimeLogHashRecord[] CutExpired();
		
		TimeLogHashRecord[] CutAll();
	}
}