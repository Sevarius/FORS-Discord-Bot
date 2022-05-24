using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ORM.DbModels
{
    [Table("user")]
    [Comment("Пользователи (администраторы)")]
    public class User
    {
        [Column("id")]
        [Comment("Идентификатор")]
        public long Id { get; set; }
        
        [Column("login")]
        [Comment("Логин")]
        public string Name { get; set; }
        
        public List<Stend> Stends { get; set; }
        
        public List<Plan> Plans { get; set; }
        
        public UserInfo Info { get; set; }
    }
}