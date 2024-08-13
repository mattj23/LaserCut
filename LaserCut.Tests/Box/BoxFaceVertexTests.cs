using LaserCut.Box;
using LaserCut.Geometry;
using LaserCut.Tests.Helpers;
using MathNet.Spatial.Euclidean;

namespace LaserCut.Tests.Box;

public class BoxFaceVertexTests
{
    [Fact]
    public void FrontFaceVertices()
    {
        var p = SimpleBoxVertices();
        var face = new BoxFrontFace(p.Item2, p.Item3, p.Item1);

        var testA = face.CsInv.Transform(face.Envelope.A);
        var testB = face.CsInv.Transform(face.Envelope.B);
        var testC = face.CsInv.Transform(face.Envelope.C);
        var testD = face.CsInv.Transform(face.Envelope.D);

        var expectA = new Point3D(-1, -0.5, 0);
        var expectB = new Point3D(1, -0.5, 0);
        var expectC = new Point3D(1, 0.5, 0);
        var expectD = new Point3D(-1, 0.5, 0);

        Assert.Equal(expectA, testA, PointCheck.Default);
        Assert.Equal(expectB, testB, PointCheck.Default);
        Assert.Equal(expectC, testC, PointCheck.Default);
        Assert.Equal(expectD, testD, PointCheck.Default);

        Assert.Equal(face.Envelope.A, face.FaceToWorld(testA.ToPoint2D()), PointCheck.Default);
        Assert.Equal(face.Envelope.B, face.FaceToWorld(testB.ToPoint2D()), PointCheck.Default);
        Assert.Equal(face.Envelope.C, face.FaceToWorld(testC.ToPoint2D()), PointCheck.Default);
        Assert.Equal(face.Envelope.D, face.FaceToWorld(testD.ToPoint2D()), PointCheck.Default);

        Assert.Equal(p.Item1.BaseInset, face.Common.A.Z - face.Envelope.A.Z, 1e-6);
        Assert.Equal(UnitVector3D.XAxis.ToVector3D(), face.Cs.ZAxis, VectorCheck.Default);
    }

    [Fact]
    public void BackFaceVertices()
    {
        var p = SimpleBoxVertices();
        var face = new BoxBackFace(p.Item2, p.Item3, p.Item1);

        var testA = face.CsInv.Transform(face.Envelope.A);
        var testB = face.CsInv.Transform(face.Envelope.B);
        var testC = face.CsInv.Transform(face.Envelope.C);
        var testD = face.CsInv.Transform(face.Envelope.D);

        var expectA = new Point3D(-1, -0.5, 0);
        var expectB = new Point3D(1, -0.5, 0);
        var expectC = new Point3D(1, 0.5, 0);
        var expectD = new Point3D(-1, 0.5, 0);

        Assert.Equal(expectA, testA, PointCheck.Default);
        Assert.Equal(expectB, testB, PointCheck.Default);
        Assert.Equal(expectC, testC, PointCheck.Default);
        Assert.Equal(expectD, testD, PointCheck.Default);

        Assert.Equal(face.Envelope.A, face.FaceToWorld(testA.ToPoint2D()), PointCheck.Default);
        Assert.Equal(face.Envelope.B, face.FaceToWorld(testB.ToPoint2D()), PointCheck.Default);
        Assert.Equal(face.Envelope.C, face.FaceToWorld(testC.ToPoint2D()), PointCheck.Default);
        Assert.Equal(face.Envelope.D, face.FaceToWorld(testD.ToPoint2D()), PointCheck.Default);

        Assert.Equal(p.Item1.BaseInset, face.Common.A.Z - face.Envelope.A.Z, 1e-6);
        Assert.Equal(-UnitVector3D.XAxis.ToVector3D(), face.Cs.ZAxis, VectorCheck.Default);
    }

    [Fact]
    public void RightFaceVertices()
    {
        var p = SimpleBoxVertices();
        var face = new BoxRightFace(p.Item2, p.Item3, p.Item1);

        var testA = face.CsInv.Transform(face.Envelope.A);
        var testB = face.CsInv.Transform(face.Envelope.B);
        var testC = face.CsInv.Transform(face.Envelope.C);
        var testD = face.CsInv.Transform(face.Envelope.D);

        var expectA = new Point3D(-1.5, -0.5, 0);
        var expectB = new Point3D(1.5, -0.5, 0);
        var expectC = new Point3D(1.5, 0.5, 0);
        var expectD = new Point3D(-1.5, 0.5, 0);

        Assert.Equal(expectA, testA, PointCheck.Default);
        Assert.Equal(expectB, testB, PointCheck.Default);
        Assert.Equal(expectC, testC, PointCheck.Default);
        Assert.Equal(expectD, testD, PointCheck.Default);

        Assert.Equal(face.Envelope.A, face.FaceToWorld(testA.ToPoint2D()), PointCheck.Default);
        Assert.Equal(face.Envelope.B, face.FaceToWorld(testB.ToPoint2D()), PointCheck.Default);
        Assert.Equal(face.Envelope.C, face.FaceToWorld(testC.ToPoint2D()), PointCheck.Default);
        Assert.Equal(face.Envelope.D, face.FaceToWorld(testD.ToPoint2D()), PointCheck.Default);

        Assert.Equal(p.Item1.BaseInset, face.Common.A.Z - face.Envelope.A.Z, 1e-6);
        Assert.Equal(UnitVector3D.YAxis.ToVector3D(), face.Cs.ZAxis, VectorCheck.Default);
    }

    [Fact]
    public void LeftFaceVertices()
    {
        var p = SimpleBoxVertices();
        var face = new BoxLeftFace(p.Item2, p.Item3, p.Item1);

        var testA = face.CsInv.Transform(face.Envelope.A);
        var testB = face.CsInv.Transform(face.Envelope.B);
        var testC = face.CsInv.Transform(face.Envelope.C);
        var testD = face.CsInv.Transform(face.Envelope.D);

        var expectA = new Point3D(-1.5, -0.5, 0);
        var expectB = new Point3D(1.5, -0.5, 0);
        var expectC = new Point3D(1.5, 0.5, 0);
        var expectD = new Point3D(-1.5, 0.5, 0);

        Assert.Equal(expectA, testA, PointCheck.Default);
        Assert.Equal(expectB, testB, PointCheck.Default);
        Assert.Equal(expectC, testC, PointCheck.Default);
        Assert.Equal(expectD, testD, PointCheck.Default);

        Assert.Equal(face.Envelope.A, face.FaceToWorld(testA.ToPoint2D()), PointCheck.Default);
        Assert.Equal(face.Envelope.B, face.FaceToWorld(testB.ToPoint2D()), PointCheck.Default);
        Assert.Equal(face.Envelope.C, face.FaceToWorld(testC.ToPoint2D()), PointCheck.Default);
        Assert.Equal(face.Envelope.D, face.FaceToWorld(testD.ToPoint2D()), PointCheck.Default);

        Assert.Equal(p.Item1.BaseInset, face.Common.A.Z - face.Envelope.A.Z, 1e-6);
        Assert.Equal(-UnitVector3D.YAxis.ToVector3D(), face.Cs.ZAxis, VectorCheck.Default);
    }

    [Fact]
    public void TopFaceVertices()
    {
        var p = SimpleBoxVertices();
        var face = new BoxTopFace(p.Item2, p.Item3, p.Item1);

        var testA = face.CsInv.Transform(face.Envelope.A);
        var testB = face.CsInv.Transform(face.Envelope.B);
        var testC = face.CsInv.Transform(face.Envelope.C);
        var testD = face.CsInv.Transform(face.Envelope.D);

        var expectA = new Point3D(-1, -1.5, 0);
        var expectB = new Point3D(1, -1.5, 0);
        var expectC = new Point3D(1, 1.5, 0);
        var expectD = new Point3D(-1, 1.5, 0);

        Assert.Equal(expectA, testA, PointCheck.Default);
        Assert.Equal(expectB, testB, PointCheck.Default);
        Assert.Equal(expectC, testC, PointCheck.Default);
        Assert.Equal(expectD, testD, PointCheck.Default);

        Assert.Equal(face.Envelope.A, face.FaceToWorld(testA.ToPoint2D()), PointCheck.Default);
        Assert.Equal(face.Envelope.B, face.FaceToWorld(testB.ToPoint2D()), PointCheck.Default);
        Assert.Equal(face.Envelope.C, face.FaceToWorld(testC.ToPoint2D()), PointCheck.Default);
        Assert.Equal(face.Envelope.D, face.FaceToWorld(testD.ToPoint2D()), PointCheck.Default);

        Assert.Equal(UnitVector3D.ZAxis.ToVector3D(), face.Cs.ZAxis, VectorCheck.Default);
    }

    [Fact]
    public void BottomFaceVertices()
    {
        var p = SimpleBoxVertices();
        var face = new BoxBottomFace(p.Item2, p.Item3, p.Item1);

        var testA = face.CsInv.Transform(face.Envelope.A);
        var testB = face.CsInv.Transform(face.Envelope.B);
        var testC = face.CsInv.Transform(face.Envelope.C);
        var testD = face.CsInv.Transform(face.Envelope.D);

        var expectA = new Point3D(-1, -1.5, 0);
        var expectB = new Point3D(1, -1.5, 0);
        var expectC = new Point3D(1, 1.5, 0);
        var expectD = new Point3D(-1, 1.5, 0);

        Assert.Equal(expectA, testA, PointCheck.Default);
        Assert.Equal(expectB, testB, PointCheck.Default);
        Assert.Equal(expectC, testC, PointCheck.Default);
        Assert.Equal(expectD, testD, PointCheck.Default);

        Assert.Equal(face.Envelope.A, face.FaceToWorld(testA.ToPoint2D()), PointCheck.Default);
        Assert.Equal(face.Envelope.B, face.FaceToWorld(testB.ToPoint2D()), PointCheck.Default);
        Assert.Equal(face.Envelope.C, face.FaceToWorld(testC.ToPoint2D()), PointCheck.Default);
        Assert.Equal(face.Envelope.D, face.FaceToWorld(testD.ToPoint2D()), PointCheck.Default);

        Assert.Equal(-UnitVector3D.ZAxis.ToVector3D(), face.Cs.ZAxis, VectorCheck.Default);
    }




    private Tuple<BoxParams, Point3D[], Point3D[]> SimpleBoxVertices()
    {
        var p = new BoxParams()
        {
            BaseInset = 0.1,
            Length = 3,
            Width = 2,
            Height = 1,
            Thickness = 0.05
        };

        var commonVertices = p.CommonVertices();
        var envelopeVertices = p.EnvelopeVertices();
        return new Tuple<BoxParams, Point3D[], Point3D[]>(p, envelopeVertices, commonVertices);
    }
}
