using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SpockWallet.Data.Migrations
{
    public partial class DbMigration5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Contracts",
                columns: table => new
                {
                    Address = table.Column<string>(nullable: false),
                    BlockNumber = table.Column<long>(nullable: false),
                    CreationTransaction = table.Column<string>(nullable: true),
                    TokenName = table.Column<string>(nullable: true),
                    Symbol = table.Column<string>(nullable: true),
                    Owner = table.Column<string>(nullable: true),
                    TotalSupply = table.Column<string>(nullable: true),
                    Decimals = table.Column<int>(nullable: false),
                    IsDelete = table.Column<bool>(nullable: false),
                    CreateTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contracts", x => x.Address);
                });

            migrationBuilder.CreateTable(
                name: "TokenTransfers",
                columns: table => new
                {
                    Hash = table.Column<string>(nullable: false),
                    BlockNumber = table.Column<long>(nullable: false),
                    Method = table.Column<string>(nullable: true),
                    From = table.Column<string>(nullable: true),
                    To = table.Column<string>(nullable: true),
                    ContractAddress = table.Column<string>(nullable: true),
                    Symbol = table.Column<string>(nullable: true),
                    Value = table.Column<decimal>(nullable: false),
                    CreateTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenTransfers", x => x.Hash);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Contracts");

            migrationBuilder.DropTable(
                name: "TokenTransfers");
        }
    }
}
