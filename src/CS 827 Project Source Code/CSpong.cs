using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using Ventuz.OSC;
using Microsoft.DirectX;

namespace CS_827_Project_Source_Code
{
    class CSpong : CGame
    {
        // States which the Pong game may be in
        enum EPongGameStates
        {
            Play = 0,
            Pause = 1,
            WaitForNextBall = 2
        }


        // Constants
        const int miBALL_START_SPEED = 200;     // Starting speed of the Ball
        const int miBALL_MAX_SPEED = 1000;      // Max speed of the Ball
        const int miBALL_SPEED_INCREMENT = 50;  // How much the speed increases each time the Player returns it
        const int miPADDLE_MAX_SPEED_PER_SECOND = 500;  // The Paddles maximum speed in pixels per second


        // Class Variables
        EPongGameStates mePongState;// The current state of the Pong game

        Rectangle mrPaddle;         // The players Paddle location and size
        Image mcPaddleImage;        // The Image to use for the Paddle

        Rectangle mrBall;           // The Balls location and size
        Image mcBallImage;          // The Image to use for the Ball
        Vector2 mcBallDirection;    // Vector used for Speed and Direction of the Ball

        Rectangle mrLeftWall, mrRightWall;  // The Walls of the game
        Rectangle mrTopWall, mrBottomWall;

        CPitchMeter mcPitchMeter;   // The Pitch Meter

        Font mcFontText;            // The Font used to draw any text
        Font mcFontPause;           // The Font used for the Paused text

        Random mcRandom;            // Used to generate random numbers

        int miScore;                // The players score
        int miLives;                // The number of lives the player has left
        bool mbGameStarted;         // Tells if the Game has been started or not

        float mfNextBallWaitTime;   // Used to wait a bit before playing the next ball

        bool mbMouseEnabled;        // Tells whether the Mouse can be used or not
        int miMouseYLastFrame;      // Holds where the Mouse Y position was last Frame

        // Sounds and FMOD variables
        FMOD.Sound msSoundBallBounce = null;
        FMOD.Sound msSoundBallDied = null;
        FMOD.Sound msSoundGameOver = null;


        // Class constructor
        public CSpong()
        {
            try
            {
                // Pre-load images
                mcPaddleImage = Image.FromFile("../../Data/Images/SpongPaddle.png");
                mcBallImage = Image.FromFile("../../Data/Images/SpongBall.png");

                // Pre-load sounds
                CFMOD.CheckResult(CFMOD.GetSystem().createSound("../../Data/Sounds/SpongBallBounce.wav", FMOD.MODE.DEFAULT, ref msSoundBallBounce), "FMOD Create Sound Error");
                CFMOD.CheckResult(CFMOD.GetSystem().createSound("../../Data/Sounds/SpongBallDied.wav", FMOD.MODE.DEFAULT, ref msSoundBallDied), "FMOD Create Sound Error");
                CFMOD.CheckResult(CFMOD.GetSystem().createSound("../../Data/Sounds/SpongGameOver.wav", FMOD.MODE.DEFAULT, ref msSoundGameOver), "FMOD Create Sound Error");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Exception caught");
            }

            // Seed the Random number generator
            mcRandom = new Random(Environment.TickCount);

            // Mouse turned off by default
            mbMouseEnabled = false;
            miMouseYLastFrame = 0;

            // Setup the Walls
            mrLeftWall = new Rectangle(60, 50, 10, 460);
            mrRightWall = new Rectangle(770, 50, 10, 460);
            mrTopWall = new Rectangle(60, 50, 720, 10);
            mrBottomWall = new Rectangle(60, 500, 720, 10);

            // Setup the Pitch Meter
            mcPitchMeter = new CPitchMeter(26, 125);
            mcPitchMeter.ShowHalfPointMarker(true);

            // Setup the Fonts
            mcFontText = new Font("Arial", 20, FontStyle.Regular);
            mcFontPause = new Font("Arial", 50, FontStyle.Regular);

            // Set the other variables default values
            Reset();
        }

        // Reset class variables to their default values
        public void Reset()
        {
            // Record that we are playing Pong
            meGame = EGames.Spong;            

            // Set the games initial State
            mePongState = EPongGameStates.Pause;

            // Record that the game hasn't started yet
            mbGameStarted = false;

            // Set the Paddles initial position
            mrPaddle = new Rectangle(80, 250, 20, 100);
            
            // Set the Balls initial position, direction, and speed
            mrBall = new Rectangle(150, 250, 20, 20);
            mcBallDirection = new Vector2(1.0f, 0.0f);
            mcBallDirection.Normalize();
            mcBallDirection.Scale(miBALL_START_SPEED);      // Initial speed
            ResetBall();

            // Start the player with no Score and 3 Lives
            miScore = 0;
            miLives = 3;

            // Set the initial Pitch Meter Indicators position
            mcPitchMeter.UpdatePitchIndicatorsPosition(0.5f);
        }

        // Do game processing and update the display
        public override void Update(float fTimeSinceLastUpdateInSeconds, Graphics cBackBuffer)
        {
            // Do actions based on which state the Pong game is in
            switch (mePongState)
            {
                default:
                case EPongGameStates.Play:
                    UpdateGameObjects(fTimeSinceLastUpdateInSeconds, cBackBuffer);
                    DrawGame(fTimeSinceLastUpdateInSeconds, cBackBuffer);
                break;

                case EPongGameStates.Pause:
                    DrawGame(fTimeSinceLastUpdateInSeconds, cBackBuffer);
                    DisplayPause(fTimeSinceLastUpdateInSeconds, cBackBuffer);
                break;

                case EPongGameStates.WaitForNextBall:
                    // Calculate how long we have waited so far
                    mfNextBallWaitTime += fTimeSinceLastUpdateInSeconds;

                    // If we have waited one second
                    if (mfNextBallWaitTime > 1.0f)
                    {
                        // Start Playing the game again
                        mePongState = EPongGameStates.Play;
                    }
                break;
            }
        }

        // Update the Ball and Paddle
        private void UpdateGameObjects(float fTimeSinceLastUpdateInSeconds, Graphics cBackBuffer)
        {
            // Temp local variables
            int iBallXDistanceTravelled = 0;    // How far the Ball has moved in the X direction
            int iBallYDistanceTravelled = 0;    // How far the Ball has moved in the Y direction
            Rectangle rBallOldPosition = mrBall;// Rectangle used to test Ball against collisions
            Rectangle rBallCollisionRect;       // Rectangle used to test Ball against collisions

            // Move the Pitch Meter according to any Input Pitch
            mcPitchMeter.AutoDetectPitchAndUpdateMeter();

            // If some Pitch Input was received
            if (CSoundInput.Instance().bInputReceived)
            {
                // Calculate what Percent of force should be used to move the Paddle
                float fPercentToMove = mcPitchMeter.mfPitchIndicatorUnitPosition - 0.5f;
                fPercentToMove *= 2.0f;

                // Calculate how many pixels to move the Paddle
                float fAmountToMove = fPercentToMove * (-miPADDLE_MAX_SPEED_PER_SECOND * fTimeSinceLastUpdateInSeconds);

                // Move the Paddle
                mrPaddle.Y += (int)fAmountToMove;
            }
            
            // Calculate how far the Ball should move
            iBallXDistanceTravelled = (int)(mcBallDirection.X * fTimeSinceLastUpdateInSeconds);
            iBallYDistanceTravelled = (int)(mcBallDirection.Y * fTimeSinceLastUpdateInSeconds);

            // Move the Ball            
            mrBall.X += iBallXDistanceTravelled;
            mrBall.Y += iBallYDistanceTravelled;

            // Test for collision between the Ball and the Walls

            // Rectangle to use to test collisions against the Ball
            rBallCollisionRect = mrBall;

            // If the Ball has travelled further than it's width or height in the last frame
            if (Math.Abs(iBallXDistanceTravelled) >= mrBall.Width || 
                Math.Abs(iBallYDistanceTravelled) >= mrBall.Height)
            {
                // If the Ball is moving right
                if (iBallXDistanceTravelled > 0)
                {
                    // If the Ball is moving down
                    if (iBallYDistanceTravelled > 0)
                    {
                        rBallCollisionRect.Location = rBallOldPosition.Location;
                        rBallCollisionRect.Width = mrBall.Right - rBallOldPosition.Left;
                        rBallCollisionRect.Height = mrBall.Bottom - rBallOldPosition.Top;
                    }
                    // Else the Ball is moving up
                    else
                    {
                        rBallCollisionRect.X = rBallOldPosition.X;
                        rBallCollisionRect.Y = mrBall.Top;
                        rBallCollisionRect.Width = mrBall.Right - rBallOldPosition.Left;
                        rBallCollisionRect.Height = rBallOldPosition.Bottom - mrBall.Top;
                    }
                }
                // Else the Ball is moving left
                else
                {
                    // If the Ball is moving down
                    if (iBallYDistanceTravelled > 0)
                    {
                        rBallCollisionRect.X = mrBall.Left;
                        rBallCollisionRect.Y = rBallOldPosition.Top;
                        rBallCollisionRect.Width = rBallOldPosition.Right - mrBall.Left;
                        rBallCollisionRect.Height = mrBall.Bottom - rBallOldPosition.Top;
                    }
                    // Else the Ball is moving up
                    else
                    {
                        rBallCollisionRect.Location = mrBall.Location;
                        rBallCollisionRect.Width = rBallOldPosition.Right - mrBall.Left;
                        rBallCollisionRect.Height = rBallOldPosition.Bottom - mrBall.Top;
                    }
                }
            }

            // If the Ball hit the Top Wall
            if (rBallCollisionRect.IntersectsWith(mrTopWall))
            {
                // Reverse it's Y direction
                mcBallDirection.Y = -mcBallDirection.Y;

                // Place it to be hitting the Wall (not in the Wall)
                mrBall.Y = mrTopWall.Bottom;

                // Play Sound
                CFMOD.PlaySound(msSoundBallBounce, false);
            }
            // If the Ball hit the Bottom Wall
            else if (rBallCollisionRect.IntersectsWith(mrBottomWall))
            {
                // Reverse it's Y direction
                mcBallDirection.Y = -mcBallDirection.Y;

                // Place it to be hitting the Wall (not in the Wall)
                mrBall.Y = mrBottomWall.Top - mrBall.Height;

                // Play Sound
                CFMOD.PlaySound(msSoundBallBounce, false);
            }

            // If the Ball hit the Right wall
            if (rBallCollisionRect.IntersectsWith(mrRightWall))
            {
                // Reverse it's X direction
                mcBallDirection.X = -mcBallDirection.X;

                // Place it to be hitting the Wall (not in the Wall)
                mrBall.X = mrRightWall.Left - mrBall.Width;

                // Play Sound
                CFMOD.PlaySound(msSoundBallBounce, false);
            }

            // Test for collision between the Ball and the Paddle
            // NOTE: Test for collision between Paddle before left wall incase ball is
            //       travelling too fast and would pass through both
            if (rBallCollisionRect.IntersectsWith(mrPaddle))
            {
                // Increase the Balls speed slightly
                float fSpeed = mcBallDirection.Length();
                fSpeed += miBALL_SPEED_INCREMENT;

                // Make sure Ball does not go too fast
                if (fSpeed > miBALL_MAX_SPEED)
                {
                    fSpeed = miBALL_MAX_SPEED;
                }

                // Reverse the Balls X direction
                mcBallDirection.X = -mcBallDirection.X;

                // Add some randomness to the Balls Y direction
                float fRandomness = mcRandom.Next(-25, 25);
                fRandomness /= 100.0f;
                mcBallDirection.Normalize();
                mcBallDirection.Y += fRandomness;

                // Set the Balls speed
                mcBallDirection.Normalize();
                mcBallDirection.Scale(fSpeed);

                // Place the Ball on the Paddle (not inside it)
                mrBall.X = mrPaddle.Right;

                // Play Sound
                CFMOD.PlaySound(msSoundBallBounce, false);
            }
            // Else if the Ball hit the Left wall
            else if (rBallCollisionRect.IntersectsWith(mrLeftWall))
            {
                // Player loses a life
                miLives--;

                // If the Player is dead
                if (miLives == 0)
                {
                    // Save the Players Score
                    int iScore = miScore;

                    // Restart the Game
                    Reset();

                    // Restore the Players Score
                    miScore = iScore;

                    // Play Game Over sound
                    CFMOD.PlaySound(msSoundGameOver, false);

                    // Check if the Player has beat their High Score
                    if (CProfiles.Instance().mcCurrentProfile.iScoreSpong < miScore)
                    {
                        // Save the Players new High Score
                        CProfiles.Instance().mcCurrentProfile.iScoreSpong = miScore;
                        CProfiles.Instance().SaveProfilesToFile();
                    }

                    // Exit this function so the Game restarts now
                    return;
                }

                // Play sound of losing a Ball
                CFMOD.PlaySound(msSoundBallDied, false);

                // Reset the Balls position and direction
                ResetBall();

                // Reset the Next Ball Wait Timer
                mfNextBallWaitTime = 0.0f;

                // Enter the Wait For Next Ball state
                mePongState = EPongGameStates.WaitForNextBall;
            }

            // Update the Players Score
            miScore += (int)(fTimeSinceLastUpdateInSeconds * 1000.0f);
        }

        // Clears the scene and draws the game (Paddle, Ball, Walls, Text)
        private void DrawGame(float fTimeSinceLastUpdateInSeconds, Graphics cBackBuffer)
        {
            Brush cBrush;                               // Used to draw solid shapes

            // Keep the Paddle in bounds
            if (mrPaddle.Top < mrTopWall.Bottom)
            {
                mrPaddle.Y = mrTopWall.Bottom;
            }
            else if (mrPaddle.Bottom > mrBottomWall.Top)
            {
                mrPaddle.Y = mrBottomWall.Top - mrPaddle.Height;
            }

            // Clear the scene
            cBackBuffer.Clear(Color.Black);

            // Draw the Players Paddle
            cBackBuffer.DrawImage(mcPaddleImage, mrPaddle);

            // Draw the Ball
            cBackBuffer.DrawImage(mcBallImage, mrBall);

            // Draw the Walls
            cBrush = new SolidBrush(Color.White);
            cBackBuffer.FillRectangle(cBrush, mrLeftWall);
            cBackBuffer.FillRectangle(cBrush, mrRightWall);
            cBackBuffer.FillRectangle(cBrush, mrTopWall);
            cBackBuffer.FillRectangle(cBrush, mrBottomWall);

            // Draw the Pitch Meter
            mcPitchMeter.Draw(cBackBuffer);

            // Draw the text
            cBrush = new SolidBrush(Color.LightBlue);
            cBackBuffer.DrawString("Score " + miScore.ToString(), mcFontText, cBrush, 10, 10);
            cBackBuffer.DrawString("Balls " + miLives.ToString(), mcFontText, cBrush, 690, 10);
            cBackBuffer.DrawString("Pause - Space Bar       Restart Game - R       Main Menu - Esc", mcFontText, cBrush, 10, 520);

            // Release the Brush resource
            cBrush.Dispose();
        }

        // Displays text letting the Player know the game is Paused
        private void DisplayPause(float fTimeSinceLastUpdateInSeconds, Graphics cBackBuffer)
        {
            Brush cBrush = new SolidBrush(Color.Blue);
            string sText = "";

            // If the Game has already been started, then it is now Paused
            if (mbGameStarted)
            {
                sText = "      Paused\n\nPress Spacebar\n    to continue";
            }
            else
            {
                sText = "Press Spacebar\n      to start";
            }

            // Display that the Game is paused
            cBackBuffer.DrawString(sText, mcFontPause, cBrush, 150, 100);

            // Clean up the Brush resource
            cBrush.Dispose();
        }

        // Reset Balls position and direction, but maintain it's speed
        private void ResetBall()
        {
            // Save Balls current speed
            float fSpeed = mcBallDirection.Length();

            // Reset Balls position
            mrBall.X = 150;
            mrBall.Y = 250;

            // Reset Balls direction randomly
            mcBallDirection.X = mcRandom.Next(40, 100);
            mcBallDirection.Y = mcRandom.Next(-50, 50);
            mcBallDirection.Normalize();

            // Set Balls speed
            mcBallDirection.Scale(fSpeed);
        }

        // Handle key presses
        public override EGames KeyDown(KeyEventArgs e)
        {
            // If the Pause button was pressed
            if (e.KeyCode == Keys.Space)
            {
                // If the Game is currently Paused
                if (mePongState == EPongGameStates.Pause)
                {
                    // Switch to Playing the game
                    mePongState = EPongGameStates.Play;

                    // If the Game hasn't been started yet
                    if (!mbGameStarted)
                    {
                        // Record that the Game has been started
                        mbGameStarted = true;

                        // Reset the Score to zero
                        miScore = 0;
                    }
                }
                // Else the Game is not Paused
                else
                {
                    // So Pause the Game
                    mePongState = EPongGameStates.Pause;
                }
            }

            // If Restart was pressed
            if (e.KeyCode == Keys.R)
            {
                Reset();
            }

            // If the Mouse toggle button was pressed
            if (e.KeyCode == Keys.M)
            {
                // Toggle the Mouse use on/off
                mbMouseEnabled = !mbMouseEnabled;
            }

            return meGame;
        }

        // Handle Mouse presses
        public override EGames MouseDown(MouseEventArgs e)
        {
            EGames eGameToPlay = meGame;

            // If the right mouse button was pressed
            if (e.Button == MouseButtons.Right)
            {
                eGameToPlay = EGames.MainMenu;
            }

            return eGameToPlay;
        }

        // Handle Mouse movement
        public override void MouseMove(MouseEventArgs e)
        {
            // If we have the Mouses position last frame
            if (miMouseYLastFrame > 0)
            {
                // If the Mouse can be used
                if (mbMouseEnabled)
                {
                    // Move the Paddle by the Mouse movement amount
                    mrPaddle.Y += (e.Y - miMouseYLastFrame);
                }                
            }

            // Save the Mouse's position this frame for next frame
            miMouseYLastFrame = e.Y;
        }
    }
}
