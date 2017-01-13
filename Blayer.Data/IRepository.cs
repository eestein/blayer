using System;
using System.Linq;

namespace Blayer.Data
{
    /// <summary>
    /// Interface for the base repository
    /// </summary>
    public interface IRepository
    {
        /// <summary>
        /// Sets the DbContext custom class
        /// </summary>
        /// <param name="context"></param>
        void SetContext(Context context);

        /// <summary>
        /// Retrieves the DbContext custom class
        /// </summary>
        /// <returns>DbContext custom class</returns>
        Context GetContext();

        /// <summary>
        /// Sets the app's context
        /// </summary>
        /// <param name="ctx">AppContext</param>
        void SetAppContext(BlayerContext ctx);

        /// <summary>
        /// Retrieves the app's context
        /// </summary>
        /// <returns>AppContext</returns>
        BlayerContext GetAppContext();

        /// <summary>
        /// Retrieves the entity type related to this repository
        /// </summary>
        /// <returns>Entity type</returns>
        Type GetEntityType();

        /// <summary>
        /// Retrieves the model configuration set for this repository, if any
        /// </summary>
        /// <returns>Model configuration</returns>
        IModelConfiguration GetConfiguration();

        /// <summary>
        /// Retrieves the notification steps set for this repository, if any
        /// </summary>
        /// <returns>Notification steps</returns>
        INotify GetNotify();

        /// <summary>
        /// Retrieves the validation steps set for this repository, if any
        /// </summary>
        /// <returns></returns>
        IValidate GetValidate();

        /// <summary>
        /// Retrieves the additional steps set for this repository, if any
        /// </summary>
        /// <returns></returns>
        IAdditionalStep GetAdditionalStep();
    }

    public interface IRepository<out T> : IRepository
        where T : class
    {
        /// <summary>
        /// Retrieves all entities of the type
        /// </summary>
        /// <returns>All entities</returns>
        IQueryable<T> GetAll();

        /// <summary>
        /// Retrieves an entity by its numeric ID
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns>Entity</returns>
        T GetById(long id);

        /// <summary>
        /// Retrieves an entity by its alphanumeric ID
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns>Entity</returns>
        T GetById(string id);
    }
}
