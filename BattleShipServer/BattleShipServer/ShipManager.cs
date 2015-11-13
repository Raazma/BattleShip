using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleShipServer
{
    public class ShipManager
    {
        public enum ShipTypes { PORTEAVIONS, CROISEUR, CONTRETORPILLEUR, SOUSMARIN, TORPILLEUR, SIZEOF_SHIPTYPES };
        public String[] ShipNames = { "Porte-avions", "Croiseur", "Contre-torpilleur", "Sous-marin", "Torpilleur" };
        public ShipTypes CurrentShipIndex;
        public Point CurrentShipPosition;
        public int[] ShipeSizes = { 5, 4, 3, 3, 2 };
        public Point[,] ShipPositions;

        public ShipManager()
        {
            // Initialisation du tableau de position
            ShipPositions = new Point[(int)ShipTypes.SIZEOF_SHIPTYPES, (int)ShipTypes.SIZEOF_SHIPTYPES];
            for (int c = 0; c < (int)ShipTypes.SIZEOF_SHIPTYPES; c++)
                for (int r = 0; r < (int)ShipTypes.SIZEOF_SHIPTYPES; r++)
                    ShipPositions[c, r] = new Point(-1, -1);

            // Le premier bateau à placer est le porte-avions
            CurrentShipIndex = ShipTypes.PORTEAVIONS;
            CurrentShipPosition = new Point();
        }


        // Crée une chaîne de caractères représentant les positions des bateaux
        public String ShipPostionToString()
        {
            String shipPositionString = "";

            for (int c = 0; c < (int)ShipTypes.SIZEOF_SHIPTYPES; c++)
                for (int r = 0; r < (int)ShipTypes.SIZEOF_SHIPTYPES; r++)
                    shipPositionString += ShipPositions[c, r].X.ToString() + "," + ShipPositions[c, r].Y.ToString() + ";";

            return shipPositionString;
        }

        // Initialise le tableau de position des bateaux à partir d'une chaîne de caractères
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

        // Vérifie s'il reste des bateaux
        public bool HasRemainingShip()
        {
            for (int c = 0; c < (int)ShipTypes.SIZEOF_SHIPTYPES; c++)
                for (int r = 0; r < (int)ShipTypes.SIZEOF_SHIPTYPES; r++)
                    if (ShipPositions[c, r].X != -1) 
                        return true;

            return false;
        }

        // Vérifie si un bateau a été touché. Si c'est le cas, on détruit cette position
        public ShipTypes HasHitShip(int col, int row)
        {
            ShipTypes ship;

            for (ship = ShipTypes.PORTEAVIONS; ship < ShipTypes.SIZEOF_SHIPTYPES; ship++)
                for (int p = 0; p < (int)ShipTypes.SIZEOF_SHIPTYPES; p++)
                    if (ShipPositions[(int)ship, p].X == col && ShipPositions[(int)ship, p].Y == row)
                    {
                        ShipPositions[(int)ship, p].X = ShipPositions[(int)ship, p].Y = -1; // Une position de (-1,-1) indique une partie de bateau détruite
                        return ship;
                    }

            return ship;
        }

        // Vérifie si le bateau est coulé
        public bool HasSunkenShip(ShipTypes ship)
        {
            for (int p = 0; p < (int)ShipTypes.SIZEOF_SHIPTYPES; p++)
                if (ShipPositions[(int)ship, p].X != -1)
                    return false;

            return true;
        }
    }
}