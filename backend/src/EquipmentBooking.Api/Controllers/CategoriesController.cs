using EquipmentBooking.Api.Common.Security;
using EquipmentBooking.Application.Catalogue;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EquipmentBooking.Api.Controllers;

/// <summary>Equipment categories with loan limits, usage guidelines and approval settings.</summary>
[ApiController]
[Route("api/v1/categories")]
[Produces("application/json")]
[Tags("Categories")]
public sealed class CategoriesController : ControllerBase
{
    private readonly CatalogueAdminService _service;

    public CategoriesController(CatalogueAdminService service)
    {
        _service = service;
    }

    /// <summary>Lists all categories.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<CategoryResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CategoryResponse>>> List(CancellationToken cancellationToken) =>
        Ok(await _service.GetCategoriesAsync(cancellationToken));

    /// <summary>Creates a category; the maximum loan duration and usage guidelines are required.</summary>
    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.Administrator)]
    [ProducesResponseType<CategoryResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CategoryResponse>> Create(SaveCategoryRequest request, CancellationToken cancellationToken)
    {
        var id = await _service.CreateCategoryAsync(request, cancellationToken);
        var categories = await _service.GetCategoriesAsync(cancellationToken);
        return Created($"/api/v1/categories/{id}", categories.Single(category => category.Id == id));
    }

    /// <summary>Updates a category.</summary>
    [HttpPut("{id:long}")]
    [Authorize(Policy = AuthorizationPolicies.Administrator)]
    [ProducesResponseType<CategoryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CategoryResponse>> Update(long id, SaveCategoryRequest request, CancellationToken cancellationToken)
    {
        await _service.UpdateCategoryAsync(id, request, cancellationToken);
        var categories = await _service.GetCategoriesAsync(cancellationToken);
        return Ok(categories.Single(category => category.Id == id));
    }
}
