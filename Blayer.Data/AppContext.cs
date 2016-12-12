using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using Blayer.Data.Utils;

namespace Blayer.Data
{
    public class AppContext : IDisposable
    {
        private readonly Context _context;
        private readonly Dictionary<Type, IRepository> _repositories;
        private readonly List<DbEntityEntry> _skipBeforeSaveList;
        private readonly List<LoadedEntity> _originalEntityEntries;
        private bool _isDisposed;

        /// <summary>
        /// Usuário logado
        /// </summary>
        public object LoggedUser { get; set; }

        #region Construtores

        private AppContext(IEnumerable<IRepository> repositories)
        {
            _skipBeforeSaveList = new List<DbEntityEntry>();
            _originalEntityEntries = new List<LoadedEntity>();
            _repositories = new Dictionary<Type, IRepository>();

            foreach (var rep in repositories)
            {
                _repositories.Add(rep.GetEntityType(), rep);
            }
        }

        public AppContext(RepositoryConfiguration config)
            : this(config.GetRepositories())
        {
            _context = new Context(_repositories);

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

        [Obsolete("não está pronto ainda, falta ver como colocar as duas coleções de repositórios em uma")]
        public AppContext(RepositoryConfiguration config, IEnumerable<IRepository> repositories)
            : this(config.GetRepositories())
        {
            foreach (var rep in config.GetRepositories())
            {
                rep.SetContext(_context);
                rep.SetAppContext(this);
                _repositories.Add(rep.GetEntityType(), rep);
            }

            foreach (var rep in repositories)
            {
                rep.SetContext(_context);
                rep.SetAppContext(this);
                _repositories.Add(rep.GetEntityType(), rep);
            }
        }

        #endregion

        #region GetRepository

        public TReturn GetRepository<T, TReturn>()
            where T : EntityBase
            where TReturn : Repository<T>
        {
            IRepository repository;
            _repositories.TryGetValue(typeof(T), out repository);

            if (repository == null)
                throw new Exception($"Repositório '{typeof(T)}' não encontrado.");

            return (TReturn)repository;
        }

        public IRepository<T> GetRepository<T>()
            where T : EntityBase
        {
            IRepository repository;
            _repositories.TryGetValue(typeof(T), out repository);

            if (repository == null)
                throw new Exception($"Repositório '{typeof(T)}' não encontrado.");

            return (IRepository<T>)repository;
        }

        #endregion

        #region CRUD

        public T Add<T>(T entity)
            where T : EntityBase
        {
            var repository = (Repository<T>)GetRepository<T>();

            entity = repository.Add(entity);
            entity.Context = this;

            return entity;
        }

        public T Add<T>()
            where T : EntityBase
        {
            var repository = (Repository<T>)GetRepository<T>();

            var entity = repository.Add();
            entity.Context = this;

            return entity;
        }

        public T Update<T>(T entity, string[] changedProperties)
            where T : EntityBase
        {
            var repository = (Repository<T>)GetRepository<T>();

            entity.Context = this;

            return repository.Update(entity, changedProperties);
        }

        public void Delete<T>(T entity)
            where T : EntityBase
        {
            entity.Context = this;
            entity.WillBeDeleted = true;
        }

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
        /// Salva as entidades no contexto
        /// </summary>
        /// <param name="disposeAfter">Se falso mantém as entidades em memória para acesso posterior, caso contrário exclui o contexto da memória.</param>
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

        public void Dispose()
        {
            if (!_isDisposed)
                _context.Dispose();
        }

        public IEnumerable<T> Search<T>()
        {
            return _context.Database.SqlQuery<T>(
                "SELECT VALUE UDF.UserDefinedFunction(@someParameter) FROM {1}",
                new ObjectParameter("someParameter", ""));
        }

        [Obsolete("Temporário, não utilizar")]
        public void Reload<T>(IEnumerable<T> collection)
        {
            var oc = ((IObjectContextAdapter)_context).ObjectContext;
            oc.Refresh(RefreshMode.StoreWins, collection);
        }

        /// <summary>
        /// Realiza as operações necessárias antes de salvar os dados.
        /// Inclui: 
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
