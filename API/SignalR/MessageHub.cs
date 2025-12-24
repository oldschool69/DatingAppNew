using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    [Authorize]
    public class MessageHub(IUnitOfWork uow, IHubContext<PresenceHub> presenceHub) : Hub
    {

        override public async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var otherUser = httpContext?.Request?.Query["userId"].ToString()
                ?? throw new HubException("Other user not found ");

            var groupName = GetGroupName(GetUserId(), otherUser);
            
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            await AddToGroup(groupName);

            var messages = await uow.MessageRepository.GetMessageThread(GetUserId(), otherUser);

            await Clients.Group(groupName).SendAsync("ReceiveMessageThread", messages);

        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await uow.MessageRepository.RemoveConnection(Context.ConnectionId);

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(CreateMessageDto createMessageDto)
        {
            var userId = GetUserId();
            var sender = await uow.MemberRepository.GetMemberByIdAsync(userId);
            var recipient = await uow.MemberRepository.GetMemberByIdAsync(createMessageDto.RecipientId);
            if (recipient == null || sender == null || recipient.Id == sender.Id) 
                throw new HubException("Cannot send this message");

            var message = new Message
            {
                SenderId = sender.Id,
                RecipientId = recipient.Id,
                Content = createMessageDto.Content
            };

            var groupName = GetGroupName(sender.Id, recipient.Id);
            var group = await uow.MessageRepository.GetMessageGroup(groupName);
            var isUserOnGroup = group != null && group.Connections.Any(c => c.UserId == message.RecipientId);
            
            if (isUserOnGroup)
            {
                message.DateRead = DateTime.UtcNow;
            }

            uow.MessageRepository.AddMessage(message);
            if (await uow.Complete())
            {
                await Clients.Group(groupName).SendAsync("NewMessage", message.ToDto());
                var connections = await PresenceTracker.GetConnectionsForUser(recipient.Id);
                if (connections != null && connections.Count > 0 && !isUserOnGroup)
                {
                    await presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived", 
                        message.ToDto());
                }
            }
        }

        private async Task<bool> AddToGroup(string groupName)
        {
            var group = await uow.MessageRepository.GetMessageGroup(groupName);
            var connection = new Connection(Context.ConnectionId, GetUserId());

            if (group == null)
            {
                group = new Group(groupName);
                uow.MessageRepository.AddGroup(group);
            }

            group.Connections.Add(connection);

            return await uow.Complete();
        }

        private static string GetGroupName(string? caller, string? other)
        {
            var stringCompare = string.CompareOrdinal(caller, other) < 0;
            return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
        }

        private string GetUserId()
        {
            return Context.User?.GetMemberId() ?? 
                throw new HubException("Cannot get member id");
        }

    }
}