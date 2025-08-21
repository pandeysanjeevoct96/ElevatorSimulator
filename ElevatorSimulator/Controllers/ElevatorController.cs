using ElevatorSimulator.Exceptions;
using ElevatorSimulator.Models;
using ElevatorSimulator.Services;
using Microsoft.AspNetCore.Mvc;

namespace ElevatorSimulator.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ElevatorController : ControllerBase
{
    private readonly IElevatorSimulationService _service;
    private readonly ILogger<ElevatorController> _logger;

    public ElevatorController(IElevatorSimulationService service, ILogger<ElevatorController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        _logger.LogInformation("Fetching elevator status at {Time}", DateTime.UtcNow);

        var status = _service.GetStatus();
        _logger.LogDebug("Status retrieved: {@Status}", status);

        return Ok(status);
    }

    [HttpPost("request")]
    public IActionResult RequestRide([FromBody] RideRequest req)
    {
        _logger.LogInformation("Ride request received at {Time}: {@Request}", DateTime.UtcNow, req);

        try
        {
            _service.RequestRide(req);
            _logger.LogInformation("Ride successfully assigned to request {@Request}", req);

            return Ok(new { message = "Ride assigned" });
        }
        catch (SimulationNotRunningException ex)
        {
            _logger.LogWarning(ex, "Simulation not running when request was made: {@Request}", req);
            return BadRequest(ex.Message);
        }
        catch (CarBusyException ex)
        {
            _logger.LogError(ex, "Elevator busy for request: {@Request}", req);
            return Conflict(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("start")]
    public IActionResult Start()
    {
        _logger.LogInformation("Starting elevator simulation at {Time}", DateTime.UtcNow);

        _service.StartSimulation();

        _logger.LogInformation("Simulation started successfully");
        return Ok(new { message = "Simulation started" });
    }

    [HttpPost("stop")]
    public IActionResult Stop()
    {
        _logger.LogInformation("Stopping elevator simulation at {Time}", DateTime.UtcNow);

        _service.StopSimulation();

        _logger.LogInformation("Simulation stopped successfully");
        return Ok(new { message = "Simulation stopped" });
    }
}