using System.Windows.Input;
using FontAwesome.Sharp;

namespace ClientApplication.ViewModel;

public class MainViewModel: ViewModelBase
{
	//Fields
    private ViewModelBase _currentChildView;
    private string _caption;
    private IconChar _icon;
    //Properties
    
    public ViewModelBase CurrentChildView
    {
        get => _currentChildView;
        set
        {
            _currentChildView = value;
            OnPropertyChanged(nameof(CurrentChildView));
        }
    }
    public string Caption
    {
        get => _caption;
        set
        {
            _caption = value;
            OnPropertyChanged(nameof(Caption));
        }
    }
    public IconChar Icon
    {
        get => _icon;
        set
        {
            _icon = value;
            OnPropertyChanged(nameof(Icon));
        }
    }
    //--> Commands
    public ICommand ShowHomeViewCommand { get; }
   
    public MainViewModel()
    {
        
        //Initialize commands
        ShowHomeViewCommand = new ViewModelCommand(ExecuteShowHomeViewCommand);
        //Default view
        ExecuteShowHomeViewCommand(null);
    }
  
    private void ExecuteShowHomeViewCommand(object obj)
    {
        CurrentChildView = new HomeViewModel();
        Caption = "Dashboard";
        Icon = IconChar.Home;
    }
}