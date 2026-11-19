using System.ComponentModel;
using System.Threading.Tasks;

namespace CoolapkUWP.ViewModels
{
    internal interface IViewModel : INotifyPropertyChanged
    {
        string Title { get; }
        Task Refresh(bool reset);
        bool IsEqual(IViewModel other);
    }
}
