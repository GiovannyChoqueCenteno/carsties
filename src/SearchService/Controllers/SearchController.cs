using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.RequestHelpers;

namespace SearchService.Controllers
{
    [Route("api/search")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private HttpClient _httpClient;

        [HttpGet]
        public async Task<ActionResult<List<Item>>> SearchItems(SearchParams searchParams)
        {
            var query = DB.PagedSearch<Item, Item>();
            query.Sort(x => x.Ascending(a => a.Make));
            if (!string.IsNullOrEmpty(searchParams.SearchTerm))
            {
                query.Match(Search.Full, searchParams.SearchTerm).SortByTextScore();
            }

            query = searchParams.OrderBy switch
            {
                "make" => query.Sort(x => x.Ascending(a => a.Make)),
                "model" => query.Sort(x => x.Ascending(a => a.Model)),
                "year" => query.Sort(x => x.Ascending(a => a.Year)),
                _ => query
            };

            query = searchParams.OrderBy switch
            {
                "finished" => query.Match(x => x.AuctionEnd < DateTime.UtcNow),
                "endingSoon" => query.Match(x => x.AuctionEnd < DateTime.UtcNow.AddHours(6)
                            && x.AuctionEnd > DateTime.UtcNow),
                _ => query.Match(x => x.AuctionEnd > DateTime.UtcNow)
            };

            if (!string.IsNullOrEmpty(searchParams.Seller))
            {
                query.Match(x => x.Seller == searchParams.Seller);
            }

            query.PageNumber(searchParams.PageNumber).PageSize(searchParams.PageSize);
            var result = await query.ExecuteAsync();
            return Ok(
                new
                {
                    result = result.Results,
                    pageCount = result.PageCount,
                    totalCount = result.TotalCount
                }
            );
        }
    }
}