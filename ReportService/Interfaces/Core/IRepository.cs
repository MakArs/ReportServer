using System.Collections.Generic;
using ReportService.Entities;

namespace ReportService.Interfaces.Core
{
    public interface IRepository
    {
        object GetBaseQueryResult(string query);
        List<DtoTaskInstance> GetInstancesByTaskId(int taskId);
        List<DtoOperInstance> GetOperInstancesByTaskInstanceId(int taskInstanceId);
        DtoOperInstance GetFullOperInstanceById(int operInstanceId);

        /// <summary>
        /// Obtains list of generic-type entities from repository.
        /// WARNING: generic type name must be database table name with "Dto" prefix
        /// </summary>
        List<T> GetListEntitiesByDtoType<T>() where T : IDtoEntity, new();

        /// <summary>
        /// Creates generic-type entity in repository.
        /// WARNING: generic type name must be database table name with "Dto" prefix
        /// </summary>
        int CreateEntity<T>(T entity) where T : IDtoEntity;

        int CreateTask(DtoTask task, params DtoOperation[] bindedOpers);

        /// <summary>
        /// Updates generic-type entity in repository.
        /// WARNING: generic type name must be database table name with "Dto" prefix
        /// </summary>
        void UpdateEntity<T>(T entity) where T : IDtoEntity;

        void UpdateTask(DtoTask task, params DtoOperation[] bindedOpers);

        /// <summary>
        /// Deletes generic-type entity in repository.
        /// WARNING: generic type name must be database table name with "Dto" prefix
        /// </summary>
        void DeleteEntity<T>(int id) where T : IDtoEntity;

        List<int> UpdateOperInstancesAndGetIds();
        List<int> UpdateTaskInstancesAndGetIds();

        void CreateBase(string baseConnStr);
    }
}
