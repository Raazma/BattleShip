using System;
using System.Collections.Generic;
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
        private IPAddress _ip;
        private TcpListener serverSocket;
        private List<TcpClient> _clientList;
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
                _clientList = new List<TcpClient>();
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

            while (!endOfGame)
            {
                try
                {
                    serverSocket.Start();

                    do
                    {
                        _clientList.Add(serverSocket.AcceptTcpClient());
                        Console.WriteLine("client connected");

                    } while (_clientList.Count < NUMBER_OF_PLAYER_REQUIRED);

                    EnvoieDuMessageDeDebut();
                    WaitForShip();



                }
                catch (Exception e)
                {
                    endOfGame = true;
                }
            }
            //la partie est fini 
            CloseCommunication();

        }

        private void ReadMove()
        {
            //Byte[] buffer = new Byte[100000];
            //Int32 bytes;
            //String move;
            //NetworkStream clientStream;

            //for (int i = 0; i < _clientList.Count; i++)
            //{
            //    clientStream = _clientList[i].GetStream();            
            //    bytes = clientStream.Read(buffer, 0, buffer.Length);
            //    move = System.Text.Encoding.ASCII.GetString(buffer, 0, bytes);
            //}


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
                    clientStream = _clientList[i].GetStream();
                    bytes = clientStream.Read(buffer, 0, buffer.Length);
                    position = System.Text.Encoding.ASCII.GetString(buffer, 0, bytes);

                    if (!String.IsNullOrEmpty(position))
                    {
                        joueurAyantPlacerLeurBateau++;
                        Console.WriteLine(joueurAyantPlacerLeurBateau);
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
                clientStream = _clientList[i].GetStream();
                messages = System.Text.Encoding.ASCII.GetBytes("START: Debut de la partie");
                clientStream.Write(messages, 0, messages.Length);
            }
        }
        private void CloseCommunication()
        {

            for (int i = 0; i < _clientList.Count; i++)
            {
                _clientList[i].Close();
            }
            serverSocket.Stop();
        }




    }
}
