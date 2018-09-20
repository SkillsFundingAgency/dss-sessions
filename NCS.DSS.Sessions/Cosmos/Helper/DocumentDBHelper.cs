
using System;
using System.Configuration;
using Microsoft.Azure.Documents.Client;

namespace NCS.DSS.Sessions.Cosmos.Helper
{
    public static class DocumentDBHelper
    {
        private static Uri _documentCollectionUri;
        private static readonly string DatabaseId = ConfigurationManager.AppSettings["DatabaseId"];
        private static readonly string CollectionId = ConfigurationManager.AppSettings["CollectionId"];

        private static Uri _customerDocumentCollectionUri;
        private static readonly string CustomerDatabaseId = ConfigurationManager.AppSettings["CustomerDatabaseId"];
        private static readonly string CustomerCollectionId = ConfigurationManager.AppSettings["CustomerCollectionId"];

        private static Uri _interactionDocumentCollectionUri;
        private static readonly string InteractionDatabaseId = ConfigurationManager.AppSettings["InteractionDatabaseId"];
        private static readonly string InteractionCollectionId = ConfigurationManager.AppSettings["InteractionCollectionId"];


        public static Uri CreateDocumentCollectionUri()
        {
            if (_documentCollectionUri != null)
                return _documentCollectionUri;

            _documentCollectionUri = UriFactory.CreateDocumentCollectionUri(
                DatabaseId,
                CollectionId);

            return _documentCollectionUri;
        }
        
        public static Uri CreateDocumentUri(Guid sessionId)
        {
            return UriFactory.CreateDocumentUri(DatabaseId, CollectionId, sessionId.ToString());
        }

        #region CustomerDB

        public static Uri CreateCustomerDocumentCollectionUri()
        {
            if (_customerDocumentCollectionUri != null)
                return _customerDocumentCollectionUri;

            _customerDocumentCollectionUri = UriFactory.CreateDocumentCollectionUri(
                CustomerDatabaseId, CustomerCollectionId);

            return _customerDocumentCollectionUri;
        }

        public static Uri CreateCustomerDocumentUri(Guid customerId)
        {
            return UriFactory.CreateDocumentUri(CustomerDatabaseId, CustomerCollectionId, customerId.ToString());
        }

        #endregion

        #region InteractionDB

        public static Uri CreateInteractionDocumentCollectionUri()
        {
            if (_interactionDocumentCollectionUri != null)
                return _interactionDocumentCollectionUri;

            _interactionDocumentCollectionUri = UriFactory.CreateDocumentCollectionUri(
                InteractionDatabaseId, InteractionCollectionId);

            return _interactionDocumentCollectionUri;
        }

        public static Uri CreateInteractionDocumentUri(Guid interactionId)
        {
            return UriFactory.CreateDocumentUri(InteractionDatabaseId, InteractionCollectionId, interactionId.ToString()); ;
        }

        #endregion   

    }
}
