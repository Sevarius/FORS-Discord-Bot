using ORM.DbModels;

namespace ORM
{
    public interface IBambooPlanRepository
    {
        Plan GetPlanInfo(string planName);
        
        void UpdatePlan(Plan plan);
    }
}