using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleShipServer
{
    class Program
    {
        static void Main(string[] args)
        {
            ServeurBattleShip server = new ServeurBattleShip(1234, "0");
            server.StartServer();

        }
    }
}
