﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace BluetoothTempEntities
{
    public class ApplicationContext : DbContext
    {
        private string _databasePath;
        public DbSet<BluetoothDeviceWasСonnected> BluetoothDevicesWasСonnected { get; set; }

        public ApplicationContext()
        {

        }
        public ApplicationContext(string databasePath)
        {
            _databasePath = databasePath;
            //this.Database.EnsureDeleted();
            this.Database.Migrate();
            
            //Database.EnsureCreated();
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
