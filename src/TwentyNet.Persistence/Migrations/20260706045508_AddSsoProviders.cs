using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TwentyNet.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSsoProviders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SsoProviders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ClientId = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ClientSecret = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    AuthorizationEndpoint = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TokenEndpoint = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UserInfoEndpoint = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EntityId = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SingleSignOnUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Certificate = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    MetadataUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SsoProviders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SsoProviders_Workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalTable: "Workspaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SsoProviders_WorkspaceId_IsActive",
                table: "SsoProviders",
                columns: new[] { "WorkspaceId", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SsoProviders");
        }
    }
}
