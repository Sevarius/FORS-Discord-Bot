using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using ORM.DbModels;

namespace ORM
{
    public class CommonRepository : ICommonRepository
    {
        private readonly MainContext _db;
        private string _plugin;

        public CommonRepository(MainContext db, IPluginGetter getter)
        {
            _db = db;
            SetInitInfo(getter);
        }

        private void SetInitInfo(IPluginGetter getter)
        {
            _plugin = getter.GetExecutingPluginName();
        }
        
        public UserInfo GerUserInfo()
        {
            var res = _db.UserInfos
                .AsQueryable()
                .FirstOrDefault(ui => ui.PluginName == _plugin);

            if (res == null)
            {
                throw new ApplicationException($"Не удалось получить информацию о пользователе для плагина {_plugin}");
            }

            return res;
        }

        public string GetJiraToken()
        {
            var res = _db.UserInfos.AsQueryable()
                .Where(ui => ui.PluginName == _plugin)
                .Select(ui => ui.JiraToken)
                .FirstOrDefault();

            if (res == null)
            {
                throw new ApplicationException($"Не удалось получить токет jira для плагина {_plugin}");
            }

            return res;
        }

        public string GetBambooToken()
        {
            var res = _db.UserInfos.AsQueryable()
                .Where(ui => ui.PluginName == _plugin)
                .Select(ui => ui.BambooToken)
                .FirstOrDefault();

            if (res == null)
            {
                throw new ApplicationException($"Не удалось получить токен bamboo для плагина {_plugin}");
            }

            return res;
        }

        public List<string> GetBambooPlans()
        {
            var res = _db.Plans.AsQueryable()
                .Where(p => p.User.Info.PluginName == _plugin)
                .Select(p => p.BambooPlanName)
                .ToList();

            if (res == null)
            {
                throw new ApplicationException($"Не удалось получить список планов bamboo для плагина {_plugin}");
            }

            return res;
        }

        public string GetBambooProjectName()
        {
            var res = _db.UserInfos.AsQueryable()
                .Where(ui => ui.PluginName == _plugin)
                .Select(ui => ui.BambooProjectName)
                .FirstOrDefault();

            if (res == null)
            {
                throw new ApplicationException($"Не удалось получить имя проекта bamboo для плагина {_plugin}");
            }

            return res;
        }
    }
}