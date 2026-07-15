namespace FortniteSwapper.Services;

/// <summary>
/// Decouples child view models from the MainViewModel navigation. Child VMs call
/// <see cref="RequestNavigate"/>; MainViewModel subscribes and switches the view.
/// </summary>
public interface INavigationService
{
    void RequestNavigate(string view);
    event System.Action<string>? NavigationRequested;
}

public class NavigationService : INavigationService
{
    public event System.Action<string>? NavigationRequested;

    public void RequestNavigate(string view) => NavigationRequested?.Invoke(view);
}
