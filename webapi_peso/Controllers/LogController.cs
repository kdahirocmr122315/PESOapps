
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.Common;
using MainpesoRepository.Dbcontext;
using static Pesomain.Model.Tbl_pesomain;

namespace User_log.Controllers
{
    [Route("Log/[controller]")]
    [ApiController]

    public class UserController : ControllerBase
    {

        private readonly MainRepository _MainRepository;

        public UserController(MainRepository mainpesoRepository)
        {
            _MainRepository = mainpesoRepository;


        }



        [Authorize]
        [HttpPost]
        public Task<bool> Login(Log_user _Log_user)
        {

            var rt = _MainRepository.verifyUser(_Log_user);

            return rt;
        }




    }
}
