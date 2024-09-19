using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using MicrosoftWord.Core.Interfaces;
using MicrosoftWord.Core.Models;
using MicrosoftWord.Core.Settings;

namespace MicrosoftWord.Api.Middleware
{
    public class AuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly JwtSettings _jwtSettings;
        private readonly IUserService _userService;

        public AuthMiddleware(RequestDelegate next, IOptions<JwtSettings> jwtSettings, IUserService userService)
        {
            _next = next;
            _jwtSettings = jwtSettings.Value;
            _userService = userService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Extract the Authorization header from the request
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            // If the header is missing or invalid, call next() and return
            if (string.IsNullOrEmpty(token))
            {
                await _next(context);
                return;
            }

            // Validate the token
            var jwtToken = ValidateToken(token);

            // If token is invalid, call next() and return
            if (jwtToken == null)
            {
                await _next(context);
                return;
            }

            // Extract the user ID from the token claims
            var userId = jwtToken.Claims.First(x => x.Type == "id").Value;

            // Retrieve the user from the userService
            var user = await _userService.GetUserByIdAsync(userId);

            // If user not found, call next() and return
            if (user == null)
            {
                await _next(context);
                return;
            }

            // Set the user on the HttpContext.Items dictionary
            context.Items["User"] = user;

            // Call next()
            await _next(context);
        }

        private JwtSecurityToken ValidateToken(string token)
        {
            // Create a TokenValidationParameters object with the JWT settings
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.ASCII.GetBytes(_jwtSettings.Secret)),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };

            // Create a JwtSecurityTokenHandler
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                // Try to validate the token
                tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);

                // If validation succeeds, return the JWT token
                return (JwtSecurityToken)validatedToken;
            }
            catch
            {
                // If validation fails, return null
                return null;
            }
        }
    }

    public static class AuthMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuthMiddleware(this IApplicationBuilder builder)
        {
            // Return builder.UseMiddleware<AuthMiddleware>()
            return builder.UseMiddleware<AuthMiddleware>();
        }
    }
}