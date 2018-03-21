

using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson;
using OrderCaptureAPI.Services;
using System;

namespace OrderCaptureAPI.Models
{
    public class Order
    {
        #region Properties
        /// <summary>
        /// OrderId
        /// </summary>
        [BsonElement("orderid")]
        public string OrderId {get;set;}

        /// <summary>
        /// Email address of the customer
        /// </summary>
        [BsonElement("emailaddress")]
        [BsonRequired]
        public string EmailAddress {get;set;}

        /// <summary>
        /// Preferred Language of the customer
        /// </summary>
        [BsonElement("preferredlanguage")]
        public string PreferredLanguage {get;set;}

        /// <summary>
        /// Product ordered by the customer
        /// </summary>
        [BsonElement("product")]
        public string Product {get;set;}

        /// <summary>
        /// Order total
        /// </summary>
        [BsonElement("total")]
        public double Total {get;set;}

        /// <summary>
        /// Source backend e.g. App Service, Container instance, K8 cluster etc
        /// </summary>
        [BsonElement("source")]
        public string Source {get;set;}

        /// <summary>
        /// Order Status
        /// </summary>
        [BsonElement("status")]
        [BsonRequired]
        public string Status {get;set;}
        #endregion
    }
}