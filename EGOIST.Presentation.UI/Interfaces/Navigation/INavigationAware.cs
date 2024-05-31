using System.Collections.Generic;

namespace EGOIST.Presentation.UI.Interfaces.Navigation;

public interface INavigationAware
{
    void Initialize(Dictionary<string, object>? parameters);
    void OnNavigatedFrom();
    void OnNavigatedTo();
}