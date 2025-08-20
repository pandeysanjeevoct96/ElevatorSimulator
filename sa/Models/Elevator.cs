namespace ElevatorSimulator.Models;

using System.Collections.Generic;

public class Elevator(int id)
{
    public int Id { get; } = id;
    public int CurrentFloor { get; private set; } = 1;
    public Direction Direction { get; private set; } = Direction.Idle;
    public Queue<int> Stops { get; } = new();

    public int MoveSecondsRemaining { get; private set; }
    public int WaitSecondsRemaining { get; private set; }

    private const int MoveTimePerFloor = 10;
    private const int DwellTime = 10;

    public void Assign(int floor)
    {
        if (Stops.Contains(floor)) return;
        
        Stops.Enqueue(floor);

        if (Direction == Direction.Idle)
            SetNextTarget();
    }

    public void Tick()
    {
        // If waiting (doors open)
        if (WaitSecondsRemaining > 0)
        {
            WaitSecondsRemaining--;
            if (WaitSecondsRemaining == 0)
                SetNextTarget();
            return;
        }
        
        if (MoveSecondsRemaining <= 0) return;
        MoveSecondsRemaining--;

        if (MoveSecondsRemaining == 0)
            ArriveAtFloor();
    }

    private void SetNextTarget()
    {
        if (Stops.Count == 0)
        {
            Direction = Direction.Idle;
            return;
        }

        var target = Stops.Peek();

        if (target > CurrentFloor)
        {
            Direction = Direction.Up;
            MoveSecondsRemaining = MoveTimePerFloor * (target - CurrentFloor);
        }
        else if (target < CurrentFloor)
        {
            Direction = Direction.Down;
            MoveSecondsRemaining = MoveTimePerFloor * (CurrentFloor - target);
        }
        else
        {
            ArriveAtFloor();
        }
    }

    private void ArriveAtFloor()
    {
        var target = Stops.Dequeue();
        CurrentFloor = target;

        // dwell time at floor
        WaitSecondsRemaining = DwellTime;

        if (Stops.Count == 0)
        {
            Direction = Direction.Idle;
        }
        else
        {
            SetNextTarget();
        }
    }
}


