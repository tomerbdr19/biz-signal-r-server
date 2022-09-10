using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;

namespace CSharp
{
    public static class Function
    {
        private static Dictionary<string,string> connectionMap = new(); 

        [FunctionName("negotiate")]
        public static SignalRConnectionInfo Negotiate(
            [HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequest req,
            [SignalRConnectionInfo(HubName = "serverless")] SignalRConnectionInfo connectionInfo)
        {
            Console.WriteLine(connectionInfo.AccessToken);
            return connectionInfo;
        }

        [FunctionName("sendMessage")]
        public static Task SendMessage(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] RequestType message,
        [SignalR(HubName = "serverless")] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            if (!connectionMap.ContainsKey(message.SendToId)) {
                return Task.CompletedTask;
            }

            var connectionId = connectionMap[message.SendToId];
            Console.WriteLine("Send message -> " + message.SendToId);

            return signalRMessages.AddAsync(
                new SignalRMessage
                {
                    // the message will only be sent to this user ID
                    ConnectionId = connectionId,
                    Target = "newMessage",
                    Arguments = new[] { message.Data }
                });
        }

        [FunctionName("redeemCoupon")]
        public static Task redeemCoupon(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] RequestType message,
        [SignalR(HubName = "serverless")] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            if (!connectionMap.ContainsKey(message.SendToId)) {
                return Task.CompletedTask;
            }

            var connectionId = connectionMap[message.SendToId];
            
            Console.WriteLine("Redeem coupon -> " + message.SendToId);

            return signalRMessages.AddAsync(
                new SignalRMessage
                {
                    // the message will only be sent to this user ID
                    ConnectionId = connectionId,
                    Target = "redeemCoupon",
                });
        }

        [FunctionName("connect")]
        public static Task connect(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] ConnectType message,
        [SignalR(HubName = "serverless")] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            Console.WriteLine("New Connection: \n" + "UserId: " + message.UserId + "\n" + "ConnectionId: " + message.ConnectionId);

            connectionMap.Remove(message.UserId);
            connectionMap.Add(message.UserId, message.ConnectionId);

            return Task.CompletedTask;
        }

        public class RequestType {
            public object Data {get; set;}
            public string SendToId {get; set;}
        }

        public class ConnectType {
            public string ConnectionId {get; set;}
            public string UserId {get; set;}
        }
    }
}