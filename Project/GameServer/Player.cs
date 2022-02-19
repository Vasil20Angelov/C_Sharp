using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GameServer
{
    class Player
    {
        #region Fields
        private Game game;
        private MainWindow server;
        private Socket connection;
        private NetworkStream socketStream;
        private BinaryWriter writer;
        private BinaryReader reader;
        private bool firstPlayer;
        public bool threadSuspended = true;
        #endregion

        public Player(Socket socket_connection, MainWindow server_ref, Game game_ref, bool first)
        {
            connection = socket_connection;
            server = server_ref;
            firstPlayer = first;
            game = game_ref;

            // Create NetworkStream object for Socket 
            socketStream = new NetworkStream(connection);

            // Create Streams for reading/writing bytes 
            writer = new BinaryWriter(socketStream);
            reader = new BinaryReader(socketStream);
        }

        public void Run()
        {
            int playerNum = firstPlayer ? 1 : 2;
            server.DisplayMessage("Player " + playerNum + " has connected to the server successfully!\n");

            writer.Write(playerNum);

            int reply;

            if (firstPlayer)
            {
                writer.Write("Waiting for another player");

                // Locked until another player connects
                lock (this)
                {
                    while (threadSuspended)
                        Monitor.Wait(this);
                }
                writer.Write("Another player has connected");
            }

            // Read data sent from the clients
            do
            {
                try
                {
                    // Read the number sent to the server (selected from the other player)
                    reply = reader.ReadInt32();

                    game.SelectNumber(reply);
                    server.NotifyPlayerMoved(firstPlayer, reply);

                    // display the message
                    server.DisplayMessage("\r\n" + "Player " + playerNum + " selected " + reply);
                }
                catch (Exception)
                {
                    break;
                }

            } while (reply != -1 && connection.Connected);

            server.DisplayMessage("\r\n\nUser terminated connection\r");

            writer?.Close();
            reader?.Close();
            socketStream?.Close();
            connection?.Close();
        }

        internal void OponentMoved(int selectedNum)
        {
            writer.Write("Opponent moved.");
            writer.Write(selectedNum);
        }

        internal void DeclareWinner(List<int> combination, bool draw, bool winner)
        {
            if (draw)
                writer.Write("It's a draw!\n");
            else if (winner)
                writer.Write($"You win with combination: {string.Join(", ", combination)}!\n");
            else
                writer.Write($"Opponent wins with combination: {string.Join(", ", combination)}!\n");
        }
    }
}
