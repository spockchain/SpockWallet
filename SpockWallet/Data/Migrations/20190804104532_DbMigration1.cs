using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SpockWallet.Data.Migrations
{
    public partial class DbMigration1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Key = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Hash = table.Column<string>(nullable: false),
                    From = table.Column<string>(nullable: true),
                    To = table.Column<string>(nullable: true),
                    BlockHash = table.Column<string>(nullable: true),
                    BlockNumber = table.Column<long>(nullable: false),
                    Value = table.Column<string>(nullable: true),
                    Gas = table.Column<string>(nullable: true),
                    GasPrice = table.Column<string>(nullable: true),
                    Input = table.Column<string>(nullable: true),
                    CreateTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Hash);
                });

            migrationBuilder.CreateTable(
                name: "Wallets",
                columns: table => new
                {
                    Address = table.Column<string>(nullable: false),
                    PrivateKey = table.Column<string>(nullable: true),
                    Balance = table.Column<string>(nullable: true),
                    StakingRequired = table.Column<string>(nullable: true),
                    PlotId = table.Column<string>(nullable: true),
                    ScanLocation = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wallets", x => x.Address);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "Wallets");
        }
    }
}
