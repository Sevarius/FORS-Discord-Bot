using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ORM.DbModels
{
    [Table("user_info")]
    [Comment("Информация о пользователе/сервере discord/проекте")]
    public class UserInfo
    {
        [Column("id")]
        [Comment("Идентификатор")]
        public long Id { get; set; }
        
        [Column("user_id")]
        [Comment("Id пользователя")]
        public long UserId { get; set; }

        public User User { get; set; }

        [Column("server_id")]
        [Comment("Id сервера discord")]
        public ulong? ServerId { get; set; }
        
        [Column("main_chat_id")]
        [Comment("Id главного чата")]
        public ulong? MainChatId { get; set; }
        
        [Column("bamboo_project_name")]
        [Comment("Наименование проекта в системе Atlassian")]
        public string BambooProjectName { get; set; }
        
        [Column("bamboo_token")]
        [Comment("Токен для обращения к Bamboo")]
        public string BambooToken { get; set; }
        
        [Column("plugin_name")]
        [Comment("Наименование плагина")]
        public string PluginName { get; set; }
        
        [Column("jira_token")]
        [Comment("Токен для обращения к Jira")]
        public string JiraToken { get; set; }
    }
}