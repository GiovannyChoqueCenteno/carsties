using AuctionService.Data;
using AuctionService.DTOs;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuctionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public AuctionController(ApplicationDbContext context,IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        [HttpGet]
        public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(){
            var auctions = await _context.Auctions.Include( x => x.Item)
            .OrderBy(x => x.Item.Make)
            .ToListAsync();
            return _mapper.Map<List<AuctionDto>>(auctions);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id){
            var auctions = await _context.Auctions.Include( x => x.Item)
            .FirstOrDefaultAsync(x => x.Id ==id);
            return _mapper.Map<AuctionDto>(auctions);
        }
    }
}
