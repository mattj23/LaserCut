namespace LaserCut.Algorithms;

/// <summary>
/// Represents an intersection between two geometric elements, with the corresponding positions on each element.
/// </summary>
/// <param name="First"></param>
/// <param name="Second"></param>
public record struct ElementIntersection(Position First, Position Second)
{
    
    
}