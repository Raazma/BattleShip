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
        public enum ShipTypes { PORTEAVIONS, CROISEUR, CONTRETORPILLEUR, SOUSMARIN, TORPILLEUR, SIZEOF_SHIPTYPES };
        public String[] ShipNames = { "Porte-avions", "Croiseur", "Contre-torpilleur", "Sous-marin", "Torpilleur" };
        public ShipTypes CurrentShipIndex;
        public Point CurrentShipPosition;
        public int[] ShipeSizes = { 5, 4, 3, 3, 2 };
        public Point[,] ShipPositions;

        public ShipManager()
        {
            ShipPositions = new Point[(int)ShipTypes.SIZEOF_SHIPTYPES, (int)ShipTypes.SIZEOF_SHIPTYPES];

            for (int c = 0; c < (int)ShipTypes.SIZEOF_SHIPTYPES; c++)
                for (int r = 0; r < (int)ShipTypes.SIZEOF_SHIPTYPES; r++)
                    ShipPositions[c, r] = new Point(-1, -1);

            CurrentShipIndex = ShipTypes.PORTEAVIONS;
            CurrentShipPosition = new Point();
        }
        public String ShipPostionToString()
        {
            String shipPositionString = "";

            for (int c = 0; c < (int)ShipTypes.SIZEOF_SHIPTYPES; c++)
                for (int r = 0; r < (int)ShipTypes.SIZEOF_SHIPTYPES; r++)
                    shipPositionString += ShipPositions[c, r].X.ToString() + "," + ShipPositions[c, r].Y.ToString() + ";";

            return shipPositionString;
        }

        public void StringToShipPosition(String shipPositionString)
        {
            int index = 0;
            ShipPositions = new Point[(int)ShipTypes.SIZEOF_SHIPTYPES, (int)ShipTypes.SIZEOF_SHIPTYPES];
            String[] positions = shipPositionString.Split(';');

            for (int c = 0; c < (int)ShipTypes.SIZEOF_SHIPTYPES; c++)
                for (int r = 0; r < (int)ShipTypes.SIZEOF_SHIPTYPES; r++)
                {
                    int col = int.Parse(positions[index].Split(',')[0]);
                    int row = int.Parse(positions[index].Split(',')[1]);
                    ShipPositions[c, r] = new Point(col, row);
                    index++;
                } 
        }
        public bool HasRemainingShip()
        {
            for (int c = 0; c < (int)ShipTypes.SIZEOF_SHIPTYPES; c++)
                for (int r = 0; r < (int)ShipTypes.SIZEOF_SHIPTYPES; r++)
                    if (ShipPositions[c, r].X != -1)
                        return true;

            return false;
        }

	public ShipTypes HasHitShip(int col, int row)
        {
            ShipTypes ship;

            for (ship = ShipTypes.PORTEAVIONS; ship < ShipTypes.SIZEOF_SHIPTYPES; ship++)
                for (int p = 0; p < (int)ShipTypes.SIZEOF_SHIPTYPES; p++)
                    if (ShipPositions[(int)ship, p].X == col && ShipPositions[(int)ship, p].Y == row)
                    {
                        ShipPositions[(int)ship, p].X = ShipPositions[(int)ship, p].Y = -1;
                        return ship;
                    }

            return ship;
        }

        public bool HasSunkenShip(ShipTypes ship)
        {
            for (int p = 0; p < (int)ShipTypes.SIZEOF_SHIPTYPES; p++)
                if (ShipPositions[(int)ship, p].X != -1)
                    return false;

            return true;
        }
    }
}