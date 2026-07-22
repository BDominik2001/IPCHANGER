using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace IpChanger.ViewModels;

/// <summary>
/// Egyszerű INotifyPropertyChanged alap az összes ViewModelhez.
/// </summary>
public abstract class ObservableObject : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    /// <summary>Beállítja a mezőt és jelzi a változást, ha az érték ténylegesen módosult.</summary>
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
