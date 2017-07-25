using Microsoft.AspNetCore.Mvc;
using ASC.Web.Controllers;
using ASC.Business.Interfaces;
using ASC.Models.Models;
using ASC.Utilities;
using System;
using System.Threading.Tasks;
using ASC.Web.Areas.ServiceRequests.Models;
using AutoMapper;
using ASC.Web.Data;
using System.Linq;
using ASC.Models.BaseTypes;
using Microsoft.AspNetCore.Identity;
using ASC.Web.Models;
using ASC.Web.Services;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Infrastructure;
using ASC.Web.ServiceHub;
using ASC.Web.Configuration;
using Microsoft.Extensions.Options;

namespace ASC.Web.Areas.ServiceRequests.Controllers
{
    [Area("ServiceRequests")]
    public class ServiceRequestController : BaseController
    {
        private readonly IServiceRequestOperations _serviceRequestOperations;
        private readonly IServiceRequestMessageOperations _serviceRequestMessageOperations;
        private readonly IConnectionManager _signalRConnectionManager;
        private readonly IMapper _mapper;
        private readonly IMasterDataCacheOperations _masterData;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IOptions<ApplicationSettings> _options;
        private readonly IOnlineUsersOperations _onlineUsersOperations;
        private readonly ISmsSender _smsSender;
        public ServiceRequestController(IServiceRequestOperations operations, 
            IServiceRequestMessageOperations messageOperations,
            IConnectionManager signalRConnectionManager,
            IMapper mapper, 
            IMasterDataCacheOperations masterData, 
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender,
            IOptions<ApplicationSettings> options,
            IOnlineUsersOperations onlineUsersOperations,
            ISmsSender smsSender)
        {
            _serviceRequestOperations = operations;
            _serviceRequestMessageOperations = messageOperations;
            _signalRConnectionManager = signalRConnectionManager;
            _mapper = mapper;
            _masterData = masterData;
            _userManager = userManager;
            _emailSender = emailSender;
            _onlineUsersOperations = onlineUsersOperations;
            _options = options;
            _smsSender = smsSender;
        }

        [HttpGet]
        public async Task<IActionResult> ServiceRequest()
        {
            var masterData = await _masterData.GetMasterDataCacheAsync();
            ViewBag.VehicleTypes = masterData.Values.Where(p => p.PartitionKey == MasterKeys.VehicleType.ToString()).ToList();
            ViewBag.VehicleNames = masterData.Values.Where(p => p.PartitionKey == MasterKeys.VehicleName.ToString()).ToList();
            return View(new NewServiceRequestViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> ServiceRequest(NewServiceRequestViewModel request)
        {
            if (!ModelState.IsValid)
            {
                var masterData = await _masterData.GetMasterDataCacheAsync();
                ViewBag.VehicleTypes = masterData.Values.Where(p => p.PartitionKey == MasterKeys.VehicleType.ToString()).ToList();
                ViewBag.VehicleNames = masterData.Values.Where(p => p.PartitionKey == MasterKeys.VehicleName.ToString()).ToList();
                return View(request);
            }

            // Map the view model to Azure model
            var serviceRequest = _mapper.Map<NewServiceRequestViewModel, ServiceRequest>(request);

            // Set RowKey, PartitionKye, RequestedDate, Status properties
            serviceRequest.PartitionKey = HttpContext.User.GetCurrentUserDetails().Email;
            serviceRequest.RowKey = Guid.NewGuid().ToString();
            serviceRequest.RequestedDate = request.RequestedDate;
            serviceRequest.Status = Status.New.ToString();

            await _serviceRequestOperations.CreateServiceRequestAsync(serviceRequest);

            return RedirectToAction("Dashboard", "Dashboard", new { Area = "ServiceRequests" });
        }

        [HttpGet]
        public async Task<IActionResult> ServiceRequestDetails(string id)
        {
            var serviceRequestDetails = await _serviceRequestOperations.GetServiceRequestByRowKey(id);

            // Access Check
            if (HttpContext.User.IsInRole(Roles.Engineer.ToString())
                && serviceRequestDetails.ServiceEngineer != HttpContext.User.GetCurrentUserDetails().Email)
            {
                throw new UnauthorizedAccessException();
            }

            if (HttpContext.User.IsInRole(Roles.User.ToString())
                && serviceRequestDetails.PartitionKey != HttpContext.User.GetCurrentUserDetails().Email)
            {
                throw new UnauthorizedAccessException();
            }

            var serviceRequestAuditDetails = await _serviceRequestOperations.GetServiceRequestAuditByPartitionKey(
                serviceRequestDetails.PartitionKey + "-" + id);

            // Select List Data
            var masterData = await _masterData.GetMasterDataCacheAsync();
            ViewBag.VehicleTypes = masterData.Values.Where(p => p.PartitionKey == MasterKeys.VehicleType.ToString()).ToList();
            ViewBag.VehicleNames = masterData.Values.Where(p => p.PartitionKey == MasterKeys.VehicleName.ToString()).ToList();
            ViewBag.Status = Enum.GetValues(typeof(Status)).Cast<Status>().Select(v => v.ToString()).ToList();
            ViewBag.ServiceEngineers = await _userManager.GetUsersInRoleAsync(Roles.Engineer.ToString());

            return View(new ServiceRequestDetailViewModel
            {
                ServiceRequest = _mapper.Map<ServiceRequest, UpdateServiceRequestViewModel>(serviceRequestDetails),
                ServiceRequestAudit = serviceRequestAuditDetails.OrderByDescending(p => p.Timestamp).ToList()
            });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateServiceRequestDetails(UpdateServiceRequestViewModel serviceRequest)
        {
            var originalServiceRequest = await _serviceRequestOperations.GetServiceRequestByRowKey(serviceRequest.RowKey);
            originalServiceRequest.RequestedServices = serviceRequest.RequestedServices;

            var isServiceRequestStatusUpdated = false;
            // Update Status only if user role is either Admin or Engineer
            // Or Customer can update the status if it is only in Pending Customer Approval.
            if (HttpContext.User.IsInRole(Roles.Admin.ToString()) || 
                HttpContext.User.IsInRole(Roles.Engineer.ToString()) || 
                (HttpContext.User.IsInRole(Roles.User.ToString()) && originalServiceRequest.Status == Status.PendingCustomerApproval.ToString()))
            {
                if (originalServiceRequest.Status != serviceRequest.Status)
                {
                    isServiceRequestStatusUpdated = true;
                }
                originalServiceRequest.Status = serviceRequest.Status;
            }

            // Update Service Engineer field only if user role is Admin
            if (HttpContext.User.IsInRole(Roles.Admin.ToString()))
            {
                originalServiceRequest.ServiceEngineer = serviceRequest.ServiceEngineer;
            }

            await _serviceRequestOperations.UpdateServiceRequestAsync(originalServiceRequest);

            if(HttpContext.User.IsInRole(Roles.Admin.ToString()) ||
                HttpContext.User.IsInRole(Roles.Engineer.ToString()) || originalServiceRequest.Status == Status.PendingCustomerApproval.ToString())
            {
                await _emailSender.SendEmailAsync(originalServiceRequest.PartitionKey,
                        "Your Service Request is almost completed!!!",
                        "Please visit the ASC application and review your Service request.");
            }

            if (isServiceRequestStatusUpdated)
            {
                await SendSmsAndWebNotifications(originalServiceRequest);
            }

            return RedirectToAction("ServiceRequestDetails", "ServiceRequest",
                new { Area = "ServiceRequests", Id = serviceRequest.RowKey });
        }

        private async Task SendSmsAndWebNotifications(ServiceRequest serviceRequest)
        {
            // Send SMS Notification
            var phoneNumber = (await _userManager.FindByEmailAsync(serviceRequest.PartitionKey)).PhoneNumber;
            if (!string.IsNullOrWhiteSpace(phoneNumber)) {
                await _smsSender.SendSmsAsync(string.Format("+91{0}", phoneNumber),
                            string.Format("Service Request Status updated to {0}", serviceRequest.Status));
            }

            // Get Customer name
            var customerName = (await _userManager.FindByEmailAsync(serviceRequest.PartitionKey)).UserName;

            // Send web notifications
            _signalRConnectionManager.GetHubContext<ServiceMessagesHub>()
               .Clients
               .User(customerName)
               .publishNotification(new
               {
                   status = serviceRequest.Status
               });
        }

        public async Task<IActionResult> CheckDenialService(DateTime requestedDate)
        {
            var serviceRequests = await _serviceRequestOperations.GetServiceRequestsByRequestedDateAndStatus(
                DateTime.UtcNow.AddDays(-90),
                new List<string>() { Status.Denied.ToString() },
                HttpContext.User.GetCurrentUserDetails().Email);

            if (serviceRequests.Any())
                return Json(data: $"There is a denied service request for you in last 90 days. Please contact ASC Admin.");

            return Json(data: true);
        }

        [HttpGet]
        public IActionResult SearchServiceRequests()
        {
            return View(new SearchServiceRequestsViewModel());
        }

        [HttpGet]
        public async Task<IActionResult> SearchServiceRequestResults(string email, DateTime? requestedDate)
        {
            List<ServiceRequest> results = new List<ServiceRequest>();
            if(String.IsNullOrEmpty(email) && !requestedDate.HasValue)
                return Json(new { data = results });

            if(HttpContext.User.IsInRole(Roles.Admin.ToString()))
                results = await _serviceRequestOperations.GetServiceRequestsByRequestedDateAndStatus(requestedDate, null, email);
            else
                results = await _serviceRequestOperations.GetServiceRequestsByRequestedDateAndStatus(requestedDate, null, email, HttpContext.User.GetCurrentUserDetails().Email);
            
            return Json(new { data = results.OrderByDescending(p => p.RequestedDate).ToList() });
        }

        [HttpGet]
        public async Task<IActionResult> ServiceRequestMessages(string serviceRequestId)
        {
            return Json((await _serviceRequestMessageOperations.GetServiceRequestMessageAsync(serviceRequestId)).OrderBy(p => p.MessageDate));
        }

        [HttpPost]
        public async Task<IActionResult> CreateServiceRequestMessage(ServiceRequestMessage message)
        {
            // Message and Service Request Id (Service request Id is the partition key for a message)
            if (string.IsNullOrWhiteSpace(message.Message) || string.IsNullOrWhiteSpace(message.PartitionKey))
                return Json(false);

            // Get Service Request details
            var serviceRequesrDetails = await _serviceRequestOperations.GetServiceRequestByRowKey(message.PartitionKey);
            
            // Populate message details
            message.FromEmail = HttpContext.User.GetCurrentUserDetails().Email;
            message.FromDisplayName = HttpContext.User.GetCurrentUserDetails().Name;
            message.MessageDate = DateTime.UtcNow;
            message.RowKey = Guid.NewGuid().ToString();

            // Get Customer and Service Engineer names
            var customerName = (await _userManager.FindByEmailAsync(serviceRequesrDetails.PartitionKey)).UserName;
            var serviceEngineerName = (await _userManager.FindByEmailAsync(serviceRequesrDetails.ServiceEngineer)).UserName;
            var adminName = (await _userManager.FindByEmailAsync(_options.Value.AdminEmail)).UserName;

            // Save the message to Azure Storage
            await _serviceRequestMessageOperations.CreateServiceRequestMessageAsync(message);

            // Broadcast the message to all clients asscoaited with Service Request
            _signalRConnectionManager.GetHubContext<ServiceMessagesHub>()
                .Clients
                .Users(new List<string> { customerName, serviceEngineerName, adminName })
                .publishMessage(message);
            // Return true
            return Json(true);
        }

        [HttpGet]
        public async Task<IActionResult> MarkOfflineUser()
        {
            // Delete the current logged in user from OnlineUsers entity
            await _onlineUsersOperations.DeleteOnlineUserAsync(HttpContext.User.GetCurrentUserDetails().Email);

            string serviceRequestId = HttpContext.Request.Headers["ServiceRequestId"];
            // Get Service Request Details
            var serviceRequest = await _serviceRequestOperations.GetServiceRequestByRowKey(serviceRequestId);

            // Get Customer and Service Engineer names
            var customerName = (await _userManager.FindByEmailAsync(serviceRequest.PartitionKey)).UserName;
            var serviceEngineerName = (await _userManager.FindByEmailAsync(serviceRequest.ServiceEngineer)).UserName;
            var adminName = (await _userManager.FindByEmailAsync(_options.Value.AdminEmail)).UserName;

            // check Admin, Service Engineer and customer are connected.
            var isAdminOnline = await _onlineUsersOperations.GetOnlineUserAsync(_options.Value.AdminEmail);
            var isServiceEngineerOnline = await _onlineUsersOperations.GetOnlineUserAsync(serviceRequest.ServiceEngineer);
            var isCustomerOnline = await _onlineUsersOperations.GetOnlineUserAsync(serviceRequest.PartitionKey);

            List<string> users = new List<string>();
            if (isAdminOnline) users.Add(adminName);
            if (isServiceEngineerOnline) users.Add(serviceEngineerName);
            if (isCustomerOnline) users.Add(customerName);

            // Send notifications
            _signalRConnectionManager.GetHubContext<ServiceMessagesHub>()
               .Clients
               .Users(users)
               .online(new
               {
                   isAd = isAdminOnline,
                   isSe = isServiceEngineerOnline,
                   isCu = isCustomerOnline
               });

            return Json(true);
        }
    }
}
