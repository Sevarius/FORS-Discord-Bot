using System.Linq;
using Common;
using ORM.DbModels;

namespace ORM
{
    public class BambooPlanRepository : IBambooPlanRepository
    {
        private readonly MainContext _db;
        private string _plugin;

        public BambooPlanRepository(MainContext db, IPluginGetter getter)
        {
            _db = db;
            SetInitInfo(getter);
        }
        
        private void SetInitInfo(IPluginGetter getter)
        {
            _plugin = getter.GetExecutingPluginName();
        }

        public Plan GetPlanInfo(string planName)
        {
            var res = _db.Plans
                .AsQueryable()
                .Where(p => p.User.Info.PluginName == _plugin)
                .FirstOrDefault(p => p.BambooPlanName == planName);

            return res;
        }

        public void UpdatePlan(Plan plan)
        {
            _db.SaveChanges();
        }
    }
}