using CarDexBackend.Domain.Entities;
using CarDexBackend.Repository.Interfaces;
using CarDexDatabase;
using Microsoft.EntityFrameworkCore;

namespace CarDexBackend.Repository.Implementations
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(CarDexDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<bool> ExistsByUsernameAsync(string username)
        {
            return await _dbSet.AnyAsync(u => u.Username == username);
        }
    }
}
