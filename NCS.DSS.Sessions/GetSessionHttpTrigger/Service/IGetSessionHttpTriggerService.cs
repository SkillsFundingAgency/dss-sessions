using NCS.DSS.Sessions.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NCS.DSS.Sessions.GetSessionHttpTrigger.Service
{
    public interface IGetSessionHttpTriggerService
    {
        Task<List<Session>> GetSessionsAsync(Guid customerId);
    }
}