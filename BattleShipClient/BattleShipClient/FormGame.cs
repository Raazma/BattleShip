using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BattleShipClient
{
    public partial class FormGame : Form
    {
        private bool isPlacingShip = false;
        private ShipManager shipManager = new ShipManager();

        public FormGame()
        {
            InitializeComponent();
        }

        private void FormGame_Load(object sender, EventArgs e)
        {
            buildDGV(DGV_AllyFleet);
            buildDGV(DGV_EnemyFleet);
            UpdateStatusLabel_ShipPlacement();
        }

        private void buildDGV(DataGridView DGV)
        {
            DGV.ColumnHeadersHeight = 20;
            DGV.RowHeadersWidth = 50;

            for (int i = 0; i < 10; i++)
            {
                DGV.Columns.Add("Col" + (i + 1).ToString(), (i + 1).ToString());
                DGV.Columns[i].Width = 30;
            }

            char rowHeader = 'A';

            for (int i = 0; i < 10; i++)
            {
                DGV.Rows.Add();
                DGV.Rows[i].HeaderCell.Value = rowHeader.ToString();
                DGV.Rows[i].Height = 30;
                rowHeader++;
            }
        }

        private void DGV_AllyFleet_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int row = e.RowIndex;
            int col = e.ColumnIndex;

            if (row != -1 && col != -1)
            {
                if (isPlacingShip)
                {
                    if (DGV_AllyFleet[col, row].Style.BackColor == Color.Green)
                    {
                        placeShipOnGrid(col, row);
                        resetShipPlacement();
                        isPlacingShip = false;
                        shipManager.CurrentShipIndex++;

                        if (shipManager.CurrentShipIndex == ShipManager.ShipTypes.SIZEOF_SHIPNAMES)
                        {
                            LBL_Status.Text = "En attente de l'autre joueur";
                        }
                        else
                        {
                            UpdateStatusLabel_ShipPlacement();
                        }
                        
                    }
                    else if (DGV_AllyFleet[col, row].Style.BackColor == Color.Blue)
                    {
                        resetShipPlacement();
                        isPlacingShip = false;
                    }
                }
                else if (DGV_AllyFleet[col, row].Style.BackColor != Color.Black)
                {
                    shipManager.CurrentShipPosition.X = col;
                    shipManager.CurrentShipPosition.Y = row;
                    DGV_AllyFleet[col, row].Style.BackColor = Color.Blue;
                    placeShipMarkers(col, row, shipManager.ShipeSizes[(int)shipManager.CurrentShipIndex] - 1);
                    isPlacingShip = true;
                }
            }

            DGV_AllyFleet.ClearSelection();
        }

        private void placeShipMarkers(int col, int row, int shipSize)
        {
            for (int c = -1; c <= 1; c++)
            {
                for (int r = -1; r <= 1; r++)
                {
                    if ((c == 0) != (r == 0))
                    {
                        if (row + (r * shipSize) >= 0 && row + (r * shipSize) < DGV_AllyFleet.Rows.Count &&
                            col + (c * shipSize) >= 0 && col + (c * shipSize) < DGV_AllyFleet.Columns.Count &&
                            !shipIsInTheWay(col + (c * shipSize), row + (r * shipSize)))
                        {
                            DGV_AllyFleet[col + (c * shipSize), row + (r * shipSize)].Style.BackColor = Color.Green;
                        }
                    }
                }
            }
        }

        private void placeShipOnGrid(int col, int row)
        {
            int colDiff = col - shipManager.CurrentShipPosition.X;
            int rowDiff = row - shipManager.CurrentShipPosition.Y;
            int offset;
            int shipPosX;
            int shipPosY;

            for (int i = 0; i < shipManager.ShipeSizes[(int)shipManager.CurrentShipIndex]; i++)
            {
                if (colDiff != 0)
                {
                    offset = colDiff > 0 ? i : -i;
                    shipPosX = shipManager.CurrentShipPosition.X + offset;
                    shipPosY = shipManager.CurrentShipPosition.Y;
                }
                else
                {
                    offset = rowDiff > 0 ? i : -i;
                    shipPosX = shipManager.CurrentShipPosition.X;
                    shipPosY = shipManager.CurrentShipPosition.Y + offset;
                }

                shipManager.ShipPositions[(int)shipManager.CurrentShipIndex, i] = new Point(shipPosX, shipPosY);
                DGV_AllyFleet[shipPosX, shipPosY].Style.BackColor = Color.Black;
            }
        }

        private bool shipIsInTheWay(int col, int row)
        {
            int colDiff = col - shipManager.CurrentShipPosition.X;
            int rowDiff = row - shipManager.CurrentShipPosition.Y;

            for (int i = 0; i < shipManager.ShipeSizes[(int)shipManager.CurrentShipIndex]; i++)
            {
                if (colDiff != 0)
                {
                    int offset = colDiff > 0 ? i : -i;
                    if (DGV_AllyFleet[shipManager.CurrentShipPosition.X + offset, shipManager.CurrentShipPosition.Y].Style.BackColor == Color.Black)
                        return true;
                }
                else
                {
                    int offset = rowDiff > 0 ? i : -i;
                    if (DGV_AllyFleet[shipManager.CurrentShipPosition.X, shipManager.CurrentShipPosition.Y + offset].Style.BackColor == Color.Black)
                        return true;
                }
            }

            return false;
        }

        private void resetShipPlacement()
        {
            for (int col = 0; col < DGV_AllyFleet.Columns.Count; col++)
            {
                for (int row = 0; row < DGV_AllyFleet.Rows.Count; row++)
                {
                    if (DGV_AllyFleet[col, row].Style.BackColor == Color.Green || DGV_AllyFleet[col, row].Style.BackColor == Color.Blue)
                    {
                        DGV_AllyFleet[col, row].Style.BackColor = Color.White;
                    }
                }
            }
        }

        private void UpdateStatusLabel_ShipPlacement()
        {
            LBL_Status.Text = "Placez le " + shipManager.ShipNames[(int)shipManager.CurrentShipIndex];
        }


    }
}
