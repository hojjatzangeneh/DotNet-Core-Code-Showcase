using Memory_Cache.Services;

using Microsoft.AspNetCore.Mvc;

namespace Memory_Cache.Controllers;

[Route("api/memory")]
public class MemoryCacheController(MemoryCacheProductService memoryCacheProductService) : BaseController<MemoryCacheProductService>(
    memoryCacheProductService);