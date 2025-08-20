using ElevatorSimulator.Exceptions;
using ElevatorSimulator.Models;
using Microsoft.Extensions.Logging;

namespace ElevatorSimulator.Services;

public class ElevatorSimulationService : IDisposable
{
    private readonly List<Elevator> _elevators = [];
    private readonly Lock _lock = new();
    private Timer? _timer;
    private readonly int _tickMs;
    private readonly ILogger<ElevatorSimulationService> _logger;

    public ElevatorSimulationService(ILogger<ElevatorSimulationService> logger, int elevatorCount = 4, int tickMs = 1000)
    {
        _tickMs = tickMs;
        _logger = logger;

        for (var i = 0; i < elevatorCount; i++)
        {
            var elevator = new Elevator(i + 1);
            _elevators.Add(elevator);
            _logger.LogInformation("Elevator {Id} created at floor {Floor}", elevator.Id, elevator.CurrentFloor);
        }
    }

    public void StartSimulation()
    {
        if (_timer != null)
        {
            _logger.LogWarning("Simulation already running. Ignoring StartSimulation.");
            return;
        }

        _logger.LogInformation("Starting elevator simulation with {Count} elevators", _elevators.Count);


        _timer = new Timer(_ =>
        {
            lock (_lock)
            {
                foreach (var elevator in _elevators)
                {
                    elevator.Tick();
                }
            }
        }, null, 0, _tickMs);
    }

    public void StopSimulation()
    {
        _timer?.Dispose();
        _timer = null;
    }

    public void RequestRide(RideRequest request)
    {
        if (_timer == null)
            throw new SimulationNotRunningException("Simulation has not been started.");

        if (request.PickupFloor is < 1 or > 10)
            throw new ArgumentOutOfRangeException(request.PickupFloor.ToString());

        if (request.DestinationFloor is < 1 or > 10)
            throw new ArgumentOutOfRangeException(request.DestinationFloor.ToString());
        
        if (request.PickupFloor  == request.DestinationFloor)
            throw new ArgumentOutOfRangeException(request.PickupFloor.ToString() + request.DestinationFloor.ToString());

        lock (_lock)
        {
            // Find the first idle elevator
            var elevator = _elevators
                .Where(e => e.Direction == Direction.Idle)
                .OrderBy(e => Math.Abs(e.CurrentFloor - request.PickupFloor))
                .FirstOrDefault();

            if (elevator == null)
                throw new CarBusyException("All cars are busy. Please try again later.");

            // Assign pickup and destination sequentially
            elevator.Assign(request.PickupFloor);
            elevator.Assign(request.DestinationFloor);
        }
    }

    public List<ElevatorStatus> GetStatus()
    {
        lock (_lock)
        {
            return _elevators.Select(e => new ElevatorStatus
            {
                Id = e.Id,
                CurrentFloor = e.CurrentFloor,
                Direction = e.Direction,
                Stops = e.Stops,
                MoveSecondsRemaining = e.MoveSecondsRemaining,
                WaitSecondsRemaining = e.WaitSecondsRemaining
            }).ToList();
        }
    }

    public void Dispose()
    {
        StopSimulation();
    }
}