using LaserCut.Geometry;

namespace LaserCut.Tests.Primitives;

public class AnglesTests
{
    [Theory]
    [InlineData(1.421, 20.2705559)]
    [InlineData(2.464, 2.4640000)]
    [InlineData(-0.923, -13.4893706)]
    [InlineData(0.111, 12.6773706)]
    [InlineData(2.603, 15.1693706)]
    [InlineData(-1.675, 17.1745559)]
    [InlineData(-1.776, -1.7760000)]
    [InlineData(1.324, -11.2423706)]
    [InlineData(2.758, -3.5251853)]
    [InlineData(1.849, -17.0005559)]
    [InlineData(1.817, -4.4661853)]
    [InlineData(-2.641, -21.4905559)]
    [InlineData(-1.834, 10.7323706)]
    [InlineData(2.604, -16.2455559)]
    [InlineData(-1.463, 4.8201853)]
    [InlineData(-2.734, -9.0171853)]
    [InlineData(-2.964, 3.3191853)]
    [InlineData(-0.903, 11.6633706)]
    [InlineData(-1.452, 11.1143706)]
    [InlineData(1.905, 1.9050000)]
    [InlineData(2.274, 2.2740000)]
    [InlineData(-0.371, -19.2205559)]
    [InlineData(-0.862, -7.1451853)]
    [InlineData(0.081, 18.9305559)]
    [InlineData(1.661, 1.6610000)]
    [InlineData(0.422, 0.4220000)]
    [InlineData(2.150, 20.9995559)]
    [InlineData(1.750, 20.5995559)]
    [InlineData(0.987, -17.8625559)]
    [InlineData(-2.430, -8.7131853)]
    public void AngleAbsSigned(double expected, double input)
    {
        var result = Angles.AsSignedAbs(input);
        Assert.Equal(expected, result, 1e-6);
    }

    [Theory]
    [InlineData(1.550, -4.7331853)]
    [InlineData(1.191, 20.0405559)]
    [InlineData(5.331, -0.9521853)]
    [InlineData(0.035, 18.8845559)]
    [InlineData(2.104, -16.7455559)]
    [InlineData(2.272, 14.8383706)]
    [InlineData(3.071, 21.9205559)]
    [InlineData(2.936, -3.3471853)]
    [InlineData(2.297, -3.9861853)]
    [InlineData(5.717, 18.2833706)]
    [InlineData(1.772, 1.7720000)]
    [InlineData(5.887, -6.6793706)]
    [InlineData(0.899, -17.9505559)]
    [InlineData(3.470, -15.3795559)]
    [InlineData(1.962, -16.8875559)]
    [InlineData(1.556, 1.5560000)]
    [InlineData(5.958, 18.5243706)]
    [InlineData(5.497, -7.0693706)]
    [InlineData(5.756, -6.8103706)]
    [InlineData(1.229, 13.7953706)]
    [InlineData(1.743, 20.5925559)]
    [InlineData(0.811, 7.0941853)]
    [InlineData(0.252, -18.5975559)]
    [InlineData(2.878, -9.6883706)]
    [InlineData(0.845, -5.4381853)]
    [InlineData(5.637, -0.6461853)]
    [InlineData(4.628, 23.4775559)]
    [InlineData(3.210, 22.0595559)]
    [InlineData(4.327, 16.8933706)]
    [InlineData(3.318, -9.2483706)]
    public void AngleAbsPositive(double expected, double input)
    {
        var result = Angles.AsPositiveAbs(input);
        Assert.Equal(expected, result, 1e-6);
    }

    [Theory]
    [InlineData(0, Math.PI / 2.0, Math.PI / 2.0)]
    [InlineData(-16.5455559, 5.990, 2.0108147)]
    [InlineData(0.9170000, 3.276, 23.0425559)]
    [InlineData(11.7323706, 4.609, -15.0745559)]
    [InlineData(16.0813706, 2.770, -12.5645559)]
    [InlineData(-11.5771853, 4.143, -7.4341853)]
    [InlineData(20.7175559, 1.992, -2.4231853)]
    [InlineData(9.1091853, 2.804, -13.2195559)]
    [InlineData(6.0721853, 4.587, -14.4735559)]
    [InlineData(13.6295559, 0.208, -17.5783706)]
    [InlineData(-5.3520000, 4.949, -0.4030000)]
    [InlineData(18.1325559, 5.574, 11.1401853)]
    [InlineData(-4.7260000, 4.761, 18.8845559)]
    [InlineData(-6.5421853, 6.170, -12.9385559)]
    [InlineData(9.7643706, 3.226, 6.7071853)]
    [InlineData(10.1803706, 0.163, -2.2230000)]
    [InlineData(9.3303706, 1.324, -20.7615559)]
    [InlineData(16.3945559, 5.435, -3.3031853)]
    [InlineData(-5.7140000, 4.718, -19.8455559)]
    [InlineData(11.8183706, 2.045, -11.2693706)]
    [InlineData(9.0173706, 5.400, -4.4321853)]
    [InlineData(13.5165559, 5.768, 0.4350000)]
    [InlineData(9.6953706, 3.769, 13.4643706)]
    [InlineData(18.2645559, 2.711, 14.6923706)]
    [InlineData(-12.4111853, 5.727, -19.2505559)]
    [InlineData(-16.8875559, 5.870, 14.1151853)]
    [InlineData(8.3213706, 2.253, 16.8575559)]
    [InlineData(5.5180000, 3.186, 14.9871853)]
    [InlineData(-3.4290000, 3.103, -19.1755559)]
    [InlineData(7.9643706, 1.023, -22.4285559)]
    [InlineData(15.1603706, 2.502, -7.4703706)]
    public void BetweenCounterClockwise(double start, double ccw, double end)
    {
        var result = Angles.BetweenCcw(start, end);
        Assert.Equal(ccw, result, 1e-6);
    }
    
    [Theory]
    [InlineData(0, Math.PI / 2.0, -Math.PI / 2.0)]
    [InlineData(5.0650000, 0.613, 17.0183706)]
    [InlineData(18.4023706, 1.110, 23.5755559)]
    [InlineData(-17.7283706, 0.435, -18.1633706)]
    [InlineData(14.6745559, 1.949, -6.1240000)]
    [InlineData(-14.2573706, 3.901, 13.2575559)]
    [InlineData(16.8283706, 5.386, -1.1240000)]
    [InlineData(-12.6575559, 2.357, -2.4481853)]
    [InlineData(13.7773706, 1.974, 5.5201853)]
    [InlineData(23.8345559, 2.865, 8.4031853)]
    [InlineData(5.2200000, 0.328, 17.4583706)]
    [InlineData(21.9875559, 2.647, -18.3585559)]
    [InlineData(-0.2170000, 2.241, 16.3915559)]
    [InlineData(11.2293706, 3.686, -23.8725559)]
    [InlineData(0.6030000, 2.017, -7.6971853)]
    [InlineData(-15.4813706, 2.351, -5.2660000)]
    [InlineData(-19.9645559, 6.267, -7.3820000)]
    [InlineData(-1.3401853, 2.311, 21.4815559)]
    [InlineData(22.8255559, 1.543, -10.1333706)]
    [InlineData(7.6471853, 0.120, -11.3223706)]
    [InlineData(12.0861853, 4.536, -5.0161853)]
    [InlineData(6.5923706, 0.858, -25.6815559)]
    [InlineData(-9.5981853, 2.417, 13.1175559)]
    [InlineData(-4.0071853, 1.865, 6.6941853)]
    [InlineData(12.3193706, 1.401, 10.9183706)]
    [InlineData(-6.1331853, 2.427, -14.8433706)]
    [InlineData(0.6390000, 1.486, 11.7193706)]
    [InlineData(-17.4623706, 0.647, -11.8261853)]
    [InlineData(8.4101853, 3.435, 4.9751853)]
    [InlineData(-22.1435559, 0.354, -16.2143706)]
    [InlineData(-8.5473706, 5.008, 17.8605559)]
    public void BetweenClockwise(double start, double cw, double end)
    {
        var result = Angles.BetweenCw(start, end);
        Assert.Equal(cw, result, 1e-6);
    }
    
}