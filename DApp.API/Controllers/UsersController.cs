using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DApp.API.Data;
using DApp.API.DTOs;
using DApp.API.Helpers;
using DApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DApp.API.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IDatingRepository _datingRepo;
        private readonly IMapper _mapper;

        public UsersController(IDatingRepository datingRepo, IMapper mapper)
        {
            _datingRepo = datingRepo;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery]UserParams userParams)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var userFromRepo = await _datingRepo.GetUser(currentUserId);
            userParams.UserId = currentUserId;
            
            if(string.IsNullOrEmpty(userParams.Gender)){
                userParams.Gender = userFromRepo.Gender == "male" ? "female" : "male";
            }            
            
            var users = await _datingRepo.GetUsers(userParams);

            Response.AddPagination(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);
            
            var usersToReturn = _mapper.Map<IEnumerable<UserForListDto>>(users);
            return Ok(usersToReturn);
        }

        [HttpGet("{id}", Name="getuser")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _datingRepo.GetUser(id);
            var userToReturn = _mapper.Map<UserForDetailedDto>(user);
            
            return Ok(userToReturn);
        }     

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserForUpdateDto dto) {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)){
                return Unauthorized();
            }

            var userFromRepo = await _datingRepo.GetUser(id);

            _mapper.Map(dto, userFromRepo);

            if (await _datingRepo.SaveAll()) {
                return NoContent();
            }

            throw new Exception($"Error updating user with id: {id}");
        }   

        [HttpPost("{id}/like/{recipientId}")]
        public async Task<IActionResult> LikeUser(int id, int recipientId) {
            if(id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)) {
                return Unauthorized();
            }

            var likes = await _datingRepo.GetLikes(id, recipientId);
            if(likes != null) {
                return BadRequest("You have already liked this person.");
            }

            var user = await _datingRepo.GetUser(recipientId);
            if(user == null) {
                return NotFound();
            }

            var like = new Like{
                LikeeId = recipientId,
                LikerId = id,
            };

            _datingRepo.Add(like);

            if(await _datingRepo.SaveAll()) {
                return Ok();
            }

            return BadRequest("Failed to like user");

        }
    }
}