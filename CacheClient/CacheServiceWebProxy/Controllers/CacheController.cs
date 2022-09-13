using Cache.Library.CacheWrapper;
using CacheServiceWebProxy.MessagePacket;
using Microsoft.AspNetCore.Mvc;

namespace CacheServiceWebProxy.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CacheController : ControllerBase
    {
        private readonly ILogger<CacheController> _logger;
        private readonly IFileCache fileCache;
        public CacheController(ILogger<CacheController> logger, IFileCache fileCache)
        {
            _logger = logger;
            this.fileCache = fileCache;
        }
        [HttpPost]
        public ActionResult<CacheResponse>Post(ReqSet request)
        {
            var response = fileCache.Set(request.Key, request.Value, request.Duration);
            return Ok(response);
        }
        [HttpGet]
        public ActionResult<CacheResponse>Get(string key)
        {
            var response = fileCache.Get(key);
            return Ok(response);
        }
        [HttpDelete]
        public ActionResult<CacheResponse>Delete(string key)
        {
            var response=fileCache.Delete(key);
            return Ok(response);
        }
    }
}
