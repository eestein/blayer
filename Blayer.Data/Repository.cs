using System;
using System.Data.Entity;
using System.Linq;
using Blayer.Data.Utils;

namespace Blayer.Data
{
    public abstract class Repository : IRepository
    {
        private Context _context;
        private AppContext _appContext;

        public abstract Type GetEntityType();

        public void SetContext(Context context)
        {
            _context = context;
        }

        public Context GetContext()
        {
            return _context;
        }

        public void SetAppContext(AppContext ctx)
        {
            _appContext = ctx;
        }

        public AppContext GetAppContext()
        {
            return _appContext;
        }

        public virtual IModelConfiguration GetConfiguration()
        {
            return null;
        }

        public virtual INotify GetNotify()
        {
            return null;
        }

        public virtual IValidate GetValidate()
        {
            return null;
        }

        public virtual IAdditionalStep GetAdditionalStep()
        {
            return null;
        }
    }

    public class Repository<T> : Repository, IRepository<T>
        where T : EntityBase
    {
        private IDbSet<T> _dbset;

        private IDbSet<T> DbSet => _dbset ?? (_dbset = GetContext().Set<T>());

        public override Type GetEntityType()
        {
            return typeof(T);
        }

        public virtual T Add()
        {
            return DbSet.Add(DbSet.Create());
        }

        public virtual T Add(T entity)
        {
            var temp = Add();
            PropertyCopy.CopyValues(entity, temp);

            return DbSet.Add(temp);
        }

        public virtual T Update(T entity, string[] changedProperties)
        {
            var proxy = GetContext().Entry(entity);

            proxy.State = EntityState.Modified;

            return entity;
        }

        public virtual void Delete(T entity)
        {
            DbSet.Remove(entity);
        }

        public virtual T GetById(long id)
        {
            return DbSet.Find(id);
        }

        public virtual T GetById(string id)
        {
            return DbSet.Find(id.ToLower());
        }

        public virtual IQueryable<T> GetAll()
        {
            return DbSet;
        }

        public void Save()
        {
            GetContext().SaveChanges();
        }
    }
}
