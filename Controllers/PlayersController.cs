using Dapper;
using DapperTest.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace DapperTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlayersController : ControllerBase
    {
        //REFER NOTES.TXT FOR ALL NOTES REGARDING DAPPER
        private readonly IConfiguration _config;

        public PlayersController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet]
        public async Task<ActionResult<List<Player>>> GetAllPlayers()
        {
            using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            IEnumerable<Player> players = await SelectAllPlayers(connection);
            return Ok(players);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Player>> GetPlayerById(int id)
        {
            using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            var player = await connection.QuerySingleOrDefaultAsync<Player>("Select * from Employee where Id = @Id",
                new { Id = id }); //returns only a single row. Throws error when more than one row is returned.
            return Ok(player);
        }
        [HttpGet("name")]
        public async Task<ActionResult<Player>> GetPlayerByName(string name)
        {
            using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            //returns only the first row. refer Notes.txt

            //var player = await connection.QueryFirstAsync<Player>("Select * from Employee where Name = @Name",
            //    new { Name = name });
            var player = await connection.QueryFirstOrDefaultAsync<Player>("Select * from Employee where Name = @Name",
                  new { name });

            return Ok(player);
        }   
        [HttpGet("{id}/{age}")]
        public async Task<ActionResult<Player>> GetPlayerByIdAndAge(int id, int age)
        {
            using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            IEnumerable<Player> player = await connection.QueryAsync<Player>("Select * from Employee where Id in @Id and Age =@Age",
                new
                {
                    Id = new List<int> { 2, 13002 },
                    Age = age
                });
            return Ok(player);
        }

        [HttpPost("Create")]
        public async Task<ActionResult<List<Player>>> CreatePlayer(Player player)
        {
            using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            await connection.ExecuteAsync("Insert into Employee (Name,Department,Age,Address) values (@Name , @Department, @Age, @Address)", player);
            return Ok(await SelectAllPlayers(connection));
        }

        [HttpPut("Update")]
        public async Task<ActionResult<List<Player>>> UpdatePlayer(Player player)
        {
            using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            await connection.ExecuteAsync("Update Employee set Name = @Name, Department = @Department,Age= @Age,Address = @Address where Id = @Id", player);
            return Ok(await SelectAllPlayers(connection));
        }

        [HttpDelete("Delete/{id}")]
        public async Task<ActionResult<List<Player>>> DeletePlayer(int id)
        {
            using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            await connection.ExecuteAsync("Delete from Employee where Id = @id", new { id });
            return Ok(await SelectAllPlayers(connection));
        }


        private static async Task<IEnumerable<Player>> SelectAllPlayers(SqlConnection connection)
        {
            return await connection.QueryAsync<Player>("Select * from Employee");
        }
    }
}
