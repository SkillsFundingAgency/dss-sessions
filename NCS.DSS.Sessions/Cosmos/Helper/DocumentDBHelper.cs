﻿
using System;
using System.Configuration;
using Microsoft.Azure.Documents.Client;

namespace NCS.DSS.Sessions.Cosmos.Helper
{
    public class DocumentDBHelper : IDocumentDBHelper
    {
        private Uri _documentCollectionUri;
        private Uri _documentUri;

        private readonly string _databaseId = ConfigurationManager.AppSettings["DatabaseId"];
        private readonly string _collectionId = ConfigurationManager.AppSettings["CollectionId"];

        private readonly string _customerDatabaseId = ConfigurationManager.AppSettings["CustomerDatabaseId"];
        private readonly string _customerCollectionId = ConfigurationManager.AppSettings["CustomerCollectionId"];
        private Uri _customerDocumentCollectionUri;

        public Uri CreateDocumentCollectionUri()
        {
            if (_documentCollectionUri != null)
                return _documentCollectionUri;

            _documentCollectionUri = UriFactory.CreateDocumentCollectionUri(
                _databaseId,
                _collectionId);

            return _documentCollectionUri;
        }
        
        public Uri CreateDocumentUri(Guid sessionId)
        {
            if (_documentUri != null)
                return _documentUri;

            _documentUri = UriFactory.CreateDocumentUri(_databaseId, _collectionId, sessionId.ToString());

            return _documentUri;

        }

        #region CustomerDB

        public Uri CreateCustomerDocumentCollectionUri()
        {
            if (_customerDocumentCollectionUri != null)
                return _customerDocumentCollectionUri;

            _customerDocumentCollectionUri = UriFactory.CreateDocumentCollectionUri(
                _customerDatabaseId, _customerCollectionId);

            return _customerDocumentCollectionUri;
        }

        #endregion   
    }
}
