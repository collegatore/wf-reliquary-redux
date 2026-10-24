using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace WF.Reliquary.Presentation;

/// <summary>
/// Lets the app's ⋮ menus (MenuFlyouts with <c>FlyoutPresenterClasses="app-menu"</c>) open
/// with no item highlighted while keyboard navigation still shows where it is.
/// Fluent's menu selects its first item on open, and arrow keys move that same
/// <c>:selected</c> state without setting <c>:focus-visible</c>, so styles alone can't
/// tell "just opened" from "moved with the keyboard". This marks the menu's presenter
/// with a <c>keyboard-nav</c> class on its first key press and clears the class each
/// time the menu opens; Styles/Dropdowns.axaml highlights <c>:selected</c> items only
/// under that class.
/// </summary>
public static class MenuKeyboardHighlight
{
    private const string MenuClass = "app-menu";
    private const string KeyboardClass = "keyboard-nav";

    /// <summary>Registers the app-wide handlers. Call once at startup.</summary>
    public static void Register()
    {
        InputElement.KeyDownEvent.AddClassHandler<MenuFlyoutPresenter>(
            (presenter, _) =>
            {
                if (presenter.Classes.Contains(MenuClass) && !presenter.Classes.Contains(KeyboardClass))
                    presenter.Classes.Add(KeyboardClass);
            },
            RoutingStrategies.Tunnel,
            handledEventsToo: true);

        // The presenter is reused across opens, so reset the flag whenever such a menu opens.
        Avalonia.Controls.Primitives.FlyoutBase.IsOpenProperty.Changed.AddClassHandler<MenuFlyout>(
            (flyout, e) =>
            {
                if (e.NewValue is not true || !flyout.FlyoutPresenterClasses.Contains(MenuClass)) return;
                var presenter = flyout.Items.OfType<Visual>()
                    .Select(item => item.FindAncestorOfType<MenuFlyoutPresenter>())
                    .FirstOrDefault(p => p is not null);
                presenter?.Classes.Remove(KeyboardClass);
            });
    }
}
