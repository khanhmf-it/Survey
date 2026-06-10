using SURVEY.Model.Models_SURVEY;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURVEY.Data.Repositories.Interfaces
{
    public interface IAuthenticationRepository: IBaseRepository<authentication, int>
    {
        //check user exist
        Task<bool> CheckUserExistAsync(string userAdid);
    }
}
