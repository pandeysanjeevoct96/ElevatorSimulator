using ElevatorSimulator.Exceptions;
using ElevatorSimulator.Models;
using Microsoft.Extensions.Logging;

namespace ElevatorSimulator.Services;

public class ElevatorSimulationService : IElevatorSimulationService, IDisposable
{
    private readonly List<Elevator> _elevators = [];
    private readonly Lock _lock = new();
    private Timer? _timer;
    private readonly int _tickMs;
    private readonly ILogger<ElevatorSimulationService> _logger;
    private bool _isRunning;

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
        _isRunning = true;


        _timer = new Timer(_ =>
        {
            lock (_lock)
            {
                foreach (var elevator in _elevators)
                {
                    int beforeFloor = elevator.CurrentFloor;
                    int beforeMove = elevator.MoveSecondsRemaining;
                    int beforeWait = elevator.WaitSecondsRemaining;
                    var beforeDir = elevator.Direction;

                    elevator.Tick();

                    _logger.LogDebug(
                        "Tick: Elevator {Id} | Floor {BeforeFloor}->{AfterFloor}, Direction {BeforeDir}->{AfterDir}, Move {BeforeMove}->{AfterMove}, Wait {BeforeWait}->{AfterWait}, Stops={Stops}",
                        elevator.Id,
                        beforeFloor, elevator.CurrentFloor,
                        beforeDir, elevator.Direction,
                        beforeMove, elevator.MoveSecondsRemaining,
                        beforeWait, elevator.WaitSecondsRemaining,
                        string.Join(",", elevator.Stops));
                }
            }
        }, null, 0, _tickMs);
    }

    public void StopSimulation()
    {
        if (_timer == null)
        {
            _logger.LogWarning("StopSimulation called but simulation is not running.");
            return;
        }

        _logger.LogInformation("Stopping elevator simulation...");
        _isRunning = false;
        _timer.Dispose();
        _timer = null;
    }

    public void RequestRide(RideRequest request)
    {
        _logger.LogInformation("New ride request: Pickup={Pickup}, Destination={Destination}", request.PickupFloor, request.DestinationFloor);

        if (_timer == null)
        {
            _logger.LogError("Ride request rejected: simulation not started.");
            throw new SimulationNotRunningException("Simulation has not been started.");
        }

        if (request.PickupFloor is < 1 or > 10)
            throw new ArgumentOutOfRangeException(nameof(request.PickupFloor), "Pickup floor must be between 1 and 10");

        if (request.DestinationFloor is < 1 or > 10)
            throw new ArgumentOutOfRangeException(nameof(request.DestinationFloor), "Destination floor must be between 1 and 10");

        if (request.PickupFloor == request.DestinationFloor)
            throw new ArgumentException("start and end floors must be different");

        lock (_lock)
        {
            // Find the first idle elevator
             var elevator = _elevators
                .Where(e => e.Direction == Direction.Idle)
                .OrderBy(e => Math.Abs(e.CurrentFloor - request.PickupFloor))
                .FirstOrDefault();

            if (elevator == null)
            {
                _logger.LogWarning("All elevators busy. Cannot assign ride Pickup={Pickup}, Destination={Destination}", request.PickupFloor, request.DestinationFloor);
                throw new CarBusyException("All cars are busy. Please try again later.");
            }

            _logger.LogInformation("Assigning Elevator {Id} to Pickup={Pickup}, then Destination={Destination}",
                elevator.Id, request.PickupFloor, request.DestinationFloor);

            elevator.Assign(request.PickupFloor);
            _logger.LogDebug("Elevator {Id} assigned Pickup={Pickup}", elevator.Id, request.PickupFloor);

            elevator.Assign(request.DestinationFloor);
            _logger.LogDebug("Elevator {Id} assigned Destination={Destination}", elevator.Id, request.DestinationFloor);
        }
    }

    public List<ElevatorStatus> GetStatus()
    {
        if (!_isRunning)
            return new List<ElevatorStatus>();

        lock (_lock)
        {
            var statusList = _elevators.Select(e => new ElevatorStatus
            {
                Id = e.Id,
                CurrentFloor = e.CurrentFloor,
                Direction = e.Direction,
                Stops = e.Stops,
                MoveSecondsRemaining = e.MoveSecondsRemaining,
                WaitSecondsRemaining = e.WaitSecondsRemaining
            }).ToList();

            foreach (var status in statusList)
            {
                _logger.LogDebug("Status: Elevator {Id} | Floor={Floor}, Direction={Direction}, Stops={Stops}, Move={Move}, Wait={Wait}",
                    status.Id, status.CurrentFloor, status.Direction,
                    string.Join(",", status.Stops), status.MoveSecondsRemaining, status.WaitSecondsRemaining);
            }

            return statusList;
        }
    }

    public void Dispose()
    {
        _logger.LogInformation("Disposing ElevatorSimulationService...");
        StopSimulation();
    }
}