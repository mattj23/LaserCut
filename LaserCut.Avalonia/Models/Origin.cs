using System.Reactive.Linq;
using System.Reactive.Subjects;
using LaserCut.Geometry;
using MathNet.Numerics.LinearAlgebra.Double;
using ReactiveUI;

namespace LaserCut.Avalonia.Models;

public class Origin : ReactiveObject, IOrigin
{
    private Matrix _parentMatrix = Isometry2.Identity();
    private Guid _parentId = Guid.Empty;
    private IDisposable? _updates;

    private readonly BehaviorSubject<Matrix> _changed;
    private double _x;
    private double _y;
    private double _r;

    public Origin() : this(Guid.NewGuid(), 0, 0, 0) { }

    public Origin(Guid id, double x, double y, double r)
    {
        Id = id;
        X = x;
        Y = y;
        R = r;
        _changed = new BehaviorSubject<Matrix>(Transform);
    }

    public Guid Id { get; }
    
    public Guid ParentId => _parentId;

    public double X
    {
        get => _x;
        private set => this.RaiseAndSetIfChanged(ref _x, value);
    }

    public double Y
    {
        get => _y;
        private set => this.RaiseAndSetIfChanged(ref _y, value);
    }

    public double R
    {
        get => _r;
        private set => this.RaiseAndSetIfChanged(ref _r, value);
    }

    public Xyr Xyr => new(X, Y, R);

    public IObservable<Matrix> MatrixChanged  => _changed.AsObservable();

    public Matrix LocalTransform => (Matrix)(Isometry2.Translate(X, Y) * Isometry2.Rotate(double.RadiansToDegrees(R)));
    
    public Matrix Transform  => (Matrix)(_parentMatrix * LocalTransform);
    
    public Matrix InverseTransform => (Matrix)Transform.Inverse();

    public void SetParent(IOrigin? parent)
    {
        _updates?.Dispose();
        if (parent == null)
        {
            _parentId = Guid.Empty;
            _updates = null;
            ParentUpdate(Isometry2.Identity());
        }
        else
        {
            _parentId = parent.Id;
            _updates = parent.MatrixChanged.Subscribe(ParentUpdate);
            ParentUpdate(parent.Transform);
        }
    }
    
    public void Update(double x, double y, double r)
    {
        X = x;
        Y = y;
        R = r;
        _changed.OnNext(Transform);
    }
    
    public void Translate(double dx, double dy)
    {
        X += dx;
        Y += dy;
        _changed.OnNext(Transform);
    }

    public void RotateInPlace(double degrees)
    {
        var radians = double.DegreesToRadians(degrees);
        Update(X, Y, R + radians);
    }

    private void ParentUpdate(Matrix matrix)
    {
        _parentMatrix = matrix;
        _changed.OnNext(Transform);
    }
    
}