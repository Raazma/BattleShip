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
        private TcpClient _socket;
        private ShipManager _shipContainer;

       public ClientConnection(TcpClient socket)
        {
            _socket = socket;
            _shipContainer = new ShipManager();
        }

        public TcpClient getSocket()
        {
            return _socket;
        }
        public ShipManager getShipManger()
        {
            return _shipContainer;
        }



    }
}
