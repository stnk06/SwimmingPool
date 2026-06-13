using System.Collections.Generic;
using SwimmingPool.Models;

namespace SwimmingPool.Services
{
    public interface ISickLeaveRepository
    {
        void AddSickLeave(SickLeave sickLeave);
        IEnumerable<SickLeave> GetSickLeavesByClient(int clientId);
    }
}