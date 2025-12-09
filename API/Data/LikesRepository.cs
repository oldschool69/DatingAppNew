using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class LikesRepository(AppDbContext context) : ILikesRepository
    {
        public void AddLike(MemberLike like)
        {
            context.Likes.Add(like);
        }

        public void DeleteLike(MemberLike like)
        {
            context.Likes.Remove(like);
        }

        public async Task<IReadOnlyList<string>> GetCurrentMemberLikeIds(string memberId)
        {
            return await context.Likes
                .Where(ml => ml.SourceMemberId == memberId)
                .Select(ml => ml.TargetMemberId)
                .ToListAsync();
        }

        public async Task<MemberLike?> GetMemberLike(string sourceMemberId, string targetMemberId)
        {
            return await context.Likes.FindAsync(sourceMemberId, targetMemberId);
        }

        public async Task<IReadOnlyList<Member>> GetMemberLikes(string predicate, string memberId)
        {
            var query = context.Likes.AsQueryable();

            switch (predicate)
            {
                case "liked":
                    return await query
                        .Where(ml => ml.SourceMemberId == memberId)
                        .Select(ml => ml.TargetMember)
                        .ToListAsync();
                case "likedBy":
                    return await query
                        .Where(ml => ml.TargetMemberId == memberId)
                        .Select(ml => ml.SourceMember)
                        .ToListAsync();
                default:
                    var likeIds = await GetCurrentMemberLikeIds(memberId);

                    return await query
                        .Where(ml => ml.TargetMemberId == memberId 
                            && likeIds.Contains(ml.TargetMemberId))
                        .Select(ml => ml.SourceMember)
                        .ToListAsync();
            }
        }

        public async Task<bool> SaveAllChanges()
        {
            return await context.SaveChangesAsync() > 0;
        }
    }
}