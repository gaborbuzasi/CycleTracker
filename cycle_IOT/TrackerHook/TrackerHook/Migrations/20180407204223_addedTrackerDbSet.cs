using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace TrackerHook.Migrations
{
    public partial class addedTrackerDbSet : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrackerEvents_Tracker_TrackerId",
                table: "TrackerEvents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tracker",
                table: "Tracker");

            migrationBuilder.RenameTable(
                name: "Tracker",
                newName: "Trackers");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Trackers",
                table: "Trackers",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TrackerEvents_Trackers_TrackerId",
                table: "TrackerEvents",
                column: "TrackerId",
                principalTable: "Trackers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrackerEvents_Trackers_TrackerId",
                table: "TrackerEvents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Trackers",
                table: "Trackers");

            migrationBuilder.RenameTable(
                name: "Trackers",
                newName: "Tracker");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tracker",
                table: "Tracker",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TrackerEvents_Tracker_TrackerId",
                table: "TrackerEvents",
                column: "TrackerId",
                principalTable: "Tracker",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
