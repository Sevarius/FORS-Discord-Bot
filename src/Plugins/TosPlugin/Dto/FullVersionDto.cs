using Newtonsoft.Json;

namespace TosPlugin.Dto
{
    /// <summary>
    /// Данные о версии компонент на стендах
    /// </summary>
    public class FullVersionDto
    {
        /// <summary>
        /// Версия КПИ
        /// </summary>
        [JsonProperty("kpi_version")]
        public string KpiVersion { get; set; }

        /// <summary>
        /// Версия API Интеграции
        /// </summary>
        [JsonProperty("api_version")]
        public string ApiVersion { get; set; }

        /// <summary>
        /// Дата формировании версии БД
        /// </summary>
        [JsonProperty("db_model_date")]
        public string DbModelDate { get; set; }

        /// <summary>
        /// Версия фронта
        /// </summary>
        [JsonProperty("front_version")]
        public string FrontVersion { get; set; }
    }
}