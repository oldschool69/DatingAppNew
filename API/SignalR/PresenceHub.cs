using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    [Authorize]
    public class PresenceHub(PresenceTracker tracker) : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await tracker.UserConnected(GetUserId(), Context.ConnectionId);

            await Clients.Others.SendAsync("UserIsOnline", GetUserId());

            var curruentUsers = await tracker.GetOnlineUsers();
            await Clients.Caller.SendAsync("GetOnlineUsers", curruentUsers);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await tracker.UserDisconnected(GetUserId(), Context.ConnectionId);

            await Clients.Others.SendAsync("UserIsOffline", GetUserId());

            var curruentUsers = await tracker.GetOnlineUsers();
            await Clients.Caller.SendAsync("GetOnlineUsers", curruentUsers);
            
            await base.OnDisconnectedAsync(exception);
        }

        private string GetUserId()
        {
            return Context.User?.GetMemberId() ?? 
                throw new HubException("Cannot get member id");
        }


    }
}