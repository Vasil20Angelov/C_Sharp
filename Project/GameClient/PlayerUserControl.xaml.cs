using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GameClient
{
    /// <summary>
    /// Interaction logic for PlayerUserControl.xaml
    /// </summary>
    public partial class PlayerUserControl : UserControl
    {
        public event EventHandler<GUI_UpdateEvent> update;

        public bool myTurn { get; set; }
        public bool matchOn { get; set; }

        public PlayerUserControl()
        {
            InitializeComponent();
            myTurn = false;
            matchOn = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!myTurn || !matchOn) return;

            myTurn = false;

            Button button = (Button)sender;
            button.IsEnabled = false;

            PlayerNumbers.Content += (string)button.Content + "  ";

            // Get the number of the button
            int number = int.Parse((string)button.Content);

            // Send the selected number to the server
            update?.Invoke(this, new GUI_UpdateEvent(number));

            Turn.Content = "Waiting for opponent's turn";
        }
    }
}
