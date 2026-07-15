using CommunityToolkit.Mvvm.ComponentModel;

namespace FortniteSwapper.ViewModels;

public abstract class ViewModelBase : ObservableObject
{
    public virtual string Title => string.Empty;
}
