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
    class CTargetPractice : CGame
    {
        // States which the Pitch Matcher game may be in
        enum ETargetPracticeGameStates
        {
            Aiming = 0,
            FiredArrow = 1
        }


        // Constants
        Vector2 mcGRAVITY_ACCELERATION;     // The force of Gravity on the arrow
        Rectangle mrBOW_AND_ARROW_POSITION; // The Position of the Bow And Arrow


        // Class Variables
        ETargetPracticeGameStates meTargetPracticeState;    // The current state of the Pitch Matcher game

        int miScore;                        // The players Score

        CPitchMeter mcPitchMeter;           // The Pitch Meter

        Image mcBowAndArrowImage;           // Holds the Image of the Bow And Arrow
        Image mcArrowImage;                 // Holds just the Image of the Arrow
        Image mcTargetImage;                // Holds the Image of the Target

        Font mcFontText;                    // The Font used to draw any text
        Font mcFontLarge;                   // The Font used for large text

        Random mcRandom;                    // Used to generate random numbers

        RectangleF mrArrow;                 // The Position of the Arrow
        Rectangle mrTarget;                 // The Position of the Target

        Vector2 mcArrowVelocity;            // The Velocity of the fired Arrow
        float mfUnitBowAndArrowPower;       // The amount of Power being used to fire the Arrow

        int miBowAndArrowRotation;          // The amount the Bow And Arrow should be Rotated
        int miArrowRotation;                // The amount the Arrow should be Rotated

        float mfResetArrowWaitTime;         // How long it has been since the Arrow "died"
        bool mbArrowHitTarget;              // Tells if the Arrow hit the Target or not

        // Sounds and FMOD variables
        FMOD.Sound msSoundHit = null;       // Sound when the player Hits the Target
        FMOD.Sound msSoundMiss = null;      // Sound when the player Misses the Target
        FMOD.Sound msSoundFire = null;      // Sound when the player Fires the Arrow

        // Class constructor
        public CTargetPractice()
        {
            try
            {
                // Pre-load images
                mcBowAndArrowImage = Image.FromFile("../../Data/Images/TargetPracticeBowAndArrow.png");
                mcArrowImage = Image.FromFile("../../Data/Images/TargetPracticeArrow.png");
                mcTargetImage = Image.FromFile("../../Data/Images/TargetPracticeTarget.png");

                // Pre-load sounds
                CFMOD.CheckResult(CFMOD.GetSystem().createSound("../../Data/Sounds/TargetPracticeHit.wav", FMOD.MODE.DEFAULT, ref msSoundHit), "FMOD Create Sound Error");
                CFMOD.CheckResult(CFMOD.GetSystem().createSound("../../Data/Sounds/TargetPracticeMiss.wav", FMOD.MODE.DEFAULT, ref msSoundMiss), "FMOD Create Sound Error");
                CFMOD.CheckResult(CFMOD.GetSystem().createSound("../../Data/Sounds/TargetPracticeFire.wav", FMOD.MODE.DEFAULT, ref msSoundFire), "FMOD Create Sound Error");
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

            // Setup the Fonts
            mcFontText = new Font("Arial", 20, FontStyle.Regular);
            mcFontLarge = new Font("Arial", 50, FontStyle.Regular);

            // Start the player with no Score
            miScore = 0;

            // Record that we are playing Pong
            meGame = EGames.TargetPractice;

            // Set the games initial State
            meTargetPracticeState = ETargetPracticeGameStates.Aiming;

            // Set the initial Pitch Meter Indicator positions
            mcPitchMeter.UpdatePitchIndicatorsPosition(0.0f);

            // Initialize the constants
            mcGRAVITY_ACCELERATION = new Vector2(0.0f, 65.0f);
            mrBOW_AND_ARROW_POSITION = new Rectangle(10, 400, 60, 69);

            // Specify the initial Target and Arrow positions
            mrTarget = new Rectangle(650, 250, 128, 128);
            mrArrow = new RectangleF(10, 10, 60, 11);
            MoveTargetToNewRandomLocation();
            MoveArrowToBow();

            // Set the Arrows initial Velocity and the initial Power
            mcArrowVelocity = new Vector2();
            mfUnitBowAndArrowPower = 0.0f;

            // Set the initial Bow And Arrow and Arrow Rotations
            miBowAndArrowRotation = miArrowRotation = 0;

            // Initialize the Reset Arrow Wait Time
            mfResetArrowWaitTime = 0.0f;

            // Initialize the Arrow Hit Target variable
            mbArrowHitTarget = false;
        }

        // Do game processing and update the display
        public override void Update(float fTimeSinceLastUpdateInSeconds, Graphics cBackBuffer)
        {
            // Update the Game Objects and Draw the Game
            UpdateGameObjects(fTimeSinceLastUpdateInSeconds, cBackBuffer);
            DrawGame(fTimeSinceLastUpdateInSeconds, cBackBuffer);
        }

        // Update the Bow And Arrow and the Arrow
        private void UpdateGameObjects(float fTimeSinceLastUpdateInSeconds, Graphics cBackBuffer)
        {
            // If the Player is Aiming
            if (meTargetPracticeState == ETargetPracticeGameStates.Aiming)
            {
                // Use the Players current Pitch as the Bow And Arrow Rotation Amount
                mcPitchMeter.AutoDetectPitchAndUpdateMeter();
                miBowAndArrowRotation = (int)(mcPitchMeter.mfPitchIndicatorUnitPosition * -45.0f);

                // Save the amount of Power being used
                mfUnitBowAndArrowPower = ((CSoundInput.Instance().fAmplitude - 90.0f) / 20.0f);

                // Make sure the Power is in a valid range of Zero to One
                if (mfUnitBowAndArrowPower < 0.0f)
                {
                    mfUnitBowAndArrowPower = 0.0f;
                }
                else if (mfUnitBowAndArrowPower > 1.0f)
                {
                    mfUnitBowAndArrowPower = 1.0f;
                }
            }
            // Else the Arrow has been fired
            else
            {
                // If the Arrow is not "dead" yet
                if (mfResetArrowWaitTime <= 0.0f)
                {
                    // Update the Arrows Velocity by Gravity
                    mcArrowVelocity += (mcGRAVITY_ACCELERATION * fTimeSinceLastUpdateInSeconds);

                    // Calculate the Arrows new Position
                    mrArrow.X += (mcArrowVelocity.X * fTimeSinceLastUpdateInSeconds);
                    mrArrow.Y += (mcArrowVelocity.Y * fTimeSinceLastUpdateInSeconds);

                    // Get the Rotation angle of the Arrow so that it faces the direction it is travelling
                    float fArrowRotationInRadians = 0.0f;
                    bool bChangeRotation = true;
                    if (mcArrowVelocity.Y < 0.0f)
                    {
                        fArrowRotationInRadians = (float)(Math.PI + Math.Atan(mcArrowVelocity.X / mcArrowVelocity.Y));
                    }
                    else if (mcArrowVelocity.Y > 0.0f)
                    {
                        fArrowRotationInRadians = (float)(Math.Atan(mcArrowVelocity.X / mcArrowVelocity.Y));
                    }
                    // Else the Arrow has no Y Velocity
                    else
                    {
                        if (mcArrowVelocity.X > 0.0f)
                        {
                            fArrowRotationInRadians = (float)(Math.PI / 2.0f);
                        }
                        else if (mcArrowVelocity.X < 0.0f)
                        {
                            fArrowRotationInRadians = (float)((3.0f * Math.PI) / 2.0f);
                        }
                        // Else the Arrow has no X Velocity
                        else
                        {
                            // Rotation is undefined, so leave rotationa as is
                            bChangeRotation = false;
                        }
                    }

                    // If the Rotation should be updated
                    if (bChangeRotation)
                    {
                        // Convert the Radians to Degrees and store the result
                        miArrowRotation = 90 - (int)(fArrowRotationInRadians * (180.0f / Math.PI));
                    }
                }

                // If the Arrow should be reset
                if (mrArrow.IntersectsWith(mrTarget) || mrArrow.X > 800 || mrArrow.Y > 485)
                {
                    // If the Arrow just "died"
                    if (mfResetArrowWaitTime == 0.0f)
                    {
                        // If the Arrow hit the Target
                        if (mrArrow.IntersectsWith(mrTarget))
                        {
                            // Record it as a hit
                            mbArrowHitTarget = true;

                            // Update the Players Score
                            miScore++;

                            // If the Players Score is more than their current High Score
                            if (miScore > CProfiles.Instance().mcCurrentProfile.iScoreTargetPractice)
                            {
                                // Update the Players High Score and Save
                                CProfiles.Instance().mcCurrentProfile.iScoreTargetPractice = miScore;
                                CProfiles.Instance().SaveProfilesToFile();
                            }

                            // Play the Hit sound
                            CFMOD.PlaySound(msSoundHit, false);
                        }
                        // Else if the Arrow did not hit anything
                        else if (mrArrow.X > 800 || mrArrow.Y > 485)
                        {
                            // Record that it was not a hit
                            mbArrowHitTarget = false;

                            // Reset the Players Score
                            miScore = 0;

                            // Play the Miss sound
                            CFMOD.PlaySound(msSoundMiss, false);
                        }

                        // Reset the Arrows Velocity to zero
                        mcArrowVelocity *= 0.0f;
                    }

                    // Update how long we have waited for
                    mfResetArrowWaitTime += fTimeSinceLastUpdateInSeconds;

                    // If we have waited long enough since the Arrow "died"
                    if (mfResetArrowWaitTime > 1.0f)
                    {
                        // Reset the Arrows Position
                        MoveArrowToBow();

                        // If the Arrow hit the Target
                        if (mbArrowHitTarget)
                        {
                            // Create a new Target to hit
                            MoveTargetToNewRandomLocation();
                        }

                        // Reset the Arrow Wait Time
                        mfResetArrowWaitTime = 0.0f;

                        // Change the Game State to Aiming
                        meTargetPracticeState = ETargetPracticeGameStates.Aiming;
                    }
                }
            }
        }

        // Clears the scene and draws the game (Paddle, Ball, Walls, Text)
        private void DrawGame(float fTimeSinceLastUpdateInSeconds, Graphics cBackBuffer)
        {
            Brush cBrush;   // Used to draw solid shapes
            Pen cPen;       // Used to draw outlines of shapes

            // Clear the scene
            cBackBuffer.Clear(Color.Black);

            // Draw the Target
            cBackBuffer.DrawImage(mcTargetImage, mrTarget);


            // Find the Middle Point of the Bow And Arrow to Rotate around
            PointF sMiddlePoint = new Point();
            sMiddlePoint.X = mrBOW_AND_ARROW_POSITION.X + (mrBOW_AND_ARROW_POSITION.Width / 2.0f);
            sMiddlePoint.Y = mrBOW_AND_ARROW_POSITION.Y + (mrBOW_AND_ARROW_POSITION.Height / 2.0f);

            // Rotate the Bow And Arrow by the specified Bow And Arrow Rotation Amount
            System.Drawing.Drawing2D.Matrix sRotationMatrix = new System.Drawing.Drawing2D.Matrix(1, 0, 0, 1, 0, 0);
            sRotationMatrix.RotateAt(miBowAndArrowRotation, sMiddlePoint);
            cBackBuffer.Transform = sRotationMatrix;

            // Draw the Bow And Arrow with the Rotation applied
            cBackBuffer.DrawImageUnscaled(mcBowAndArrowImage, mrBOW_AND_ARROW_POSITION);


            // If the Arrow has been fired
            if (meTargetPracticeState == ETargetPracticeGameStates.FiredArrow)
            {
                // Find the Middle Point of the Arrow to Rotate around
                sMiddlePoint.X = mrArrow.X + (mrArrow.Width / 2.0f);
                sMiddlePoint.Y = mrArrow.Y + (mrArrow.Height / 2.0f);

                // Rotate theArrow by the specified Arrow Rotation Amount
                sRotationMatrix = new System.Drawing.Drawing2D.Matrix(1, 0, 0, 1, 0, 0);
                sRotationMatrix.RotateAt(miArrowRotation, sMiddlePoint);
                cBackBuffer.Transform = sRotationMatrix;

                // Draw the Bow And Arrow with the Rotation applied
                cBackBuffer.DrawImageUnscaled(mcArrowImage, (int)mrArrow.X, (int)mrArrow.Y);
            }

            // Reset the World Transformation Matrix to get rid of Rotation
            cBackBuffer.Transform = new System.Drawing.Drawing2D.Matrix();

            // Draw the Power Meter
            Rectangle rPowerMeterFill = new Rectangle(mrBOW_AND_ARROW_POSITION.X + 1, mrBOW_AND_ARROW_POSITION.Y + mrBOW_AND_ARROW_POSITION.Height + 31, 199, 24);
            cPen = new Pen(Color.LightBlue);
            cBrush = new LinearGradientBrush(rPowerMeterFill, Color.Yellow, Color.Red, LinearGradientMode.Horizontal);
            
            // Draw Power Meter outline
            cBackBuffer.DrawRectangle(cPen, rPowerMeterFill.X - 1, rPowerMeterFill.Y - 1, rPowerMeterFill.Width + 1, rPowerMeterFill.Height + 1);
            
            // Draw the Power Meter Gradient
            cBackBuffer.FillRectangle(cBrush, rPowerMeterFill);

            // Cover up some of the Power Meter Gradient to show the Power being used
            cBrush = new SolidBrush(Color.Black);
            int iBlackFillWidth = (int)(rPowerMeterFill.Width * (1.0f - mfUnitBowAndArrowPower));
            Rectangle rPowerMeterBlackFill = new Rectangle(rPowerMeterFill.X + (rPowerMeterFill.Width - iBlackFillWidth), rPowerMeterFill.Y, iBlackFillWidth, 24);

            // If the Black Fill has a Width
            if (rPowerMeterBlackFill.Width > 0)
            {
                // Draw the Black Fill overtop of the gradient
                cBackBuffer.FillRectangle(cBrush, rPowerMeterBlackFill);
            }


            // Display to press Space Bar to Pause the game
            cBrush = new SolidBrush(Color.LightBlue);
            cBackBuffer.DrawString("Shoot - Space Bar / Left Mouse", mcFontText, cBrush, 75, 530);

            // Display th press N for a New Target
            cBackBuffer.DrawString("New Target - N", mcFontText, cBrush, 550, 530);

            // Display the players Score
            cBackBuffer.DrawString("Targets Hit In A Row " + miScore, mcFontText, cBrush, 10, 10);

            // Display the players current High Score
            cBackBuffer.DrawString("Your High Score " + CProfiles.Instance().mcCurrentProfile.iScoreTargetPractice.ToString(), mcFontText, cBrush, 500, 10);

            // If the Arrow is "dead"
            if (mfResetArrowWaitTime > 0.0f)
            {
                string sResultText = "";

                // If it Hit the Target
                if (mbArrowHitTarget)
                {
                    sResultText = "HIT";
                }
                // Else it missed the Target
                else
                {
                    sResultText = "MISS";
                }

                // Display if the Player hit the Target or not
                cBackBuffer.DrawString(sResultText, mcFontLarge, cBrush, 300, 200);
            }

            // Release the Brush resource
            cBrush.Dispose();
        }

        // Function to Move the Target to a new Random location
        private void MoveTargetToNewRandomLocation()
        {
            mrTarget.Y = mcRandom.Next(50, 400);
        }

        // Function to Move the Arrow to be on the Bow
        private void MoveArrowToBow()
        {
            mrArrow.X = mrBOW_AND_ARROW_POSITION.X;
            mrArrow.Y = mrBOW_AND_ARROW_POSITION.Y + 40;

            // Reset the Bow orientation
            mcPitchMeter.UpdatePitchIndicatorsPosition(0.0f);
        }

        // Function to Fire the Arrow
        private void FireArrow()
        {
            // If the Player is currently Aiming
            if (meTargetPracticeState == ETargetPracticeGameStates.Aiming)
            {
                // Switch to Shooting the Arrow
                meTargetPracticeState = ETargetPracticeGameStates.FiredArrow;

                // Set the Arrows Velocity based on the current Pitch and Amplitude of the Players voice

                // Find the Middle Point of the Bow And Arrow to Rotate around
                PointF sMiddlePoint = new Point();
                sMiddlePoint.X = mrBOW_AND_ARROW_POSITION.X + (mrBOW_AND_ARROW_POSITION.Width / 2.0f);
                sMiddlePoint.Y = mrBOW_AND_ARROW_POSITION.Y + (mrBOW_AND_ARROW_POSITION.Height / 2.0f);

                // Rotate the Bow And Arrow by the specified Bow And Arrow Rotation Amount
                System.Drawing.Drawing2D.Matrix sRotationMatrix = new System.Drawing.Drawing2D.Matrix(1, 0, 0, 1, 0, 0);
                sRotationMatrix.RotateAt(miBowAndArrowRotation, sMiddlePoint);
                PointF[] sPoint = new PointF[1];
                sPoint[0].X = sMiddlePoint.X + 1;
                sPoint[0].Y = sMiddlePoint.Y;
                sRotationMatrix.TransformPoints(sPoint);

                // Set the Arrows initial Direction to travel
                mcArrowVelocity.X = sPoint[0].X - sMiddlePoint.X;
                mcArrowVelocity.Y = sPoint[0].Y - sMiddlePoint.Y;
                mcArrowVelocity.Normalize();

                // Set the Arrows initial speed
                mcArrowVelocity *= (mfUnitBowAndArrowPower * 300.0f);
                mcArrowVelocity.X += 100.0f;

                // Set the Arrows Rotation to be the same as the Bow And Arrow initially
                miArrowRotation = miBowAndArrowRotation;

                // Play the Fire Arrow sound
                CFMOD.PlaySound(msSoundFire, false);
            }
        }

        // Handle key presses
        public override EGames KeyDown(KeyEventArgs e)
        {
            // If the Fire button was pressed
            if (e.KeyCode == Keys.Space)
            {
                // Fire the Arrow
                FireArrow();
            }

            // If the New Target button was pressed
            if (e.KeyCode == Keys.N)
            {
                // Move the Target
                MoveTargetToNewRandomLocation();
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
            // Else if the Left mouse button was pressed
            else if (e.Button == MouseButtons.Left)
            {
                // Fire the Arrow
                FireArrow();
            }

            return eGameToPlay;
        }
    }
}
