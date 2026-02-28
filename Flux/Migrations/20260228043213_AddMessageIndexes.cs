using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Flux.Migrations
{
    /// <inheritdoc />
    public partial class AddMessageIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Messages_ChannelId",
                table: "Messages");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ChannelId_CreatedAt",
                table: "Messages",
                columns: new[] { "ChannelId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Messages_ChannelId_CreatedAt",
                table: "Messages");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ChannelId",
                table: "Messages",
                column: "ChannelId");
        }
    }
}
