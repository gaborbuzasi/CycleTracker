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
        private readonly TrackerContext db = new TrackerContext();

        [HttpPost]
        public TwiMLResult Index(TwilioSmsHookModel twilioSmsHookModel)
        {
            var body = twilioSmsHookModel.Body;
            Debug.WriteLine($"Message received {body}");

            if (body.ToLower() == "enable")
            {
                SendDeviceMessage(body.ToUpper());

                return SendMessagingResponse("Theft protection successfully enabled.");
            }
            else if (body.ToLower() == "disable")
            {
                SendDeviceMessage(body.ToUpper());

                return SendMessagingResponse("Theft protection successfully disabled.");
            }
            else if (body.ToLower().Contains("register"))
            {
                return RegisterDevice(body);
            }

            var commandReceived = SmsParser.ParseSmsBody(twilioSmsHookModel.Body);

            switch (commandReceived.CommandId)
            {
                case Command.SET_INITIAL_STATE:
                    return SendMessagingResponse("OK");
                case Command.SET_LOCATION:
                    return SendMessagingResponse("OK");
                default:
                    return SendMessagingResponse("Sorry, what did you mean?");
            }
        }

        private TwiMLResult RegisterDevice(string body)
        {
            var data = body.Split(' ');
            var devicePhoneNumber = data[1];
            var deviceNickName = data[2];



            var response = new MessagingResponse();
            response.Message($"Your device {deviceNickName} with number {devicePhoneNumber} has been successfully registered");
            return TwiML(response);
        }

        [NonAction]
        private TwiMLResult SendMessagingResponse(string message)
        {
            var messagingResponse = new MessagingResponse();
            messagingResponse.Message(message);
            return TwiML(messagingResponse);

            // +882360003082111
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

        private void SendDeviceMessage(string message)
        {
            using (var client = new HttpClient())
            {
                var sms = new MokanixSmsModel
                {
                    message = message
                };

                client.BaseAddress = new Uri("https://mokanix.io/v1/assets/8944501101188633318/sms");
                client.DefaultRequestHeaders.Add("X-API-KEY", "t-c4d088c8-5dca-4cac-9fbb-8f7e507909a8");
                var response = client.PostAsJsonAsync("https://mokanix.io/v1/assets/8944501101188633318/sms", sms).Result;
                if (response.IsSuccessStatusCode)
                {
                    Console.Write("Success");
                }
                else
                    Console.Write("Error");
            }
        }
    }
}