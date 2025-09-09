using Memory_Cache.Services;

using Microsoft.AspNetCore.Mvc;

namespace Memory_Cache.Controllers;

[Route("api/lazy")]
public class LazyCacheController(LazyCacheProductService lazyCacheProductService) : BaseController<LazyCacheProductService>(
    lazyCacheProductService);