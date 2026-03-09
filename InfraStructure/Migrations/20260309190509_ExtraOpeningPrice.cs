using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraStructure.Migrations
{
    /// <inheritdoc />
    public partial class ExtraOpeningPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ExtraOpeningPrice",
                table: "Clients",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PalletPrice",
                table: "Clients",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExtraOpeningPrice",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "PalletPrice",
                table: "Clients");
        }
    }
}
