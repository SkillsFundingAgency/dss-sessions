using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.Common.Standard.Logging;
using DFC.Functions.DI.Standard.Attributes;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using NCS.DSS.Sessions.SessionChangeFeedTrigger.Service;

namespace NCS.DSS.Sessions.SessionChangeFeedTrigger.Function
{
    public static class SessionChangeFeedTrigger
    {
        private const string DatabaseName = "%DatabaseId%";
        private const string CollectionName = "%CollectionId%";
        private const string ConnectionString = "SessionConnectionString";
        private const string LeaseCollectionName = "%LeaseCollectionName%";
        private const string LeaseCollectionPrefix = "%LeaseCollectionPrefix%";

        [FunctionName("SessionChangeFeedTrigger")]
        public static async Task Run([CosmosDBTrigger(
            DatabaseName,
            CollectionName,
            ConnectionStringSetting = ConnectionString,
            LeaseCollectionName = LeaseCollectionName,
            LeaseCollectionPrefix = LeaseCollectionPrefix,
            CreateLeaseCollectionIfNotExists = true
            )]IReadOnlyList<Document> documents, ILogger log,
            [Inject]ILoggerHelper loggerHelper,
            [Inject]ISessionChangeFeedTriggerService changeFeedTriggerService)
        {
            loggerHelper.LogMethodEnter(log);
            
            try
            {
                foreach (var document in documents)
                {
                    loggerHelper.LogInformationMessage(log, Guid.NewGuid(), string.Format("Attempting to send document id: {0} to service bus queue", document.Id));
                    await changeFeedTriggerService.SendMessageToChangeFeedQueueAsync(document);
                }
            }
            catch (Exception ex)
            {
                loggerHelper.LogException(log, Guid.NewGuid(), "Error when trying to add message to service bus queue", ex);
            }

            loggerHelper.LogMethodExit(log);
        }
    }
}
