using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EGOIST.Domain.Abstracts;

public abstract class BaseEntity : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public void Notify<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
