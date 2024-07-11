using System.Collections.Generic;
using System.Threading.Tasks;

namespace EGOIST.Presentation.UI.Interfaces.Navigation;

public interface INavigationAware
{
    Task Initialize(Dictionary<string, object>? parameters);
    Task OnNavigatedFrom();
    Task OnNavigatedTo();
}