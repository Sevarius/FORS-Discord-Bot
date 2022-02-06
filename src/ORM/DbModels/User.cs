using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ORM.DbModels
{
    [Table("user")]
    public class User
    {
        [Column("id")]
        public long Id { get; set; }
        
        [Column("login")]
        public string Name { get; set; }
        
        public List<Stend> Stends { get; set; }
        
        public List<Plan> Plans { get; set; }
        
        public UserInfo Info { get; set; }
    }
}