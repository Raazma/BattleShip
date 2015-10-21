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
    public partial class FormGame : Form
    {
        private bool isPlacingShip = false;
        private ShipManager shipManager = new ShipManager();
        private ServerConnection connection;

        public FormGame()
        {
            InitializeComponent();
            buildDGV(DGV_AllyFleet);
            buildDGV(DGV_EnemyFleet);
            DGV_AllyFleet.ClearSelection();
            DGV_EnemyFleet.ClearSelection();

            DGV_AllyFleet.Enabled = DGV_EnemyFleet.Enabled = false;
        }

        public void SetConnection(ServerConnection conn)
        {
            connection = conn;
        }

        private void FormGame_Load(object sender, EventArgs e)
        {
        }

        private void FormGame_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("Êtes-vous sûr de vouloir quitter ?", "Confirmation", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                connection.StopThread();
            }
            else
            {
                e.Cancel = true;
            }
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

            for (int col = 0; col < DGV.Columns.Count; col++)
            {
                for (int row = 0; row < DGV.Rows.Count; row++)
                {
                    DGV[col, row].Style.BackColor = Properties.Settings.Default.SeaColor;
                }
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
                    if (DGV_AllyFleet[col, row].Style.BackColor == Properties.Settings.Default.MarkerColor)
                    {
                        placeShipOnGrid(col, row);
                        resetShipPlacement();
                        isPlacingShip = false;
                        shipManager.CurrentShipIndex++;

                        if (shipManager.CurrentShipIndex == ShipManager.ShipTypes.SIZEOF_SHIPTYPES)
                        {
                            LBL_Status.Text = "En attente de l'autre joueur";
                        }
                        else
                        {
                            UpdateStatusLabel_ShipPlacement();
                        }
                    }
                    else if (DGV_AllyFleet[col, row].Style.BackColor == Properties.Settings.Default.ResetColor)
                    {
                        resetShipPlacement();
                        isPlacingShip = false;
                    }
                }
                else if (DGV_AllyFleet[col, row].Style.BackColor != Properties.Settings.Default.ShipColor)
                {
                    shipManager.CurrentShipPosition.X = col;
                    shipManager.CurrentShipPosition.Y = row;
                    DGV_AllyFleet[col, row].Style.BackColor = Properties.Settings.Default.ResetColor;
                    placeShipMarkers(col, row, shipManager.ShipeSizes[(int)shipManager.CurrentShipIndex] - 1);
                    isPlacingShip = true;
                }
            }

            DGV_AllyFleet.ClearSelection();

            if (shipManager.CurrentShipIndex == ShipManager.ShipTypes.SIZEOF_SHIPTYPES)
            {
                connection.SendShipPosition(shipManager);
                DGV_AllyFleet.Enabled = false;
                DGV_EnemyFleet.Enabled = true; // TO REPLACE WITH SERVER COMMAND
                LBL_Status.Text = "En attente de l'autre joueur";
            }
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
                            DGV_AllyFleet[col + (c * shipSize), row + (r * shipSize)].Style.BackColor = Properties.Settings.Default.MarkerColor;
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
                DGV_AllyFleet[shipPosX, shipPosY].Style.BackColor = Properties.Settings.Default.ShipColor;
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
                    if (DGV_AllyFleet[shipManager.CurrentShipPosition.X + offset, shipManager.CurrentShipPosition.Y].Style.BackColor == Properties.Settings.Default.ShipColor)
                        return true;
                }
                else
                {
                    int offset = rowDiff > 0 ? i : -i;
                    if (DGV_AllyFleet[shipManager.CurrentShipPosition.X, shipManager.CurrentShipPosition.Y + offset].Style.BackColor == Properties.Settings.Default.ShipColor)
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
                    if (DGV_AllyFleet[col, row].Style.BackColor == Properties.Settings.Default.MarkerColor || DGV_AllyFleet[col, row].Style.BackColor == Properties.Settings.Default.ResetColor)
                    {
                        DGV_AllyFleet[col, row].Style.BackColor = Properties.Settings.Default.SeaColor;
                    }
                }
            }
        }

        private void UpdateStatusLabel_ShipPlacement()
        {
            LBL_Status.Text = "Placez le " + shipManager.ShipNames[(int)shipManager.CurrentShipIndex];
        }

        public void StartShipPlacement()
        {
            UpdateStatusLabel_ShipPlacement();
            DGV_AllyFleet.Enabled = true;
        }


        public void PlayHitAnimation(int col, int row, DataGridView fleet)
        {
            for (int i = 0; i <= 10; i++)
            {
                if (i % 2 == 0)
                {
                    fleet[col, row].Style.BackColor = Properties.Settings.Default.HitColor;
                }
                else
                {
                    fleet[col, row].Style.BackColor = Properties.Settings.Default.ShipColor;
                }
                fleet.Refresh();
                Thread.Sleep(50);
            }
        }

        public void PlayMissAnimation(int col, int row, DataGridView fleet)
        {
            for (int i = 0; i <= 10; i++)
            {
                if (i % 2 == 0)
                {
                    fleet[col, row].Style.BackColor = Properties.Settings.Default.SeaColor;
                }
                else
                {
                    fleet[col, row].Style.BackColor = Properties.Settings.Default.MissColor;
                }
                fleet.Refresh();
                Thread.Sleep(50);
            }
        }

        private void DGV_EnemyFleet_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int row = e.RowIndex;
            int col = e.ColumnIndex;

            DGV_EnemyFleet.ClearSelection();

            if (row != -1 && col != -1)
            {
                if (DGV_EnemyFleet[col, row].Style.BackColor == Properties.Settings.Default.HitColor ||
                    DGV_EnemyFleet[col, row].Style.BackColor == Properties.Settings.Default.MissColor)
                {
                    LBL_Status.Text = "Vous avez déjà tiré à cet endroit";
                }
                else
                {
                    DGV_EnemyFleet.Enabled = false;
                    connection.SendShot(col, row);
                }
            }
        }

        public void StartTurn()
        {
            DGV_EnemyFleet.Enabled = true;
            LBL_Status.Text = "C'est à votre tour !";
        }

        public void EnemySunk(String ship, int col, int row)
        {
            PlayHitAnimation(col, row, DGV_EnemyFleet);
            LBL_Status.Text = "Vous avez coulé le " + ship + " ennemi !";
        }

        public void AllySunk(String ship, int col, int row)
        {
            PlayHitAnimation(col, row, DGV_AllyFleet);
            LBL_Status.Text = "Votre " + ship + "a été coulé !";
        }

        public void EnemyHit(int col, int row)
        {
            PlayHitAnimation(col, row, DGV_EnemyFleet);
            LBL_Status.Text = "Vous avez touché un navire !";
        }

        public void AllyHit(int col, int row)
        {
            PlayHitAnimation(col, row, DGV_AllyFleet);
            LBL_Status.Text = "Votre navire a été touché !";
        }

        public void EnemyMiss(int col, int row)
        {
            PlayMissAnimation(col, row, DGV_EnemyFleet);
            DGV_EnemyFleet[col, row].Style.BackColor = Properties.Settings.Default.MissColor;
            LBL_Status.Text = "Vous avez raté";
        }

        public void AllyMiss(int col, int row)
        {
            PlayMissAnimation(col, row, DGV_AllyFleet);
            LBL_Status.Text = "Votre ennemi a raté";
        }
    }
}
