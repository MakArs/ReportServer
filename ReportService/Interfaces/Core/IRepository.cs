using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ReportService.Entities;
using ReportService.Entities.Dto;

namespace ReportService.Interfaces.Core
{
    public interface IRepository
    {
        Task<object> GetBaseQueryResult(string query, CancellationToken token);
        TaskState GetTaskStateById(long taskId);
        Task<List<DtoTaskInstance>> GetAllTaskInstances(long taskId);
        List<DtoOperInstance> GetTaskOperInstances(long taskInstanceId);
        List<DtoOperInstance> GetFullTaskOperInstances(long taskInstanceId);
        DtoOperInstance GetFullOperInstanceById(long operInstanceId);

        /// <summary>
        /// Obtains list of generic-type entities from repository.
        /// WARNING: generic type name must be database table name with "Dto" prefix
        /// </summary>
        List<T> GetListEntitiesByDtoType<T>() where T : class, IDtoEntity;

        /// <summary>
        /// Creates generic-type entity in repository.
        /// WARNING: generic type name must be database table name with "Dto" prefix.
        /// WARNING: key type must be same with table primary key
        /// </summary>
        long CreateEntity<T>(T entity) where T : class, IDtoEntity;

        long CreateTask(DtoTask task, params DtoOperation[] bindedOpers);
        /// <summary>
        /// Updates generic-type entity in repository.
        /// WARNING: generic type name must be database table name with "Dto" prefix
        /// </summary>
        void UpdateEntity<T>(T entity) where T : class, IDtoEntity;

        void UpdateTask(DtoTask task, params DtoOperation[] bindedOpers);

        /// <summary>
        /// Deletes generic-type entity in repository.
        /// WARNING: generic type name must be database table name with "Dto" prefix.
        /// WARNING: key type must be same with table primary key
        /// </summary>
        void DeleteEntity<T, TKey>(TKey id) where T : IDtoEntity;

        List<long> UpdateOperInstancesAndGetIds();

        List<long> UpdateTaskInstancesAndGetIds();

        long CreateTaskRequestInfo(TaskRequestInfo taskRequestInfo);
        List<TaskRequestInfo> GetListTaskRequestInfoByIds(long[] taskRequestIds);
        TaskRequestInfo GetTaskRequestInfoById(long taskRequestId);
        void CreateBase(string baseConnStr);
    }
}