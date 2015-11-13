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
        String lastMessage;

        public FormGame()
        {
            // Construction de l'interface graphique
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

        private void FormGame_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Si la partie est en cours, on demande de confirmer la fermeture
            if (connection.IsAlive())
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
        }

        // Construit une grille de jeu
        private void buildDGV(DataGridView DGV)
        {
            // Hauteur et largeur des cellules
            DGV.ColumnHeadersHeight = 20;
            DGV.RowHeadersWidth = 50;

            // Création des titre des colonnes
            for (int i = 0; i < 10; i++)
            {
                DGV.Columns.Add("Col" + (i + 1).ToString(), (i + 1).ToString());
                DGV.Columns[i].Width = 30;
            }

            // Création des titres des rangées
            char rowHeader = 'A';
            for (int i = 0; i < 10; i++)
            {
                DGV.Rows.Add();
                DGV.Rows[i].HeaderCell.Value = rowHeader.ToString();
                DGV.Rows[i].Height = 30;
                rowHeader++;
            }

            // Appliquer la couleur de fond à chaque cellule
            for (int col = 0; col < DGV.Columns.Count; col++)
            {
                for (int row = 0; row < DGV.Rows.Count; row++)
                {
                    DGV[col, row].Style.BackColor = Properties.Settings.Default.SeaColor;
                }
            }
        }

        // Clic dans la grille de flotte alliée, pour placer les bateaux
        private void DGV_AllyFleet_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // L'endroit où s'est produit le clic
            int row = e.RowIndex;
            int col = e.ColumnIndex;

            // Si c'est un clic dans la grille
            if (row != -1 && col != -1)
            {
                // Si le joueur est en train de placer un bateau
                if (isPlacingShip)
                {
                    // Si le joueur a cliquer sur un marqueur de positionnement
                    if (DGV_AllyFleet[col, row].Style.BackColor == Properties.Settings.Default.MarkerColor)
                    {
                        // On place le bateau
                        placeShipOnGrid(col, row);
                        // On passe au bateau suivant
                        resetShipPlacement();
                        isPlacingShip = false;
                        shipManager.CurrentShipIndex++;

                        // Si le joueur a terminé de placer ses bateaux
                        if (shipManager.CurrentShipIndex == ShipManager.ShipTypes.SIZEOF_SHIPTYPES)
                        {
                            LBL_Status.Text = "En attente de l'autre joueur";
                        }
                        else
                        {
                            // On affiche le prochain bateau à placer
                            UpdateStatusLabel_ShipPlacement();
                        }
                    }
                    // Le joueur a cliqué sur la position initiale, on arrête le placement de bateau
                    else if (DGV_AllyFleet[col, row].Style.BackColor == Properties.Settings.Default.ResetColor)
                    {
                        resetShipPlacement();
                        isPlacingShip = false;
                    }
                }
                // Le joueur n'est pas en train de placer un bateau et a cliqué sur l'eau
                else if (DGV_AllyFleet[col, row].Style.BackColor != Properties.Settings.Default.ShipColor)
                {
                    // On place la position initiale
                    shipManager.CurrentShipPosition.X = col;
                    shipManager.CurrentShipPosition.Y = row;
                    DGV_AllyFleet[col, row].Style.BackColor = Properties.Settings.Default.ResetColor;
                    // On place les marqueurs de positionnement
                    placeShipMarkers(col, row, shipManager.ShipeSizes[(int)shipManager.CurrentShipIndex] - 1);
                    isPlacingShip = true;
                }
            }

            DGV_AllyFleet.ClearSelection();

            // Le joueur a terminé de placer ses bateaux, on les envoie au serveur
            if (shipManager.CurrentShipIndex == ShipManager.ShipTypes.SIZEOF_SHIPTYPES)
            {
                connection.SendShipPosition(shipManager);
                DGV_AllyFleet.Enabled = DGV_EnemyFleet.Enabled = false;
                LBL_Status.Text = "En attente de l'autre joueur";
            }
        }

        // Affiche les marqueurs de positionnement aux endroits valides pour placer un bateau
        private void placeShipMarkers(int col, int row, int shipSize)
        {
            // On parcourt les cellules adjacentes au clic
            for (int c = -1; c <= 1; c++)
            {
                for (int r = -1; r <= 1; r++)
                {
                    if ((c == 0) != (r == 0)) // (c == 0) XOR (r == 0) Si on est dans un direction orthogonale
                    {
                        if (row + (r * shipSize) >= 0 && row + (r * shipSize) < DGV_AllyFleet.Rows.Count && // Si le navire ne dépasse pas la grille en rangées
                            col + (c * shipSize) >= 0 && col + (c * shipSize) < DGV_AllyFleet.Columns.Count && // Si le navire ne dépasse pas la grille en colonnes
                            !shipIsInTheWay(col + (c * shipSize), row + (r * shipSize))) // Si il n'y a pas un autre bateau dans le chemin
                        {
                            // On place un marqueur
                            DGV_AllyFleet[col + (c * shipSize), row + (r * shipSize)].Style.BackColor = Properties.Settings.Default.MarkerColor;
                        }
                    }
                }
            }
        }

        // Place un bateau sur la grille
        private void placeShipOnGrid(int col, int row)
        {
            int colDiff = col - shipManager.CurrentShipPosition.X;
            int rowDiff = row - shipManager.CurrentShipPosition.Y;
            int offset;
            int shipPosX;
            int shipPosY;

            for (int i = 0; i < shipManager.ShipeSizes[(int)shipManager.CurrentShipIndex]; i++)
            {
                // Le bateau est postionné de haut en bas
                if (colDiff != 0)
                {
                    offset = colDiff > 0 ? i : -i; // On détermine si le bateau est vers le haut ou le bas
                    // On calculue la position du prochain point du bateau
                    shipPosX = shipManager.CurrentShipPosition.X + offset;
                    shipPosY = shipManager.CurrentShipPosition.Y;
                }
                // Le bateau est positionné de gauche à droite
                else
                {
                    offset = rowDiff > 0 ? i : -i; // On détermine si le bateau est vers la gauche ou la droite
                    // On calculue la position du prochain point du bateau
                    shipPosX = shipManager.CurrentShipPosition.X;
                    shipPosY = shipManager.CurrentShipPosition.Y + offset;
                }

                // On place le prochain point du bateau
                shipManager.ShipPositions[(int)shipManager.CurrentShipIndex, i] = new Point(shipPosX, shipPosY);
                DGV_AllyFleet[shipPosX, shipPosY].Style.BackColor = Properties.Settings.Default.ShipColor;
            }
        }

        // Vérifie s'il y a un bateau dans le chemin
        private bool shipIsInTheWay(int col, int row)
        {
            int colDiff = col - shipManager.CurrentShipPosition.X;
            int rowDiff = row - shipManager.CurrentShipPosition.Y;

            for (int i = 0; i < shipManager.ShipeSizes[(int)shipManager.CurrentShipIndex]; i++)
            {
                // Le bateau est postionné de haut en bas
                if (colDiff != 0)
                {
                    int offset = colDiff > 0 ? i : -i; // On détermine si le bateau est vers le haut ou le bas
                    if (DGV_AllyFleet[shipManager.CurrentShipPosition.X + offset, shipManager.CurrentShipPosition.Y].Style.BackColor == Properties.Settings.Default.ShipColor)
                        return true; // Il y a un bateau dans le chemin
                }
                // Le bateau est positionné de gauche à droite
                else
                {
                    int offset = rowDiff > 0 ? i : -i; // On détermine si le bateau est vers la gauche ou la droite
                    if (DGV_AllyFleet[shipManager.CurrentShipPosition.X, shipManager.CurrentShipPosition.Y + offset].Style.BackColor == Properties.Settings.Default.ShipColor)
                        return true; // Il y a un bateau dans le chemin
                }
            }

            // Il n'y a pas de bateau dans le chemin
            return false;
        }

        // Retire les marqueurs de positionnement des bateaux
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

        // Met à jour le label avec le prochain bateau à placer
        private void UpdateStatusLabel_ShipPlacement()
        {
            LBL_Status.Text = "Placez le " + shipManager.ShipNames[(int)shipManager.CurrentShipIndex];
        }

        // Débute le placement des bateaux
        public void StartShipPlacement()
        {
            UpdateStatusLabel_ShipPlacement();
            DGV_AllyFleet.Enabled = true;
            DGV_EnemyFleet.Enabled = false;
        }

        // Affiche une animation de bateau touché
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

        // Affiche une animation de bateau raté
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

        // Clic dans la grille de flotte ennemie, pour envoyer un tir au serveur
        private void DGV_EnemyFleet_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // L'endroit où s'est produit le clic
            int row = e.RowIndex;
            int col = e.ColumnIndex;

            DGV_EnemyFleet.ClearSelection();

            // Si c'est un clic dans la grille
            if (row != -1 && col != -1)
            {
                // On vérifie si le joueur a déjà tiré à cet endroit
                if (DGV_EnemyFleet[col, row].Style.BackColor == Properties.Settings.Default.HitColor ||
                    DGV_EnemyFleet[col, row].Style.BackColor == Properties.Settings.Default.MissColor)
                {
                    LBL_Status.Text = "Vous avez déjà tiré à cet endroit";
                }
                else
                {
                    // Position de tir valide, on l'envoie
                    DGV_EnemyFleet.Enabled = false;
                    connection.SendShot(col, row);
                }
            }
        }

        // Début du tour du joueur
        public void StartTurn()
        {
            // Permettre de tirer
            DGV_EnemyFleet.Enabled = true;

            // Afficher le dernier message du serveur et l'indication que c'est à noter tour
            if (!String.IsNullOrEmpty(lastMessage))
                LBL_Status.Text = lastMessage + ", c'est à votre tour !";
            else
                LBL_Status.Text = "C'est à votre tour !";
        }

        // Un bateau ennemi a été coulé
        public void EnemySunk(String ship, int col, int row)
        {
            PlayHitAnimation(col, row, DGV_EnemyFleet);
            LBL_Status.Text = "Vous avez coulé le " + ship + " ennemi !";
        }

        // Un bateau du joueur a été coulé
        public void AllySunk(String ship, int col, int row)
        {
            PlayHitAnimation(col, row, DGV_AllyFleet);
            lastMessage = "Votre " + ship + " a été coulé";
        }

        // Un bateau ennemi a été touché
        public void EnemyHit(int col, int row)
        {
            PlayHitAnimation(col, row, DGV_EnemyFleet);
            LBL_Status.Text = "Vous avez touché un navire !";
        }

        // Un bateau du joueur a été touché
        public void AllyHit(int col, int row)
        {
            PlayHitAnimation(col, row, DGV_AllyFleet);
            lastMessage = "Votre navire a été touché";
        }

        // Un bateau ennemi a été raté
        public void EnemyMiss(int col, int row)
        {
            PlayMissAnimation(col, row, DGV_EnemyFleet);
            DGV_EnemyFleet[col, row].Style.BackColor = Properties.Settings.Default.MissColor;
            LBL_Status.Text = "Vous avez raté";
        }

        // Un bateau du joueur a été raté
        public void AllyMiss(int col, int row)
        {
            PlayMissAnimation(col, row, DGV_AllyFleet);
            lastMessage = "Votre ennemi a raté";
        }

        // Le joueur a perdu la partie
        public void GameLost()
        {
            LBL_Status.Text = "Vous avez perdu !";
            LBL_Status.ForeColor = Color.Red;
        }

        // Le joueur a gagné la partie
        public void GameWon()
        {
            LBL_Status.Text = "Vous avez gagné !";
            LBL_Status.ForeColor = Color.Green;
        }

        // L'ennemi s'est déconnecté
        public void EnemyDisconnected()
        {
            LBL_Status.Text = "Votre ennemi a déclaré forfait !";
            DGV_EnemyFleet.Enabled = DGV_AllyFleet.Enabled = false;
        }

        // Le serveur s'est déconnecté
        public void ServerDisconnected()
        {
            LBL_Status.Text = "Connexion au serveur perdue !";
            DGV_EnemyFleet.Enabled = DGV_AllyFleet.Enabled = false;
        }
    }
}
