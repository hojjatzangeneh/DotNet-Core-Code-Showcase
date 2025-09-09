using Memory_Cache.Services;

using Microsoft.AspNetCore.Mvc;

namespace Memory_Cache.Controllers;

[Route("api/hybrid")]
public class HybridCacheController(HybridCacheProductService hybridCacheProductService) : BaseController<HybridCacheProductService>(
    hybridCacheProductService);