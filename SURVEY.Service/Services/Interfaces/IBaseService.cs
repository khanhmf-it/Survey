using SURVEY.Model.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SURVEY.Service.Services.Interfaces
{
    public interface IBaseService<T, TKey, TDto> where T : class
    {
        Task<GenericResponse<IEnumerable<TDto>>> GetAllAsync();
        Task<GenericResponse<long>> GetCountAllAsync();
        Task<GenericResponse<long>> GetCountByConditionAsync(SearchOptions options);
        Task<GenericResponse<PagedList<TDto>>> SearchByConditionAsync(SearchOptions options);
        Task<GenericResponse<TDto>> GetByIdAsync(TKey id);
        Task<GenericResponse<IEnumerable<TDto>>> GetByIdsAsync(IEnumerable<TKey> ids);
        Task<GenericResponse<TKey>> AddAsync(T entity);
        Task<GenericResponse<TDto>> AddAsyncV2(T entity);
        Task<GenericResponse<int>> AddMultiAsync(IEnumerable<T> entities);
        Task<GenericResponse<IEnumerable<TDto>>> AddMultiAsyncV2(IEnumerable<T> entities);
        Task<GenericResponse<TDto>> UpdateAsync(T entity);
        Task<GenericResponse<int>> UpdateMultiAsync(IEnumerable<T> entities);
        Task<GenericResponse<bool>> DeleteAsync(TKey id);
        Task<GenericResponse<bool>> DeleteLogicAsync(TKey id);
        Task<GenericResponse<bool>> DeleteRangeAsync(IEnumerable<TKey> ids);
        Task<GenericResponse<bool>> DeleteLogicMultiAsync(IEnumerable<TKey> ids);
    }
}
