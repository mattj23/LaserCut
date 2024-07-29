using System;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using LaserCut.Algorithms;
using LaserCut.Avalonia.Models;
using LaserCut.Avalonia.Sample.ViewModels;
using LaserCut.Avalonia.ViewModels;
using LaserCut.Avalonia.Views;
using ReactiveUI;

namespace LaserCut.Avalonia.Sample.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }
    
    protected override void OnDataContextChanged(EventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.ImportMeshInteraction.RegisterHandler(DoImportMeshInteraction);
        }
        
        base.OnDataContextChanged(e);
    }

    private async Task DoImportMeshInteraction(InteractionContext<Unit, ImportedGeometry?> ctx)
    {
        // File open dialog
        var options = new FilePickerOpenOptions
        {
            Title = "Open STL File",
            FileTypeFilter = new[]
                { new FilePickerFileType("Stereolithography") { Patterns = new[] { "*.stl", "*.STL" } } },
            AllowMultiple = false
        };
        var files = await StorageProvider.OpenFilePickerAsync(options);

        if (files.FirstOrDefault() is { } file)
        {
            var vm = new MeshImportViewModel(file.TryGetLocalPath());
            var dialog = new MeshImportDialog { DataContext = vm };
            var result = await dialog.ShowDialog<ImportedGeometry?>(this);

            ctx.SetOutput(result);
        }
        else
        {
            ctx.SetOutput(null);
        }
        
    }
}