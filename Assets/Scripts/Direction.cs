using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
    NORTH = 0,
    RIGHT = 1,
    EAST = 2,
    BOTTOM = 3,
    SOUTH = 4,
    LEFT = 5,
    WEST = 6,
    TOP = 7
}

public static class DirectionHelper
{
    public static Direction FromVector(Vector2 direction)
    {
        if(direction == Vector2.zero)
        {
            Debug.Log("WARNING :: Try to get a Direction from a null vector, returned NORTH by default !");
            return Direction.NORTH;
        }
        else if(direction.x > 0)
        {
            if (direction.y > 0) return Direction.NORTH;
            else if (direction.y == 0) return Direction.RIGHT;
            else return Direction.EAST;
        }
        else if (direction.x < 0)
        {
            if (direction.y > 0) return Direction.WEST;
            else if (direction.y == 0) return Direction.LEFT;
            else return Direction.SOUTH;
        }
        else
        {
            if (direction.y > 0) return Direction.TOP;
            else return Direction.BOTTOM;
        }
    }

    public static Direction BetweenTwoObjects(GameObject from, GameObject to)
    {
        Vector2 direction = to.transform.position - from.transform.position;
        return FromVector(direction.normalized);
    }

    public static Direction Previous(Direction dir)
    {
        if (dir == Direction.NORTH) return Direction.TOP;
        dir--;
        return dir;
    }

    public static Direction Next(Direction dir)
    {
        if(dir == Direction.TOP) return Direction.NORTH;
        dir++;
        return dir;
    }

    public static float AngleDeg(Direction dir)
    {
        switch (dir)
        {
            case Direction.NORTH: return 135f;
            case Direction.RIGHT: return 90f;
            case Direction.EAST: return 45f;
            case Direction.BOTTOM: return 0f;
            case Direction.SOUTH: return 315f;
            case Direction.LEFT: return 270f;
            case Direction.WEST: return 225f;
            default: return 180f;
        }
    }

    public static Vector2 FromDirection(Direction dir)
    {
        switch(dir)
        {
            case Direction.NORTH: return new Vector2(1f, 1f);
            case Direction.RIGHT: return new Vector2(1f, 0f);
            case Direction.EAST: return new Vector2(1f, -1f);
            case Direction.BOTTOM: return new Vector2(0f, -1f);
            case Direction.SOUTH: return new Vector2(-1f, -1f);
            case Direction.LEFT: return new Vector2(-1f, 0f);
            case Direction.WEST: return new Vector2(-1f, 1f);
            case Direction.TOP: return new Vector2(0f, 1f);
        }
        return new Vector2(0f, 0f);
    }

    public static Direction Inverse(Direction dir)
    {
        switch (dir)
        {
            case Direction.NORTH: return Direction.SOUTH;
            case Direction.SOUTH: return Direction.NORTH;
            case Direction.EAST: return Direction.WEST;
            case Direction.WEST: return Direction.EAST;
            case Direction.LEFT: return Direction.RIGHT;
            case Direction.RIGHT: return Direction.LEFT;
            case Direction.TOP: return Direction.BOTTOM;
            case Direction.BOTTOM: return Direction.TOP;
            default:
            {
                Debug.Log("Direction :: Inverse :: Achievement Get \"How did we get here ?\"");
                return new Direction();
            }
        }
    }

    public static Vector3Int DirectionOffsetGrid(Direction dir)
    {
        switch (dir)
        {
            case Direction.NORTH: return new Vector3Int(1, 0);
            case Direction.SOUTH: return new Vector3Int(-1, 0);
            case Direction.EAST: return new Vector3Int(0, -1);
            case Direction.WEST: return new Vector3Int(0, 1);
            default: return Vector3Int.zero;
        }
    }
}
