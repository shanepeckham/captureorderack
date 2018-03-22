using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrderCaptureAPI.Models;
using OrderCaptureAPI.Util;
using OrderCaptureAPI.Services;
using Microsoft.ApplicationInsights;

namespace OrderCaptureAPI.Controllers
{
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/[controller]")]
    public class OrderController : Controller
    {
        private readonly ILogger _logger;
        private readonly TelemetryClient _telemetryClient;
        private OrderService _orderService;

        public OrderController(ILoggerFactory loggerFactory, TelemetryClient telemetryClient)
        {
            // Set the dependency injected parameters          
            _logger = loggerFactory.CreateLogger("OrderController");
            _telemetryClient = telemetryClient;

            // Initialize the Order Service
            _orderService = new OrderService(loggerFactory, telemetryClient);
        }

        // POST /order
        [HttpPost]
        public async Task<JsonResult> Post(Order order)
        {
            _logger.LogInformation("OrderController POST");

            try
            {
                // Add the order to MongoDB
                var orderId = await _orderService.AddOrderToMongoDB(order);

                // Add the order to AMQP
                await _orderService.AddOrderToAMQP(order);

                // Return OrderId
                return Json(new { OrderId = orderId });
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, ex.Message, order);
                return new JsonErrorResult(new { Error = ex.Message });
            }
        }
    }
}
