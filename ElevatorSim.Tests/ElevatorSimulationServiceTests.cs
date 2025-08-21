using ElevatorSimulator.Exceptions;
using ElevatorSimulator.Models;
using ElevatorSimulator.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System.Linq;

namespace ElevatorSim.Tests
{
    public class ElevatorSimulationServiceTests
    {
        [Fact]
        public void RequestRide_Throws_WhenNotRunning()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ElevatorSimulationService>>();
            var service = new ElevatorSimulationService(mockLogger.Object);
            var request = new RideRequest(1, 3);

            // Act & Assert
            var ex = Assert.Throws<SimulationNotRunningException>(() => service.RequestRide(request));
            Assert.Equal("Simulation has not been started.", ex.Message);
        }

        [Fact]
        public void StartSimulation_AllowsRequests()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ElevatorSimulationService>>();
            var service = new ElevatorSimulationService(mockLogger.Object);

            // Act
            service.StartSimulation();
            var request = new RideRequest(1, 3);
            service.RequestRide(request);

            // Assert
            var status = service.GetStatus();
            Assert.NotEmpty(status);

            service.StopSimulation();
        }

        [Fact]
        public void StartSimulation_Twice_DoesNotThrow()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ElevatorSimulationService>>();
            var service = new ElevatorSimulationService(mockLogger.Object);

            // Act & Assert
            service.StartSimulation();
            service.StartSimulation(); // Should not throw or reset state unexpectedly
        }

        [Fact]
        public void StopSimulation_Twice_DoesNotThrow()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ElevatorSimulationService>>();
            var service = new ElevatorSimulationService(mockLogger.Object);

            // Act & Assert
            service.StartSimulation();
            service.StopSimulation();
            service.StopSimulation(); // Should not throw
        }

        [Fact]
        public void GetStatus_ReturnsEmpty_WhenNotStarted()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ElevatorSimulationService>>();
            var service = new ElevatorSimulationService(mockLogger.Object);

            // Act
            var status = service.GetStatus();

            // Assert
            Assert.Empty(status);
        }

        [Fact]
        public void RequestRide_Throws_WhenSameStartAndEndFloor()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ElevatorSimulationService>>();
            var service = new ElevatorSimulationService(mockLogger.Object);
            service.StartSimulation();
            var request = new RideRequest(2, 2);

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => service.RequestRide(request));
            Assert.Contains("start and end floors must be different", ex.Message);
        }

        [Fact]
        public void RequestRide_Throws_WhenFloorOutOfRange()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ElevatorSimulationService>>();
            var service = new ElevatorSimulationService(mockLogger.Object);
            service.StartSimulation();
            var invalidLow = new RideRequest(-1, 2);
            var invalidHigh = new RideRequest(1, 100);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => service.RequestRide(invalidLow));
            Assert.Throws<ArgumentOutOfRangeException>(() => service.RequestRide(invalidHigh));
        }

        [Fact]
        public void MultipleRides_AreHandled()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ElevatorSimulationService>>();
            var service = new ElevatorSimulationService(mockLogger.Object);
            service.StartSimulation();

            // Act
            service.RequestRide(new RideRequest(1, 2));
            service.RequestRide(new RideRequest(2, 3));
            service.RequestRide(new RideRequest(3, 1));

            // Assert
            var status = service.GetStatus();
            Assert.NotEmpty(status);
            Assert.True(status.All(e => e.Id > 0));
        }
    }
}
