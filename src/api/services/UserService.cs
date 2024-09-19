using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using MicrosoftWord.Core.Models;
using MicrosoftWord.Core.Interfaces;
using MicrosoftWord.Core.Exceptions;

namespace MicrosoftWord.Api.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext dbContext;
        private readonly UserManager<User> userManager;

        public UserService(ApplicationDbContext context, UserManager<User> userManager)
        {
            this.dbContext = context;
            this.userManager = userManager;
        }

        public async Task<UserDto> GetUser(string userId)
        {
            // Find the user in the database
            var user = await dbContext.Users.FindAsync(userId);

            // If not found, throw NotFoundException
            if (user == null)
            {
                throw new NotFoundException($"User with ID {userId} not found.");
            }

            // Create and return a UserDto
            return new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email
                // Add other properties as needed
            };
        }

        public async Task<UserDto> UpdateUser(string userId, UpdateUserRequest request)
        {
            // Find the user in the database
            var user = await dbContext.Users.FindAsync(userId);

            // If not found, throw NotFoundException
            if (user == null)
            {
                throw new NotFoundException($"User with ID {userId} not found.");
            }

            // Update user properties based on the request
            user.UserName = request.UserName ?? user.UserName;
            user.Email = request.Email ?? user.Email;
            // Update other properties as needed

            // Save changes to the database
            await dbContext.SaveChangesAsync();

            // Create and return a UserDto
            return new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email
                // Add other properties as needed
            };
        }

        public async Task<UserSettingsDto> GetUserSettings(string userId)
        {
            // Find the user in the database
            var user = await dbContext.Users.FindAsync(userId);

            // If not found, throw NotFoundException
            if (user == null)
            {
                throw new NotFoundException($"User with ID {userId} not found.");
            }

            // Retrieve the user's settings
            var settings = await dbContext.UserSettings.FirstOrDefaultAsync(s => s.UserId == userId);

            // Create and return a UserSettingsDto
            return new UserSettingsDto
            {
                UserId = userId,
                Theme = settings?.Theme ?? "Default",
                Language = settings?.Language ?? "English"
                // Add other settings as needed
            };
        }

        public async Task<UserSettingsDto> UpdateUserSettings(string userId, UpdateUserSettingsRequest request)
        {
            // Find the user in the database
            var user = await dbContext.Users.FindAsync(userId);

            // If not found, throw NotFoundException
            if (user == null)
            {
                throw new NotFoundException($"User with ID {userId} not found.");
            }

            // Update user settings based on the request
            var settings = await dbContext.UserSettings.FirstOrDefaultAsync(s => s.UserId == userId);
            if (settings == null)
            {
                settings = new UserSettings { UserId = userId };
                dbContext.UserSettings.Add(settings);
            }

            settings.Theme = request.Theme ?? settings.Theme;
            settings.Language = request.Language ?? settings.Language;
            // Update other settings as needed

            // Save changes to the database
            await dbContext.SaveChangesAsync();

            // Create and return a UserSettingsDto
            return new UserSettingsDto
            {
                UserId = userId,
                Theme = settings.Theme,
                Language = settings.Language
                // Add other settings as needed
            };
        }

        public async Task<bool> ChangePassword(string userId, string currentPassword, string newPassword)
        {
            // Find the user in the database
            var user = await userManager.FindByIdAsync(userId);

            // If not found, throw NotFoundException
            if (user == null)
            {
                throw new NotFoundException($"User with ID {userId} not found.");
            }

            // Verify the current password
            if (!await userManager.CheckPasswordAsync(user, currentPassword))
            {
                return false;
            }

            // Change the password using userManager
            var result = await userManager.ChangePasswordAsync(user, currentPassword, newPassword);

            // Return the result of the password change operation
            return result.Succeeded;
        }

        public async Task<IEnumerable<DocumentSummaryDto>> GetUserDocuments(string userId)
        {
            // Find the user in the database
            var user = await dbContext.Users.FindAsync(userId);

            // If not found, throw NotFoundException
            if (user == null)
            {
                throw new NotFoundException($"User with ID {userId} not found.");
            }

            // Retrieve documents owned by the user
            var documents = await dbContext.Documents
                .Where(d => d.OwnerId == userId)
                .Select(d => new DocumentSummaryDto
                {
                    Id = d.Id,
                    Title = d.Title,
                    LastModified = d.LastModified
                    // Add other properties as needed
                })
                .ToListAsync();

            // Create and return a list of DocumentSummaryDto objects
            return documents;
        }

        public async Task<IEnumerable<DocumentSummaryDto>> GetUserCollaborations(string userId)
        {
            // Find the user in the database
            var user = await dbContext.Users.FindAsync(userId);

            // If not found, throw NotFoundException
            if (user == null)
            {
                throw new NotFoundException($"User with ID {userId} not found.");
            }

            // Retrieve documents the user is collaborating on
            var collaborations = await dbContext.DocumentCollaborators
                .Where(dc => dc.UserId == userId)
                .Select(dc => new DocumentSummaryDto
                {
                    Id = dc.Document.Id,
                    Title = dc.Document.Title,
                    LastModified = dc.Document.LastModified
                    // Add other properties as needed
                })
                .ToListAsync();

            // Create and return a list of DocumentSummaryDto objects
            return collaborations;
        }

        public async Task<UserDto> CreateUser(CreateUserRequest request)
        {
            // Validate the request
            if (string.IsNullOrEmpty(request.UserName) || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                throw new ArgumentException("Username, email, and password are required.");
            }

            // Create a new User object
            var user = new User
            {
                UserName = request.UserName,
                Email = request.Email
                // Set other properties as needed
            };

            // Create the user using userManager
            var result = await userManager.CreateAsync(user, request.Password);

            // If creation fails, throw an exception
            if (!result.Succeeded)
            {
                throw new ApplicationException($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            // Create and return a UserDto
            return new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email
                // Add other properties as needed
            };
        }

        public async Task DeleteUser(string userId)
        {
            // Find the user in the database
            var user = await userManager.FindByIdAsync(userId);

            // If not found, throw NotFoundException
            if (user == null)
            {
                throw new NotFoundException($"User with ID {userId} not found.");
            }

            // Delete the user using userManager
            var result = await userManager.DeleteAsync(user);

            // If deletion fails, throw an exception
            if (!result.Succeeded)
            {
                throw new ApplicationException($"Failed to delete user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
    }
}