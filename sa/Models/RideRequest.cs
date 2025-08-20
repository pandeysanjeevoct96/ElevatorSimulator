namespace ElevatorSimulator.Models;

public sealed record RideRequest(int PickupFloor, int DestinationFloor)
{
    public Direction DesiredDirection => DestinationFloor > PickupFloor ? Direction.Up : Direction.Down;
}