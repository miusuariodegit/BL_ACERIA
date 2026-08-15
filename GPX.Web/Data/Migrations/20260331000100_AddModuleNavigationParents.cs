using Microsoft.EntityFrameworkCore.Migrations;

// [GPX-DOC-v1] ================================================================================
// Migracion que anade la jerarquia padre/hijo a AppModules para el menu de navegacion.
// ================================================================================================

#nullable disable

namespace GPX.Web.Migrations {
    /// <summary>
    /// Clase AddModuleNavigationParents. Migracion que anade la jerarquia padre/hijo a AppModules para el
    /// menu de navegacion.
    /// </summary>
    public partial class AddModuleNavigationParents : Migration {
        /// <summary>
        /// Up.
        /// </summary>
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.AddColumn<string>(
                name: "ParentCode",
                table: "AppModules",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ParentIconCssClass",
                table: "AppModules",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ParentName",
                table: "AppModules",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ParentDisplayOrder",
                table: "AppModules",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <summary>
        /// Down.
        /// </summary>
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropColumn(
                name: "ParentCode",
                table: "AppModules");

            migrationBuilder.DropColumn(
                name: "ParentIconCssClass",
                table: "AppModules");

            migrationBuilder.DropColumn(
                name: "ParentName",
                table: "AppModules");

            migrationBuilder.DropColumn(
                name: "ParentDisplayOrder",
                table: "AppModules");
        }
    }
}
