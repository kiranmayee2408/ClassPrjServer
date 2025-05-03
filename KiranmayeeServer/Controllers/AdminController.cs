using System.IdentityModel.Tokens.Jwt;
using KiranmayeeServer.Dtos;
using KiranModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace KiranmayeeServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController(UserManager<WorldCitiesUser> userManager, JwtHandler jwtHandler) : ControllerBase
    {
        [HttpPost("Login")]
        public async Task<ActionResult> LoginAsync(LoginRequest request)
        {
            WorldCitiesUser user = await userManager.FindByNameAsync(request.UserName);

            if (user == null) {
                return Unauthorized("Unknown User");
            }
            bool success = await userManager.CheckPasswordAsync(user, request.Password);

                if(!success)
            {
                return Unauthorized("Wrong Password");
            }
            JwtSecurityToken token = await jwtHandler.GetTokenAsync(user);

            string tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return Ok(new LoginResponse
            { 
                Success = true,
                Message= "Kiranmayee",
                Token = tokenString
            });
        }
    }
}
