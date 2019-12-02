using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;

namespace SpockWallet.Data
{
    public class DataContext : DbContext
    {
        public static string DbPassword = "";
        public static bool IsClientV_1_0_5 = false;

        public DbSet<Wallet> Wallets { get; set; }
        
        public DbSet<Transaction> Transactions { get; set; }

        //public DbSet<Setting> Settings { get; set; }

        public DbSet<Config> Configs { get; set; }

        public DbSet<Contract> Contracts { get; set; }

        public DbSet<TokenTransfer> TokenTransfers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var conn = new SQLiteConnection(@"Data Source=wallet.db;");
            conn.Open();

            var command = conn.CreateCommand();
            if (IsClientV_1_0_5)
                command.CommandText = $"PRAGMA key = '{DbPassword}';";
            else
                command.CommandText = $"PRAGMA key = {DbPassword};";

            command.ExecuteNonQuery(); 
            optionsBuilder.UseSqlite(conn);
        }
    }
}
