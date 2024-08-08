using MathNet.Numerics.LinearAlgebra.Double;

namespace LaserCut.Avalonia.Models;

public interface IOrigin
{
    Guid Id { get; }
    double X { get; }
    double Y { get; }
    double R { get; }
    
    IObservable<Matrix> MatrixChanged { get; }
    
    IObservable<IOrigin> ParentChanged { get; }
    
    Guid ParentId { get; }
    
    /// <summary>
    /// Gets the overall transform which includes both all parent transforms and the local transform created by the
    /// origin's X, Y, and R values
    /// </summary>
    Matrix Transform { get; }
    
    Matrix InverseTransform { get; }
    
    Matrix ParentTransform { get; }
    
    Matrix ParentInverse { get; }
}