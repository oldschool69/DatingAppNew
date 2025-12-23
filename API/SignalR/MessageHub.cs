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
using Microsoft.Extensions.Primitives;

namespace API.SignalR
{
    [Authorize]
    public class MessageHub(IMessageRepository messageRepository, IMemberRepository memberRepository) : Hub
    {

        override public async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var otherUser = httpContext?.Request?.Query["userId"].ToString()
                ?? throw new HubException("Other user not found ");

            var groupName = GetGroupName(GetUserId(), otherUser);
            
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            await AddToGroup(groupName);

            var messages = await messageRepository.GetMessageThread(GetUserId(), otherUser);

            await Clients.Group(groupName).SendAsync("ReceiveMessageThread", messages);

        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // var httpContext = Context.GetHttpContext();
            // var otherUser = httpContext?.Request?.Query["userId"].ToString()
            //     ?? throw new HubException("Other user not found ");

            // var groupName = GetGroupName(GetUserId(), otherUser);
            
            // await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await messageRepository.RemoveConnection(Context.ConnectionId);

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(CreateMessageDto createMessageDto)
        {
            var userId = GetUserId();
            var sender = await memberRepository.GetMemberByIdAsync(userId);
            var recipient = await memberRepository.GetMemberByIdAsync(createMessageDto.RecipientId);

            if (recipient == null || sender == null || recipient.Id == sender.Id) 
                throw new HubException("Cannot send this message");

            var message = new Message
            {
                SenderId = sender.Id,
                RecipientId = recipient.Id,
                Content = createMessageDto.Content
            };

            var groupName = GetGroupName(sender.Id, recipient.Id);
            var group = await messageRepository.GetMessageGroup(groupName);
            
            if (group != null && group.Connections.Any(c => c.UserId == message.RecipientId))
            {
                message.DateRead = DateTime.UtcNow;
            }

            messageRepository.AddMessage(message);

            if (await messageRepository.SaveAllAsyncChanges())
            {
                await Clients.Group(groupName).SendAsync("NewMessage", message.ToDto());
            }
        }

        private async Task<bool> AddToGroup(string groupName)
        {
            var group = await messageRepository.GetMessageGroup(groupName);
            var connection = new Connection(Context.ConnectionId, GetUserId());

            if (group == null)
            {
                group = new Group(groupName);
                messageRepository.AddGroup(group);
            }

            group.Connections.Add(connection);

            return await messageRepository.SaveAllAsyncChanges();
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