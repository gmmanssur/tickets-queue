using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using TicketsQueue.Application.Commons;
using TicketsQueue.Application.Commons.Interfaces;
using TicketsQueue.Application.UseCases;

namespace TicketsQueue.API.Controllers
{
    /// <summary>
    /// Controller responsible for managing ticket operations in the queue.
    /// </summary>
    /// <param name="ticketsQueueService">The service for managing ticket operations.</param>
    [ApiController]
    [Route("api/[controller]")]
    public class TicketsController(ITicketsQueueService ticketsQueueService) : ControllerBase
    {
        readonly ITicketsQueueService _ticketsQueueService = ticketsQueueService;

        /// <summary>
        /// Creates a new ticket in the queue.
        /// </summary>
        /// <param name="ticket">Data required to create the ticket.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>The created ticket.</returns>
        /// <response code="201">Ticket created successfully.</response>
        /// <response code="400">The request contains invalid data.</response>
        /// <response code="409">A conflicting ticket already exists.</response>
        /// <response code="500">An unexpected server error occurred.</response>
        [HttpPost]
        [ProducesResponseType(typeof(TicketsQueueResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateTicket(
            [FromBody] TicketsQueueRequest ticket,
            CancellationToken cancellationToken)
        {
            TicketQueueResult<TicketsQueueResponse> createdTicket = await _ticketsQueueService.CreateTicketAsync(ticket, cancellationToken);

            if (!createdTicket.IsSuccess || createdTicket.Data is null)
            {
                return Conflict(new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Ticket conflict",
                    Detail = createdTicket.Error
                });
            }

            return CreatedAtAction(
                nameof(GetTicket),
                new { id = createdTicket.Data.Id },
                createdTicket.Data);
        }

        /// <summary>
        /// Retrieves a ticket by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the ticket to retrieve.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>The requested ticket.</returns>
        /// <response code="200">Ticket retrieved successfully.</response>
        /// <response code="404">The ticket was not found.</response>
        /// <response code="500">An unexpected server error occurred.</response>    
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(TicketsQueueResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTicket(Guid id, CancellationToken cancellationToken)
        {
            TicketQueueResult<TicketsQueueResponse> ticket = await _ticketsQueueService.GetTicketsByIdAsync(id, cancellationToken);

            if (!ticket.IsSuccess || ticket.Data is null)
            {
                return Conflict(new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Ticket conflict",
                    Detail = ticket.Error
                });
            }

            return Ok(ticket);
        }
    }
}
