using System.Windows;
using System.Windows.Controls;

namespace TopWordsTestApp.Views
{
	public partial class GearsControl : UserControl
	{
		public GearsControl()
		{
			this.InitializeComponent();
		}

        public static readonly DependencyProperty IsRunningProperty =
        DependencyProperty.Register("IsRunning", typeof(bool), typeof(GearsControl), new PropertyMetadata(false));

        public bool IsRunning
        {
            get { return (bool)GetValue(IsRunningProperty); }
            set { SetValue(IsRunningProperty, value); }
        }
	}
}