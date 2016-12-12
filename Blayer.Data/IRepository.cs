using System;
using System.Linq;

namespace Blayer.Data
{
    public interface IRepository
    {
        void SetContext(Context context);

        Context GetContext();

        void SetAppContext(AppContext ctx);

        AppContext GetAppContext();

        Type GetEntityType();

        IModelConfiguration GetConfiguration();

        INotify GetNotify();

        IValidate GetValidate();

        IAdditionalStep GetAdditionalStep();
    }

    public interface IRepository<out T> : IRepository
        where T : class
    {
        IQueryable<T> GetAll();

        T GetById(long id);

        T GetById(string id);
    }
}
