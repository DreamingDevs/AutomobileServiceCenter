
using ASC.Business.Interfaces;
using ASC.Utilities;
using ASC.Web.Configuration;
using ASC.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Infrastructure;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ASC.Web.ServiceHub
{
    public class ServiceMessagesHub : Hub
    {
        private readonly IConnectionManager _signalRConnectionManager;
        private readonly IHttpContextAccessor _userHttpContext;
        private readonly IServiceRequestOperations _serviceRequestOperations;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOnlineUsersOperations _onlineUserOperations;
        private readonly IOptions<ApplicationSettings> _options;
        private readonly string _serviceRequestId;
        public ServiceMessagesHub(IConnectionManager signalRConnectionManager,
            IHttpContextAccessor userHttpContext,
            IServiceRequestOperations serviceRequestOperations,
            UserManager<ApplicationUser> userManager,
            IOnlineUsersOperations onlineUserOperations,
            IOptions<ApplicationSettings> options)
        {
            _signalRConnectionManager = signalRConnectionManager;
            _userHttpContext = userHttpContext;
            _serviceRequestOperations = serviceRequestOperations;
            _userManager = userManager;
            _onlineUserOperations = onlineUserOperations;
            _options = options;

            _serviceRequestId = _userHttpContext.HttpContext.Request.Headers["ServiceRequestId"];
        }

        public override async Task OnConnected()
        {
            if (!string.IsNullOrWhiteSpace(_serviceRequestId))
            {
                await _onlineUserOperations.CreateOnlineUserAsync(_userHttpContext.HttpContext.User.GetCurrentUserDetails().Email);
                await UpdateServiceRequestClients();
            }
            await base.OnConnected();
        }

        public override async Task OnDisconnected(bool stopCalled)
        {
            if (!string.IsNullOrWhiteSpace(_serviceRequestId))
            {
                await _onlineUserOperations.DeleteOnlineUserAsync(_userHttpContext.HttpContext.User.GetCurrentUserDetails().Email);
                await UpdateServiceRequestClients();
            }
            await base.OnDisconnected(stopCalled);
        }

        private async Task UpdateServiceRequestClients()
        {
            // Get Hub Context
            var hubContext = _signalRConnectionManager.GetHubContext<ServiceMessagesHub>();

            // Get Service Request Details
            var serviceRequest = await _serviceRequestOperations.GetServiceRequestByRowKey(_serviceRequestId);

            // Get Customer and Service Engineer names
            var customerName = (await _userManager.FindByEmailAsync(serviceRequest.PartitionKey)).UserName;
            var serviceEngineerName = (await _userManager.FindByEmailAsync(serviceRequest.ServiceEngineer)).UserName;
            var adminName = (await _userManager.FindByEmailAsync(_options.Value.AdminEmail)).UserName;

            // check Admin, Service Engineer and customer are connected.
            var isAdminOnline = await _onlineUserOperations.GetOnlineUserAsync(_options.Value.AdminEmail);
            var isServiceEngineerOnline = await _onlineUserOperations.GetOnlineUserAsync(serviceRequest.ServiceEngineer);
            var isCustomerOnline = await _onlineUserOperations.GetOnlineUserAsync(serviceRequest.PartitionKey);

            List<string> users = new List<string>();
            if (isAdminOnline) users.Add(adminName);
            if (isServiceEngineerOnline) users.Add(serviceEngineerName);
            if (isCustomerOnline) users.Add(customerName);

            // Send notifications
            hubContext
               .Clients
               .Users(users)
               .online(new
               {
                   isAd = isAdminOnline,
                   isSe = isServiceEngineerOnline,
                   isCu = isCustomerOnline
               });
        }
    }
}
