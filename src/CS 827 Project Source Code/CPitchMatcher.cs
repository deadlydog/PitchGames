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
    class CPitchMatcher : CGame
    {
        // States which the Pitch Matcher game may be in
        enum EPitchMatcherGameStates
        {
            Play = 0,
            Pause = 1,
        }


        // Constants
        int miGAME_DURATION;                                // How long the Game should last for
        float mfMAX_TARGET_PITCH_WAIT_TIME;                 // The Max amount of Time the Target Pitch should remain constant
        float mfMAX_UNIT_PITCH_DIFFERENCE_TO_EARN_POINTS;   // The Max distance between the Target Pitch and the Player Pitch to earn points


        // Class Variables
        EPitchMatcherGameStates mePitchMatcherState;    // The current state of the Pitch Matcher game

        float mfTimeBeforeChangingTargetPitchInSeconds; // How long in seconds the Target Pitch should wait before changing
        float mfUnitTargetPitchToAchieve;               // The Target Pitch the Pitch To Match should move towards
        float mfTargetPitchChangeVelocity;              // How fast the Target Pitch To Match should move towards the UnitTargetPitchToAchieve
        float mfCurrentUnitTargetPitch;                 // Where the Target Pitch currently is

        int miScore;                        // The players Score
        bool mbGameStarted;                 // Tells if the Game has Started yet or not

        float mfGameTimeRemaining;          // How much longer the current Game will last for

        CPitchMeter mcPitchMeter;           // The Pitch Meter
        CPitchMeter mcPitchMeterToMatch;    // The Pitch Meter the Player is trying to match

        Font mcFontText;                    // The Font used to draw any text
        Font mcFontPause;                   // The Font used for the Paused text

        Random mcRandom;                    // Used to generate random numbers


        // Class constructor
        public CPitchMatcher()
        {
            try
            {
                // Pre-load images
                

                // Pre-load sounds
                
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Exception caught");
            }

            // Seed the Random number generator
            mcRandom = new Random(Environment.TickCount);

            // Setup the Pitch Meter
            mcPitchMeter = new CPitchMeter(500, 125);
            mcPitchMeter.ShowFourthsMarkers(true);

            // Setup the Pitch Meter to Match
            mcPitchMeterToMatch = new CPitchMeter(300, 125);
            mcPitchMeterToMatch.ShowFourthsMarkers(true);

            // Setup the Fonts
            mcFontText = new Font("Arial", 20, FontStyle.Regular);
            mcFontPause = new Font("Arial", 50, FontStyle.Regular);

            // Specify the Game Duration, Max Target Wait Time, and Max Pitch Difference to get Points
            miGAME_DURATION = 30;
            mfMAX_TARGET_PITCH_WAIT_TIME = 3.0f;
            mfMAX_UNIT_PITCH_DIFFERENCE_TO_EARN_POINTS = 0.25f;

            // Start the player with no Score
            miScore = 0;

            // Set the other variables default values
            Reset();
        }

        // Reset class variables to their default values
        public void Reset()
        {
            // Record that we are playing Pong
            meGame = EGames.PitchMatcher;

            // Set the games initial State
            mePitchMatcherState = EPitchMatcherGameStates.Pause;

            // Record that the Game hasn't started yet
            mbGameStarted = false;

            // Set the initial game Duration
            mfGameTimeRemaining = miGAME_DURATION;

            // Reset the Pitch To Match variables
            mfTimeBeforeChangingTargetPitchInSeconds = 0.0f;
            mfUnitTargetPitchToAchieve = 0.0f;
            mfTargetPitchChangeVelocity = 0.0f;
            mfCurrentUnitTargetPitch = 0.5f;

            // Set the initial Pitch Meter Indicator positions
            mcPitchMeter.UpdatePitchIndicatorsPosition(0.5f);
            mcPitchMeterToMatch.UpdatePitchIndicatorsPosition(0.5f);
        }

        // Do game processing and update the display
        public override void Update(float fTimeSinceLastUpdateInSeconds, Graphics cBackBuffer)
        {
            // Do actions based on which state the game is in
            switch (mePitchMatcherState)
            {
                default:
                case EPitchMatcherGameStates.Play:
                    UpdateGameObjects(fTimeSinceLastUpdateInSeconds, cBackBuffer);
                    DrawGame(fTimeSinceLastUpdateInSeconds, cBackBuffer);
                break;

                case EPitchMatcherGameStates.Pause:
                    DrawGame(fTimeSinceLastUpdateInSeconds, cBackBuffer);
                    DisplayPause(fTimeSinceLastUpdateInSeconds, cBackBuffer);
                break;
            }
        }

        // Updated the Ball and Paddle
        private void UpdateGameObjects(float fTimeSinceLastUpdateInSeconds, Graphics cBackBuffer)
        {
            // Subtract the Time Passed from the Time to Wait before changing Target Pitches
            mfTimeBeforeChangingTargetPitchInSeconds -= fTimeSinceLastUpdateInSeconds;

            // Move the Target Pitch by it's Velocity
            float fOldUnitTargetPitch = mfCurrentUnitTargetPitch;
            mfCurrentUnitTargetPitch += mfTargetPitchChangeVelocity * fTimeSinceLastUpdateInSeconds;

            // If the moving Target Pitch passed it's desired Pitch
            if (mfTargetPitchChangeVelocity != 0.0f &&
                ((fOldUnitTargetPitch <= mfUnitTargetPitchToAchieve && mfCurrentUnitTargetPitch >= mfUnitTargetPitchToAchieve) ||
                (fOldUnitTargetPitch >= mfUnitTargetPitchToAchieve && mfCurrentUnitTargetPitch <= mfUnitTargetPitchToAchieve)))
            {
                // Set the Current Target Pitch to the Pitch To Achieve
                mfCurrentUnitTargetPitch = mfUnitTargetPitchToAchieve;

                // Randomly specify how long we should wait at this Target Pitch for
                mfTimeBeforeChangingTargetPitchInSeconds = ((float)mcRandom.NextDouble() * mfMAX_TARGET_PITCH_WAIT_TIME);

                // Set the Target Pitch Velocity to zero
                mfTargetPitchChangeVelocity = 0.0f;
            }

            // If the Pitch To Match should change, AND it's not changing already
            if (mfTimeBeforeChangingTargetPitchInSeconds <= 0.0f && mfTargetPitchChangeVelocity == 0.0f)
            {
                // Specify the new Target Pitch to achieve
                mfUnitTargetPitchToAchieve = (float)mcRandom.NextDouble();

                // Specify the new Target Pitch Velocity
                mfTargetPitchChangeVelocity = (float)(mcRandom.Next(1, 10) * 0.1f);

                // If the Velocity should be negative
                if (mfUnitTargetPitchToAchieve < mfCurrentUnitTargetPitch)
                {
                    mfTargetPitchChangeVelocity *= -1.0f;
                }
            }

            // Update the Pitch Meter to Match
            mcPitchMeterToMatch.UpdatePitchIndicatorsPosition(mfCurrentUnitTargetPitch);

            // Update the Players Pitch Meter
            mcPitchMeter.AutoDetectPitchAndUpdateMeter();

            
            // Get how far away from the Target Pitch the Player is
            float fPitchDifference = Math.Abs(mcPitchMeter.mfPitchIndicatorUnitPosition - mfCurrentUnitTargetPitch);
            
            // If the Player is close enough to the Target Pitch
            if (fPitchDifference <= mfMAX_UNIT_PITCH_DIFFERENCE_TO_EARN_POINTS)
            {
                // Give the Player points based on how close they are to the Target Pitch
                miScore += (int)(Math.Abs(fPitchDifference - mfMAX_UNIT_PITCH_DIFFERENCE_TO_EARN_POINTS) * 1000);
            }


            // Subtract the Time passed from the Remaining Game Time
            mfGameTimeRemaining -= fTimeSinceLastUpdateInSeconds;

            // If the Game should be over
            if (mfGameTimeRemaining < 0.0f)
            {
                // If the Player beat their old High Score
                if (miScore > CProfiles.Instance().mcCurrentProfile.iScorePitchMatcher)
                {
                    // Update and save the Players new High Score
                    CProfiles.Instance().mcCurrentProfile.iScorePitchMatcher = miScore;
                    CProfiles.Instance().SaveProfilesToFile();
                }

                // Restart the Game
                Reset();
            }
        }

        // Clears the scene and draws the game (Paddle, Ball, Walls, Text)
        private void DrawGame(float fTimeSinceLastUpdateInSeconds, Graphics cBackBuffer)
        {
            Brush cBrush;   // Used to draw solid shapes

            // Clear the scene
            cBackBuffer.Clear(Color.Black);

            // Draw the Pitch Meter            
            mcPitchMeter.Draw(cBackBuffer);

            // Draw the Pitch Meter To Match
            mcPitchMeterToMatch.Draw(cBackBuffer);


            // Display to press Space Bar to Pause the game
            cBrush = new SolidBrush(Color.LightBlue);
            cBackBuffer.DrawString("Pause - Space Bar", mcFontText, cBrush, 475, 530);

            // Display to press R to Restart the game
            cBackBuffer.DrawString("Restart - R", mcFontText, cBrush, 150, 530);

            // Display the players Score
            cBackBuffer.DrawString("Score " + miScore, mcFontText, cBrush, 10, 10);

            // Display how to earn points
            cBackBuffer.DrawString("Match your pitch closer to the Target pitch to earn more points", mcFontText, cBrush, 10, 50);

            // Display how many seconds remain in the game
            cBackBuffer.DrawString("Time " + mfGameTimeRemaining.ToString("#"), mcFontText, cBrush, 100, 260);

            // Release the Brush resource
            cBrush.Dispose();
        }

        // Displays text letting the Player know the game is Paused
        private void DisplayPause(float fTimeSinceLastUpdateInSeconds, Graphics cBackBuffer)
        {
            Brush cBrush = new SolidBrush(Color.Blue);

            // If the Game was Started already
            if (mbGameStarted)
            {
                // Display that the Game is paused
                cBackBuffer.DrawString("      Paused\n\nPress Spacebar\n    to continue", mcFontPause, cBrush, 150, 100);
            }
            // Else the Game hasn't been Started yet
            else
            {
                // Display to press the Spacebar to Start the Game
                cBackBuffer.DrawString("Press Spacebar\n      to begin", mcFontPause, cBrush, 150, 100);
            }

            // Clean up the Brush resource
            cBrush.Dispose();
        }

        // Handle key presses
        public override EGames KeyDown(KeyEventArgs e)
        {
            // If the Pause button was pressed
            if (e.KeyCode == Keys.Space)
            {
                // If the Game is currently Paused
                if (mePitchMatcherState == EPitchMatcherGameStates.Pause)
                {
                    // Switch to Playing the game
                    mePitchMatcherState = EPitchMatcherGameStates.Play;

                    // If the Game hadn't started yet
                    if (!mbGameStarted)
                    {
                        // Record that the Game has now Started
                        mbGameStarted = true;

                        // Start the player with no Score
                        miScore = 0;
                    }
                }
                // Else the Game is not Paused
                else
                {
                    // So Pause the Game
                    mePitchMatcherState = EPitchMatcherGameStates.Pause;
                }
            }

            // If Restart was pressed
            if (e.KeyCode == Keys.R)
            {
                Reset();
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
    }
}
