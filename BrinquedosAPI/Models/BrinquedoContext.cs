using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BrinquedosAPI.Models
{
    public class BrinquedoContext : DbContext
    {
        public BrinquedoContext(DbContextOptions<BrinquedoContext> opcao)
        : base(opcao)
        {
        }

        public DbSet<TodosBrinquedos> TodoBrinquedos { get; set; } = null!;
    }
}
