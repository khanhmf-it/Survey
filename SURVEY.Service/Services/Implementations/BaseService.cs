using AutoMapper;
using SURVEY.Data.Repositories.Interfaces;
using SURVEY.Model.Common;

using SURVEY.Service.Services.Interfaces;


namespace SURVEY.Service.Services.Implementations
{
    public class BaseService<T, TKey, TDto> : IBaseService<T, TKey, TDto>
    where T : class
    {
        protected readonly IBaseRepository<T, TKey> _repository;
        protected readonly IMapper _mapper;
        public BaseService(IBaseRepository<T, TKey> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public virtual async Task<GenericResponse<IEnumerable<TDto>>> GetAllAsync()
        {
            var response = new GenericResponse<IEnumerable<TDto>>();
            try
            {
                var entities = await _repository.GetAllWithDapperAsync();
                response.Data = _mapper.Map<IEnumerable<TDto>>(entities);
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public virtual async Task<GenericResponse<long>> GetCountAllAsync()
        {
            var response = new GenericResponse<long>();
            try
            {
                response.Data = await _repository.GetCountAllAsync();
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public virtual async Task<GenericResponse<long>> GetCountByConditionAsync(SearchOptions options)
        {
            var response = new GenericResponse<long>();
            try
            {
                response.Data = await _repository.GetCountByConditionAsync(options);
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public virtual async Task<GenericResponse<PagedList<TDto>>> SearchByConditionAsync(SearchOptions options)
        {
            var response = new GenericResponse<PagedList<TDto>>();
            try
            {
                if (options.PageNumber == 0) options.PageNumber = 1;
                if (options.PageSize == 0) options.PageSize = 10;

                PagedList<TDto> resultFinal = new PagedList<TDto>();
                var result = await _repository.SearchByConditionAsync(options);
                if (result != null && result.Data != null)
                {
                    resultFinal.PageSize = result.PageSize;
                    resultFinal.PageIndex = result.PageIndex;
                    resultFinal.TotalPages = result.TotalPages;
                    resultFinal.TotalFilter = result.TotalFilter;
                    resultFinal.TotalCount = result.TotalCount;
                    resultFinal.Data = _mapper.Map<List<TDto>>(result.Data);
                }
                response.Data = resultFinal;
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public virtual async Task<GenericResponse<TDto>> GetByIdAsync(TKey id)
        {
            var response = new GenericResponse<TDto>();
            try
            {
                var entity = await _repository.GetByIdAsync(id);
                if (entity == null)
                {
                    response.Success = true;
                    response.Message = "No item found";
                }
                response.Data = _mapper.Map<TDto>(entity);
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public virtual async Task<GenericResponse<IEnumerable<TDto>>> GetByIdsAsync(IEnumerable<TKey> ids)
        {
            GenericResponse<IEnumerable<TDto>> result = new GenericResponse<IEnumerable<TDto>>();
            try
            {
                var entities = await _repository.GetByIdsAsync(ids);
                result.Data = _mapper.Map<IEnumerable<TDto>>(entities);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }
            return result;
        }

        public virtual async Task<GenericResponse<TKey>> AddAsync(T entity)
        {
            var response = new GenericResponse<TKey>();
            try
            {
                var result = await _repository.AddAsync(entity);
                response.Success = true;
                response.Data = result;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public virtual async Task<GenericResponse<TDto>> AddAsyncV2(T entity)
        {
            var response = new GenericResponse<TDto>();
            try
            {
                var entityResult = await _repository.AddAsyncV2(entity);
                response.Data = _mapper.Map<TDto>(entityResult);
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public virtual async Task<GenericResponse<int>> AddMultiAsync(IEnumerable<T> entities)
        {
            var response = new GenericResponse<int>();
            try
            {
                var result = await _repository.AddMultiAsync(entities);
                response.Data = result;
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public virtual async Task<GenericResponse<IEnumerable<TDto>>> AddMultiAsyncV2(IEnumerable<T> entities)
        {
            var response = new GenericResponse<IEnumerable<TDto>>();
            try
            {
                var result = await _repository.AddMultiAsyncV2(entities);
                response.Data = _mapper.Map<IEnumerable<TDto>>(result);
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public virtual async Task<GenericResponse<TDto>> UpdateAsync(T entity)
        {
            var response = new GenericResponse<TDto>();
            try
            {
                var result = await _repository.UpdateAsync(entity);
                response.Data = _mapper.Map<TDto>(entity);
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public virtual async Task<GenericResponse<int>> UpdateMultiAsync(IEnumerable<T> entities)
        {
            var response = new GenericResponse<int>();
            try
            {
                var result = await _repository.UpdateMultiAsync(entities);
                response.Data = result;
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public virtual async Task<GenericResponse<bool>> DeleteAsync(TKey id)
        {
            var response = new GenericResponse<bool>();
            try
            {
                await _repository.DeleteAsync(id);
                response.Data = true;
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Data = false;
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public virtual async Task<GenericResponse<bool>> DeleteLogicAsync(TKey id)
        {
            var response = new GenericResponse<bool>();
            try
            {
                await _repository.DeleteLogicAsync(id);
                response.Data = true;
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Data = false;
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public virtual async Task<GenericResponse<bool>> DeleteRangeAsync(IEnumerable<TKey> ids)
        {
            var response = new GenericResponse<bool>();
            try
            {
                await _repository.DeleteRangeAsync(ids);
                await _repository.SaveChangesAsync();
                response.Data = true;
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Data = false;
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public virtual async Task<GenericResponse<bool>> DeleteLogicMultiAsync(IEnumerable<TKey> ids)
        {
            var response = new GenericResponse<bool>();
            try
            {
                await _repository.DeleteLogicMultiAsync(ids);
                response.Data = true;
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Data = false;
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }
    }
}
