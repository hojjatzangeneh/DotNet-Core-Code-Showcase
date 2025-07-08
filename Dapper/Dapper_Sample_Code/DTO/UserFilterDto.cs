
namespace Dapper_Sample_Code.DTO;

public class UserFilterDto
{
    public bool Desc { get; set; } = false;

    public string? EmailContains { get; set; }

    public string? NameContains { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public string SortBy { get; set; } = "Id";
}