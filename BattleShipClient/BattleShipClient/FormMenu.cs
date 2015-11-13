using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BattleShipClient
{
    public partial class FormMenu : Form
    {
        public FormMenu()
        {
            InitializeComponent();
        }

        private void BTN_Start_Click(object sender, EventArgs e)
        {
            try
            {
                // Ouverture d'un socket à l'adresse et port spécifiés
                Int32 port = int.Parse(TB_Port.Text);
                TcpClient socket = new TcpClient();
                var result = socket.BeginConnect(TB_AdresseIP.Text, port, null, null);
                // Tentative de connexion pendant 1 seconde
                var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1));
                // Le serveur n'a pas répondu
                if (!success)
                    throw new Exception("Il n'y a pas de serveur disponible");

                // On a trouvé un serveur, on démarre la partie
                FormGame game = new FormGame();
                ServerConnection conn = new ServerConnection(game, socket);
                game.SetConnection(conn);
                // On démarre le thred d'écoute du serveur
                Thread serverThread = new Thread(conn.ListenToServer);
                serverThread.Start();
                while (!serverThread.IsAlive);
                Thread.Sleep(1);

                // Affichage du jeu
                game.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
