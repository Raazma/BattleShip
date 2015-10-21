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

        private int _port;
        private int currentPlayer;
        private IPAddress _ip;
        private TcpListener serverSocket;
        private List<ClientConnection> _clientList;
        private TcpClient client;
        private const int NUMBER_OF_PLAYER_REQUIRED = 2;
        private bool endOfGame = false;
        Byte[] buffer = new Byte[100000];
        Int32 bytes;
        String move;
        NetworkStream clientStream;
        public ServeurBattleShip(int port, String adresseIp)
        {
            try
            {
                _clientList = new List<ClientConnection>();
                _port = port;
                _ip = IPAddress.Parse(adresseIp);
                serverSocket = new TcpListener(_ip, port);

            }
            catch (Exception e)
            {
                Console.WriteLine("l'adresse " + adresseIp + " est invalide");
            }

        }

        public void StartServer()
        {

            try
            {
                serverSocket.Start();

                do
                {
                    _clientList.Add(new ClientConnection(serverSocket.AcceptTcpClient()));
                    Console.WriteLine("client connected");

                } while (_clientList.Count < NUMBER_OF_PLAYER_REQUIRED);

                EnvoieDuMessageDeDebut();
                WaitForShip();

                currentPlayer = 0;
                while (!endOfGame)
                {
                    sendMessageToClient(currentPlayer, "YOUR_TURN: turn");
                    ReadShot();
                    currentPlayer = (currentPlayer + 1) % NUMBER_OF_PLAYER_REQUIRED;
                    Console.WriteLine(currentPlayer);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
               // endOfGame = true;
            }

            //la partie est fini 
            CloseCommunication();

        }

        private void ReadShot()
        {

            clientStream = _clientList[currentPlayer].getSocket().GetStream();
            bytes = clientStream.Read(buffer, 0, buffer.Length);
            move = System.Text.Encoding.ASCII.GetString(buffer, 0, bytes);
            Console.WriteLine(move.Split(',')[0] + "  " + move.Split(',')[1]);
            HandleShot((int.Parse(move.Split(',')[0])), int.Parse(move.Split(',')[1]));

        }
        private void WaitForShip()
        {

            int joueurAyantPlacerLeurBateau = 0;
            bool bateauPlacer = false;
            String position;

            while (!bateauPlacer)
            {
                for (int i = 0; i < _clientList.Count; i++)
                {
                    clientStream = _clientList[i].getSocket().GetStream();
                    bytes = clientStream.Read(buffer, 0, buffer.Length);
                    position = System.Text.Encoding.ASCII.GetString(buffer, 0, bytes);

                    if (!String.IsNullOrEmpty(position))
                    {
                        joueurAyantPlacerLeurBateau++;
                        _clientList[i].getShipManger().StringToShipPosition(position);
                        Point[,] foo = _clientList[i].getShipManger().ShipPositions;
                    }
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

            for (int i = 0; i < _clientList.Count; i++)
            {
                _clientList[i].getSocket().Close();
            }
            serverSocket.Stop();
        }
        private void HandleShot(int col, int row)
        {
            // ShipManager from OtherPlayer
            int otherPlayer = (currentPlayer + 1 )% NUMBER_OF_PLAYER_REQUIRED;
            ShipManager otherPlayerShip = _clientList[otherPlayer].getShipManger();
            ShipManager.ShipTypes shipHit = otherPlayerShip.HasHitShip(col, row);

            if (shipHit != ShipManager.ShipTypes.SIZEOF_SHIPTYPES)
            {

                if (otherPlayerShip.HasSunkenShip(shipHit))
                {
                    /* SERVER
                     * CurrentPlayer ENEMY_SUNK:ShipName;col,row
                     * OtherPlayer ALLY_SUNK:ShipName;col,row
                     * */
                    sendMessageToClient(currentPlayer, "ENEMY_SUNK:" + otherPlayerShip.ShipNames[(int)shipHit] + ";" + col.ToString() + "," + row.ToString());
                    sendMessageToClient(otherPlayer, "ALLY_SUNK:" + otherPlayerShip.ShipNames[(int)shipHit] + ";" + col.ToString() + "," + row.ToString());
                }
                else
                {
                    /* SERVER
                     * CurrentPlayer ENEMY_HIT:col,row
                     * OtherPlayer ALLY_HIT:col,row
                     * */
                    sendMessageToClient(currentPlayer, "ENEMY_HIT:" + col.ToString() + "," + row.ToString());
                    sendMessageToClient(otherPlayer, "ALLY_HIT:" + col.ToString() + "," + row.ToString());

                }
            }
            else
            {
                /* SERVER
                 * CurrentPlayer ENEMY_MISS:col,row
                 * OtherPlayer ALLY_MISS:col,row
                 * */

                sendMessageToClient(currentPlayer, "ENEMY_MISS:" + col.ToString() + "," + row.ToString());
                sendMessageToClient(otherPlayer, "ALLY_MISS:" + col.ToString() + "," + row.ToString());
            }
        }
        private void sendMessageToClient(int index, String message)
        {
            Byte[] messageByte;
            clientStream = _clientList[index].getSocket().GetStream();
            messageByte = System.Text.Encoding.ASCII.GetBytes(message);
            clientStream.Write(messageByte, 0, messageByte.Length);

        }



    }
}
