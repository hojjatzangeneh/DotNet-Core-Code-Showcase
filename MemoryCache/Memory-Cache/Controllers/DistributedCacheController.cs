using Memory_Cache.Services;

using Microsoft.AspNetCore.Mvc;

namespace Memory_Cache.Controllers;

[Route("api/distributed")]
public class DistributedCacheController(DistributedCacheProductService distributedCacheProductService) : BaseController<DistributedCacheProductService>(
    distributedCacheProductService);