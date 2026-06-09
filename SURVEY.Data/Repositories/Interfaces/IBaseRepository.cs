using SURVEY.Model.Common;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace SURVEY.Data.Repositories.Interfaces
{
    public interface IBaseRepository<T, TKey> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> GetAllWithDapperAsync();
        Task<long> GetCountAllAsync();
        Task<long> GetCountByConditionAsync(SearchOptions options);
        Task<PagedList<T>> SearchByConditionAsync(SearchOptions options);
        Task<T> GetByIdAsync(TKey id);
        Task<IEnumerable<T>> GetByIdsAsync(IEnumerable<TKey> ids);
        Task<TKey> AddAsync(T entity);
        Task<T> AddAsyncV2(T entity);
        Task<int> AddMultiAsync(IEnumerable<T> entities);
        Task<List<T>> AddMultiAsyncV2(IEnumerable<T> entities);
        Task<bool> IsExistsAsync(string fieldName, object value);
        Task<int> UpdateAsync(T entity);
        Task<int> UpdateMultiAsync(IEnumerable<T> entities);
        Task<int> DeleteAsync(TKey id);
        Task<int> DeleteLogicAsync(TKey id);
        Task DeleteLogicMultiAsync(IEnumerable<TKey> ids);
        Task DeleteRangeAsync(IEnumerable<TKey> ids);
        Task SaveChangesAsync();
        Task<IQueryable<T>> GetQueryableAsync();
    }
}
