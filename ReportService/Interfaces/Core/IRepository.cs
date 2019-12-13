using System.Collections.Generic;
using ReportService.Entities;
using ReportService.Entities.Dto;

namespace ReportService.Interfaces.Core
{
    public interface IRepository
    {
        object GetBaseQueryResult(string query);
        DependencyState GetDependencyStateByTaskId(long taskId);
        List<DtoTaskInstance> GetInstancesByTaskId(long taskId);
        List<DtoOperInstance> GetOperInstancesByTaskInstanceId(long taskInstanceId);
        DtoOperInstance GetFullOperInstanceById(long operInstanceId);

        /// <summary>
        /// Obtains list of generic-type entities from repository.
        /// WARNING: generic type name must be database table name with "Dto" prefix
        /// </summary>
        List<T> GetListEntitiesByDtoType<T>() where T : IDtoEntity, new();

        /// <summary>
        /// Creates generic-type entity in repository.
        /// WARNING: generic type name must be database table name with "Dto" prefix.
        /// WARNING: key type must be same with table primary key
        /// </summary>
        TKey CreateEntity<T, TKey>(T entity) where T : IDtoEntity;

        long CreateTask(DtoTask task, params DtoOperation[] bindedOpers);

        /// <summary>
        /// Updates generic-type entity in repository.
        /// WARNING: generic type name must be database table name with "Dto" prefix
        /// </summary>
        void UpdateEntity<T>(T entity) where T : IDtoEntity;

        void UpdateTask(DtoTask task, params DtoOperation[] bindedOpers);

        /// <summary>
        /// Deletes generic-type entity in repository.
        /// WARNING: generic type name must be database table name with "Dto" prefix.
        /// WARNING: key type must be same with table primary key
        /// </summary>
        void DeleteEntity<T, TKey>(TKey id) where T : IDtoEntity;

        List<long> UpdateOperInstancesAndGetIds();

        List<long> UpdateTaskInstancesAndGetIds();

        void CreateBase(string baseConnStr);
    }
}