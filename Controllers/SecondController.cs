using Dapper;
using DapperTest.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
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
        [HttpGet("get-all-cricketers")]
        public async Task<ActionResult<IEnumerable<Cricketer>>> GetAllCricketers()
        {
            using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            DataTable table = new DataTable();
            DbDataReader cricketers = await connection.ExecuteReaderAsync("Select * from Cricketers");
            table.Load(cricketers);
            IEnumerable<Cricketer> result = new List<Cricketer>();
            foreach(DataRow row in table.Rows)
            {
                var cricketer = new Cricketer()
                {
                    Id = (int)row["Id"],
                    Team= (string)row["Team"],
                    Captain = (string)row["Captain"],
                    Cups = (int)row["Cups"]

                };
                result.AsList().Add(cricketer);
            }
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Cricketer>> GetCricketerByIdUsingSp(int id)
        {
            using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("Id", id,DbType.Int32);
            var value = dynamicParameters.Get<int>("Id");
            var result = await connection.QueryFirstOrDefaultAsync<Cricketer>("GetCricketerById",dynamicParameters,commandType: CommandType.StoredProcedure);
            return Ok(result);
        }

    }
}
