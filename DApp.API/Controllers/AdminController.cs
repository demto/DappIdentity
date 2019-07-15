using System.Threading.Tasks;
using DApp.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using DApp.API.DTOs;
using Microsoft.AspNetCore.Identity;
using DApp.API.Models;

namespace DApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController: ControllerBase
    {
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;

        public AdminController(DataContext context,
            UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize(Policy="RequireAdmin")]
        [HttpGet("usersWithRoles")]
        public async Task<IActionResult> GetUsersWithRoles() {
            var users = await (from user in _context.Users orderby user.UserName
                        select new {
                            id = user.Id,
                            userName = user.UserName,
                            roles = (from userRole in user.UserRoles
                                    join role in _context.Roles on userRole.RoleId equals role.Id
                                    select role.Name).ToList()
                        }).ToListAsync();

            return Ok(users);
        }

        [Authorize(Policy = "RequireAdmin")]
        [HttpPost("editRoles/{userName}")]
        public async Task<IActionResult> EditRoles(string userName, RoleEditDto dto) {
            var user = await _userManager.FindByNameAsync(userName);
            var userRoles = await _userManager.GetRolesAsync(user);

            var selectedRoles = dto.RoleNames;

            var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));
            if (!result.Succeeded) {
                return BadRequest("Failed to add user to roles.");
            }

            result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));
            if (!result.Succeeded) {
                return BadRequest("Failed to remove user from roles.");
            }

            return Ok(await _userManager.GetRolesAsync(user));
        }
    }
}