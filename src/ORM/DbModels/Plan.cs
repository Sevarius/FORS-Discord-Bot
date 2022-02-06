using System.ComponentModel.DataAnnotations.Schema;

namespace ORM.DbModels
{
    [Table("plan")]
    public class Plan
    {
        [Column("id")]
        public long Id { get; set; }
        
        [Column("user_id")]
        public long UserId { get; set; }
        
        public User User { get; set; }
        
        [Column("bamboo_plan_name")]
        public string BambooPlanName { get; set; }
        
        [Column("build_start_count")]
        public int BuildStartCount { get; set; }
        
        [Column("build_end_count")]
        public int BuildEndCount { get; set; }
        
        [Column("related_chat_id")]
        public ulong? RelatedChatId { get; set; }
        
        [Column("previous_commits")]
        public string PreviousCommits { get; set; }
    }
}