using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TwentyNet.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomFieldsAndRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomFields",
                table: "People",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomFields",
                table: "Companies",
                type: "jsonb",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CustomFieldDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ObjectName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Options = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomFieldDefinitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomFieldDefinitions_Workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalTable: "Workspaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecordRelations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceObjectName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SourceRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetObjectName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TargetRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    RelationType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecordRelations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecordRelations_Workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalTable: "Workspaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomFieldDefinitions_WorkspaceId_ObjectName_Name",
                table: "CustomFieldDefinitions",
                columns: new[] { "WorkspaceId", "ObjectName", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecordRelations_WorkspaceId_SourceObjectName_SourceRecordId",
                table: "RecordRelations",
                columns: new[] { "WorkspaceId", "SourceObjectName", "SourceRecordId" });

            migrationBuilder.CreateIndex(
                name: "IX_RecordRelations_WorkspaceId_TargetObjectName_TargetRecordId",
                table: "RecordRelations",
                columns: new[] { "WorkspaceId", "TargetObjectName", "TargetRecordId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomFieldDefinitions");

            migrationBuilder.DropTable(
                name: "RecordRelations");

            migrationBuilder.DropColumn(
                name: "CustomFields",
                table: "People");

            migrationBuilder.DropColumn(
                name: "CustomFields",
                table: "Companies");
        }
    }
}
