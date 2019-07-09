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
    // [ServiceFilter(typeof(LogUserActivity))]
    [Authorize]
    [Route("api/users/{userId}/[controller]")]
    [ApiController]
    public class MessagesController: ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IDatingRepository _datingRepo;

        public MessagesController(IDatingRepository datingRepo, IMapper mapper)
        {
            _datingRepo = datingRepo;
            _mapper = mapper;
        }

        public IDatingRepository DatingRepo { get; }

        [HttpGet("{id}", Name="GetMessage")]
        public async Task<IActionResult> GetMessage(int userId, int messageId) {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)){
                return Unauthorized();
            }

            var message = await _datingRepo.GetMessage(messageId);

            if (message == null) {
                return NotFound();
            }

            return Ok(message);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMessage(int userId, MessageForCreationDto dto) {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)){
                return Unauthorized();
            }

            dto.SenderId = userId;
            var recipient = await _datingRepo.GetUser(dto.RecipientId);
            var sender = await _datingRepo.GetUser(dto.SenderId);
            
            if (recipient == null) {
                return BadRequest("Cannot find recipient.");
            }

            var message = _mapper.Map<Message>(dto);

            _datingRepo.Add(message);

            if(await _datingRepo.SaveAll()) {
                var messageToReturn = _mapper.Map<MessageToReturnDto>(message);

                // return CreatedAtRoute("GetMessage",
                //     new {messageId = message.MessageId},
                //     messageToReturn);
                return Ok(messageToReturn);
            }
            
            throw new Exception("Failed to create message.");
        } 

        [HttpGet]
        public async Task<IActionResult> GetMesagesForUser(int userId, [FromQuery] MessageParams messageParams) {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)){
                return Unauthorized();
            }

            messageParams.UserId = userId;
            
            var messagesFromRepo = await _datingRepo.GetMessagesForUser(messageParams);

            var messages = _mapper.Map<IEnumerable<MessageToReturnDto>>(messagesFromRepo);

            Response.AddPagination(messagesFromRepo.CurrentPage, messagesFromRepo.PageSize, messagesFromRepo.TotalCount, messagesFromRepo.TotalPages);

            return Ok(messages);
        }

        [HttpGet("thread/{recipientId}")]
        public async Task<IActionResult> GetThreadForUser(int userId, int recipientId) {
            if(userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)) {
                return Unauthorized();
            }

            var messages = await _datingRepo.GetMessageThread(userId, recipientId);

            var mappedMessages = _mapper.Map<IEnumerable<MessageToReturnDto>>(messages);

            return Ok(mappedMessages);
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> DeleteMessage(int id, int userId) {
            if(userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)) {
                return Unauthorized();
            }

            var message = await _datingRepo.GetMessage(id);

            if(message.SenderId == userId) {
                message.SenderDeleted = true;
            }

            if(message.RecipientId == userId) {
                message.RecipientDeleted = true;
            }

            if(message.RecipientDeleted && message.SenderDeleted) {
                _datingRepo.Delete(message);
            }

            if(await _datingRepo.SaveAll()) {
                return NoContent();
            }

            throw new Exception("Could not delete message.");
        }

        [HttpPost("{id}/read")]
        public async Task<IActionResult> MessageRead(int userId, int id) {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)) {
                return Unauthorized();
            }

            var messageFromRepo = await _datingRepo.GetMessage(id);

            if(messageFromRepo.RecipientId != userId) {
                return Unauthorized();
            }

            messageFromRepo.IsRead = true;
            messageFromRepo.DateRead = DateTime.Now;

            if(await _datingRepo.SaveAll()){
                return NoContent();
            }

            throw new Exception("Could not mark message as read.");

        }
    }
}