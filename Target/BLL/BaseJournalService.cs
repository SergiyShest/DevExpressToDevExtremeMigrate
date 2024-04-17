using Core;
using DAL.Core;
using System.Security.Principal;

namespace BLL
{
    public class BaseJournalService<T>  where T : class, IEntity
    {
        public BaseJournalService() { }

        public virtual async Task<IQueryable<T>> Get()
        {
            return  await Task.Run(()=>TestDataHelper.GetFilledEntities<T>(16));
        }

    }

}
