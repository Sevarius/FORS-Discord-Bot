using System.Collections.Generic;
using Contract.Bamboo;

namespace Contract.Interfaces
{
    /// <summary>
    /// Реализует получение информации о сборке планов с изменениями
    /// </summary>
    public interface IBambooBuildPlanService
    {
        /// <summary>
        /// Получить коммиты с изменениями для планов сборки
        /// </summary>
        /// <param name="planName">Имя плана сборки</param>
        /// <param name="count">Сколько последовательно планов надо выгрузить</param>
        /// <param name="start">Сколько планов надо пропустить начиная с последней сборки</param>
        /// <returns>Список изменений</returns>
        List<JiraIssue> GetCommitsForPlan(string planName, int count = 1, int start = 0);

        /// <summary>
        /// Получить полную информацию о сборках плана
        /// </summary>
        /// <param name="planName">Имя плана сборки</param>
        /// <param name="count">Сколько последовательно планов надо выгрузить</param>
        /// <param name="start">Сколько планов надо пропустить начиная с последней сборки</param>
        /// <returns>Список информации о планах сборки</returns>
        List<PlanInfo> GetPlanBuilds(string planName, int count = 1, int start = 0);
    }
}