using Microsoft.EntityFrameworkCore.Migrations;

namespace Assignment3.Migrations
{
    public partial class nomovie : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cinemas_Movies_movieID",
                table: "Cinemas");

            migrationBuilder.DropIndex(
                name: "IX_Cinemas_movieID",
                table: "Cinemas");

            migrationBuilder.DropColumn(
                name: "movieID",
                table: "Cinemas");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "movieID",
                table: "Cinemas",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cinemas_movieID",
                table: "Cinemas",
                column: "movieID");

            migrationBuilder.AddForeignKey(
                name: "FK_Cinemas_Movies_movieID",
                table: "Cinemas",
                column: "movieID",
                principalTable: "Movies",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
