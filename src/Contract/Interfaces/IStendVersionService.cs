namespace Contract.Interfaces
{
    /// <summary>
    /// Класс для получения версии компонент на стенде
    /// </summary>
    public interface IStendVersionService
    {
        /// <summary>
        /// Получить json с версией компонент на стенде
        /// </summary>
        /// <param name="stendName">Имя стенда</param>
        /// <returns>json с результатом от сервера</returns>
        string GetVersionFromStend(string stendName);
    }
}