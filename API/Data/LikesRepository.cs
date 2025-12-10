using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Entities;
using API.Helpers;
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

        public async Task<PaginatedResult<Member>> GetMemberLikes(LikedParams likedParams)
        {
            var likesQuery = context.Likes.AsQueryable();
            IQueryable<Member> membersQuery;

            switch (likedParams.Predicate)
            {
                case "liked":
                    membersQuery = likesQuery
                        .Where(ml => ml.SourceMemberId == likedParams.CurrentMemberId)
                        .Select(ml => ml.TargetMember);
                    break;
                case "likedBy":
                    membersQuery = likesQuery
                        .Where(ml => ml.TargetMemberId == likedParams.CurrentMemberId)
                        .Select(ml => ml.SourceMember);
                    break;
                default:
                    var likeIds = await GetCurrentMemberLikeIds(likedParams.CurrentMemberId);

                    membersQuery = likesQuery
                        .Where(ml => ml.TargetMemberId == likedParams.CurrentMemberId 
                            && likeIds.Contains(ml.SourceMemberId))
                        .Select(ml => ml.SourceMember);
                    break;
            }

            return await PaginationHelper.CreateAsync(membersQuery,
                likedParams.PageNumber, likedParams.PageSize);
        }

        public async Task<bool> SaveAllChanges()
        {
            return await context.SaveChangesAsync() > 0;
        }
    }
}