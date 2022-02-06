using System.Collections.Generic;
using ORM.DbModels;

namespace ORM
{
    public interface ICommonRepository
    {
        UserInfo GerUserInfo();

        string GetJiraToken();

        string GetBambooToken();

        List<string> GetBambooPlans();

        string GetBambooProjectName();
    }
}