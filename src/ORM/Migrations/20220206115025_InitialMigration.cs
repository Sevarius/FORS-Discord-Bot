using Microsoft.EntityFrameworkCore.Migrations;

namespace ORM.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    login = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "plan",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    user_id = table.Column<long>(type: "INTEGER", nullable: false),
                    bamboo_plan_name = table.Column<string>(type: "TEXT", nullable: true),
                    build_start_count = table.Column<int>(type: "INTEGER", nullable: false),
                    build_end_count = table.Column<int>(type: "INTEGER", nullable: false),
                    related_chat_id = table.Column<ulong>(type: "INTEGER", nullable: true),
                    previous_commits = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plan", x => x.id);
                    table.ForeignKey(
                        name: "FK_plan_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "stend",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    user_id = table.Column<long>(type: "INTEGER", nullable: false),
                    stend_name = table.Column<string>(type: "TEXT", nullable: true),
                    url = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stend", x => x.id);
                    table.ForeignKey(
                        name: "FK_stend_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_info",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    user_id = table.Column<long>(type: "INTEGER", nullable: false),
                    server_id = table.Column<ulong>(type: "INTEGER", nullable: true),
                    main_chat_id = table.Column<ulong>(type: "INTEGER", nullable: true),
                    bamboo_project_name = table.Column<string>(type: "TEXT", nullable: true),
                    bamboo_token = table.Column<string>(type: "TEXT", nullable: true),
                    plugin_name = table.Column<string>(type: "TEXT", nullable: true),
                    jira_token = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_info", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_info_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_plan_user_id",
                table: "plan",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_stend_user_id",
                table: "stend",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_info_user_id",
                table: "user_info",
                column: "user_id",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "plan");

            migrationBuilder.DropTable(
                name: "stend");

            migrationBuilder.DropTable(
                name: "user_info");

            migrationBuilder.DropTable(
                name: "user");
        }
    }
}
