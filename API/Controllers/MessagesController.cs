using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class MessagesController(IUnitOfWork uow) : BaseApiController
    {
        [HttpPost]
        public async Task<ActionResult> CreateMessage(CreateMessageDto createMessageDto)
        {
            var userId = User.GetMemberId();
            var sender = await uow.MemberRepository.GetMemberByIdAsync(userId);
            var recipient = await uow.MemberRepository.GetMemberByIdAsync(createMessageDto.RecipientId);

            if (recipient == null || sender == null || recipient.Id == sender.Id) 
                return BadRequest("Cannot send this message");

            var message = new Message
            {
                SenderId = sender.Id,
                RecipientId = recipient.Id,
                Content = createMessageDto.Content
            };

            uow.MessageRepository.AddMessage(message);

            if (await uow.Complete()) return Ok(message.ToDto());
            
            return BadRequest("Failed to send message");
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResult<MessageDto>>> GetMessagesByContainer([FromQuery] MessageParams messageParams)
        {
            var userId = User.GetMemberId();
            messageParams.MemberId = userId;

            var messages = await uow.MessageRepository.GetMessagesForMember(messageParams);

            return Ok(messages);
        }

        [HttpGet("thread/{recipientMemberId}")]
        public async Task<ActionResult<IReadOnlyList<MessageDto>>> GetMessageThread(string recipientMemberId)
        {
            var currentMemberId = User.GetMemberId();

            var messages = await uow.MessageRepository.GetMessageThread(currentMemberId, recipientMemberId);

            return Ok(messages);
        }

        [HttpDelete("{messageId}")]
        public async Task<ActionResult> DeleteMessage(string messageId)
        {
            var memberId = User.GetMemberId();

            var message = await uow.MessageRepository.GetMessage(messageId);

            if (message == null) return BadRequest("Message not found");

            if (message.SenderId != memberId && message.RecipientId != memberId)
                return BadRequest("You are not authorized to delete this message");

            if (message.SenderId == memberId) message.SenderDeleted = true;
            if (message.RecipientId == memberId) message.RecipientDeleted = true;

            if (message is {SenderDeleted: true, RecipientDeleted: true})
                uow.MessageRepository.DeleteMessage(message);

            if (await uow.Complete()) return Ok();

            return BadRequest("Problem deleting the message");
        }
    }
}