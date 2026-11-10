using EquipmentBooking.Api.Common.Security;
using EquipmentBooking.Application.Approval;
using EquipmentBooking.Application.Bookings;
using EquipmentBooking.Application.Loans;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EquipmentBooking.Api.Controllers;

/// <summary>Booking requests, decisions of managers and loans (issue and return).</summary>
[ApiController]
[Route("api/v1/bookings")]
[Produces("application/json")]
[Tags("Bookings")]
public sealed class BookingsController : ControllerBase
{
    private readonly BookingService _bookings;
    private readonly ApprovalQueueService _approvals;
    private readonly LoanService _loans;

    public BookingsController(BookingService bookings, ApprovalQueueService approvals, LoanService loans)
    {
        _bookings = bookings;
        _approvals = approvals;
        _loans = loans;
    }

    /// <summary>Requests an item for a date range; the request blocks the item and runs the approval route.</summary>
    /// <remarks>An overlapping request is rejected with 409 and the conflicting periods in "conflictingPeriods".</remarks>
    [HttpPost]
    [ProducesResponseType<BookingDetails>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<BookingDetails>> Create(CreateBookingRequest request, CancellationToken cancellationToken)
    {
        var booking = await _bookings.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = booking.Id }, booking);
    }

    /// <summary>Returns a booking to its requester, the manager of the requester's department or an administrator.</summary>
    [HttpGet("{id:long}")]
    [ProducesResponseType<BookingDetails>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookingDetails>> Get(long id, CancellationToken cancellationToken) =>
        Ok(await _bookings.GetAsync(id, cancellationToken));

    /// <summary>Changes the dates of a requested or approved booking; the request is checked and routed again.</summary>
    [HttpPatch("{id:long}")]
    [ProducesResponseType<BookingDetails>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<BookingDetails>> ChangeDates(long id, ChangeBookingDatesRequest request, CancellationToken cancellationToken) =>
        Ok(await _bookings.ChangeDatesAsync(id, request, cancellationToken));

    /// <summary>Cancels a requested or approved booking and frees its dates.</summary>
    [HttpPost("{id:long}/cancellation")]
    [ProducesResponseType<BookingDetails>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<BookingDetails>> Cancel(long id, CancellationToken cancellationToken) =>
        Ok(await _bookings.CancelAsync(id, cancellationToken));

    /// <summary>Approves or declines a routed request of the manager's department; a decline needs a comment.</summary>
    [HttpPost("{id:long}/decisions")]
    [Authorize(Policy = AuthorizationPolicies.Manager)]
    [ProducesResponseType<BookingDetails>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<BookingDetails>> Decide(long id, DecideRequest request, CancellationToken cancellationToken) =>
        Ok(await _approvals.DecideAsync(id, request, cancellationToken));

    /// <summary>Returns the return history of the requester to the department manager.</summary>
    [HttpGet("{id:long}/requester-history")]
    [Authorize(Policy = AuthorizationPolicies.Manager)]
    [ProducesResponseType<RequesterHistoryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RequesterHistoryResponse>> GetRequesterHistory(long id, CancellationToken cancellationToken) =>
        Ok(await _approvals.GetRequesterHistoryAsync(id, cancellationToken));

    /// <summary>Records the issue of an approved booking from its start date.</summary>
    [HttpPost("{id:long}/loan")]
    [Authorize(Policy = AuthorizationPolicies.Administrator)]
    [ProducesResponseType<BookingDetails>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<BookingDetails>> Issue(long id, CancellationToken cancellationToken) =>
        Ok(await _loans.IssueAsync(id, cancellationToken));

    /// <summary>Records the return of an issued or overdue loan with the condition of the item.</summary>
    [HttpPut("{id:long}/loan/return")]
    [Authorize(Policy = AuthorizationPolicies.Administrator)]
    [ProducesResponseType<BookingDetails>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<BookingDetails>> Return(long id, ReturnLoanRequest request, CancellationToken cancellationToken) =>
        Ok(await _loans.ReturnAsync(id, request, cancellationToken));
}
