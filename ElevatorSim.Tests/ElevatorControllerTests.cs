using ElevatorSimulator.Controllers;
using ElevatorSimulator.Exceptions;
using ElevatorSimulator.Models;
using ElevatorSimulator.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ElevatorSim.Tests
{
    public class ElevatorControllerTests
    {
        private readonly Mock<IElevatorSimulationService> _mockService;
        private readonly Mock<ILogger<ElevatorController>> _mockLogger;
        private readonly ElevatorController _controller;

        public ElevatorControllerTests()
        {
            _mockService = new Mock<IElevatorSimulationService>(MockBehavior.Strict);
            _mockLogger = new Mock<ILogger<ElevatorController>>();
            _controller = new ElevatorController(_mockService.Object, _mockLogger.Object);
        }

        [Fact]
        public void GetStatus_ReturnsOkWithElevatorStatusList()
        {
            // Arrange
            var status = new List<ElevatorStatus> { new ElevatorStatus { Id = 1, CurrentFloor = 1 } };
            _mockService.Setup(s => s.GetStatus()).Returns(status);

            // Act
            var result = _controller.GetStatus();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsAssignableFrom<List<ElevatorStatus>>(okResult.Value);
            Assert.Single(value);
            Assert.Equal(1, value[0].Id);
            _mockService.Verify(s => s.GetStatus(), Times.Once);
        }

        [Fact]
        public void RequestRide_ReturnsOk_WhenSuccessful()
        {
            // Arrange
            var req = new RideRequest(1, 5);
            _mockService.Setup(s => s.RequestRide(req)).Verifiable();

            // Act
            var result = _controller.RequestRide(req);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            Assert.Equal("Ride assigned", okResult.Value.GetType().GetProperty("message")?.GetValue(okResult.Value));
            _mockService.Verify(s => s.RequestRide(req), Times.Once);
        }

        [Fact]
        public void RequestRide_ReturnsBadRequest_WhenSimulationNotRunning()
        {
            // Arrange
            var req = new RideRequest(1, 5);
            _mockService.Setup(s => s.RequestRide(req)).Throws(new SimulationNotRunningException("Simulation not started"));

            // Act
            var result = _controller.RequestRide(req);

            // Assert
            var badReq = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Simulation not started", badReq.Value);
            _mockService.Verify(s => s.RequestRide(req), Times.Once);
        }

        [Fact]
        public void RequestRide_ReturnsConflict_WhenCarBusy()
        {
            // Arrange
            var req = new RideRequest(2, 7);
            _mockService.Setup(s => s.RequestRide(req)).Throws(new CarBusyException("All cars are busy"));

            // Act
            var result = _controller.RequestRide(req);

            // Assert
            var conflict = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal("All cars are busy", conflict.Value);
            _mockService.Verify(s => s.RequestRide(req), Times.Once);
        }

        [Fact]
        public void RequestRide_ReturnsBadRequest_WhenPickupAndDestinationAreTheSame()
        {
            // Arrange
            var req = new RideRequest(5, 5);
            _mockService.Setup(s => s.RequestRide(req)).Throws(new ArgumentException("Pickup and destination floors must be different."));

            // Act
            var result = _controller.RequestRide(req);

            // Assert
            var badReq = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Pickup and destination floors must be different.", badReq.Value);
            _mockService.Verify(s => s.RequestRide(req), Times.Once);
        }

        [Fact]
        public void Start_ReturnsOk()
        {
            // Arrange
            _mockService.Setup(s => s.StartSimulation()).Verifiable();

            // Act
            var result = _controller.Start();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            Assert.Equal("Simulation started", okResult.Value.GetType().GetProperty("message")?.GetValue(okResult.Value));
            _mockService.Verify(s => s.StartSimulation(), Times.Once);
        }

        [Fact]
        public void Stop_ReturnsOk()
        {
            // Arrange
            _mockService.Setup(s => s.StopSimulation()).Verifiable();

            // Act
            var result = _controller.Stop();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            Assert.Equal("Simulation stopped", okResult.Value.GetType().GetProperty("message")?.GetValue(okResult.Value));
            _mockService.Verify(s => s.StopSimulation(), Times.Once);
        }
    }
}
