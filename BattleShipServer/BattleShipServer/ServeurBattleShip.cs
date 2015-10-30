using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BattleShipServer
{
    class ServeurBattleShip
    {
        //le port
        private int _port;
        //le joueur qui joue actuellement
        private int currentPlayer;
        //l'adresse de connection
        private IPAddress _ip;
        //le socket serveur
        private TcpListener serverSocket;
        //la liste des clients avec leur connection
        private List<ClientConnection> _clientList;
        //constante le nombre de joueur demander pour une partie
        private const int NUMBER_OF_PLAYER_REQUIRED = 2;
        //bool qui verifie la fin de partie
        private bool endOfGame = false;
        //le buffer pour lire dans le socket
        Byte[] buffer = new Byte[100000];
        //les bytes lu dans le socket
        Int32 bytes;
        //ce que le client a effectuer comme coup
        String move;
        //la liaison avec le client
        NetworkStream clientStream;
        public ServeurBattleShip(int port, String adresseIp)
        {
            //construit une liste de client initialise le tcpListener au port et l'adresse Ip voulu
            try
            {
                _clientList = new List<ClientConnection>();
                _port = port;
                _ip = IPAddress.Parse(adresseIp);
                serverSocket = new TcpListener(_ip, port);

            }
            catch (Exception e)
            {
                //la connection a été impossible
                Console.WriteLine("l'adresse " + adresseIp + " est invalide");
            }

        }

        public void StartServer()
        {

            try
            {
                //initialise le serveur
                serverSocket.Start();

                do
                {
                    //attend une connection de client recommence l'attente temps qu'on a pas le nombre de joueur voulu
                    _clientList.Add(new ClientConnection(serverSocket.AcceptTcpClient()));
                    Console.WriteLine("client connected");

                } while (_clientList.Count < NUMBER_OF_PLAYER_REQUIRED);

                //envoie un message a tout les joueurs que la partie peut commencer
                EnvoieDuMessageDeDebut();
                //attend que les joueurs place leurs bateaux
                WaitForShip();
                //le joueur qui doit jouer son tour on commence avec le joueur 1
                currentPlayer = 0;

                //boucle de jeu
                while (!endOfGame)
                {
                    //envoie du message qui dit au joueur que c'est a son tour de jouer
                    sendMessageToClient(currentPlayer, "YOUR_TURN: turn");
                    //lecture du coup du joueur
                    ReadShot();
                    //on passe au joueur suivant
                    currentPlayer = (currentPlayer + 1) % NUMBER_OF_PLAYER_REQUIRED;

                    //verifie si il reste des bateaux en vie si il en reste pu fin de partie
                    if (!_clientList[currentPlayer].getShipManger().HasRemainingShip())
                        endOfGame = true;

                }
                //envoie des messages au joueur perdu ou gagné
                sendMessageToClient(currentPlayer, "LOST: perdu");
                sendMessageToClient((currentPlayer + 1) % NUMBER_OF_PLAYER_REQUIRED, "WON: gagner");

            }
            catch (Exception e)
            {
                //il ya eu une perte de connection lors de la partie
                switch (e.Message.Split(':')[0])
                {
                    case "PLAYER_DISCONNECTED":
                        int player = (int.Parse(e.Message.Split(':')[1]) + 1) % NUMBER_OF_PLAYER_REQUIRED;
                        sendMessageToClient(player, "PLAYER_DISCONNECTED: le joueur est deconnecté");
                        break;
                    default:
                        Console.WriteLine(e.Message);
                        break;
                }
                //on fini la partie pu de connection
                endOfGame = true;
            }

            //on ferme toutes les connections avec les clients
            CloseCommunication();

        }

        private void ReadShot()
        {
            //lit le coup du current player et envoie le coup a la fonction Handleshot
            clientStream = _clientList[currentPlayer].getSocket().GetStream();
            bytes = clientStream.Read(buffer, 0, buffer.Length);
            move = System.Text.Encoding.ASCII.GetString(buffer, 0, bytes);
            Console.WriteLine(move.Split(',')[0] + "  " + move.Split(',')[1]);
            HandleShot((int.Parse(move.Split(',')[0])), int.Parse(move.Split(',')[1]));

        }
        private void WaitForShip()
        {
            //attend que tout les joueurs est placée leurs bateaux et les place dans leur shipManager

            int joueurAyantPlacerLeurBateau = 0;
            bool bateauPlacer = false;
            String position;

            while (!bateauPlacer)
            {
                for (int i = 0; i < _clientList.Count; i++)
                {

                    clientStream = _clientList[i].getSocket().GetStream();//prend le stream
                    try
                    {
                        bytes = clientStream.Read(buffer, 0, buffer.Length);//lecture des positions
                        if (bytes == 0)
                            throw new Exception("PLAYER_DISCONNECTED:" + i.ToString());
                    }
                    catch (Exception e)
                    {
                        throw new Exception("PLAYER_DISCONNECTED:" + i.ToString());
                    }



                    position = System.Text.Encoding.ASCII.GetString(buffer, 0, bytes);//transforme en String

                    //le joueur a placer c'est bateau on les mets dans on ShipManager
                    if (!String.IsNullOrEmpty(position))
                    {
                        joueurAyantPlacerLeurBateau++;
                        _clientList[i].getShipManger().StringToShipPosition(position);
                        Point[,] foo = _clientList[i].getShipManger().ShipPositions;
                    }
                    //tous les Joueurs ont placé leurs bateaux la partie peu commencer
                    if (joueurAyantPlacerLeurBateau == _clientList.Count)
                    {
                        bateauPlacer = true;
                        Console.WriteLine("tout les joueur on placer leur bateau");

                    }

                }
            }


        }
        private void EnvoieDuMessageDeDebut()
        {
            //envoie le message de debut de partie a tout les joueur dans la liste
            Byte[] messages;
            for (int i = 0; i < _clientList.Count; i++)
            {
                clientStream = _clientList[i].getSocket().GetStream();
                messages = System.Text.Encoding.ASCII.GetBytes("START: Debut de la partie");
                clientStream.Write(messages, 0, messages.Length);
            }
        }
        private void CloseCommunication()
        {
            //ferme toute les connections dans la liste
            try
            {
                for (int i = 0; i < _clientList.Count; i++)
                {
                    _clientList[i].getSocket().Close();
                }
                serverSocket.Stop();
            }
            catch (Exception e)
            {
               
            }
        }
        private void HandleShot(int col, int row)
        {
            // ShipManager de l'autre joueur
            int otherPlayer = (currentPlayer + 1) % NUMBER_OF_PLAYER_REQUIRED;
            ShipManager otherPlayerShip = _clientList[otherPlayer].getShipManger();
            ShipManager.ShipTypes shipHit = otherPlayerShip.HasHitShip(col, row);

            if (shipHit != ShipManager.ShipTypes.SIZEOF_SHIPTYPES)
            {

                if (otherPlayerShip.HasSunkenShip(shipHit))
                {
                    //le joueur actuelle a couler un bateau enemy on envoie les messages approprier avec la position et le type de bateau a tous les joueurs
                    sendMessageToClient(currentPlayer, "ENEMY_SUNK:" + otherPlayerShip.ShipNames[(int)shipHit] + ";" + col.ToString() + "," + row.ToString());
                    sendMessageToClient(otherPlayer, "ALLY_SUNK:" + otherPlayerShip.ShipNames[(int)shipHit] + ";" + col.ToString() + "," + row.ToString());
                }
                else
                {
                    //le coup a toucher mais pas de bateau couler  on envoie les message appropier au joueurs avec la position
                    sendMessageToClient(currentPlayer, "ENEMY_HIT:" + col.ToString() + "," + row.ToString());
                    sendMessageToClient(otherPlayer, "ALLY_HIT:" + col.ToString() + "," + row.ToString());

                }
            }
            else
            {               
                //le coup a rater on envoie les messages appropier a tout les joueurs avec la positions
                sendMessageToClient(currentPlayer, "ENEMY_MISS:" + col.ToString() + "," + row.ToString());
                sendMessageToClient(otherPlayer, "ALLY_MISS:" + col.ToString() + "," + row.ToString());
            }
        }
        private void sendMessageToClient(int index, String message)
        {
            //envoie un message a un joueur a l'index voulue 
            try
            {
                Byte[] messageByte;
                clientStream = _clientList[index].getSocket().GetStream();
                messageByte = System.Text.Encoding.ASCII.GetBytes(message);
                clientStream.Write(messageByte, 0, messageByte.Length);
            }
            catch (Exception e)
            {
                throw new Exception("PLAYER_DISCONNECTED:" + index.ToString());
            }

        }



    }
}
