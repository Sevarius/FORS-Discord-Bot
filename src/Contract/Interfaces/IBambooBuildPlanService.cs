using System.Collections.Generic;
using Contract.Bamboo;

namespace Contract.Interfaces
{
    public interface IBambooBuildPlanService
    {
        List<JiraIssue> GetCommitsForAllPlans();

        List<JiraIssue> GetCommitsForPlan(string planName, int count = 1, int start = 0);

        List<PlanInfo> GetPlanBuilds(string planName, int count = 1, int start = 0);
    }
}