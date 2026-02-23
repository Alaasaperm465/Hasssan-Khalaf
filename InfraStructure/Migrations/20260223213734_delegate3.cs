using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfraStructure.Migrations
{
    /// <inheritdoc />
    public partial class delegate3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DelegateId",
                table: "Outbounds",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Outbounds_DelegateId",
                table: "Outbounds",
                column: "DelegateId");

            migrationBuilder.AddForeignKey(
                name: "FK_Outbounds_Delegate_DelegateId",
                table: "Outbounds",
                column: "DelegateId",
                principalTable: "Delegate",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Outbounds_Delegate_DelegateId",
                table: "Outbounds");

            migrationBuilder.DropIndex(
                name: "IX_Outbounds_DelegateId",
                table: "Outbounds");

            migrationBuilder.DropColumn(
                name: "DelegateId",
                table: "Outbounds");
        }
    }
}
