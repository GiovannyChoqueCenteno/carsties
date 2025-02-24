using AuctionService.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Data{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options ) : base(options){

        }

        public DbSet<Auction> Auctions {get;set;}
    }
}