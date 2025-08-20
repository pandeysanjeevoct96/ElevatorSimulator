namespace ElevatorSimulator.Models;

public class ElevatorStatus {
    public int Id { get; set; }
    public int CurrentFloor { get; set; }
    public Direction Direction { get; set; }
    public Queue<int> Stops { get; set; } = [];
    public int MoveSecondsRemaining { get; set; }
    public int WaitSecondsRemaining { get; set; }
}