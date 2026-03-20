using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Flux.Migrations
{
    /// <inheritdoc />
    public partial class AddCallSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CallSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChannelId = table.Column<Guid>(type: "uuid", nullable: false),
                    ThreadMessageId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CallSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CallSessions_Channels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "Channels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CallSessions_Messages_ThreadMessageId",
                        column: x => x.ThreadMessageId,
                        principalTable: "Messages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CallSessions_ChannelId",
                table: "CallSessions",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_CallSessions_ThreadMessageId",
                table: "CallSessions",
                column: "ThreadMessageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CallSessions");
        }
    }
}
