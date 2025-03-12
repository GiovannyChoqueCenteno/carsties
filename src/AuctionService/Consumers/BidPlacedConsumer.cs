using System;
using AuctionService.Data;
using Contracts;
using MassTransit;

namespace AuctionService.Consumers;

public class BidPlacedConsumer : IConsumer<BidPlaced>
{
    private readonly ApplicationDbContext _dbContext;
    public BidPlacedConsumer(ApplicationDbContext context)
    {
        _dbContext = context;
    }
    public async Task Consume(ConsumeContext<BidPlaced> context)
    {
        var auction = await _dbContext.Auctions.FindAsync(context.Message.AuctionId);
        if (auction.CurrentHighBid == null
            || context.Message.BidStatus.Contains("Accepted")
                && context.Message.Amount > auction.CurrentHighBid
         )
        {
            auction.CurrentHighBid = context.Message.Amount;
            await _dbContext.SaveChangesAsync();
        }
    }
}
