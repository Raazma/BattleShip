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
        private volatile bool IsRunning;

        public ServerConnection(FormGame client, TcpClient soc)
        {
            gameClient = client;
            socket = soc;
            socket.ReceiveTimeout = 1000;
            stream = socket.GetStream();
        }

        public void ListenToServer()
        {
            try
            {
                String serverMessage = "";
                String instruction = "";
                String param = "";
                IsRunning = true;

                while (IsRunning)
                {
                    try
                    {
                        Byte[] data = new Byte[1024];

                        Int32 bytes = stream.Read(data, 0, data.Length);
                        serverMessage = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                        instruction = serverMessage.Split(':')[0];
                        param = serverMessage.Split(':')[1];

                        switch (instruction)
                        {
                            case "START":
                                gameClient.StartShipPlacement();
                                break;
                            case "END":
                                IsRunning = false;
                                break;
                        }
                    }
                    catch (IOException ioe)
                    {
                        if (ioe.Message.Contains("closed"))
                        {
                            MessageBox.Show("Connection to server lost");
                            IsRunning = false;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }

            stream.Close();
            socket.Close();
            MessageBox.Show("End of communication");
        }

        public void StopThread()
        {
            IsRunning = false;
        }

        public void SendShipPosition(ShipManager ships)
        {
            String positions = ships.ShipPostionToString();
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(positions);
            stream.Write(data, 0, data.Length);
        }
    }
}
