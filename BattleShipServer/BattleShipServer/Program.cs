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
            //instancie un serveur au port et a l'adresse choisi
            ServeurBattleShip server = new ServeurBattleShip(1234, "0");
            //part le serveur
            server.StartServer();

        }
    }
}
