using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class MessageRepository(AppDbContext context) : IMessageRepository
    {
        public void AddMessage(Message message)
        {
            context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            context.Messages.Remove(message);
        }

        public async Task<Message?> GetMessage(string messageId)
        {
            return await context.Messages.FindAsync(messageId);
        }

        public async Task<PaginatedResult<MessageDto>> GetMessagesForMember(MessageParams messageParams)
        {
            var query = context.Messages
                .OrderByDescending(m => m.MessageSent)
                .AsQueryable();

            query = messageParams.Container.ToLower() switch
            {
                "outbox" => query.Where(m => m.SenderId == messageParams.MemberId &&
                    !m.SenderDeleted),
                _ => query.Where(m => m.RecipientId == messageParams.MemberId &&
                    !m.RecipientDeleted)
            };

            var messageQuery = query.Select(MessageExtensions.ToDtoProjection);

            return await PaginationHelper.CreateAsync(messageQuery, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IReadOnlyList<MessageDto>> GetMessageThread(string currentMemberId, string recipientMemberId)
        {
            await context.Messages
                .Where(m =>
                       m.RecipientId == currentMemberId 
                        && m.SenderId == recipientMemberId && 
                        m.DateRead == null)
                .ExecuteUpdateAsync(m => m.SetProperty(msg => msg.DateRead, DateTime.UtcNow));

            return await context.Messages
                .Where(m =>
                       (m.RecipientId == currentMemberId &&  
                        !m.SenderDeleted 
                        && m.SenderId == recipientMemberId) ||
                       (m.RecipientId == recipientMemberId 
                        && !m.RecipientDeleted 
                        && m.SenderId == currentMemberId))
                .OrderBy(m => m.MessageSent)
                .Select(MessageExtensions.ToDtoProjection)
                .ToListAsync();
        }

        public async Task<bool> SaveAllAsyncChanges()
        {
            return await context.SaveChangesAsync() > 0;
        }
    }
}