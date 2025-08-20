using ElevatorSimulator.Exceptions;
using ElevatorSimulator.Models;
using ElevatorSimulator.Services;
using Microsoft.AspNetCore.Mvc;

namespace ElevatorSimulator.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ElevatorController(ElevatorSimulationService sim, ILogger<ElevatorController> logger) : ControllerBase
{
    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        logger.LogInformation("Fetching elevator status at {Time}", DateTime.UtcNow);

        var status = sim.GetStatus();
        logger.LogDebug("Status retrieved: {@Status}", status);

        return Ok(status);
    }

    [HttpPost("request")]
    public IActionResult RequestRide([FromBody] RideRequest req)
    {
        logger.LogInformation("Ride request received at {Time}: {@Request}", DateTime.UtcNow, req);

        try
        {
            sim.RequestRide(req);
            logger.LogInformation("Ride successfully assigned to request {@Request}", req);

            return Ok(new { message = "Ride assigned" });
        }
        catch (SimulationNotRunningException ex)
        {
            logger.LogWarning(ex, "Simulation not running when request was made: {@Request}", req);
            return BadRequest(ex.Message);
        }
        catch (CarBusyException ex)
        {
            logger.LogError(ex, "Elevator busy for request: {@Request}", req);
            return Conflict(ex.Message);
        }
    }

    [HttpPost("start")]
    public IActionResult Start()
    {
        logger.LogInformation("Starting elevator simulation at {Time}", DateTime.UtcNow);

        sim.StartSimulation();

        logger.LogInformation("Simulation started successfully");
        return Ok(new { message = "Simulation started" });
    }

    [HttpPost("stop")]
    public IActionResult Stop()
    {
        logger.LogInformation("Stopping elevator simulation at {Time}", DateTime.UtcNow);

        sim.StopSimulation();

        logger.LogInformation("Simulation stopped successfully");
        return Ok(new { message = "Simulation stopped" });
    }
}