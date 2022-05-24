using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ORM.DbModels
{
    [Table("stend")]
    [Comment("Информация о стендах сборки")]
    public class Stend
    {
        [Column("id")]
        [Comment("Идентификатор")]
        public long Id { get; set; }
        
        [Column("user_id")]
        [Comment("Id пользователя")]
        public long UserId { get; set; }
        
        public User User { get; set; }
        
        [Column("stend_name")]
        [Comment("Имя (наименование стенда)")]
        public string StendName { get; set; }
        
        [Column("url")]
        [Comment("Адрес для вытаскивания информации о стенде")]
        public string Url { get; set; }
    }
}