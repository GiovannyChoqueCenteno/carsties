using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers
{
    [Route("api/auctions")]
    [ApiController]
    public class AuctionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IPublishEndpoint _publishEndPoint;

        public AuctionController(ApplicationDbContext context, IMapper mapper
        , IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _mapper = mapper;
            _publishEndPoint = publishEndpoint;
        }
        [HttpGet]
        public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string date)
        {
            var query = _context.Auctions.OrderBy(x => x.Item.Make).AsQueryable();
            if (!string.IsNullOrEmpty(date))
            {
                query = query.Where(x => x.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);
            }
            var auctions = await _context.Auctions.Include(x => x.Item)
            .OrderBy(x => x.Item.Make)
            .ToListAsync();
            return await query.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider).ToListAsync();
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
        {
            var auctions = await _context.Auctions.Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == id);
            return _mapper.Map<AuctionDto>(auctions);
        }
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto createAuctionDto)
        {
            var auction = _mapper.Map<Auction>(createAuctionDto);
            auction.Seller = User.Identity.Name;
            _context.Auctions.Add(auction);

            var newAuction = _mapper.Map<AuctionDto>(auction);

            await _publishEndPoint.Publish(_mapper.Map<AuctionCreated>(newAuction));

            var result = await _context.SaveChangesAsync() > 0;

            if (!result) return BadRequest("Failed to create auction");
            return CreatedAtAction(nameof(GetAuctionById), new { id = auction.Id }, newAuction);
        }
        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult<AuctionDto>> UpdateAuction(Guid id, UpdateActionDto updateAuctionDto)
        {
            var auction = await _context.Auctions.Include(x => x.Item)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (auction == null) return NotFound();
            if (auction.Seller != User.Identity.Name) return Forbid();
            auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
            auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
            auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
            auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
            auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;

            await _publishEndPoint.Publish(_mapper.Map<AuctionUpdated>(auction));
            var result = await _context.SaveChangesAsync() > 0;
            if (!result) return BadRequest("Failed to update auction");
            return Ok();
        }
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult<AuctionDto>> DeleteAction(Guid id)
        {
            var auction = await _context.Auctions.FindAsync(id);
            if (auction == null) return NotFound();
            if (auction.Seller != User.Identity.Name) return Forbid();
            _context.Auctions.Remove(auction);

            await _publishEndPoint.Publish<AuctionDeleted>(new
            {
                Id = auction.Id
            });

            var result = await _context.SaveChangesAsync() > 0;
            if (!result) return BadRequest("Failed to update auction");
            return Ok();
        }
    }
}
