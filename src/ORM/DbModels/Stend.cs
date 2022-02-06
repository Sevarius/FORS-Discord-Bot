using System.ComponentModel.DataAnnotations.Schema;

namespace ORM.DbModels
{
    [Table("stend")]
    public class Stend
    {
        [Column("id")]
        public long Id { get; set; }
        
        [Column("user_id")]
        public long UserId { get; set; }
        
        public User User { get; set; }
        
        [Column("stend_name")]
        public string StendName { get; set; }
        
        [Column("url")]
        public string Url { get; set; }
    }
}