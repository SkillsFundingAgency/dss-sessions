﻿using System;
using System.Net;
using System.Threading.Tasks;
using NCS.DSS.Sessions.Cosmos.Provider;
using NCS.DSS.Sessions.Models;
using NCS.DSS.Sessions.ServiceBus;

namespace NCS.DSS.Sessions.PostSessionHttpTrigger.Service
{
    public class PostSessionHttpTriggerService : IPostSessionHttpTriggerService
    {
        public async Task<Session> CreateAsync(Session session)
        {
            if (session == null)
                return null;

            session.SetDefaultValues();

            var documentDbProvider = new DocumentDBProvider();

            var response = await documentDbProvider.CreateSessionAsync(session);

            return response.StatusCode == HttpStatusCode.Created ? (dynamic)response.Resource : null;
        }

        public async Task SendToServiceBusQueueAsync(Session session, string reqUrl)
        {
            await ServiceBusClient.SendPostMessageAsync(session, reqUrl);
        }
    }
}
