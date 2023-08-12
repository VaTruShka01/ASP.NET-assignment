using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Assignment.Data.Migrations
{
    public partial class AddActiveColumnAddDependecyCart : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CartId1",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Active",
                table: "Carts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CartId1",
                table: "Orders",
                column: "CartId1",
                unique: true,
                filter: "[CartId1] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Carts_CartId1",
                table: "Orders",
                column: "CartId1",
                principalTable: "Carts",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Carts_CartId1",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_CartId1",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CartId1",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Active",
                table: "Carts");
        }
    }
}
