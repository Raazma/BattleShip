using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BattleShipServer
{
    class ShipManager
    {
        public enum ShipTypes { PORTEAVIONS, CROISEUR, CONTRETORPILLEUR, SOUSMARIN, TORPILLEUR, SIZEOF_SHIPNAMES };
        public String[] ShipNames = { "Porte-avions", "Croiseur", "Contre-torpilleur", "Sous-marin", "Torpilleur" };
        public ShipTypes CurrentShipIndex;
        public Point CurrentShipPosition;
        public int[] ShipeSizes = { 5, 4, 3, 3, 2 };
        public Point[,] ShipPositions;

        public ShipManager()
        {
            ShipPositions = new Point[(int)ShipTypes.SIZEOF_SHIPNAMES, (int)ShipTypes.SIZEOF_SHIPNAMES];

            for (int c = 0; c < (int)ShipTypes.SIZEOF_SHIPNAMES; c++)
                for (int r = 0; r < (int)ShipTypes.SIZEOF_SHIPNAMES; r++)
                    ShipPositions[c, r] = new Point(-1, -1);

            CurrentShipIndex = ShipTypes.PORTEAVIONS;
            CurrentShipPosition = new Point();
        }
    }
}