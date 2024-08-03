﻿using LaserCut.Avalonia.Models;
using ReactiveUI;

namespace LaserCut.Avalonia.ViewModels;

public class OriginEditViewModel: XyrViewModel
{
    private readonly Origin _origin;
    
    public OriginEditViewModel(Origin origin, UnitViewModel units) : base(units, true)
    {
        _origin = origin;
        SetValues(_origin.X, _origin.Y, _origin.R);
        _origin.WhenAnyValue(x => x.X, x => x.Y, x => x.R).Subscribe(OnOriginGotNewValues);
    }

    protected override void OnNewEditValues(double xMm, double yMm, double r)
    {
        _origin.Update(xMm, yMm, r);
    }
    
    private void OnOriginGotNewValues((double, double, double) v)
    {
        SetValues(v.Item1, v.Item2, v.Item3);
    }
}