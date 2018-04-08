using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TrackerHook.API.Helpers;
using TrackerHook.API.Models;
using Twilio;
using Twilio.AspNet.Core;
using Twilio.Rest.Api.V2010.Account;
using Twilio.TwiML;
using Twilio.Types;

namespace TrackerHook.API.Controllers
{
    [Produces("application/json")]
    [Route("api/sms")]
    public class SmsController : TwilioController
    {
        private readonly TrackerContext _db;

        public SmsController(TrackerContext dbContext)
        {
            _db = dbContext;
        }

        [HttpPost]
        public TwiMLResult Index(TwilioSmsHookModel twilioSmsHookModel)
        {
            var body = twilioSmsHookModel.Body;
            var receivedFrom = twilioSmsHookModel.From;
            Debug.WriteLine($"Message received {body}");

            try
            {
                if (body.ToLower().Contains("enable"))
                {
                    if (SendDeviceMessage(body))
                    {
                        return SendMessagingResponse("Theft protection successfully enabled.");
                    }
                    else
                    {
                        return SendMessagingResponse("We're really sorry, but your device isn't registered in our system.");
                    }
                }
                else if (body.ToLower().Contains("disable"))
                {
                    if (SendDeviceMessage(body))
                    {
                        return SendMessagingResponse("Theft protection successfully disabled.");
                    }
                    else
                    {
                        return SendMessagingResponse("We're really sorry, but your device isn't registered in our system.");
                    }

                }
                else if (body.ToLower().Contains("register"))
                {
                    return RegisterDevice(twilioSmsHookModel);
                }

                var commandReceived = SmsParser.ParseSmsBody(body);
                commandReceived.TrackerId = _db.Trackers.First(x => x.DevicePhoneNumber == receivedFrom).Id;

                return HandleSmsCommandReceived(commandReceived);
            }
            catch (Exception ex)
            {
                _db.Logs.Add(new Log
                {
                    Error = ex.Message,
                    Time = DateTime.Now
                });

                return SendMessagingResponse("I'm sorry, we did not quite catch that.");
            }
        }

        private TwiMLResult HandleSmsCommandReceived(TrackerModel commandReceived)
        {
            if (SetDeviceLocation(commandReceived) > 0)
            {
                var ownerPhoneNumber = _db.Trackers.FirstOrDefault(x => x.Id == commandReceived.TrackerId)?.OwnerPhoneNumber;
                var textMessage = string.Empty;

                switch (commandReceived.CommandId)
                {
                    case Command.SET_INITIAL_STATE:

                        textMessage = "Initial location set";
                        break;
                    case Command.SET_LOCATION:
                        textMessage = "Your location";
                        break;
                    default:
                        break;
                }


                SendTextMessage(ownerPhoneNumber, textMessage);
                return SendMessagingResponse("OK");
            }
            else
                return SendMessagingResponse("ERROR");
        }

        private int SetDeviceLocation(TrackerModel command)
        {
            int returnValue = 0;
            try
            {
                _db.TrackerEvents.Add(new TrackerEvent
                {
                    IsAlert = command.CommandId == Command.SET_INITIAL_STATE ? false : true,
                    Latitude = double.Parse(command.Latitude),
                    Longitude = double.Parse(command.Longitude),
                    SatellitePrecision = int.Parse(command.SatellitePrecision),
                    Time = DateTime.Now,
                    TrackerId = command.TrackerId
                });
                returnValue = _db.SaveChanges();
            }
            catch (Exception ex)
            {
                _db.Logs.Add(new Log
                {
                    Error = ex.Message,
                    Time = DateTime.Now
                });
                _db.SaveChanges();
            }

            return returnValue;
        }

        private TwiMLResult RegisterDevice(TwilioSmsHookModel hookModel)
        {
            var response = new MessagingResponse();
            var data = hookModel.Body.Split(' ');
            var devicePhoneNumber = data[1].Trim();
            var deviceNickName = data[2].Trim().ToLower();
            var ownerPhoneNumber = hookModel.From;

            if (_db.Trackers.Any(x => x.DeviceNickName == deviceNickName))
            {
                response.Message("A device with that name is already registered. Please use another one.");
                return TwiML(response);
            }

            _db.Trackers.Add(new Tracker
            {
                DeviceNickName = deviceNickName,
                DevicePhoneNumber = devicePhoneNumber,
                OwnerPhoneNumber = ownerPhoneNumber
            });

            _db.SaveChanges();

            response.Message($"Your device {deviceNickName} with number {devicePhoneNumber} has been successfully registered");
            return TwiML(response);
        }

        [NonAction]
        private TwiMLResult SendMessagingResponse(string message)
        {
            var messagingResponse = new MessagingResponse();
            messagingResponse.Message(message);
            return TwiML(messagingResponse);
        }

        [NonAction]
        private void SendTextMessage(string phoneNumber, string messageText)
        {
            // Find your Account Sid and Auth Token at twilio.com/console
            const string accountSid = "ACfcc6c7984be14c54414ae757245fe958";
            const string authToken = "52652fda5d5b996ccbed5f4e2534d588";

            TwilioClient.Init(accountSid, authToken);

            var to = new PhoneNumber(phoneNumber);
            var message = MessageResource.Create(
                to,
                from: new PhoneNumber("+441173257381"),
                body: messageText);

            Console.WriteLine(message.Sid);
        }

        private bool SendDeviceMessage(string message)
        {
            var changeDeviceStatusMessage = message.Split(' ');
            var commandForDevice = changeDeviceStatusMessage[0].Trim().ToLower();
            var nickname = changeDeviceStatusMessage[1].Trim().ToLower();
            var phoneNumberOfDevice = _db.Trackers.FirstOrDefault(x => x.DeviceNickName.ToLower() == nickname);

            if (phoneNumberOfDevice != null)
            {
                using (var client = new HttpClient())
                {
                    var sms = new MokanixSmsModel
                    {
                        message = commandForDevice
                    };

                    client.BaseAddress = new Uri("https://mokanix.io/v1/assets/8944501101188633318/sms");
                    client.DefaultRequestHeaders.Add("X-API-KEY", "t-c4d088c8-5dca-4cac-9fbb-8f7e507909a8");
                    var response = client.PostAsJsonAsync("https://mokanix.io/v1/assets/8944501101188633318/sms", sms).Result;
                    Debug.WriteLine(response);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.Write("Success");
                    }
                    else
                        Console.Write("Error");

                    return true;
                }
            }
            else
            {
                // TODO: raise exception to enabler that device isn't registered.
                return false;
            }
            
        }
    }
}