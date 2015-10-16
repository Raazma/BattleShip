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
            Int32 port = int.Parse(TB_Port.Text);
            TcpClient socket = new TcpClient(TB_AdresseIP.Text, port);
            FormGame game = new FormGame();
            ServerConnection conn = new ServerConnection(game, socket);
            game.SetConnection(conn);

            Thread serverThread = new Thread(conn.ListenToServer);
            serverThread.Start();
            while (!serverThread.IsAlive) ;
            Thread.Sleep(1);

            game.ShowDialog();
        }
    }
}
