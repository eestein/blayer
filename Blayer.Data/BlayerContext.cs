using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using Blayer.Data.Utils;

namespace Blayer.Data
{
    public class BlayerContext : IDisposable
    {
        private readonly Context _context;
        private readonly Dictionary<Type, IRepository> _repositories;
        private readonly List<DbEntityEntry> _skipBeforeSaveList;
        private readonly List<LoadedEntity> _originalEntityEntries;
        private bool _isDisposed;

        /// <summary>
        /// Logged user
        /// </summary>
        public object LoggedUser { get; set; }

        #region Constructors

        /// <summary>
        /// Creates a new instance using the informed repositories
        /// </summary>
        /// <param name="repositories">Repositories</param>
        private BlayerContext(IEnumerable<IRepository> repositories)
        {
            _skipBeforeSaveList = new List<DbEntityEntry>();
            _originalEntityEntries = new List<LoadedEntity>();
            _repositories = new Dictionary<Type, IRepository>();

            foreach (var rep in repositories)
            {
                _repositories.Add(rep.GetEntityType(), rep);
            }
        }

        /// <summary>
        /// Creates a new instance using repositories from the configuration
        /// </summary>
        /// <param name="config">Repository configuration</param>
        /// <param name="repositories">Extra repositories to be loaded</param>
        public BlayerContext(RepositoryConfiguration config, params IRepository[] repositories)
            : this(config.GetRepositories().Concat(repositories))
        {
            _context = new Context(_repositories, config.ConnectionString);

            ((IObjectContextAdapter)_context).ObjectContext.ObjectMaterialized += (sender, e) =>
            {
                ((EntityBase)e.Entity).Context = this;
            };

            _context.Database.Connection.Disposed += (sender, args) =>
            {
                _isDisposed = true;
            };

            foreach (var rep in _repositories)
            {
                rep.Value.SetContext(_context);
                rep.Value.SetAppContext(this);
            }
        }

        #endregion

        #region GetRepository

        /// <summary>
        /// Retrieves a repository casting to a specific type
        /// </summary>
        /// <typeparam name="T">Repository base class type</typeparam>
        /// <typeparam name="TReturn">Repository type</typeparam>
        /// <returns>Repository converted to the chosen return type</returns>
        public TReturn GetRepository<T, TReturn>()
            where T : EntityBase
            where TReturn : Repository<T>
        {
            IRepository repository;
            _repositories.TryGetValue(typeof(T), out repository);

            if (repository == null)
                throw new Exception($"Repository '{typeof(T)}' not found.");

            return (TReturn)repository;
        }

        /// <summary>
        /// Retrieves a repository
        /// </summary>
        /// <typeparam name="T">Repository base class type</typeparam>
        /// <returns>Repository</returns>
        public IRepository<T> GetRepository<T>()
            where T : EntityBase
        {
            IRepository repository;
            _repositories.TryGetValue(typeof(T), out repository);

            if (repository == null)
                throw new Exception($"Repository '{typeof(T)}' not found.");

            return (IRepository<T>)repository;
        }

        #endregion

        #region CRUD

        /// <summary>
        /// Adds a new entity to the context with the provided data
        /// </summary>
        /// <typeparam name="T">Type of the entity</typeparam>
        /// <param name="entity">Entity's data</param>
        /// <returns>Entity attached to the context</returns>
        public T Add<T>(T entity)
            where T : EntityBase
        {
            var repository = (Repository<T>)GetRepository<T>();

            entity = repository.Add(entity);
            entity.Context = this;

            return entity;
        }

        /// <summary>
        /// Adds a new, empty entity to the context
        /// </summary>
        /// <typeparam name="T">Type of the entity</typeparam>
        /// <returns>Entity attached to the context</returns>
        public T Add<T>()
            where T : EntityBase
        {
            var repository = (Repository<T>)GetRepository<T>();

            var entity = repository.Add();
            entity.Context = this;

            return entity;
        }

        /// <summary>
        /// Updates an entity
        /// </summary>
        /// <typeparam name="T">Type of the entity</typeparam>
        /// <param name="entity">Entity's data</param>
        /// <param name="changedProperties">Properties changed</param>
        /// <returns>Entity attached to the context</returns>
        [Obsolete("Don't use it for now. Weird behavior, wait until fixed. Meanwhile use the approach found here https://github.com/eestein/blayer/wiki/Performing-CRUD-operations#update")]
        public T Update<T>(T entity, string[] changedProperties)
            where T : EntityBase
        {
            var repository = (Repository<T>)GetRepository<T>();

            entity.Context = this;

            return repository.Update(entity, changedProperties);
        }

        /// <summary>
        /// Marks an entity for removal
        /// </summary>
        /// <typeparam name="T">Type of the entity</typeparam>
        /// <param name="entity">Entity's data</param>
        public void Delete<T>(T entity)
            where T : EntityBase
        {
            entity.Context = this;
            entity.WillBeDeleted = true;
        }

        /// <summary>
        /// Marks a group of entities for removal
        /// </summary>
        /// <typeparam name="T">Entity's data</typeparam>
        /// <param name="where">Expression to filter data to be removed</param>
        public void DeleteWhere<T>(Expression<Func<T, bool>> where)
            where T : EntityBase
        {
            var repository = (Repository<T>)GetRepository<T>();
            var objects = repository.GetAll().Where(where).AsEnumerable();

            foreach (var entity in objects)
                entity.WillBeDeleted = true;
        }

        #endregion

        /// <summary>
        /// Saves the attached entities
        /// </summary>
        /// <param name="disposeAfter">If true, maintains the entities in memory for further access, else removes context from memory.</param>
        public void Save(bool disposeAfter = true)
        {
            DoBeforeSave();
            _context.SaveChanges();
            DoAfterSave();

            _skipBeforeSaveList.Clear();
            _originalEntityEntries.Clear();

            if (disposeAfter)
                _context.Dispose();
        }

        /// <summary>
        /// Removes the context from memory
        /// </summary>
        public void Dispose()
        {
            if (!_isDisposed)
                _context.Dispose();
        }

        /// <summary>
        /// Executes a specific query on the database.
        /// Ex: query = "SELECT VALUE UDF.UserDefinedFunction(@someParameter) FROM ..."
        ///     parameters = new ObjectParameter("someParameter", "some value")
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="query">Query to be executed</param>
        /// <param name="parameters">Query parameters</param>
        /// <returns></returns>
        public IEnumerable<T> ExecuteQuery<T>(string query, params SqlParameter[] parameters)
        {
            return _context.Database.SqlQuery<T>(query, parameters);
        }

        /// <summary>
        /// Runs the necessary operations, if any, before saving.
        /// - Validations and additional steps
        /// </summary>
        private void DoBeforeSave()
        {
            foreach (var entry in _context.ChangeTracker.Entries().Where(e => !_skipBeforeSaveList.Contains(e) && ((e.State != EntityState.Unchanged && e.State != EntityState.Detached) || ((EntityBase)e.Entity).WillBeDeleted)))
            {
                _skipBeforeSaveList.Add(entry);

                var entityType = entry.Entity.GetType().BaseType.Name;

                if (entityType == nameof(EntityBase))
                    entityType = entry.Entity.GetType().Name;

                var keyValuePair = _repositories.FirstOrDefault(kvp => kvp.Key.Name == entityType);

                if (keyValuePair.Value == null)
                    continue;

                var repository = keyValuePair.Value;
                var type = repository.GetEntityType();
                var additionalStep = repository.GetAdditionalStep();
                var validate = repository.GetValidate();
                var loadedEntity = new LoadedEntity { Entry = entry };

                if (((EntityBase)entry.Entity).WillBeDeleted)
                {
                    loadedEntity.OriginalState = EntityState.Deleted;
                    validate?.Validate(EntityState.Deleted, entry.Entity, null);
                    additionalStep?.Execute(EntityState.Deleted, entry.Entity, null);

                    DoBeforeSave();

                    // If the item was removed virtually or it's just not going to be
                    // physically removed anymore we skip the database removal.
                    if (((EntityBase)entry.Entity).WillBeDeleted)
                        repository.InvokeMethod("Delete", entry.Entity);
                }
                else
                {
                    var originalEntity = entry.State == EntityState.Modified ? GetOriginalEntity(entry, type) : null;

                    loadedEntity.OriginalState = entry.State;
                    loadedEntity.OriginalEntity = originalEntity;
                    validate?.Validate(entry.State, entry.Entity, originalEntity);
                    additionalStep?.Execute(entry.State, entry.Entity, originalEntity);

                    DoBeforeSave();
                }

                _originalEntityEntries.Add(loadedEntity);
            }
        }

        /// <summary>
        /// Runs the necessary operations, if any, after saving.
        /// - Notifications
        /// </summary>
        private void DoAfterSave()
        {
            foreach (var dbEntityEntry in _originalEntityEntries)
            {
                var entityType = dbEntityEntry.Entry.Entity.GetType().BaseType.Name;

                if (entityType == nameof(EntityBase))
                    entityType = dbEntityEntry.Entry.Entity.GetType().Name;

                var keyValuePair = _repositories.FirstOrDefault(kvp => kvp.Key.Name == entityType);

                if (keyValuePair.Value == null)
                    continue;

                var repository = keyValuePair.Value;
                var notify = repository.GetNotify();

                notify?.Notify(dbEntityEntry.OriginalState, dbEntityEntry.Entry.Entity, dbEntityEntry.OriginalEntity);
            }
        }

        /// <summary>
        /// Retrieves the original, unmodified entity for comparison
        /// </summary>
        /// <param name="entry">DbEntityEntry referring to the entity</param>
        /// <param name="type">Type of the entity</param>
        /// <returns></returns>
        private static object GetOriginalEntity(DbEntityEntry entry, Type type)
        {
            var originalEntity = Activator.CreateInstance(type);

            for (var i = 0; i < entry.OriginalValues.PropertyNames.Count(); i++)
            {
                var propertyName = entry.OriginalValues.PropertyNames.ElementAt(i);
                var property = originalEntity.GetType().GetProperty(propertyName);

                property.SetValue(originalEntity, entry.OriginalValues[propertyName]);
            }

            return originalEntity;
        }
    }
}
