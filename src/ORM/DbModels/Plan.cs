using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ORM.DbModels
{
    [Table("plan")]
    [Comment("Информация о плане сборки")]
    public class Plan
    {
        [Column("id")]
        [Comment("Идентификатор")]
        public long Id { get; set; }
        
        [Column("user_id")]
        [Comment("Id пользователя")]
        public long UserId { get; set; }
        
        public User User { get; set; }
        
        [Column("bamboo_plan_name")]
        [Comment("Имя плана сборки")]
        public string BambooPlanName { get; set; }
        
        [Column("build_start_count")]
        [Comment("Счётчик начала сборки плана")]
        public int BuildStartCount { get; set; }
        
        [Column("build_end_count")]
        [Comment("Счётчик окончания сборки плана")]
        public int BuildEndCount { get; set; }
        
        [Column("related_chat_id")]
        [Comment("Id чата в который соотносится с данным планом сборки")]
        public ulong? RelatedChatId { get; set; }
        
        [Column("previous_commits")]
        [Comment("Коммиты предыдущей сборки (Подразумевается)")]
        public string PreviousCommits { get; set; }
    }
}