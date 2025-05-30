using System.Data.Common;
using API_Maquinas.Models;
using Microsoft.EntityFrameworkCore;


namespace API_Maquinas.DTOs
{
    public class StoredContext : DbContext
    {
        public StoredContext(DbContextOptions<StoredContext> options) : base(options) { }


        public DbSet<Maquinas> Maquinas { get; set; }


    }
}
