using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIFeed.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FeedItems",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    Source = table.Column<string>(type: "TEXT", nullable: false),
                    Tags = table.Column<string>(type: "TEXT", nullable: false),
                    Score = table.Column<int>(type: "INTEGER", nullable: false),
                    Summary = table.Column<string>(type: "TEXT", nullable: true),
                    Author = table.Column<string>(type: "TEXT", nullable: true),
                    PublishedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    IngestedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedItems", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FeedItems_IngestedAt",
                table: "FeedItems",
                column: "IngestedAt");

            migrationBuilder.CreateIndex(
                name: "IX_FeedItems_PublishedAt",
                table: "FeedItems",
                column: "PublishedAt");

            migrationBuilder.CreateIndex(
                name: "IX_FeedItems_Source",
                table: "FeedItems",
                column: "Source");

            migrationBuilder.CreateIndex(
                name: "IX_FeedItems_Source_Url",
                table: "FeedItems",
                columns: new[] { "Source", "Url" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeedItems");
        }
    }
}
