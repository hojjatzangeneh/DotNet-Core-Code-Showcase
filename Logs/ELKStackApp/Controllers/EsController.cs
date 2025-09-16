using ELKStackApp.Models;
using ELKStackApp.Services;

using Microsoft.AspNetCore.Mvc;

namespace ELKStackApp.Controllers;

[ApiController]
[Route("[controller]")]
public class EsController(IElasticSearchService<MyDocument> elasticSearchService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateDocument([FromBody] MyDocument document)
    {
        string response = await elasticSearchService.CreateDocumentAsync(document);
        return Ok(response);
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteDocument(int id)
    {
        bool response = await elasticSearchService.DeleteDocumentByIdAsync(id);
        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllDocuments()
    {
        IEnumerable<MyDocument> response = await elasticSearchService.SearchDocumentsAsync(string.Empty);
        return Ok(response);
    }

    [HttpGet("read/{id}")]
    public async Task<IActionResult> GetDocument(int id)
    {
        MyDocument response = await elasticSearchService.GetDocumentByIdAsync(id);
        if(response is null)
        {
            return NotFound();
        }
        return Ok(response);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateDocument([FromBody] MyDocument document)
    {
        bool response = await elasticSearchService.UpdateDocumentAsync(document.Id, document);
        return Ok(response);
    }
}