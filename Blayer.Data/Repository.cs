using System;
using System.Data.Entity;
using System.Linq;
using Blayer.Data.Utils;

namespace Blayer.Data
{
    /// <summary>
    /// Repository base class
    /// </summary>
    public abstract class Repository : IRepository
    {
        // Custom DbContext class
        private Context _context;
        // App's context
        private BlayerContext _blayerContext;

        /// <summary>
        /// Retrieves the entity type related to this repository
        /// </summary>
        /// <returns>Entity type</returns>
        public abstract Type GetEntityType();

        /// <summary>
        /// Sets the DbContext custom class
        /// </summary>
        /// <param name="context"></param>
        public void SetContext(Context context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves the DbContext custom class
        /// </summary>
        /// <returns>DbContext custom class</returns>
        public Context GetContext()
        {
            return _context;
        }

        /// <summary>
        /// Sets the app's context
        /// </summary>
        /// <param name="ctx">AppContext</param>
        public void SetAppContext(BlayerContext ctx)
        {
            _blayerContext = ctx;
        }

        /// <summary>
        /// Retrieves the app's context
        /// </summary>
        /// <returns>AppContext</returns>
        public BlayerContext GetAppContext()
        {
            return _blayerContext;
        }

        /// <summary>
        /// Retrieves the model configuration set for this repository, if any
        /// </summary>
        /// <returns>Model configuration</returns>
        public virtual IModelConfiguration GetConfiguration()
        {
            return null;
        }

        /// <summary>
        /// Retrieves the notification steps set for this repository, if any
        /// </summary>
        /// <returns>Notification steps</returns>
        public virtual INotify GetNotify()
        {
            return null;
        }

        /// <summary>
        /// Retrieves the validation steps set for this repository, if any
        /// </summary>
        /// <returns></returns>
        public virtual IValidate GetValidate()
        {
            return null;
        }

        /// <summary>
        /// Retrieves the additional steps set for this repository, if any
        /// </summary>
        /// <returns></returns>
        public virtual IAdditionalStep GetAdditionalStep()
        {
            return null;
        }
    }

    /// <summary>
    /// Typed repository base class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Repository<T> : Repository, IRepository<T>
        where T : EntityBase
    {
        private IDbSet<T> _dbset;

        private IDbSet<T> DbSet => _dbset ?? (_dbset = GetContext().Set<T>());

        /// <summary>
        /// Retrieves the entity type related to this repository
        /// </summary>
        /// <returns>Entity type</returns>
        public override Type GetEntityType()
        {
            return typeof(T);
        }

        /// <summary>
        /// Adds a new, empty entity to the context
        /// </summary>
        /// <returns>Entity attached to the context</returns>
        public virtual T Add()
        {
            return DbSet.Add(DbSet.Create());
        }

        /// <summary>
        /// Adds a new entity to the context with the provided data
        /// </summary>
        /// <typeparam name="T">Type of the entity</typeparam>
        /// <param name="entity">Entity's data</param>
        /// <returns>Entity attached to the context</returns>
        public virtual T Add(T entity)
        {
            var temp = Add();
            PropertyCopy.CopyValues(entity, temp);

            return DbSet.Add(temp);
        }

        /// <summary>
        /// Updates an entity
        /// </summary>
        /// <typeparam name="T">Type of the entity</typeparam>
        /// <param name="entity">Entity's data</param>
        /// <param name="changedProperties">Properties changed</param>
        /// <returns>Entity attached to the context</returns>
        public virtual T Update(T entity, string[] changedProperties)
        {
            var proxy = GetContext().Entry(entity);

            proxy.State = EntityState.Modified;

            return entity;
        }

        /// <summary>
        /// Removes an entity for removal
        /// </summary>
        /// <typeparam name="T">Type of the entity</typeparam>
        /// <param name="entity">Entity's data</param>
        public virtual void Delete(T entity)
        {
            DbSet.Remove(entity);
        }

        /// <summary>
        /// Retrieves an entity by its numeric ID
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns>Entity</returns>
        public virtual T GetById(long id)
        {
            return DbSet.Find(id);
        }

        /// <summary>
        /// Retrieves an entity by its alphanumeric ID
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns>Entity</returns>
        public virtual T GetById(string id)
        {
            return DbSet.Find(id.ToLower());
        }

        /// <summary>
        /// Retrieves all entities of the type
        /// </summary>
        /// <returns>All entities</returns>
        public virtual IQueryable<T> GetAll()
        {
            return DbSet;
        }

        /// <summary>
        /// Saves the changes to the context
        /// </summary>
        public void Save()
        {
            GetContext().SaveChanges();
        }
    }
}
