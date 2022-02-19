using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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
    public partial class MainWindow : Window
    {
        #region Fields
        private Thread outputThread;
        private TcpClient connection;
        private NetworkStream stream;
        private BinaryWriter writer;
        private BinaryReader reader;
        private bool matchOn = true; 
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            pl_ucl.update += SendMessage;
            EstablishConnection();
        }

        // Sends message to the server
        private void SendMessage(object sender, GUI_UpdateEvent e)
        {
            writer.Write(e.Number);
        }

        // Creates the connection
        private void EstablishConnection()
        {
            connection = new TcpClient("127.0.0.1", 50000);
            stream = connection.GetStream();
            writer = new BinaryWriter(stream);
            reader = new BinaryReader(stream);
            outputThread = new Thread(new ThreadStart(Run));
            outputThread.Start();
        }

        // Displays message from the server
        private delegate void DisplayDelegate(string message);
        private void DisplayMessage(string message)
        {
            Label label = pl_ucl.Turn;
            if (!label.Dispatcher.CheckAccess())         
                Dispatcher.Invoke(new DisplayDelegate(DisplayMessage), new object[] { message });       
            else           
                label.Content = message;          
        }

        // Update the GUI objects after opponent's turn
        private delegate void DisplayOpponentLabel(int number);
        private void UpdateOpponentLabel(int number)
        {
            Label label = pl_ucl.OpponentNumbers;
            if (!label.Dispatcher.CheckAccess())
                Dispatcher.Invoke(new DisplayOpponentLabel(UpdateOpponentLabel), new object[] { number });
            else
            {
                string num = number.ToString();
                label.Content += num + "  ";

                // Find the button with content /num/ and disable it
                Grid grid = pl_ucl.grid;
                foreach (Button btn in grid.Children.OfType<Button>())
                {
                    if ((string)btn.Content == num)
                    {
                        btn.IsEnabled = false;
                        break;
                    }
                }
            }
        }

        // Run the game
        public void Run()
        {
            // Allow only 2 players in the server at the same time
            int input = -1;
            try
            {
                input = reader.ReadInt32();
            }
            catch (IOException)
            {
                MessageBox.Show("Server is full!", "Error");
                connection.Close();
                Thread.Sleep(2000);
                Environment.Exit(0);
                return;
            }

            pl_ucl.myTurn = input == 1;
            DisplayMessage(pl_ucl.myTurn ? "Your turn" : "Waiting for opponent's turn");
            try
            {
                while (matchOn)
                {
                    ProcessMessage(reader.ReadString());
                }
            }
            catch (IOException)
            {
                MessageBox.Show("Connection lost!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ProcessMessage(string message)
        {
            if (message == "Opponent moved.")
            {
                int number = reader.ReadInt32();
                DisplayMessage("Your turn");
                Dispatcher.Invoke(new DisplayOpponentLabel(UpdateOpponentLabel), new object[] { number });
                pl_ucl.myTurn = true;
            }
            else if (message == "Waiting for another player")
            {
                DisplayMessage(message);
                pl_ucl.myTurn = false;
            }
            else if (message == "Another player has connected")
            {
                DisplayMessage("Your turn");
                pl_ucl.myTurn = true;
            }
            else // For game result
            {
                DisplayMessage(message);
                pl_ucl.matchOn = false;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            System.Environment.Exit(System.Environment.ExitCode);
        }
    }
}
