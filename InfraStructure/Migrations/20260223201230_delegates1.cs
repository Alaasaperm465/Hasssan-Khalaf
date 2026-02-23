using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraStructure.Migrations
{
    /// <inheritdoc />
    public partial class delegates1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClientId1",
                table: "Delegate",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Delegate_ClientId1",
                table: "Delegate",
                column: "ClientId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Delegate_Clients_ClientId1",
                table: "Delegate",
                column: "ClientId1",
                principalTable: "Clients",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Delegate_Clients_ClientId1",
                table: "Delegate");

            migrationBuilder.DropIndex(
                name: "IX_Delegate_ClientId1",
                table: "Delegate");

            migrationBuilder.DropColumn(
                name: "ClientId1",
                table: "Delegate");
        }
    }
}
