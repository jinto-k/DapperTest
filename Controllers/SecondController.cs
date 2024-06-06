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
            IEnumerable<Cricketer> cricketers = await multi.ReadAsync<Cricketer>();
            Player player = await multi.ReadFirstAsync<Player>();
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
            dynamicParameters.Add("Id", id, DbType.Int32);
            dynamicParameters.Add("Team", null, DbType.String, direction: ParameterDirection.Output, 50);
            var result = await connection.QueryAsync<Cricketer>("GetCricketerById", dynamicParameters, commandType: CommandType.StoredProcedure);
            _ = dynamicParameters.Get<string>("Team"); //learn about Get method
            return Ok(result);
        }

        [HttpGet("name")]

        public async Task<ActionResult<Cricketer>> GetCricketerByName(string name)
        {
            using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            var sql = "Select * from Cricketers where Captain = @Name";
            var param = new DbString() //The IsAnsi and IsFixedLength properties are used to determine the type of string we are using
            {
                Value = name,
                IsAnsi = true,
                IsFixedLength = true
            };
            var result = await connection.QueryFirstOrDefaultAsync<Cricketer>(sql, new {Name= param.Value});
            return Ok(result);  

        }

        [HttpGet("table")]
        public async Task<ActionResult<Cricketer>> GetCricketerUsingTable()
        {
            using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));

            DataTable table = new DataTable();
            table.Columns.Add("ID",typeof(int));
            table.Columns.Add("Captain", typeof(string));

            table.Rows.Add(1, "Dhoni");

            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("GetTeam", table.AsTableValuedParameter("[dbo].[GetTeam]"));

            var result = await connection.QueryFirstOrDefaultAsync<Cricketer>("GetTeamSP", dynamicParameters, commandType: CommandType.StoredProcedure);
            return Ok(result);
        }

        [HttpGet("split")]
        public async Task<ActionResult<IEnumerable<Cricket>>> GetCricketUsingSplit()
        {
            using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            var sql = "Select c.id, c.Team,c.captain,e.Name,e.id,e.captain From [dbo].[Cricketers] c Join [dbo].[InternationalTeams] e on c.Id = e.id";
            var result = await connection.QueryAsync<Cricketer, InternationalTeam, Cricket>(sql, (cricketer, internationalTeam) =>
                         {
                             Cricket cricket = new Cricket()
                             {
                                 Cricketer = cricketer,
                                 InternationalTeam = internationalTeam
                             };
                             return cricket;
                         },splitOn: "Name");
            return Ok(result);

        }
    }
}
