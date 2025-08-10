using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BasketballLiveScore.Data;
using BasketballLiveScore.Models;
using BasketballLiveScore.Repositories.Interfaces;

namespace BasketballLiveScore.Repositories.Implementations
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        private readonly BasketballDbContext _context;
        private readonly DbSet<User> _dbSet;

        public UserRepository(BasketballDbContext context) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<User>();
        }

        public override async Task<User?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public override async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public override async Task AddAsync(User entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public IEnumerable<User> Find(Expression<Func<User, bool>> predicate)
        {
            return _dbSet.Where(predicate).ToList();
        }

        public void AddRange(IEnumerable<User> entities)
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities));

            _dbSet.AddRange(entities);
        }

        public void Update(User entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            _dbSet.Update(entity);
        }

        public void Remove(User entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            _dbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<User> entities)
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities));
            _dbSet.RemoveRange(entities);
        }

        public User? GetByUsernameAndPassword(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return null;

            return _dbSet.FirstOrDefault(u =>
                u.Username == username &&
                u.Password == password &&
                u.IsActive);
        }

        public bool UserExists(string username)
        {
            if (string.IsNullOrEmpty(username))
                return false;

            return _dbSet.Any(u => u.Username == username);
        }

        public User? GetByUsername(string username)
        {
            if (string.IsNullOrEmpty(username))
                return null;

            return _dbSet.FirstOrDefault(u => u.Username == username);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
                return null;

            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<User>> GetActiveUsersAsync()
        {
            return await _dbSet.Where(u => u.IsActive).ToListAsync();
        }
    }
}
