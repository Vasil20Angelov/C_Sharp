using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

namespace GameServer
{
    public partial class MainWindow : Window
    {
        #region Fields
        private Player player1;
        private Player player2;
        private Thread readThread;
        private Thread[] playerThreads;
        private Game game; 
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            playerThreads = new Thread[2];
            game = new Game();

            // Start the server
            readThread = new Thread(new ThreadStart(RunServer));
            readThread.Start();
        }

        // Close all threads associated with this application
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            System.Environment.Exit(System.Environment.ExitCode);
        }

        // Hosts the server
        public void RunServer()
        {
            
            TcpListener listener;

            // wait for a Players to connect
            try
            {
                IPAddress local = IPAddress.Parse("127.0.0.1");
                listener = new TcpListener(local, 50000);

                // TcpListener waits for connection requests
                listener.Start();

                // Accept the first player
                player1 = new Player(listener.AcceptSocket(), this, game, true);
                playerThreads[0] = new Thread(new ThreadStart(player1.Run));
                playerThreads[0].Start();

                // Accept the second player
                player2 = new Player(listener.AcceptSocket(), this, game, false);
                playerThreads[1] = new Thread(new ThreadStart(player2.Run));
                playerThreads[1].Start();

                // Lock the player until another player connects
                lock (player1)
                {
                    player1.threadSuspended = false;
                    Monitor.Pulse(player1);
                }

            }
            catch (Exception error)
            {
                MessageBox.Show(error.ToString());
            }
        }

        // Notify that the player moved and check if the match has ended
        internal void NotifyPlayerMoved(bool firstPlayer, int selectedNum)
        {
            // If the player has a winning combination -> get it
            List<int> winningNumbers = game.CheckCombination();

            // Notify the other player that a number has been selected by his opponent
            if (firstPlayer)
                player2.OponentMoved(selectedNum);
            else
                player1.OponentMoved(selectedNum);

            bool draw = false;
            if (winningNumbers == null)
            {
                if (game.SelectedNumbers == 9) // If all numbers are selected -> it's a draw
                    draw = true;
                else
                    return;
            }

            // Declare the winner
            player1.DeclareWinner(winningNumbers, draw, firstPlayer);
            player2.DeclareWinner(winningNumbers, draw, !firstPlayer);
        }

        // Displays messages from the clients
        public void DisplayMessage(string message)
        {
            // Execute in thread safe way
            if (!TBlock.Dispatcher.CheckAccess())
                TBlock.Dispatcher.Invoke(new Action(() => TBlock.Text += message));           
            else
                TBlock.Text += message;
        }
    }

    public class Game
    {
        #region Fields
        private List<int> player1_numbers;
        private List<int> player2_numbers;
        private int selectedNumbers;
        private bool firstPlayer; 
        #endregion

        public bool FirstPlayer { get { return firstPlayer; } }

        public int SelectedNumbers { get { return selectedNumbers; } }

        public Game()
        {
            firstPlayer = true;
            selectedNumbers = 0;
            player1_numbers = new List<int>();
            player2_numbers = new List<int>();
        }

        // Selects a number for the current player
        public void SelectNumber(int number)
        {
            ++selectedNumbers;

            if (FirstPlayer)
                player1_numbers.Add(number);
            else
                player2_numbers.Add(number);

            firstPlayer = !firstPlayer;
        }

        // The winning combination of the last player that played
        public List<int> CheckCombination()
        {
            List<int> winningNumbers = new List<int>();
            List<int> current = FirstPlayer ? player2_numbers : player1_numbers;

            for (int i = 0; i < current.Count - 2; ++i)
            {
                for (int j = i + 1; j < current.Count - 1; ++j)
                {
                    int sum = current[i] + current[j];
                    if (sum > 15)
                        continue;

                    for (int k = j + 1; k < current.Count; ++k)
                    {
                        if (sum + current[k] == 15)
                        {
                            winningNumbers.Add(current[i]);
                            winningNumbers.Add(current[j]);
                            winningNumbers.Add(current[k]);
                            return winningNumbers;
                        }
                    }
                }
            }

            return null;
        }
    }
}