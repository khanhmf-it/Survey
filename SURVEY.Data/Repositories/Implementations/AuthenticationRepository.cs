
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using SURVEY.Data.Repositories.Interfaces;
using SURVEY.Model.Common;
using SURVEY.Model.Models_SURVEY;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURVEY.Data.Repositories.Implementations
{
    public class AuthenticationRepository : BaseRepository<authentication, int>, IAuthenticationRepository
    {
        private readonly SURVEYContext _context;

        public AuthenticationRepository(SURVEYContext context, IOptions<ConnectionStringOptions> options, IConfiguration configuration) : base(context, options, configuration)
        {
            _context = context;
        }
        //check user exist
        public async Task<bool> CheckUserExistAsync(string userAdid)
        {
            return await Task.FromResult(_context.authentications.Any(a => a.userAdid == userAdid));
        }
    }
}
