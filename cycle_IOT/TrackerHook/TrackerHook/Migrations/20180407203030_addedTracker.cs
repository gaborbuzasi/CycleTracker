using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace TrackerHook.Migrations
{
    public partial class addedTracker : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tracker",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    DeviceNickName = table.Column<string>(nullable: true),
                    DevicePhoneNumber = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tracker", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrackerEvents_TrackerId",
                table: "TrackerEvents",
                column: "TrackerId");

            migrationBuilder.AddForeignKey(
                name: "FK_TrackerEvents_Tracker_TrackerId",
                table: "TrackerEvents",
                column: "TrackerId",
                principalTable: "Tracker",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrackerEvents_Tracker_TrackerId",
                table: "TrackerEvents");

            migrationBuilder.DropTable(
                name: "Tracker");

            migrationBuilder.DropIndex(
                name: "IX_TrackerEvents_TrackerId",
                table: "TrackerEvents");
        }
    }
}
