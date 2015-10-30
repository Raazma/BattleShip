using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BattleShipServer
{

    class ClientConnection
    {
        //le socket du client
        private TcpClient _socket;
        //le manager de bateau du client
        private ShipManager _shipContainer;

        public ClientConnection(TcpClient socket)
        {
            //inialise le socket et le shipmanager du client
            _socket = socket;
            _shipContainer = new ShipManager();
        }

        public TcpClient getSocket()
        {
            //retourne le socket du client
            return _socket;
        }
        public ShipManager getShipManger()
        {
            //retourne le shipManager du client
            return _shipContainer;
        }



    }
}
