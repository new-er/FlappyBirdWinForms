using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace FlappyBird
{
    public class Form1 : Form
    {
        private Timer gameTimer;

        private float birdY;
        private float birdVelocity;

        private const float Gravity = 0.45f;
        private const float JumpStrength = -7.5f;

        private const int BirdX = 100;
        private const int BirdSize = 24;

        private int score;
        private bool gameOver;
        private bool started;

        private Random random = new Random();
        private List<Pipe> pipes = new List<Pipe>();

        private const int PipeWidth = 60;
        private const int PipeGap = 145;
        private const int PipeSpeed = 3;

        public Form1()
        {
            InitializeGame();
        }

        private void InitializeGame()
        {
            Text = "Flappy Bird";
            ClientSize = new Size(480, 640);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            DoubleBuffered = true;
            KeyPreview = true;
            BackColor = Color.SkyBlue;

            gameTimer = new Timer();
            gameTimer.Interval = 16; // roughly 60 FPS
            gameTimer.Tick += GameLoop;

            KeyDown += Form1_KeyDown;
            MouseDown += Form1_MouseDown;

            ResetGame();
        }

        private void ResetGame()
        {
            birdY = ClientSize.Height / 2 - BirdSize / 2;
            birdVelocity = 0;

            score = 0;
            gameOver = false;
            started = false;

            pipes.Clear();

            AddPipe(ClientSize.Width + 100);
            AddPipe(ClientSize.Width + 350);

            gameTimer.Stop();

            Invalidate();
        }

        private void StartGame()
        {
            if (gameOver)
            {
                ResetGame();
            }

            started = true;
            birdVelocity = JumpStrength;
            gameTimer.Start();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                if (!started || gameOver)
                {
                    StartGame();
                }
                else
                {
                    Flap();
                }
            }

            if (e.KeyCode == Keys.R && gameOver)
            {
                ResetGame();
            }
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            if (!started || gameOver)
            {
                StartGame();
            }
            else
            {
                Flap();
            }
        }

        private void Flap()
        {
            birdVelocity = JumpStrength;
        }

        private void GameLoop(object sender, EventArgs e)
        {
            if (!started || gameOver)
                return;

            // Bird physics
            birdVelocity += Gravity;
            birdY += birdVelocity;

            // Move pipes
            for (int i = pipes.Count - 1; i >= 0; i--)
            {
                pipes[i].X -= PipeSpeed;

                // Award point when bird passes pipe
                if (!pipes[i].Passed &&
                    pipes[i].X + PipeWidth < BirdX)
                {
                    pipes[i].Passed = true;
                    score++;
                }

                // Remove pipes that have left the screen
                if (pipes[i].X + PipeWidth < 0)
                {
                    pipes.RemoveAt(i);
                }
            }

            // Add new pipes
            if (pipes.Count > 0)
            {
                Pipe last = pipes[pipes.Count - 1];

                if (last.X < ClientSize.Width - 220)
                {
                    AddPipe(ClientSize.Width + 20);
                }
            }

            CheckCollision();

            Invalidate();
        }

        private void AddPipe(int x)
        {
            // Keep enough room for the gap
            int minimumTop = 80;
            int maximumTop = ClientSize.Height - PipeGap - 120;

            int topHeight = random.Next(minimumTop, maximumTop);

            Pipe pipe = new Pipe();
            pipe.X = x;
            pipe.TopHeight = topHeight;
            pipe.Passed = false;

            pipes.Add(pipe);
        }

        private void CheckCollision()
        {
            Rectangle birdRect = new Rectangle(
                BirdX,
                (int)birdY,
                BirdSize,
                BirdSize);

            // Top/bottom of screen
            if (birdY < 0 ||
                birdY + BirdSize > ClientSize.Height)
            {
                EndGame();
                return;
            }

            foreach (Pipe pipe in pipes)
            {
                Rectangle topPipe = new Rectangle(
                    pipe.X,
                    0,
                    PipeWidth,
                    pipe.TopHeight);

                Rectangle bottomPipe = new Rectangle(
                    pipe.X,
                    pipe.TopHeight + PipeGap,
                    PipeWidth,
                    ClientSize.Height -
                    (pipe.TopHeight + PipeGap));

                if (birdRect.IntersectsWith(topPipe) ||
                    birdRect.IntersectsWith(bottomPipe))
                {
                    EndGame();
                    return;
                }
            }
        }

        private void EndGame()
        {
            gameOver = true;
            gameTimer.Stop();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;

            DrawBackground(g);
            DrawPipes(g);
            DrawBird(g);
            DrawScore(g);

            if (!started)
            {
                DrawStartScreen(g);
            }
            else if (gameOver)
            {
                DrawGameOver(g);
            }
        }

        private void DrawBackground(Graphics g)
        {
            // Sky
            using (SolidBrush skyBrush =
                new SolidBrush(Color.SkyBlue))
            {
                g.FillRectangle(
                    skyBrush,
                    ClientRectangle);
            }

            // Simple clouds
            using (SolidBrush cloudBrush =
                new SolidBrush(Color.White))
            {
                g.FillEllipse(cloudBrush, 40, 80, 80, 35);
                g.FillEllipse(cloudBrush, 75, 65, 70, 45);
                g.FillEllipse(cloudBrush, 115, 80, 80, 35);

                g.FillEllipse(cloudBrush, 300, 140, 80, 35);
                g.FillEllipse(cloudBrush, 340, 125, 70, 45);
                g.FillEllipse(cloudBrush, 380, 140, 80, 35);
            }

            // Ground
            int groundHeight = 45;

            using (SolidBrush groundBrush =
                new SolidBrush(Color.FromArgb(220, 190, 80)))
            {
                g.FillRectangle(
                    groundBrush,
                    0,
                    ClientSize.Height - groundHeight,
                    ClientSize.Width,
                    groundHeight);
            }

            // Grass
            using (SolidBrush grassBrush =
                new SolidBrush(Color.ForestGreen))
            {
                g.FillRectangle(
                    grassBrush,
                    0,
                    ClientSize.Height - groundHeight,
                    ClientSize.Width,
                    8);
            }
        }

        private void DrawPipes(Graphics g)
        {
            foreach (Pipe pipe in pipes)
            {
                using (SolidBrush pipeBrush =
                    new SolidBrush(Color.ForestGreen))
                using (Pen pipeOutline =
                    new Pen(Color.DarkGreen, 2))
                {
                    Rectangle top = new Rectangle(
                        pipe.X,
                        0,
                        PipeWidth,
                        pipe.TopHeight);

                    Rectangle topCap = new Rectangle(
                        pipe.X - 4,
                        pipe.TopHeight - 25,
                        PipeWidth + 8,
                        25);

                    Rectangle bottom = new Rectangle(
                        pipe.X,
                        pipe.TopHeight + PipeGap,
                        PipeWidth,
                        ClientSize.Height -
                        (pipe.TopHeight + PipeGap));

                    Rectangle bottomCap = new Rectangle(
                        pipe.X - 4,
                        pipe.TopHeight + PipeGap,
                        PipeWidth + 8,
                        25);

                    g.FillRectangle(pipeBrush, top);
                    g.DrawRectangle(pipeOutline, top);

                    g.FillRectangle(pipeBrush, topCap);
                    g.DrawRectangle(pipeOutline, topCap);

                    g.FillRectangle(pipeBrush, bottom);
                    g.DrawRectangle(pipeOutline, bottom);

                    g.FillRectangle(pipeBrush, bottomCap);
                    g.DrawRectangle(pipeOutline, bottomCap);
                }
            }
        }

        private void DrawBird(Graphics g)
        {
            int x = BirdX;
            int y = (int)birdY;

            // Body
            using (SolidBrush yellow =
                new SolidBrush(Color.Gold))
            using (Pen outline =
                new Pen(Color.DarkOrange, 2))
            {
                g.FillEllipse(
                    yellow,
                    x,
                    y,
                    BirdSize,
                    BirdSize);

                g.DrawEllipse(
                    outline,
                    x,
                    y,
                    BirdSize,
                    BirdSize);
            }

            // Wing
            using (SolidBrush wing =
                new SolidBrush(Color.Orange))
            {
                g.FillEllipse(
                    wing,
                    x - 2,
                    y + 11,
                    14,
                    9);
            }

            // Eye
            using (SolidBrush black =
                new SolidBrush(Color.Black))
            {
                g.FillEllipse(
                    black,
                    x + 14,
                    y + 5,
                    5,
                    5);
            }

            // Beak
            Point[] beak =
            {
                new Point(x + BirdSize - 2, y + 10),
                new Point(x + BirdSize + 8, y + 14),
                new Point(x + BirdSize - 2, y + 17)
            };

            using (SolidBrush orange =
                new SolidBrush(Color.OrangeRed))
            {
                g.FillPolygon(orange, beak);
            }
        }

        private void DrawScore(Graphics g)
        {
            using (Font font =
                new Font("Arial", 32, FontStyle.Bold))
            using (SolidBrush brush =
                new SolidBrush(Color.White))
            using (Pen shadow =
                new Pen(Color.Black, 3))
            {
                string text = score.ToString();

                SizeF size = g.MeasureString(text, font);

                float x = (ClientSize.Width - size.Width) / 2;
                float y = 15;

                // Simple text shadow
                g.DrawString(
                    text,
                    font,
                    new SolidBrush(Color.Black),
                    x + 2,
                    y + 2);

                g.DrawString(
                    text,
                    font,
                    brush,
                    x,
                    y);
            }
        }

        private void DrawStartScreen(Graphics g)
        {
            using (Font titleFont =
                new Font("Arial", 36, FontStyle.Bold))
            using (Font instructionFont =
                new Font("Arial", 16, FontStyle.Bold))
            using (SolidBrush black =
                new SolidBrush(Color.Black))
            {
                string title = "FLAPPY BIRD";
                string instruction =
                    "SPACE / CLICK TO FLY";

                SizeF titleSize =
                    g.MeasureString(title, titleFont);

                SizeF instructionSize =
                    g.MeasureString(instruction, instructionFont);

                g.DrawString(
                    title,
                    titleFont,
                    black,
                    (ClientSize.Width - titleSize.Width) / 2,
                    240);

                g.DrawString(
                    instruction,
                    instructionFont,
                    black,
                    (ClientSize.Width - instructionSize.Width) / 2,
                    300);
            }
        }

        private void DrawGameOver(Graphics g)
        {
            using (SolidBrush overlay =
                new SolidBrush(Color.FromArgb(100, Color.Black)))
            {
                g.FillRectangle(
                    overlay,
                    ClientRectangle);
            }

            using (Font titleFont =
                new Font("Arial", 36, FontStyle.Bold))
            using (Font scoreFont =
                new Font("Arial", 20, FontStyle.Bold))
            using (SolidBrush brush =
                new SolidBrush(Color.White))
            {
                string title = "GAME OVER";
                string scoreText =
                    "Score: " + score +
                    "\n\nSPACE / CLICK TO RESTART";

                SizeF titleSize =
                    g.MeasureString(title, titleFont);

                g.DrawString(
                    title,
                    titleFont,
                    brush,
                    (ClientSize.Width - titleSize.Width) / 2,
                    230);

                SizeF scoreSize =
                    g.MeasureString(scoreText, scoreFont);

                g.DrawString(
                    scoreText,
                    scoreFont,
                    brush,
                    (ClientSize.Width - scoreSize.Width) / 2,
                    300);
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (gameTimer != null)
                gameTimer.Dispose();

            base.OnFormClosed(e);
        }

        private class Pipe
        {
            public int X;
            public int TopHeight;
            public bool Passed;
        }
    }
}