using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

/*
Gameplay deets:
    The boss moves only 1 tile and attacks in a chess like pattern similar to that to the Empress [knight + rook] if player is in range.
    The player can move in 4 directions and attack with spacebar. 
    The boss does this pattern: act -> wait -> act -> wait [and so on] to give the player a chance to react!

Fuzzy Logic Explanation:
    Here is a quick rundown on how it works:
    The boss takes 4 values as crisp inputs 
    Player HP, Boss HP, Distance and Player Aggression.
    With these 4 inputs, it goes through the MAMDANI fuzzification process [this is really just sir alliac's code]. 
    
    They are fed through a triangular membership and is evaluated with 3 rules:
    1. Flee ONLY if dying AND player is right next to you
    2. Hold position if in the sweet spot, OR if player is super passive
    3. Hunt the player if healthy, OR if the player ran far away

    Based on sir alliac's discussion, AND finds the minimum of the two values, while OR finds the maximum of the two values.
    When all three rules are evaluated, they are defuzzified into crisp values and become input to the boss's logic
*/

namespace FuzzyLogicAct
{
    public partial class Form1 : Form
    {
        int playerX = 0, playerY = 0;
        int bossX = 9, bossY = 9;
        int player_dmg = 10, boss_melee_dmg = 15, boss_ranged_dmg = 10;
        double playerHP = 100, bossHP = 100;
        double playerAggression = 50;
        bool bossIsWaiting = false;

        Panel[,] gridPanels = new Panel[10, 10];

        public Form1()
        {
            InitializeComponent();
            this.Text = "Fuzzy Logic Boss Fight";
            this.DoubleBuffered = true;

            SetupArena();
            UpdateUI("PLAYER MOVES FIRST.");
        }

        //UI & INPUT HANDLING
        private void SetupArena()
        {
            int availableWidth = pnlArena.ClientSize.Width;
            int availableHeight = pnlArena.ClientSize.Height;

            int tileSize = Math.Min(availableWidth, availableHeight) / 10;
            pnlArena.ClientSize = new Size(tileSize * 10, tileSize * 10);

            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    Panel p = new Panel();
                    p.Bounds = new Rectangle(x * tileSize, y * tileSize, tileSize - 1, tileSize - 1);
                    p.BackColor = Color.LightGray;

                    pnlArena.Controls.Add(p);
                    gridPanels[x, y] = p;
                }
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (playerHP <= 0 || bossHP <= 0) return base.ProcessCmdKey(ref msg, keyData);

            double oldDistance = GetManhattanDistance(playerX, playerY, bossX, bossY);
            bool playerAttacked = false;
            bool validMove = false;

            if (keyData == Keys.W && playerY > 0) { playerY--; validMove = true; }
            else if (keyData == Keys.S && playerY < 9) { playerY++; validMove = true; }
            else if (keyData == Keys.A && playerX > 0) { playerX--; validMove = true; }
            else if (keyData == Keys.D && playerX < 9) { playerX++; validMove = true; }
            else if (keyData == Keys.Space)
            {
                playerAttacked = true;
                validMove = true;
                playerAggression += 20;

                if (oldDistance == 1) bossHP -= player_dmg;
            }

            if (validMove)
            {
                ExecuteBossTurn(playerAttacked, oldDistance);
                return true; // Input handled
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        //FUZZY LOGIC CORE. [THIS IS WHERE THE MAGIC HAPPENS]

        private void ExecuteBossTurn(bool playerAttacked, double oldDistance)
        {
            double newDistance = GetManhattanDistance(playerX, playerY, bossX, bossY);

            // 1. Crisp Aggression Tracking
            if (!playerAttacked)
            {
                if (newDistance < oldDistance) playerAggression += 15;
                else if (newDistance > oldDistance) playerAggression -= 15;
                else playerAggression = CalculateDecay(playerAggression, 50, 5);
            }
            playerAggression = Math.Max(0, Math.Min(100, playerAggression));

            // 2. Fuzzification
            double distNear = TriangularMembership(newDistance, 0, 0, 4);
            double distMid = TriangularMembership(newDistance, 2, 5, 8);
            double distFar = TriangularMembership(newDistance, 6, 18, 18);

            double bossHpLow = TriangularMembership(bossHP, 0, 0, 50);
            double bossHpHigh = TriangularMembership(bossHP, 50, 100, 100);

            double aggPassive = TriangularMembership(playerAggression, 0, 0, 50);
            double aggAggressive = TriangularMembership(playerAggression, 50, 100, 100);

            // 3. RULE EVALUATION
            // Rule 1: Flee ONLY if dying AND player is right next to you
            double rule1_strength = Math.Min(bossHpLow, distNear);

            // Rule 2: Hold position if in the sweet spot, OR if player is super passive
            double rule2_strength = Math.Max(distMid, aggPassive);

            // Rule 3: Hunt the player if healthy, OR if the player ran far away
            double rule3_strength = Math.Max(bossHpHigh, distFar);

            // 4. Defuzzification (Centroid)
            double sumNumerator = 0.0;
            double sumDenominator = 0.0;
            double step = 1.0;

            for (double y = 0.0; y <= 100.0; y += step)
            {
                double outDefensive = TriangularMembership(y, 0, 0, 40);
                double outNeutral = TriangularMembership(y, 30, 50, 70);
                double outAggressive = TriangularMembership(y, 60, 100, 100);

                double clippedDef = Math.Min(rule1_strength, outDefensive);
                double clippedNeu = Math.Min(rule2_strength, outNeutral);
                double clippedAgg = Math.Min(rule3_strength, outAggressive);

                double aggregatedY = Math.Max(clippedDef, Math.Max(clippedNeu, clippedAgg));

                sumNumerator += y * aggregatedY * step;
                sumDenominator += aggregatedY * step;
            }

            double crispStance = (sumDenominator > 0.0) ? (sumNumerator / sumDenominator) : 50.0;

            // 5. Boss Execution & Wait Pacing
            string bossAction = "";

            if (bossIsWaiting)
            {
                bossAction = "Evaluating... (Waiting Turn)";
                bossIsWaiting = false;
            }
            else
            {
                int pdx = Math.Abs(playerX - bossX);
                int pdy = Math.Abs(playerY - bossY);

                bool canRangedAttack = (playerX == bossX || playerY == bossY) ||
                                       ((pdx == 2 && pdy == 1) || (pdx == 1 && pdy == 2));

                if (crispStance < 40)
                {
                    // Defensive Stance
                    if (canRangedAttack)
                    {
                        bossAction = "Defensive (PANIC LASER!)";
                        playerHP -= boss_ranged_dmg;
                    }
                    else
                    {
                        bossAction = "Defensive (Moved Away)";
                        MoveBoss(false);
                    }
                }
                else if (crispStance >= 40 && crispStance <= 65)
                {
                    bossAction = "Neutral (Positioning)";
                    MoveBoss(true);
                }
                else
                {
                    // Aggressive Stance
                    if (GetManhattanDistance(playerX, playerY, bossX, bossY) == 1)
                    {
                        bossAction = "Aggressive (MELEE ATTACK!)";
                        playerHP -= boss_melee_dmg;
                    }
                    else if (canRangedAttack)
                    {
                        bossAction = "Aggressive (RANGED LASER!)";
                        playerHP -= boss_ranged_dmg;
                    }
                    else
                    {
                        bossAction = "Aggressive (Moved Towards)";
                        MoveBoss(true);
                    }
                }
                bossIsWaiting = true;
            }

            // Generate Debug String
            string debugTxt = $"Distance: {newDistance} | BossHP: {bossHP} | Player Aggression: {playerAggression}\n";
            debugTxt += $"Rules -> Def: {rule1_strength:F2}, Neu: {rule2_strength:F2}, Agg: {rule3_strength:F2}\n";
            debugTxt += $"Centroid Stance: {crispStance:F2}/100\n";
            debugTxt += $"Action Taken: {bossAction}";

            if (playerHP <= 0) debugTxt += "\n\n*** YOU DIED ***";
            else if (bossHP <= 0) debugTxt += "\n\n*** BOSS DEFEATED ***";

            UpdateUI(debugTxt);
        }

        // --- HELPERS & GRAPHICS ---

        private void UpdateUI(string debugText)
        {
            lblPlayerStats.Text = $"Player HP: {playerHP}\nMelee DMG: {player_dmg}";
            lblBossStats.Text = $"Boss HP: {bossHP}\nMelee DMG: {boss_melee_dmg}\n Laser DMG: {boss_ranged_dmg}";
            lblFuzzyDebug.Text = debugText;

            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    int dx = Math.Abs(x - bossX);
                    int dy = Math.Abs(y - bossY);

                    //Straight lines and Knight L-shapes only
                    bool isStraight = (x == bossX || y == bossY);
                    bool isLShape = ((dx == 2 && dy == 1) || (dx == 1 && dy == 2));

                    if (isStraight || isLShape)
                    {
                        gridPanels[x, y].BackColor = Color.LightCoral;
                    }
                    else
                    {
                        gridPanels[x, y].BackColor = Color.LightGray;
                    }

                    //Draw Entities on top
                    if (x == playerX && y == playerY)
                        gridPanels[x, y].BackColor = Color.DodgerBlue;
                    else if (x == bossX && y == bossY)
                        gridPanels[x, y].BackColor = Color.Crimson;
                }
            }
        }

        private void MoveBoss(bool moveTowards)
        {
            if (moveTowards)
            {
                if (bossX < playerX) bossX++;
                else if (bossX > playerX) bossX--;
                else if (bossY < playerY) bossY++;
                else if (bossY > playerY) bossY--;
            }
            else
            {
                if (bossX < playerX && bossX > 0) bossX--;
                else if (bossX > playerX && bossX < 9) bossX++;
                else if (bossY < playerY && bossY > 0) bossY--;
                else if (bossY > playerY && bossY < 9) bossY++;
            }
        }

        private double GetManhattanDistance(int x1, int y1, int x2, int y2)
        {
            return Math.Abs(x2 - x1) + Math.Abs(y2 - y1);
        }

        private double CalculateDecay(double current, double target, double rate)
        {
            if (current > target) return Math.Max(target, current - rate);
            if (current < target) return Math.Min(target, current + rate);
            return current;
        }

        private double TriangularMembership(double x, double a, double b, double c)
        {
            if (x <= a || x >= c) return 0.0;
            if (x == b) return 1.0;
            if (x > a && x < b) return (x - a) / (b - a);
            return (c - x) / (c - b);
        }
    }
}