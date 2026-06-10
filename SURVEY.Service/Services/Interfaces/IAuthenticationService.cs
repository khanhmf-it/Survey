using SURVEY.Model.Common;
using SURVEY.Model.DTOs;
using SURVEY.Model.Models_SURVEY;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURVEY.Service.Services.Interfaces
{
    public interface IAuthenticationService: IBaseService<authentication, int , authenticationDTO>
    {
        //check user exist
        Task<GenericResponse<bool>> CheckUserExistAsync(string userAdid);
    }
}
