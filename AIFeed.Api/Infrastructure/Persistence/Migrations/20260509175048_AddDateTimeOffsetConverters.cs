using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIFeed.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDateTimeOffsetConverters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "PublishedAt",
                table: "FeedItems",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<long>(
                name: "IngestedAt",
                table: "FeedItems",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "TEXT");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "PublishedAt",
                table: "FeedItems",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "IngestedAt",
                table: "FeedItems",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER");
        }
    }
}
