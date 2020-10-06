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
    class CHighScores : CGame
    {
        // Constants


        // Class Variables
        Font mcFontTitle;                           // The Font used to draw the Title
        Font mcFontText;                            // The Font used to draw any text

        int miSpongHighScore;                       // The Highest Score in Spong
        string msSpongHighScoreName;                // The Profile Name of the Highest Score holder

        int miShootingGallerySong1HighScore;        // The Highest Score in Shooting Gallery Song 1
        string msShootingGallerySong1HighScoreName; // The Profile Name of the Highest Score holder

        int miShootingGallerySong2HighScore;        // The Highest Score in Shooting Gallery Song 2
        string msShootingGallerySong2HighScoreName; // The Profile Name of the Highest Score holder

        int miShootingGallerySong3HighScore;        // The Highest Score in Shooting Gallery Song 3
        string msShootingGallerySong3HighScoreName; // The Profile Name of the Highest Score holder

        int miPitchMatcherHighScore;                // The Highest Score in Pitch Matcher
        string msPitchMatcherHighScoreName;         // The Profile Name of the Highest Score holder

        int miTargetPracticeHighScore;              // The Highest Score in Target Practice
        string msTargetPracticeHighScoreName;       // The Profile Name of the Highest Score holder


        // Class constructor
        public CHighScores()
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

            // Setup the Fonts
            mcFontTitle = new Font("Arial", 30, FontStyle.Regular);
            mcFontText = new Font("Arial", 20, FontStyle.Regular);

            // Record that we are playing Pong
            meGame = EGames.HighScores;

            // Initialize all of the High Scores
            miSpongHighScore = miPitchMatcherHighScore = miTargetPracticeHighScore = 0;
            miShootingGallerySong1HighScore = miShootingGallerySong2HighScore = miShootingGallerySong3HighScore = 0;

            // Find and store the global High Scores
            CProfiles.Instance().mcProfileList.ForEach(delegate(CProfile cProfile)
            {
                // If this Profiles Spong Score is better than the current High Score
                if (cProfile.iScoreSpong >= miSpongHighScore)
                {
                    // Save this Profile as the High Score holder for now
                    miSpongHighScore = cProfile.iScoreSpong;
                    msSpongHighScoreName = cProfile.sName;
                }

                // If this Profiles Shooting Gallery Song 1 Score is better than the current High Score
                if (cProfile.iScoreShootingGallerySong1 >= miShootingGallerySong1HighScore)
                {
                    // Save this Profile as the High Score holder for now
                    miShootingGallerySong1HighScore = cProfile.iScoreShootingGallerySong1;
                    msShootingGallerySong1HighScoreName = cProfile.sName;
                }

                // If this Profiles Shooting Gallery Song 2 Score is better than the current High Score
                if (cProfile.iScoreShootingGallerySong2 >= miShootingGallerySong2HighScore)
                {
                    // Save this Profile as the High Score holder for now
                    miShootingGallerySong2HighScore = cProfile.iScoreShootingGallerySong2;
                    msShootingGallerySong2HighScoreName = cProfile.sName;
                }

                // If this Profiles Shooting Gallery Song 2 Score is better than the current High Score
                if (cProfile.iScoreShootingGallerySong2 >= miShootingGallerySong1HighScore)
                {
                    // Save this Profile as the High Score holder for now
                    miShootingGallerySong2HighScore = cProfile.iScoreShootingGallerySong2;
                    msShootingGallerySong3HighScoreName = cProfile.sName;
                }

                // If this Profiles Pitch Matcher Score is better than the current High Score
                if (cProfile.iScorePitchMatcher >= miPitchMatcherHighScore)
                {
                    // Save this Profile as the High Score holder for now
                    miPitchMatcherHighScore = cProfile.iScorePitchMatcher;
                    msPitchMatcherHighScoreName = cProfile.sName;
                }

                // If this Profiles Target Practice Score is better than the current High Score
                if (cProfile.iScoreTargetPractice >= miTargetPracticeHighScore)
                {
                    // Save this Profile as the High Score holder for now
                    miTargetPracticeHighScore = cProfile.iScoreTargetPractice;
                    msTargetPracticeHighScoreName = cProfile.sName;
                }
            });
        }

        // Do game processing and update the display
        public override void Update(float fTimeSinceLastUpdateInSeconds, Graphics cBackBuffer)
        {
            Brush cBrush;           // Used to draw text
            Brush cGameNameBrush;   // Used to draw the Name of the Games text

            // Save a handle to the Current Profile for readability
            CProfile cProfile = CProfiles.Instance().mcCurrentProfile;

            // Clear the scene
            cBackBuffer.Clear(Color.Black);

            // Display the High Scores title and the Players High Scores title
            cBrush = new SolidBrush(Color.RoyalBlue);
            cBackBuffer.DrawString("High Scores", mcFontTitle, cBrush, 250, 50);
            cBackBuffer.DrawString("      Your\nHigh Scores", mcFontTitle, cBrush, 550, 10);

            // Change the Brush Color
            cBrush = new SolidBrush(Color.LightBlue);
            cGameNameBrush = new SolidBrush(Color.LightGreen);

            // Display the Spong Scores
            cBackBuffer.DrawString("Spong", mcFontText, cGameNameBrush, 10, 110);
            cBackBuffer.DrawString(msSpongHighScoreName, mcFontText, cBrush, 250, 110);
            cBackBuffer.DrawString(miSpongHighScore.ToString(), mcFontText, cBrush, 325, 140);
            cBackBuffer.DrawString(cProfile.iScoreSpong.ToString(), mcFontText, cBrush, 625, 140);

            // Display the Shooting Gallery Song 1 Scores
            cBackBuffer.DrawString("Shooting Gallery", mcFontText, cGameNameBrush, 10, 180);
            cBackBuffer.DrawString("Song 1", mcFontText, cGameNameBrush, 50, 230);
            cBackBuffer.DrawString(msShootingGallerySong1HighScoreName, mcFontText, cBrush, 250, 230);
            cBackBuffer.DrawString(miShootingGallerySong1HighScore.ToString(), mcFontText, cBrush, 325, 260);
            cBackBuffer.DrawString(cProfile.iScoreShootingGallerySong1.ToString(), mcFontText, cBrush, 625, 260);

            // Display the Shooting Gallery Song 2 Scores
            cBackBuffer.DrawString("Song 2", mcFontText, cGameNameBrush, 50, 290);
            cBackBuffer.DrawString(msShootingGallerySong2HighScoreName, mcFontText, cBrush, 250, 290);
            cBackBuffer.DrawString(miShootingGallerySong2HighScore.ToString(), mcFontText, cBrush, 325, 320);
            cBackBuffer.DrawString(cProfile.iScoreShootingGallerySong2.ToString(), mcFontText, cBrush, 625, 320);

            // Display the Shooting Gallery Song 3 Scores
            cBackBuffer.DrawString("Song 3", mcFontText, cGameNameBrush, 50, 350);
            cBackBuffer.DrawString(msShootingGallerySong3HighScoreName, mcFontText, cBrush, 250, 350);
            cBackBuffer.DrawString(miShootingGallerySong3HighScore.ToString(), mcFontText, cBrush, 325, 380);
            cBackBuffer.DrawString(cProfile.iScoreShootingGallerySong3.ToString(), mcFontText, cBrush, 625, 380);

            // Display the Pitch Matcher Scores
            cBackBuffer.DrawString("Pitch Matcher", mcFontText, cGameNameBrush, 10, 420);
            cBackBuffer.DrawString(msPitchMatcherHighScoreName, mcFontText, cBrush, 250, 420);
            cBackBuffer.DrawString(miPitchMatcherHighScore.ToString(), mcFontText, cBrush, 325, 450);
            cBackBuffer.DrawString(cProfile.iScorePitchMatcher.ToString(), mcFontText, cBrush, 625, 450);

            // Display the Target Practice Scores
            cBackBuffer.DrawString("Target Practice", mcFontText, cGameNameBrush, 10, 490);
            cBackBuffer.DrawString(msTargetPracticeHighScoreName, mcFontText, cBrush, 250, 490);
            cBackBuffer.DrawString(miTargetPracticeHighScore.ToString(), mcFontText, cBrush, 325, 520);
            cBackBuffer.DrawString(cProfile.iScoreTargetPractice.ToString(), mcFontText, cBrush, 625, 520);

            // Release the Brush resources
            cBrush.Dispose();
            cGameNameBrush.Dispose();
        }

        // Handle key presses
        public override EGames KeyDown(KeyEventArgs e)
        {
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
