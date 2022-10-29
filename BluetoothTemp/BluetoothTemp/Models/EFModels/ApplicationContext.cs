using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace BluetoothTemp.Models.EFModels
{
    public class ApplicationContext : DbContext
    {
        private string _databasePath;
        public DbSet<BluetoothDeviceWasСonnected> BluetoothDevicesWasСonnected { get; set; }

        public ApplicationContext(string databasePath)
        {
            _databasePath = databasePath;
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Filename={_databasePath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BluetoothDeviceWasСonnected>()
                .Property(p => p.Id)
                .ValueGeneratedOnAdd();
        }
    }
}
