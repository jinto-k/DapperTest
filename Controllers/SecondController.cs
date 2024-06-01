using Dapper;
using DapperTest.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace DapperTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SecondController : ControllerBase
    {
        private readonly IConfiguration _config;

        public SecondController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Player>>> Get()
        {
            using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            var sql = @"
                        Select * from Cricketers
                        Select * from Employee
                      ";
            var multi = await connection.QueryMultipleAsync(sql);
            IEnumerable<Cricketer> cricketers = multi.Read<Cricketer>();
            Player player = multi.ReadFirst<Player>();
            return  Ok(cricketers);
        }
    }
}
