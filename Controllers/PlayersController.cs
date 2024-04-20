﻿using Dapper;
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
        public async Task<ActionResult<Player>>GetPlayerById(int id)
        {
            using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            var player = await connection.QueryFirstAsync<Player>("Select * from Employee where Id = @Id", new { Id = id });
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
            await connection.ExecuteAsync("Delete from Employee where Id = @id", new { id = id});
            return Ok(await SelectAllPlayers(connection));
        }


        private static async Task<IEnumerable<Player>> SelectAllPlayers(SqlConnection connection)
        {
            return await connection.QueryAsync<Player>("Select * from Employee");
        }
    }
}