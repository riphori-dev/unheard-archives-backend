using Microsoft.AspNetCore.Mvc;
using MediatR;
using Tywynh.Application.Confessions.Commands.CreateConfession;
using Tywynh.Application.Confessions.Commands.ApproveConfession;
using Tywynh.Application.Confessions.Commands.AddResonance;
using Tywynh.Application.Confessions.Commands.AddEcho;
using Tywynh.Application.Confessions.Queries.GetConfessionById;
using Tywynh.Application.Confessions.Queries.GetConfessions;
using Tywynh.Application.Confessions.DTOs;
using Tywynh.Application.Resonances.Commands.AddResonance;
using Tywynh.Application.Resonances.Queries.GetResonances;
using Tywynh.Application.DailyEchoes.Queries.GetDailyEcho;
using Tywynh.Application.DailyEchoes.Commands.SetDailyEcho;
using Tywynh.Application.DailyEchoes.Commands.AddInteraction;
using Tywynh.Application.DailyEchoes.Queries.GetDailyEchoInteractions;
using Tywynh.Domain.Entities;

namespace Tywynh.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConfessionsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ConfessionsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<ActionResult<CreateConfessionDTO>> CreateConfession([FromBody] CreateConfessionCommand command)
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetConfession), new { id = result.Id }, result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ConfessionDto>> GetConfession(Guid id)
        {
            try
            {
                var result = await _mediator.Send(new GetConfessionByIdQuery(id));
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ConfessionDto>>> GetConfessions()
        {
            var result = await _mediator.Send(new GetConfessionsQuery());
            return Ok(result);
        }

        [HttpPost("{id}/approve")]
        public async Task<ActionResult> ApproveConfession(Guid id)
        {
            try
            {
                await _mediator.Send(new ApproveConfessionCommand(id));
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost("{id}/resonance")]
        public async Task<ActionResult> AddResonance(Guid id)
        {
            try
            {
                await _mediator.Send(new Application.Confessions.Commands.AddResonance.AddResonanceCommand(id));
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost("{id}/echo")]
        public async Task<ActionResult> AddEcho(Guid id)
        {
            try
            {
                await _mediator.Send(new AddEchoCommand(id));
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        //[HttpPost("{id}/resonance")]
        //public async Task<ActionResult> AddResonance(Guid id, [FromBody] Application.Confessions.Commands.AddResonance.AddResonanceCommand command)
        //{
        //    try
        //    {
        //        // Update the command with the confession ID from the route
        //        var updatedCommand = command with { ConfessionId = id };
        //        await _mediator.Send(updatedCommand);
        //        return NoContent();
        //    }
        //    catch (KeyNotFoundException)get
        //    {
        //        return NotFound();
        //    }
        //}

        [HttpGet("{id}/resonances")]
        public async Task<ActionResult<IEnumerable<Domain.Entities.Resonance>>> GetResonances(Guid id)
        {
            try
            {
                var result = await _mediator.Send(new GetResonancesQuery(id));
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet("daily-echo")]
        public async Task<ActionResult<ConfessionDto>> GetDailyEcho(DateTime? date = null)
        {
            try
            {
                var result = await _mediator.Send(new GetDailyEchoQuery(date));
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost("daily-echo")]
        public async Task<ActionResult> SetDailyEcho([FromBody] SetDailyEchoCommand command)
        {
            try
            {
                await _mediator.Send(command);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{echoDate}/interact")]
        public async Task<ActionResult> AddInteraction(DateTime echoDate, [FromBody] AddInteractionCommand command)
        {
            try
            {
                // Update command with echo date from route
                var updatedCommand = command with { EchoDate = echoDate };
                var result = await _mediator.Send(updatedCommand);
                return result ? Ok() : BadRequest("Already interacted with this daily echo.");
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet("{echoDate}/interactions")]
        public async Task<ActionResult<IEnumerable<DailyEchoInteraction>>> GetDailyEchoInteractions(DateTime echoDate)
        {
            try
            {
                var result = await _mediator.Send(new GetDailyEchoInteractionsQuery(echoDate));
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
