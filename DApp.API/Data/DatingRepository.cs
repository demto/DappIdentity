using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DApp.API.Helpers;
using DApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DApp.API.Data
{
    public class DatingRepository : IDatingRepository
    {
        private readonly DataContext _context;
        public DatingRepository(DataContext context)
        {
            _context = context;

        }
        public void Add<T>(T entity) where T : class
        {
            _context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _context.Remove(entity);
        }

        public async Task<User> GetUser(int id)
        {
            var user = await _context.Users.Include(p => p.Photos).FirstOrDefaultAsync(u => u.Id == id);
            return user;
        }

        public async Task<PagedList<User>> GetUsers(UserParams userParams)
        {
            var users = _context.Users
                .Include(p => p.Photos)
                .OrderByDescending(u => u.LastActive)
                .AsQueryable();

            if(userParams.Likers) {
                var userLikers = await GetUserLikes(userParams.UserId, userParams.Likers);
                users = users.Where(u => userLikers.Contains(u.Id));
            }

            if(userParams.Likees) {
                var userLikees = await GetUserLikes(userParams.UserId, userParams.Likers);
                users = users.Where(u => userLikees.Contains(u.Id));
            }

            if(userParams.MaxAge != 0 || userParams.MinAge != 0) {
                var minDob = DateTime.Today.AddYears(-userParams.MaxAge-1);
                var maxDob = DateTime.Today.AddYears(-userParams.MinAge);
                users = users.Where(
                    u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);
            }
            
            if(!string.IsNullOrEmpty(userParams.OrderBy)){
                switch(userParams.OrderBy){
                    case("created"):
                        users = users.OrderByDescending(u => u.Created);
                        break;
                    default:
                        users = users.OrderByDescending(u => u.LastActive);
                        break;
                }
            }

            users = users.Where(u => u.Id != userParams.UserId);
            users = users.Where(u => u.Gender == userParams.Gender);

            return await PagedList<User>.CreateAsync(users, userParams.PageNumber, userParams.PageSize);
        }

        public async Task<IEnumerable<int>> GetUserLikes(int id, bool likers) {
            var user = await _context.Users
                .Include(u => u.Likees)
                .Include(u => u.Likers)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (likers) {
                return user.Likers.Where(u => u.LikeeId == id).Select(i => i.LikerId);
            }
            else {
                return user.Likees.Where(u => u.LikerId == id).Select(i => i.LikeeId);
            }
        }

        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Photo> GetPhoto(int id) {
            var photo = await _context.Photos.FirstOrDefaultAsync(p => p.Id == id);
            return photo;
        }

        public async Task<Photo> GetMainPhotoForUser (int userId) {
            return await _context.Photos
                .Where(u => u.UserId == userId)
                .FirstOrDefaultAsync(p => p.IsMain);
        }

        public Task<Like> GetLikes(int id, int recipientId)
        {
            var like = _context.Likes.FirstOrDefaultAsync(u => u.LikerId == id && u.LikeeId == recipientId);
            return like;
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages.FindAsync(id);
        }

        public async Task<PagedList<Message>> GetMessagesForUser(MessageParams messageParams)
        {
            var messages = _context.Messages
                .Include(m => m.Sender).ThenInclude(u => u.Photos)
                .Include(m => m.Recipient).ThenInclude(u => u.Photos)
                .AsQueryable();

            switch(messageParams.MessageContainer) {

                case "Inbox":
                    messages = messages.Where(m => m.RecipientId == messageParams.UserId
                        && m.RecipientDeleted == false);
                    break;
                case "Outbox":
                    messages = messages.Where(m => m.SenderId == messageParams.UserId
                        && m.SenderDeleted == false);
                    break;
                default:
                    messages = messages.Where(m => m.RecipientId == messageParams.UserId
                        && m.RecipientDeleted == false
                        && !m.IsRead);
                    break;
            }

            messages = messages.OrderByDescending(m => m.MessageSentAt);

            return await PagedList<Message>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<Message>> GetMessageThread(int userId, int recipientId)
        {
            var messages = await _context.Messages
                .Include(u => u.Sender).ThenInclude(u => u.Photos)
                .Include(u => u.Recipient).ThenInclude(u => u.Photos)
                .Where(u => (u.RecipientId == recipientId && u.SenderId == userId && u.SenderDeleted == false) ||
                    (u.RecipientId == userId) && (u.SenderId == recipientId) && (u.RecipientDeleted == false))
                .OrderByDescending(m => m.MessageSentAt)
                .ToListAsync();

            return messages;
        }
    }
}