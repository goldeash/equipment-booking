using System.Security.Cryptography;
using System.Text;
using EquipmentBooking.Infrastructure.Jobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EquipmentBooking.Api.Controllers;

public sealed record JobRunResponse(string Job, int? Processed, bool AlreadyRunning);

/// <summary>Trigger for scheduled jobs used by an external scheduler; protected by a shared secret header.</summary>
[ApiController]
[Route("internal/jobs")]
[Produces("application/json")]
[ApiExplorerSettings(IgnoreApi = true)]
[AllowAnonymous]
public sealed class InternalJobsController : ControllerBase
{
    public const string KeyHeader = "X-Job-Key";

    private readonly JobRunner _runner;
    private readonly JobOptions _options;

    public InternalJobsController(JobRunner runner, IOptions<JobOptions> options)
    {
        _runner = runner;
        _options = options.Value;
    }

    [HttpPost("{name}/runs")]
    public async Task<ActionResult<JobRunResponse>> Run(string name, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_options.TriggerKey) || !JobNames.All.Contains(name))
        {
            return NotFound();
        }

        var presented = Request.Headers[KeyHeader].ToString();
        if (!CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(presented), Encoding.UTF8.GetBytes(_options.TriggerKey)))
        {
            return Unauthorized();
        }

        var processed = await _runner.RunAsync(name, cancellationToken);
        return Ok(new JobRunResponse(name, processed, processed is null));
    }
}
