using Microsoft.EntityFrameworkCore;

namespace CoreTodosAPI.Models
{
    public class ToDoDbContext : DbContext
    {
        public ToDoDbContext(DbContextOptions<ToDoDbContext> options) : base(options)
        {
            //ef core interception not generally available as of recording time
        }

        public DbSet<ToDoItem> ToDoItems { get; set; }
    }
}
