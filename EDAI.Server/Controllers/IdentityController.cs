using System.IdentityModel.Tokens.Jwt;
using EDAI.Server.Data;
using EDAI.Shared.Models.DTO;
using EDAI.Shared.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace EDAI.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IdentityController(EdaiContext context, UserManager<EDAIUser> userManager, IConfiguration configuration) 
    : ControllerBase
{
    [HttpPost("login",Name = "Login")]
    public async Task<IActionResult> Login(LoginRequestDto loginRequestDto)
    {
        var user = await userManager.FindByEmailAsync(loginRequestDto.Email);

        if (user is null)
        {
            return Unauthorized("User does not exist");
        }
        else if (!await userManager.CheckPasswordAsync(user, loginRequestDto.Password))
        {
            return Unauthorized("Wrong password");
        }

        var roles = await userManager.GetRolesAsync(user);

        var claims = new List<Claim>()
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id)
        };
        
        claims.AddRange(roles.Select( r => new Claim(ClaimTypes.Role, r)));

        var jwtKey = configuration["Jwt:Key"];
        if (String.IsNullOrEmpty(jwtKey))
        {
            throw new NullReferenceException($"Jwt key could not be retrieved from settings file");
        }
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var issuer = configuration["Jwt:Issuer"];
        var audience = configuration["Jwt:Audience"];

        if (String.IsNullOrEmpty(issuer))
        {
            throw new NullReferenceException($"Could not retrieve Jwt Issuer");

        }
        else if (String.IsNullOrEmpty(audience))
        {
            throw new NullReferenceException($"Could not retrieve Jwt audience");
        }
        
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new { token = tokenString });
    }

    [HttpPost("register",Name = "Register")]
    public async Task<IActionResult> Register(RegisterRequestDto registerRequestDto)
    {
        var user = new EDAIUser()
        {
            UserName = registerRequestDto.Email,
            Email = registerRequestDto.Email
        };

        var result = await userManager.CreateAsync(user, registerRequestDto.Password);

        if (result.Succeeded)
        {
            return Ok("Successfull registration");
        }
        else
        {
            return BadRequest(result.Errors.Select(e => e.Description));
        }
    }
    
    


}