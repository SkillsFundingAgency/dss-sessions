using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NCS.DSS.Sessions.Models;

namespace NCS.DSS.Sessions.GetSessionByIdHttpTrigger
{
    class GetSessionByIdHttpTriggerService
    {
        public async Task<List<Models.Session>> GetSessions(Guid sessionId)
        {
            var result = GenerateSampleData();
            result.FirstOrDefault(m => m.SessionId == sessionId);
            return await Task.FromResult(result);
        }

        private List<Models.Session> GenerateSampleData()
        {
            var cList = new List<Models.Session>();

            cList.Add(new Models.Session { SessionId = Guid.NewGuid(), DateandTimeOfSession = DateTime.Now.AddMonths(-6), LastModifiedTouchpointId = Guid.NewGuid(), VenuePostCode = "AAAA AAA" });
            cList.Add(new Models.Session { SessionId = Guid.NewGuid(), DateandTimeOfSession = DateTime.Now.AddMonths(-6), LastModifiedTouchpointId = Guid.NewGuid(), VenuePostCode = "BBBB BBB" });
            cList.Add(new Models.Session { SessionId = Guid.NewGuid(), DateandTimeOfSession = DateTime.Now.AddMonths(-6), LastModifiedTouchpointId = Guid.NewGuid(), VenuePostCode = "CCCC CCC" });
            cList.Add(new Models.Session { SessionId = Guid.NewGuid(), DateandTimeOfSession = DateTime.Now.AddMonths(-6), LastModifiedTouchpointId = Guid.NewGuid(), VenuePostCode = "DDDD DDD" });
            cList.Add(new Models.Session { SessionId = Guid.NewGuid(), DateandTimeOfSession = DateTime.Now.AddMonths(-6), LastModifiedTouchpointId = Guid.NewGuid(), VenuePostCode = "EEEE EEE" });

            return cList;
        }


    }
}
