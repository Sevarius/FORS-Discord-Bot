using System.ComponentModel.DataAnnotations.Schema;

namespace ORM.DbModels
{
    [Table("user_info")]
    public class UserInfo
    {
        [Column("id")]
        public long Id { get; set; }
        
        [Column("user_id")]
        public long UserId { get; set; }

        public User User { get; set; }

        [Column("server_id")]
        public ulong? ServerId { get; set; }
        
        [Column("main_chat_id")]
        public ulong? MainChatId { get; set; }
        
        [Column("bamboo_project_name")]
        public string BambooProjectName { get; set; }
        
        [Column("bamboo_token")]
        public string BambooToken { get; set; }
        
        [Column("plugin_name")]
        public string PluginName { get; set; }
        
        [Column("jira_token")]
        public string JiraToken { get; set; }
    }
}