﻿using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyTask.Models;
using MyTask.Services;
using MyTask.Interface;
using System.Linq;
using System.IO;
using System.Text.Json;


namespace MyTask.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class AdminController : ControllerBase
    {
        IUserService UserService;
        private List<User> users;
        private string userFile = "Users.json";
        public AdminController(IUserService UserService) {
            this.UserService=UserService; 
            this.userFile = Path.Combine("Data", "Users.json");
            using (var jsonFile = System.IO.File.OpenText(userFile))
            {
                users = JsonSerializer.Deserialize<List<User>>(jsonFile.ReadToEnd(),
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
        }
       

    
        [HttpPost]
        [Route("[action]")]
        public ActionResult Login([FromBody] User User ) {
            var user = users.FirstOrDefault(x => x.Username == User.Username && x.Password == User.Password);
            if(user == null)
                return Unauthorized();
                    var claims = new List<Claim>
                    {
                        new Claim("type" ,"User"),
                        new Claim("userId", user.userId.ToString())
                    };
                    if(user.isAdmin==true)
                        claims.Add(new Claim("type", "Admin"));
            var token = TaskTokenService.GetToken(claims);

            return new OkObjectResult(TaskTokenService.WriteToken(token));
        }


        [HttpGet]
        [Route("[action]")]
        [Authorize(Policy = "Admin")]

        public ActionResult<List<User>> Get()
        {
            
            return UserService.GetAllUsers();
        }


        [HttpPost]
        // [Route("[action]")]
        [Authorize(Policy="Admin")]
        public ActionResult Post(User user)
        {
            var newPassword = UserService.AddUser(user);

            return CreatedAtAction("Post", 
                new {password = newPassword}, UserService.GetUserById(newPassword));
        }

        [HttpDelete("{password}")]
        // [Route("[action]")]
        [Authorize(Policy = "Admin")]
        public ActionResult deleteUser(string password)
        {
            var result = UserService.DeleteUser(password);
            if (!result)
            {
                return BadRequest();
            }
            return NoContent();
        }


    }
}
