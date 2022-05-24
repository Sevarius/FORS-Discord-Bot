namespace Contract.Interfaces
{
    /// <summary>
    /// Реализует проставление тегов задачам
    /// </summary>
    public interface IJiraLabelsService
    {
        /// <summary>
        /// Добавить тег задаче
        /// </summary>
        /// <param name="taskKey">Код задачи</param>
        /// <param name="labels">список тегов</param>
        void PutLabelsToTask(string taskKey, params string[] labels);
    }
}