using LaserCut.Algorithms.Loop;
using LaserCut.Geometry;
using Microsoft.FSharp.Collections;
using Plotly.NET;
using Plotly.NET.LayoutObjects;

namespace LaserCut.Tests.Plotting;

public class DebugPlot
{
    private readonly string _title;
    private readonly List<Trace> _traces = new();
    
    public DebugPlot(string title)
    {
        _title = title;
    }
    
    public void Add(Body body, string label)
    {
        var outerName = body.Inners.Count == 0 ? label : $"{label} Outer";
        Add(body.Outer, outerName);
        for (int i = 0; i < body.Inners.Count; i++)
        {
            Add(body.Inners[i], $"{label} Inner {i}");
        }
    }
    
    public void Add(BoundaryLoop loop, string label)
    {
        // TODO: Fix this
        var points = loop.ToItemArray();
        var x = points.Select(p => p.Point.X).ToList();
        var y = points.Select(p => p.Point.Y).ToList();
        x.Add(x.First());
        y.Add(y.First());

        var trace = new Trace("scatter");
        trace.SetValue("x", x);
        trace.SetValue("y", y);
        trace.SetValue("mode", "lines+markers");
        trace.SetValue("name", label);
        _traces.Add(trace);
    }

    public void Add(PointLoop loop, string label)
    {
        var points = loop.ToItemArray();
        var x = points.Select(p => p.X).ToList();
        var y = points.Select(p => p.Y).ToList();
        x.Add(x.First());
        y.Add(y.First());

        var trace = new Trace("scatter");
        trace.SetValue("x", x);
        trace.SetValue("y", y);
        trace.SetValue("mode", "lines+markers");
        trace.SetValue("name", label);
        _traces.Add(trace);
    }
    
    public void Plot()
    {
        var xAxis = new LinearAxis();
        xAxis.SetValue("showgrid", true);
        xAxis.SetValue("zeroline", true);
        xAxis.SetValue("showline", true);
        xAxis.SetValue("autorange", true);
        
        var yAxis = new LinearAxis();
        yAxis.SetValue("showgrid", true);
        yAxis.SetValue("zeroline", true);
        yAxis.SetValue("showline", true);
        yAxis.SetValue("scaleanchor", "x");
        yAxis.SetValue("scaleratio", 1);
        yAxis.SetValue("autorange", true);

        var layout = new Layout();
        layout.SetValue("title", _title);
        layout.SetValue("xaxis", xAxis);
        layout.SetValue("yaxis", yAxis);
        layout.SetValue("showlegend", true);
        layout.SetValue("autosize", true);
        
        // Create empty chart
        var traces = ListModule.OfSeq(_traces);
        var chart = GenericChart.ofTraceObjects(true, traces)
            .WithLayout(layout);
        chart.Show();
    }
    
}