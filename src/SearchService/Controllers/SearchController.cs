using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Controllers
{
    [Route("api/search")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private HttpClient _httpClient;

        [HttpGet]
        public async Task<ActionResult<List<Item>>> SearchItems(string searhTerm, int pageNumber = 1, int pageSize = 4)
        {
            var query = DB.PagedSearch<Item>();
            query.Sort(x => x.Ascending(a => a.Make));
            if (!string.IsNullOrEmpty(searhTerm))
            {
                query.Match(Search.Full, searhTerm).SortByTextScore();
            }
            query.PageNumber(pageNumber).PageSize(pageSize);
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