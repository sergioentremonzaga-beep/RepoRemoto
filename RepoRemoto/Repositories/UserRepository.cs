using Microsoft.EntityFrameworkCore;
using RepoRemoto.Entity;

namespace RepoRemoto.Repositories;

public class UserRepository(AppDbContext dbContext)
{
    public async Task<List<UserEntity>> GetAllAsync()
    {
        return await dbContext.Users.ToListAsync();
    }
    
    public async Task<UserEntity?> GetByIdAsync(int id)
    {
        return await dbContext.Users.FindAsync(id);
    }
    
    public async Task CreateAsync(UserEntity entity)
    {
        dbContext.Users.Add(entity);
        await dbContext.SaveChangesAsync();
    }
    
    public async Task SaveRangeAsync(IEnumerable<UserEntity> entities)
    {
        await dbContext.Users.AddRangeAsync(entities);
        await dbContext.SaveChangesAsync();
    }
    
    public async Task UpdateAsync(UserEntity entity)
    {
        dbContext.Users.Update(entity);
        await dbContext.SaveChangesAsync();
    }
    
    public async Task DeleteAsync(UserEntity entity)
    {
        dbContext.Users.Remove(entity);
        await dbContext.SaveChangesAsync();
    }
    
    public async Task ClearAsync()
    {
        dbContext.Users.RemoveRange(dbContext.Users);
        await dbContext.SaveChangesAsync();
    }
}