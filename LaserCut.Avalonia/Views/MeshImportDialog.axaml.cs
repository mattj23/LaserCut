using System.Reactive;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LaserCut.Avalonia.Models;
using LaserCut.Avalonia.ViewModels;
using ReactiveUI;

namespace LaserCut.Avalonia.Views;

public partial class MeshImportDialog : Window
{
    public MeshImportDialog()
    {
        InitializeComponent();
    }
    
    protected override void OnDataContextChanged(EventArgs e)
    {
        if (DataContext is MeshImportViewModel vm)
        {
            vm.ZoomToFit.RegisterHandler(DoZoomToFit);
            vm.Confirm.RegisterHandler(DoConfirm);
        }
        
        base.OnDataContextChanged(e);
    }

    private Task DoConfirm(InteractionContext<ImportedGeometry, Unit> interaction)
    {
        Close(interaction.Input);
        interaction.SetOutput(Unit.Default);
        return Task.CompletedTask;
    }

    private async Task DoZoomToFit(InteractionContext<Unit, Unit> interaction)
    {
        await Viewport.ZoomToFit();
        interaction.SetOutput(Unit.Default);
    }

}