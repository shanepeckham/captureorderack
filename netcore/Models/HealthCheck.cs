

using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson;
using OrderCaptureAPI.Services;
using System;

namespace OrderCaptureAPI.Models
{
    public class HealthCheck
    {
        #region Properties
        /// <summary>
        /// Database
        /// </summary>
        public string Database {get;set;}

        /// <summary>
        /// IsDatabaseHealthy
        /// </summary>
        /// <returns></returns>
        public bool IsDatabaseHealthy { get; set; }

        /// <summary>
        /// DatabaseError
        /// </summary>
        /// <returns></returns>
        public string DatabaseError { get; set; }

        /// <summary>
        /// MessageQueue
        /// </summary>
        public string MessageQueue {get;set;}

        /// <summary>
        /// IsMessageQueueHealthy
        /// </summary>
        /// <returns></returns>
        public bool IsMessageQueueHealthy { get; set; }

        /// <summary>
        /// MessageQueueError
        /// </summary>
        public string MessageQueueError {get;set;}

        #endregion
    }
}