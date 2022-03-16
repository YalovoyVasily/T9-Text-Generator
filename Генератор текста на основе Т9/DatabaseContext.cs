using System;
using Microsoft.EntityFrameworkCore;

namespace Генератор_текста_на_основе_Т9
{
    class DatabaseContext : DbContext
    {
        public DbSet<Word> Words { get; set; }


        public DatabaseContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=T9TextGeneratorDB;Trusted_Connection=True;");
        }
    }

    public class Word
    {
        public int Id { get; set; }
        public string WordFirst { get; set; }
        public string WordSecond { get; set; }
        public int Count { get; set; }
    }
}
