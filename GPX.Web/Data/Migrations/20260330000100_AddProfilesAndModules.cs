using GPX.Web.Data;
using Microsoft.EntityFrameworkCore.Migrations;

// [GPX-DOC-v1] ================================================================================
// Migracion que anade las tablas AppProfiles, AppModules y AppProfileModules.
// ================================================================================================

#nullable disable

namespace GPX.Web.Migrations {
    [Microsoft.EntityFrameworkCore.Infrastructure.DbContext(typeof(ApplicationDbContext))]
    [Migration("20260330000100_AddProfilesAndModules")]
    /// <summary>
    /// Clase AddProfilesAndModules. Migracion que anade las tablas AppProfiles, AppModules y
    /// AppProfileModules.
    /// </summary>
    public partial class AddProfilesAndModules : Migration {
        /// <summary>
        /// Up.
        /// </summary>
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.CreateTable(
                name: "AppModules",
                columns: table => new {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Route = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    IconCssClass = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_AppModules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppProfiles",
                columns: table => new {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_AppProfiles", x => x.Id);
                });

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "AspNetUsers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ProfileId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AppProfileModules",
                columns: table => new {
                    ProfileId = table.Column<int>(type: "int", nullable: false),
                    ModuleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_AppProfileModules", x => new { x.ProfileId, x.ModuleId });
                    table.ForeignKey(
                        name: "FK_AppProfileModules_AppModules_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "AppModules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppProfileModules_AppProfiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "AppProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ProfileId",
                table: "AspNetUsers",
                column: "ProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_AppModules_Code",
                table: "AppModules",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppModules_Route",
                table: "AppModules",
                column: "Route",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppProfileModules_ModuleId",
                table: "AppProfileModules",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_AppProfiles_Name",
                table: "AppProfiles",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_AppProfiles_ProfileId",
                table: "AspNetUsers",
                column: "ProfileId",
                principalTable: "AppProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <summary>
        /// Down.
        /// </summary>
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_AppProfiles_ProfileId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "AppProfileModules");

            migrationBuilder.DropTable(
                name: "AppModules");

            migrationBuilder.DropTable(
                name: "AppProfiles");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_ProfileId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ProfileId",
                table: "AspNetUsers");
        }
    }
}
