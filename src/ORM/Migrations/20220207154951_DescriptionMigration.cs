using Microsoft.EntityFrameworkCore.Migrations;

namespace ORM.Migrations
{
    public partial class DescriptionMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterTable(
                name: "user_info",
                comment: "Информация о пользователе/сервере discord/проекте");

            migrationBuilder.AlterTable(
                name: "user",
                comment: "Пользователи (администраторы)");

            migrationBuilder.AlterTable(
                name: "stend",
                comment: "Информация о стендах сборки");

            migrationBuilder.AlterTable(
                name: "plan",
                comment: "Информация о плане сборки");

            migrationBuilder.AlterColumn<long>(
                name: "user_id",
                table: "user_info",
                type: "INTEGER",
                nullable: false,
                comment: "Id пользователя",
                oldClrType: typeof(long),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<ulong>(
                name: "server_id",
                table: "user_info",
                type: "INTEGER",
                nullable: true,
                comment: "Id сервера discord",
                oldClrType: typeof(ulong),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "plugin_name",
                table: "user_info",
                type: "TEXT",
                nullable: true,
                comment: "Наименование плагина",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<ulong>(
                name: "main_chat_id",
                table: "user_info",
                type: "INTEGER",
                nullable: true,
                comment: "Id главного чата",
                oldClrType: typeof(ulong),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "jira_token",
                table: "user_info",
                type: "TEXT",
                nullable: true,
                comment: "Токен для обращения к Jira",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "bamboo_token",
                table: "user_info",
                type: "TEXT",
                nullable: true,
                comment: "Токен для обращения к Bamboo",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "bamboo_project_name",
                table: "user_info",
                type: "TEXT",
                nullable: true,
                comment: "Наименование проекта в системе Atlassian",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "id",
                table: "user_info",
                type: "INTEGER",
                nullable: false,
                comment: "Идентификатор",
                oldClrType: typeof(long),
                oldType: "INTEGER")
                .Annotation("Sqlite:Autoincrement", true)
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AlterColumn<string>(
                name: "login",
                table: "user",
                type: "TEXT",
                nullable: true,
                comment: "Логин",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "id",
                table: "user",
                type: "INTEGER",
                nullable: false,
                comment: "Идентификатор",
                oldClrType: typeof(long),
                oldType: "INTEGER")
                .Annotation("Sqlite:Autoincrement", true)
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AlterColumn<long>(
                name: "user_id",
                table: "stend",
                type: "INTEGER",
                nullable: false,
                comment: "Id пользователя",
                oldClrType: typeof(long),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "url",
                table: "stend",
                type: "TEXT",
                nullable: true,
                comment: "Адрес для вытаскивания информации о стенде",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "stend_name",
                table: "stend",
                type: "TEXT",
                nullable: true,
                comment: "Имя (наименование стенда)",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "id",
                table: "stend",
                type: "INTEGER",
                nullable: false,
                comment: "Идентификатор",
                oldClrType: typeof(long),
                oldType: "INTEGER")
                .Annotation("Sqlite:Autoincrement", true)
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AlterColumn<long>(
                name: "user_id",
                table: "plan",
                type: "INTEGER",
                nullable: false,
                comment: "Id пользователя",
                oldClrType: typeof(long),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<ulong>(
                name: "related_chat_id",
                table: "plan",
                type: "INTEGER",
                nullable: true,
                comment: "Id чата в который соотносится с данным планом сборки",
                oldClrType: typeof(ulong),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "previous_commits",
                table: "plan",
                type: "TEXT",
                nullable: true,
                comment: "Коммиты предыдущей сборки (Подразумевается)",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "build_start_count",
                table: "plan",
                type: "INTEGER",
                nullable: false,
                comment: "Счётчик начала сборки плана",
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<int>(
                name: "build_end_count",
                table: "plan",
                type: "INTEGER",
                nullable: false,
                comment: "Счётчик окончания сборки плана",
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "bamboo_plan_name",
                table: "plan",
                type: "TEXT",
                nullable: true,
                comment: "Имя плана сборки",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "id",
                table: "plan",
                type: "INTEGER",
                nullable: false,
                comment: "Идентификатор",
                oldClrType: typeof(long),
                oldType: "INTEGER")
                .Annotation("Sqlite:Autoincrement", true)
                .OldAnnotation("Sqlite:Autoincrement", true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterTable(
                name: "user_info",
                oldComment: "Информация о пользователе/сервере discord/проекте");

            migrationBuilder.AlterTable(
                name: "user",
                oldComment: "Пользователи (администраторы)");

            migrationBuilder.AlterTable(
                name: "stend",
                oldComment: "Информация о стендах сборки");

            migrationBuilder.AlterTable(
                name: "plan",
                oldComment: "Информация о плане сборки");

            migrationBuilder.AlterColumn<long>(
                name: "user_id",
                table: "user_info",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER",
                oldComment: "Id пользователя");

            migrationBuilder.AlterColumn<ulong>(
                name: "server_id",
                table: "user_info",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(ulong),
                oldType: "INTEGER",
                oldNullable: true,
                oldComment: "Id сервера discord");

            migrationBuilder.AlterColumn<string>(
                name: "plugin_name",
                table: "user_info",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true,
                oldComment: "Наименование плагина");

            migrationBuilder.AlterColumn<ulong>(
                name: "main_chat_id",
                table: "user_info",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(ulong),
                oldType: "INTEGER",
                oldNullable: true,
                oldComment: "Id главного чата");

            migrationBuilder.AlterColumn<string>(
                name: "jira_token",
                table: "user_info",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true,
                oldComment: "Токен для обращения к Jira");

            migrationBuilder.AlterColumn<string>(
                name: "bamboo_token",
                table: "user_info",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true,
                oldComment: "Токен для обращения к Bamboo");

            migrationBuilder.AlterColumn<string>(
                name: "bamboo_project_name",
                table: "user_info",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true,
                oldComment: "Наименование проекта в системе Atlassian");

            migrationBuilder.AlterColumn<long>(
                name: "id",
                table: "user_info",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER",
                oldComment: "Идентификатор")
                .Annotation("Sqlite:Autoincrement", true)
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AlterColumn<string>(
                name: "login",
                table: "user",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true,
                oldComment: "Логин");

            migrationBuilder.AlterColumn<long>(
                name: "id",
                table: "user",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER",
                oldComment: "Идентификатор")
                .Annotation("Sqlite:Autoincrement", true)
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AlterColumn<long>(
                name: "user_id",
                table: "stend",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER",
                oldComment: "Id пользователя");

            migrationBuilder.AlterColumn<string>(
                name: "url",
                table: "stend",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true,
                oldComment: "Адрес для вытаскивания информации о стенде");

            migrationBuilder.AlterColumn<string>(
                name: "stend_name",
                table: "stend",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true,
                oldComment: "Имя (наименование стенда)");

            migrationBuilder.AlterColumn<long>(
                name: "id",
                table: "stend",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER",
                oldComment: "Идентификатор")
                .Annotation("Sqlite:Autoincrement", true)
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AlterColumn<long>(
                name: "user_id",
                table: "plan",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER",
                oldComment: "Id пользователя");

            migrationBuilder.AlterColumn<ulong>(
                name: "related_chat_id",
                table: "plan",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(ulong),
                oldType: "INTEGER",
                oldNullable: true,
                oldComment: "Id чата в который соотносится с данным планом сборки");

            migrationBuilder.AlterColumn<string>(
                name: "previous_commits",
                table: "plan",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true,
                oldComment: "Коммиты предыдущей сборки (Подразумевается)");

            migrationBuilder.AlterColumn<int>(
                name: "build_start_count",
                table: "plan",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldComment: "Счётчик начала сборки плана");

            migrationBuilder.AlterColumn<int>(
                name: "build_end_count",
                table: "plan",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldComment: "Счётчик окончания сборки плана");

            migrationBuilder.AlterColumn<string>(
                name: "bamboo_plan_name",
                table: "plan",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true,
                oldComment: "Имя плана сборки");

            migrationBuilder.AlterColumn<long>(
                name: "id",
                table: "plan",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER",
                oldComment: "Идентификатор")
                .Annotation("Sqlite:Autoincrement", true)
                .OldAnnotation("Sqlite:Autoincrement", true);
        }
    }
}
