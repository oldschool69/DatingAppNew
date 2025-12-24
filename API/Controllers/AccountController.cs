using System;
using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController(UserManager<AppUser> userManager, ITokenService tokenService) : BaseApiController
{
    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {

        var user = new AppUser
        {
            Email = registerDto.Email,
            UserName = registerDto.Email,
            DisplayName = registerDto.DisplayName,
            Member = new Member
            {
                DisplayName = registerDto.DisplayName,
                Gender = registerDto.Gender,
                City = registerDto.City,
                Country = registerDto.Country,
                DateOfBirth = registerDto.DateOfBirth
            }
        };

        var result = await userManager.CreateAsync(user, registerDto.Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("identity", error.Description);
            }

            return ValidationProblem();
        }

        await userManager.AddToRoleAsync(user, "Member");

        await SetRefreshTokenCookie(user);

        return await user.ToUserDto(tokenService);
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var user = await userManager.FindByEmailAsync(loginDto.Email);
        if (user == null)
        {
            return Unauthorized("Invalid email");
        }

        var result = await userManager.CheckPasswordAsync(user, loginDto.Password);

        if (!result)
        {
            return Unauthorized("Invalid password");
        }

        await SetRefreshTokenCookie(user);

        return await user.ToUserDto(tokenService);
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<UserDto>> RefreshToken()
    {
        var refreshToken = Request.Cookies["refreshToken"];

        if (string.IsNullOrEmpty(refreshToken))
        {
            return NoContent();
        }

        var user = await userManager.Users.SingleOrDefaultAsync(u => u.RefreshToken == refreshToken
            && u.RefreshTokenExpiry > DateTime.Now);

        if (user == null || user.RefreshTokenExpiry <= DateTime.Now)
        {
            return Unauthorized("Invalid or expired refresh token");
        }

        await SetRefreshTokenCookie(user);

        return await user.ToUserDto(tokenService);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await userManager.Users
            .Where(x => x.Id == User.GetMemberId())
            .ExecuteUpdateAsync(u => u
                .SetProperty(x => x.RefreshToken, _ => null)
                .SetProperty(x => x.RefreshTokenExpiry, _ => null));
        
        Response.Cookies.Delete("refreshToken");

        return Ok();
    }

    private async Task SetRefreshTokenCookie(AppUser user)
    {
        var refreshToken = tokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.Now.AddDays(7);

        await userManager.UpdateAsync(user);

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.Now.AddDays(7)
        };

        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }

}
