﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SpockWallet.Data;

namespace SpockWallet.Data.Migrations
{
    [DbContext(typeof(DataContext))]
    [Migration("20190804104532_DbMigration1")]
    partial class DbMigration1
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079");

            modelBuilder.Entity("SpockWallet.Data.Setting", b =>
                {
                    b.Property<string>("Key")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Value");

                    b.HasKey("Key");

                    b.ToTable("Settings");
                });

            modelBuilder.Entity("SpockWallet.Data.Transaction", b =>
                {
                    b.Property<string>("Hash")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("BlockHash");

                    b.Property<long>("BlockNumber");

                    b.Property<DateTime>("CreateTime");

                    b.Property<string>("From");

                    b.Property<string>("Gas");

                    b.Property<string>("GasPrice");

                    b.Property<string>("Input");

                    b.Property<string>("To");

                    b.Property<string>("Value");

                    b.HasKey("Hash");

                    b.ToTable("Transactions");
                });

            modelBuilder.Entity("SpockWallet.Data.Wallet", b =>
                {
                    b.Property<string>("Address")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Balance");

                    b.Property<string>("PlotId");

                    b.Property<string>("PrivateKey");

                    b.Property<long>("ScanLocation");

                    b.Property<string>("StakingRequired");

                    b.HasKey("Address");

                    b.ToTable("Wallets");
                });
#pragma warning restore 612, 618
        }
    }
}
