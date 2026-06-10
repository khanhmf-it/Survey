using AutoMapper;
using SURVEY.Data.Repositories.Implementations;
using SURVEY.Data.Repositories.Interfaces;
using SURVEY.Model.Common;
using SURVEY.Model.DTOs;
using SURVEY.Model.Models_SURVEY;
using SURVEY.Service.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURVEY.Service.Services.Implementations
{
    public class AuthenticationService: BaseService<authentication, int, authenticationDTO>, IAuthenticationService
    {
        private readonly IAuthenticationRepository _repository;
        private readonly IMapper _mapper;
        public AuthenticationService(IAuthenticationRepository repository, IMapper mapper) : base(repository, mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<GenericResponse<bool>> CheckUserExistAsync(string userAdid)
        {
          var result = new GenericResponse<bool>();
            try
            {
                result.Data = await _repository.CheckUserExistAsync(userAdid);
                result.Success = true;
            }catch(Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }
            return result;
        }
    }
}