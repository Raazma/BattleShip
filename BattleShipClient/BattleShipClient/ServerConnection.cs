using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BattleShipClient
{
    public class ServerConnection
    {
        FormGame gameClient;
        TcpClient socket;
        NetworkStream stream;
        String serverMessage;
        private volatile bool IsRunning;

        public ServerConnection(FormGame client, TcpClient soc)
        {
            // Interface de jeux
            gameClient = client;
            // Socket vers le serveur
            socket = soc;
            // Délai d'inactivité
            socket.ReceiveTimeout = 1000;
            // Ouverture du flux
            stream = socket.GetStream();
        }

        public void ListenToServer()
        {
            try
            {
                serverMessage = "";
                String instruction = "";
                String param = "";
                IsRunning = true;

                String ship = "";
                int col, row;
                while (IsRunning)
                {
                    try
                    {
                        // Lecture d'une instruction du serveur
                        Byte[] data = new Byte[1024];
                        Int32 bytes = stream.Read(data, 0, data.Length);
                        serverMessage = System.Text.Encoding.ASCII.GetString(data, 0, bytes);


                        if (!String.IsNullOrEmpty(serverMessage))
                        {
                            // Séparation de l'instruction et du paramètre
                            instruction = serverMessage.Split(':')[0];
                            param = serverMessage.Split(':')[1];

                            // Traitement de l'instruction
                            switch (instruction)
                            {
                                case "START": // Début de la partie
                                    gameClient.StartShipPlacement();
                                    break;
                                case "YOUR_TURN": // Début du tour du joueur
                                    gameClient.StartTurn();
                                    break;
                                case "ENEMY_SUNK": // Le joueur a coulé un bateau ennemi
                                    ship = param.Split(';')[0];
                                    col = int.Parse(param.Split(';')[1].Split(',')[0]);
                                    row = int.Parse(param.Split(';')[1].Split(',')[1]);
                                    gameClient.EnemySunk(ship, col, row);
                                    break;
                                case "ALLY_SUNK": // Un bateau du joueur a été coulé
                                    ship = param.Split(';')[0];
                                    col = int.Parse(param.Split(';')[1].Split(',')[0]);
                                    row = int.Parse(param.Split(';')[1].Split(',')[1]);
                                    gameClient.AllySunk(ship, col, row);
                                    break;
                                case "ENEMY_HIT": // Le joueur a touché un bateau ennemi
                                    col = int.Parse(param.Split(',')[0]);
                                    row = int.Parse(param.Split(',')[1]);
                                    gameClient.EnemyHit(col, row);
                                    break;
                                case "ALLY_HIT": // Un bateau du joueur a été touché
                                    col = int.Parse(param.Split(',')[0]);
                                    row = int.Parse(param.Split(',')[1]);
                                    gameClient.AllyHit(col, row);
                                    break;
                                case "ENEMY_MISS": // Le joueur a raté un bateau ennemi
                                    col = int.Parse(param.Split(',')[0]);
                                    row = int.Parse(param.Split(',')[1]);
                                    gameClient.EnemyMiss(col, row);
                                    break;
                                case "ALLY_MISS": // Un bateau du joueur a été raté
                                    col = int.Parse(param.Split(',')[0]);
                                    row = int.Parse(param.Split(',')[1]);
                                    gameClient.AllyMiss(col, row);
                                    break;
                                case "LOST": // Le joueur a perdu la partie
                                    gameClient.GameLost();
                                    IsRunning = false;
                                    break;
                                case "WON": // Le joueur a gagné la partie
                                    gameClient.GameWon();
                                    IsRunning = false;
                                    break;
                                case "PLAYER_DISCONNECTED": // L'ennemi s'est déconnecté
                                    gameClient.EnemyDisconnected();
                                    IsRunning = false;
                                    break;
                                case "END": // Fin de la partie
                                    IsRunning = false;
                                    break;
                            }
                        }
                    }
                    catch (IOException ioe)
                    {
                        // On vérifie si il s'agit d'une fermeture du serveur
                        if (ioe.Message.Contains("fermée") || ioe.Message.Contains("closed"))
                        {
                            MessageBox.Show("Connexion au serveur perdue");
                            IsRunning = false;
                        }

                        // Sinon, il s'agit d'un timeout, on recommence la boucle
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                MessageBox.Show("Server Message : " + serverMessage);
            }
            finally
            {
                stream.Close();
                socket.Close();
            }
        }

        // Vérifie si le thread est en vie
        public bool IsAlive()
        {
            return IsRunning;
        }

        // Met fin au thread et envoie un message de déconnexion
        public void StopThread()
        {
            IsRunning = false;
            SendDisconnectNotice();
        }

        // Envoie la liste des bateaux au serveur
        public void SendShipPosition(ShipManager ships)
        {
            String positions = ships.ShipPostionToString();
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(positions);
            stream.Write(data, 0, data.Length);
        }

        // Envoie un tir au serveur
        public void SendShot(int col, int row)
        {
            String shot = col.ToString() + "," + row.ToString();
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(shot);
            stream.Write(data, 0, data.Length);
        }

        // Envoie un message de déconnexion
        private void SendDisconnectNotice()
        {
            String notice = "DISCONNECT";
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(notice);
            stream.Write(data, 0, data.Length);
        }
    }
}
