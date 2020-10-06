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
    class CMainMenu : CGame
    {
        // Class Variables
        EGames meSelectedGame;              // The Game which is currently Selected
        float mfSelectedGameScale;          // How much to grow/shrink the Selected Games icon
        bool mbSelectedGameScaleGrow;       // Tells if the Games icon should be grown or shrunk

        Image mcBackgroundImage;            // Main Menus Background image
        Image mcPongImage;                  // Pongs icon image
        Image mcShootingGalleryImage;       // Shooting Gallery icon image
        Image mcPitchMatcherImage;          // Pitch Matcher icon image
        Image mcTargetPracticeImage;        // Target Practice icon image
        Image mcProfileSettingsImage;       // Profile Settings icon image
        Image mcHighScoresImage;            // High Scores icon image

        Rectangle mrPongIcon;               // Pong game icon position and size
        Rectangle mrShootingGalleryIcon;    // Shooting Gallery game icon position and size
        Rectangle mrPitchMatcherIcon;       // Pitch Matcher game icon position and size
        Rectangle mrTargetPracticeIcon;     // Target Practice game icon position and size
        Rectangle mrProfileSettingsIcon;    // Profile Settings icon position and size
        Rectangle mrHighScoresIcon;         // The High Scores icon position and size
        Rectangle mrToggleSoundArea;        // The clickable area to toggle the sound on/off

        Font mcFontText;                    // The Font used to draw the text

        FMOD.Sound msSoundSelectionChanged; // Sound to play when the Menu Selection is changed

        // Class constructor
        public CMainMenu()
        {
            try
            {
                // Pre-load the images
                mcBackgroundImage = Image.FromFile("../../Data/Images/MainMenuBackground.png");
                mcPongImage = Image.FromFile("../../Data/Images/MainMenuPong.png");
                mcShootingGalleryImage = Image.FromFile("../../Data/Images/MainMenuShootingGallery.png");
                mcPitchMatcherImage = Image.FromFile("../../Data/Images/MainMenuPitchMatcher.png");
                mcTargetPracticeImage = Image.FromFile("../../Data/Images/MainMenuTargetPractice.png");
                mcProfileSettingsImage = Image.FromFile("../../Data/Images/MainMenuProfileSettings.png");
                mcHighScoresImage = Image.FromFile("../../Data/Images/MainMenuHighScores.png");

                // Pre-load the sounds
                CFMOD.CheckResult(CFMOD.GetSystem().createSound("../../Data/Sounds/MenuSelectionChanged.wav", FMOD.MODE.DEFAULT, ref msSoundSelectionChanged), "FMOD Create Sound Error");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Exception caught");
            }

            // Setup the Font to use to display the current Profile
            mcFontText = new Font("Arial", 14, FontStyle.Regular);

            // Set the clickable area to toggle the Sound on/off
            mrToggleSoundArea = new Rectangle(640, 530, 160, 50); 

            // Setup default variable values
            Reset();
        }

        // Reset class variables back to default values
        public void Reset()
        {
            // Record that we are in the Main Menu
            meGame = EGames.MainMenu;

            // Select Pong by default
            meSelectedGame = EGames.Spong;
            
            // Set the grow/shrink variables
            mfSelectedGameScale = 0.0f;
            mbSelectedGameScaleGrow = true;
        }

        // Update the Main Menu display
        public override void Update(float fTimeSinceLastUpdateInSeconds, Graphics cBackBuffer)
        {
            // Brush and Pen to draw with
            Brush cBrush;
            Pen cPen;

            // Reset the Game Icon position and sizes
            mrPongIcon = new Rectangle(25, 250, 128, 128);
            mrShootingGalleryIcon = new Rectangle(150, 400, 128, 128);
            mrPitchMatcherIcon = new Rectangle(275, 250, 128, 128);
            mrTargetPracticeIcon = new Rectangle(400, 400, 128, 128);
            mrProfileSettingsIcon = new Rectangle(525, 250, 128, 128);
            mrHighScoresIcon = new Rectangle(650, 400, 128, 128);

            // If the Selected Game icon should grow
            if (mbSelectedGameScaleGrow)
            {
                // Grow at a rate of 20% per second
                mfSelectedGameScale += (0.2f * fTimeSinceLastUpdateInSeconds);

                // If the Image has grown all the way
                if (mfSelectedGameScale >= 0.1f)
                {
                    mfSelectedGameScale = 0.1f;
                    mbSelectedGameScaleGrow = false;
                }
            }
            // Else it should shrink
            else
            {
                // Shrink at a rate of 20% per second
                mfSelectedGameScale -= (0.2f * fTimeSinceLastUpdateInSeconds);

                // If the Image has been shrunk all the way
                if (mfSelectedGameScale <= -0.1f)
                {
                    mfSelectedGameScale = -0.1f;
                    mbSelectedGameScaleGrow = true;
                }
            }

            // Grow/Shrink the Selected Game icon
            switch (meSelectedGame)
            {
                default:
                case EGames.Spong:
                    mrPongIcon.Inflate((int)(mfSelectedGameScale * mrPongIcon.Width), (int)(mfSelectedGameScale * mrPongIcon.Height));
                break;

                case EGames.ShootingGallery:
                    mrShootingGalleryIcon.Inflate((int)(mfSelectedGameScale * mrShootingGalleryIcon.Width), (int)(mfSelectedGameScale * mrShootingGalleryIcon.Height));
                break;

                case EGames.PitchMatcher:
                    mrPitchMatcherIcon.Inflate((int)(mfSelectedGameScale * mrPitchMatcherIcon.Width), (int)(mfSelectedGameScale * mrPitchMatcherIcon.Height));
                break;

                case EGames.TargetPractice:
                    mrTargetPracticeIcon.Inflate((int)(mfSelectedGameScale * mrTargetPracticeIcon.Width), (int)(mfSelectedGameScale * mrTargetPracticeIcon.Height));
                break;

                case EGames.ProfileSettings:
                    mrProfileSettingsIcon.Inflate((int)(mfSelectedGameScale * mrProfileSettingsIcon.Width), (int)(mfSelectedGameScale * mrProfileSettingsIcon.Height));
                break;

                case EGames.HighScores:
                    mrHighScoresIcon.Inflate((int)(mfSelectedGameScale * mrHighScoresIcon.Width), (int)(mfSelectedGameScale * mrHighScoresIcon.Height));
                break;
            }

            // Clear the Drawing Surface
            cBackBuffer.Clear(Color.Black);

            // Draw the Main Menu background
            cBackBuffer.DrawImage(mcBackgroundImage, 0, 0, 800, 600);

            // Draw Pong game icon
            cBackBuffer.DrawImage(mcPongImage, mrPongIcon);

            // Draw Shooting Gallery game icon
            cBackBuffer.DrawImage(mcShootingGalleryImage, mrShootingGalleryIcon);

            // Draw Pitch Matcher game icon
            cBackBuffer.DrawImage(mcPitchMatcherImage, mrPitchMatcherIcon);

            // Draw Target Practice game icon
            cBackBuffer.DrawImage(mcTargetPracticeImage, mrTargetPracticeIcon);

            // Draw the Profile Settings icon
            cBackBuffer.DrawImage(mcProfileSettingsImage, mrProfileSettingsIcon);

            // Draw the High Scores icon
            cBackBuffer.DrawImage(mcHighScoresImage, mrHighScoresIcon);

            // Display which Profile is currently being used
            cBrush = new SolidBrush(Color.WhiteSmoke);
            string sWelcome = "Profile: " + CProfiles.Instance().mcCurrentProfile.sName;
            cBackBuffer.DrawString(sWelcome, mcFontText, cBrush, 5, 3);
            
            // Display the Toggle Sound option
            cPen = new Pen(Color.WhiteSmoke);
            string sPlaySounds = "Play Sounds ";
            if (CFMOD.mbSoundOn){ sPlaySounds += " X"; }
            cBackBuffer.DrawString(sPlaySounds, mcFontText, cBrush, 650, 540);
            cBackBuffer.DrawRectangle(cPen, 765, 540, 20, 20);
            
            // Release the Brush and Pen resources
            cBrush.Dispose();
            cPen.Dispose();
        }

        // If a Keyboard Key is pressed
        public override EGames KeyDown(KeyEventArgs e)
        {
            // Game to load
            EGames eGameToPlay = meGame;

            // If the player is switching the selection
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Up)
            {
                // Switch selection to previous game
                meSelectedGame--;

                // Reset the Game Scale
                mfSelectedGameScale = 0.0f;

                // Wrap around if necessary
                if (meSelectedGame == EGames.MainMenu)
                {
                    meSelectedGame = EGames.HighScores;
                }

                // Play Menu Selection Changed sound
                CFMOD.PlaySound(msSoundSelectionChanged, false);
            }
            else if (e.KeyCode == Keys.Right || e.KeyCode == Keys.Down)
            {
                // Switch selection to next game
                meSelectedGame++;

                // Reset the Game Scale
                mfSelectedGameScale = 0.0f;

                // Wrap around if necessary
                if (meSelectedGame > EGames.HighScores)
                {
                    meSelectedGame = EGames.Spong;
                }

                // Play Menu Selection Changed sound
                CFMOD.PlaySound(msSoundSelectionChanged, false);
            }

            // If the player has chosen the Game to play
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space)
            {
                eGameToPlay = meSelectedGame;
            }

            // If the player is toggling the Sound on/off
            if (e.KeyCode == Keys.S)
            {
                // Toggle the Sound on/off
                CFMOD.mbSoundOn = !CFMOD.mbSoundOn;
            }

            // Return which Game should be played now
            return eGameToPlay;
        }        

        // If a Mouse button is pressed
        public override EGames MouseDown(MouseEventArgs e)
        {
            // Save the current game being played (i.e. Main Menu)
            EGames eGameToPlay = meGame;

            // Get Mouses Position and store it as a rectangle
            Rectangle rMouse = new Rectangle(e.X, e.Y, 1, 1);

            // If the Left Mouse button was pressed
            if (e.Button == MouseButtons.Left)
            {
                // If they clicked to Toggle the Sound on/off
                if (mrToggleSoundArea.IntersectsWith(rMouse))
                {
                    // Toggle the Sound on/off
                    CFMOD.mbSoundOn = !CFMOD.mbSoundOn;
                }
                // Else play the Selected game
                else
                {
                    // Get which game to start playing
                    eGameToPlay = meSelectedGame;
                }
            }

            // Return which Game should be played now
            return eGameToPlay;
        }

        // If the Mouse is moved
        public override void MouseMove(MouseEventArgs e)
        {
            // Get Mouses Position and store it as a rectangle
            Rectangle rMouse = new Rectangle(e.X, e.Y, 1, 1);

            // If the Mouse is over the Pong game icon
            if (mrPongIcon.IntersectsWith(rMouse) && meSelectedGame != EGames.Spong)
            {
                // Set Pong as the Selected game
                meSelectedGame = EGames.Spong;

                // Reset the Game Scale
                mfSelectedGameScale = 0.0f;

                // Play Menu Selection Changed sound
                CFMOD.PlaySound(msSoundSelectionChanged, false);
            }
            // Else if the Mouse is over the Shooting Gallery game icon
            else if (mrShootingGalleryIcon.IntersectsWith(rMouse) && meSelectedGame != EGames.ShootingGallery)
            {
                // Set Shooting Gallery as the Selected game
                meSelectedGame = EGames.ShootingGallery;

                // Reset the Game Scale
                mfSelectedGameScale = 0.0f;

                // Play Menu Selection Changed sound
                CFMOD.PlaySound(msSoundSelectionChanged, false);
            }
            // Else if the Mouse is over the Pitch Matcher game icon
            else if (mrPitchMatcherIcon.IntersectsWith(rMouse) && meSelectedGame != EGames.PitchMatcher)
            {
                // Set Pitch Matcher as the Selected game
                meSelectedGame = EGames.PitchMatcher;

                // Reset the Game Scale
                mfSelectedGameScale = 0.0f;

                // Play Menu Selection Changed sound
                CFMOD.PlaySound(msSoundSelectionChanged, false);
            }
            // Else if the Mouse is over the Target Practice game icon
            else if (mrTargetPracticeIcon.IntersectsWith(rMouse) && meSelectedGame != EGames.TargetPractice)
            {
                // Set Target Practice as the Selected game
                meSelectedGame = EGames.TargetPractice;

                // Reset the Game Scale
                mfSelectedGameScale = 0.0f;

                // Play Menu Selection Changed sound
                CFMOD.PlaySound(msSoundSelectionChanged, false);
            }
            // Else if the Mouse is over the Profile Settings icon
            else if (mrProfileSettingsIcon.IntersectsWith(rMouse) && meSelectedGame != EGames.ProfileSettings)
            {
                // Set the Profile Settings as the Selected icon
                meSelectedGame = EGames.ProfileSettings;

                // Reset the Game Scale
                mfSelectedGameScale = 0.0f;

                // Play Menu Selection Changed sound
                CFMOD.PlaySound(msSoundSelectionChanged, false);
            }
            // Else if the Mouse is over the High Scores icon
            else if (mrHighScoresIcon.IntersectsWith(rMouse) && meSelectedGame != EGames.HighScores)
            {
                // Set High Scores as the Selected game
                meSelectedGame = EGames.HighScores;

                // Reset the Game Scale
                mfSelectedGameScale = 0.0f;

                // Play Menu Selection Changed sound
                CFMOD.PlaySound(msSoundSelectionChanged, false);
            }
        }
    }
}
