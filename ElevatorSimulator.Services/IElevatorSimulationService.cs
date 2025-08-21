using ElevatorSimulator.Models;

namespace ElevatorSimulator.Services
{
    public interface IElevatorSimulationService
    {
        void StartSimulation();
        void StopSimulation();
        void RequestRide(RideRequest request);
        List<ElevatorStatus> GetStatus();
    }
}