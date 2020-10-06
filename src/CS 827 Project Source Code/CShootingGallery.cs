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
    class CShootingGallery : CGame
    {
        // States which the Pong game may be in
        enum EShootingGalleryGameStates
        {
            Play = 0,
            Pause = 1,
            ChoosingSong = 2
        }

        // The choices of Music to play
        enum EMusic
        {
            Mario = 0,
            Simple = 1,
            DansMario = 2
        }

        // The Rows the Players Pitch can be in
        enum ETargetRows
        {
            None = 0,
            Top = 1,
            Middle = 2,
            Bottom = 3
        }

        // Class to hold information about the Midi Music
        class CMusic
        {
            public EMusic eMusic;           // The Music To Play
            public int iChannelToUse;       // Which Channel to take the Midi information from (0 means use all Channels)
            public int iLowPitchCeiling;    // The ceiling of the Low Pitch zone (i.e. Bottom Row)
            public int iMiddlePitchCeiling; // The ceiling of the Middle Pitch zone (i.e. Middle Row)

            // Constructor
            public CMusic(EMusic _eMusic, int _iChannelToUse, int _iLowPitchCeiling, int _iMiddlePitchCeiling)
            {
                eMusic = _eMusic;
                iChannelToUse = _iChannelToUse;
                iLowPitchCeiling = _iLowPitchCeiling;
                iMiddlePitchCeiling = _iMiddlePitchCeiling;
            }
        }

        // Class to hold a Target
        class CTarget
        {
            public RectangleF rRect;            // The Targets current Position
            public Vector2 sVelocity;           // The Targets Velocity in pixels per second
            public Vector2 sStartingVelocity;   // The Targets Starting Velocity in pixels per second
            public bool bFacingLeft;            // The direction the Target is Facing
            public DateTime cTimeToBeAtCenter;  // The Time the Target should be at the center of the screen

            // Constructor
            public CTarget()
            {
                Purge();
            }

            // Function to reset the class variables to their default values
            public void Purge()
            {
                rRect = new Rectangle();
                sVelocity = new Vector2();
                sStartingVelocity = new Vector2();
                bFacingLeft = false;
                cTimeToBeAtCenter = new DateTime();
            }
        }


        // Constants
        CMusic mcSIMPLE_MUSIC;
        CMusic mcDANS_MARIO_MUSIC;
        CMusic mcMARIO_MUSIC;
        int miTOP_ROW_Y;
        int miMIDDLE_ROW_Y;
        int miBOTTOM_ROW_Y;
        int miSHOOT_TARGET_AREA_LEFT;
        int miSHOOT_TARGET_AREA_RIGHT;
        int miSHOOT_TARGET_AREA_TOP;
        int miSHOOT_TARGET_AREA_BOTTOM;
        int miCENTER_OF_SCREEN_X;
        

        // Class Variables
        List<CTarget> mcTargetList;                 // Holds the List of Targets

        int miScore;                                // The Players Score
        EMusic meHighlightedMusic;                  // The Music which is currently Highlighted (i.e. Selected)
        float mfSecondsSongHasBeenCompleteFor;      // The Length in seconds that the Song has been done playing for

        Image mcTargetFacingLeftImage;              // Holds the Image of a Target which Faces Left
        Image mcTargetFacingRightImage;             // Holds the Image of a Target which Faces Right
        ImageAttributes mcTargetImageAttributes;    // Holds the Attributes to apply to the Images (Transparency Color)

        EShootingGalleryGameStates meShootingGalleryState;  // The current state of the Shooting Gallery game
        CMusic mcCurrentMusic;                              // The Music which should be playing

        AxWMPLib.AxWindowsMediaPlayer mcWindowsMediaPlayer; // Handle to the Windows Media Player control
        Random mcRandom;                                    // Used to generate random numbers

        Font mcFontText;            // The Font used to draw any text
        Font mcFontPause;           // The Font used for the Paused text
        Font mcFontChooseMusicTitle;// The Font used to draw the choose music title text

        CPitchMeter mcPitchMeter;   // The Pitch Meter


        // Class constructor
        public CShootingGallery(AxWMPLib.AxWindowsMediaPlayer cWindowsMediaPlayer)
        {
            try
            {
                // Pre-load images
                mcTargetFacingLeftImage = Image.FromFile("../../Data/Images/ShootingGalleryDuckFacingLeft.png");
                mcTargetFacingRightImage = Image.FromFile("../../Data/Images/ShootingGalleryDuckFacingRight.png");

                // Pre-load sounds
   
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Exception caught");
            }

            // Seed the Random number generator
            mcRandom = new Random(Environment.TickCount);

            // Save a handle to the Windows Media Player
            mcWindowsMediaPlayer = cWindowsMediaPlayer;            

            // Set the Transparent Color of the Images
            mcTargetImageAttributes = new ImageAttributes();
            mcTargetImageAttributes.SetColorKey(Color.FromArgb(255, 0, 255), Color.FromArgb(255, 0, 255));

            // Setup the Fonts
            mcFontText = new Font("Arial", 20, FontStyle.Regular);
            mcFontPause = new Font("Arial", 120, FontStyle.Regular);
            mcFontChooseMusicTitle = new Font("Arial", 40, FontStyle.Bold);

            // Set the Row Y coordinates
            miTOP_ROW_Y = 75;
            miMIDDLE_ROW_Y = 210;
            miBOTTOM_ROW_Y = 350;

            // Set the Shoot Target Area Range
            miSHOOT_TARGET_AREA_LEFT = 375;
            miSHOOT_TARGET_AREA_RIGHT = 426;
            miSHOOT_TARGET_AREA_TOP = 75;
            miSHOOT_TARGET_AREA_BOTTOM = 450;

            // Specify the Center X pixel of the screen
            miCENTER_OF_SCREEN_X = 398;

            // Initialize the Pitch Meter
            mcPitchMeter = new CPitchMeter(miSHOOT_TARGET_AREA_LEFT + 22, miSHOOT_TARGET_AREA_TOP, 5, (miSHOOT_TARGET_AREA_BOTTOM - miSHOOT_TARGET_AREA_TOP));
            mcPitchMeter.ShowThirdsMarkers(true);

            // Set the Mario Music parameters
            mcSIMPLE_MUSIC = new CMusic(EMusic.Simple, 1, 52, 55);
            mcDANS_MARIO_MUSIC = new CMusic(EMusic.DansMario, 1, 76, 78);
            mcMARIO_MUSIC = new CMusic(EMusic.Mario, 1, 54, 59);

            // Set the Players Score to zero
            miScore = 0;

            // Set the other variables default values
            Reset();
        }

        // Reset class variables to their default values
        public void Reset()
        {
            // Record that we are playing Pong
            meGame = EGames.ShootingGallery;

            // Set the games initial State
            meShootingGalleryState = EShootingGalleryGameStates.ChoosingSong;

            // Set the default Music to use
            meHighlightedMusic = EMusic.Mario;
            mcCurrentMusic = mcMARIO_MUSIC;

            // Stop any Music that might be playing
            mcWindowsMediaPlayer.Ctlcontrols.stop();

            // Reset the duration the Song has been done playing for
            mfSecondsSongHasBeenCompleteFor = 0.0f;

            // Create the Target List
            mcTargetList = new List<CTarget>();
        }

        // Do game processing and update the display
        public override void Update(float fTimeSinceLastUpdateInSeconds, Graphics cBackBuffer)
        {
            // Check what State the game is in
            switch (meShootingGalleryState)
            {
                case EShootingGalleryGameStates.Play:
                    UpdateGameObjects(fTimeSinceLastUpdateInSeconds, cBackBuffer);
                    DrawGame(fTimeSinceLastUpdateInSeconds, cBackBuffer);
                break;

                case EShootingGalleryGameStates.Pause:
                    DrawGame(fTimeSinceLastUpdateInSeconds, cBackBuffer);
                    DisplayPaused(fTimeSinceLastUpdateInSeconds, cBackBuffer);
                break;

                case EShootingGalleryGameStates.ChoosingSong:
                    ChooseSong(fTimeSinceLastUpdateInSeconds, cBackBuffer);
                break;
            }
        }

        // Function to Update all of the Game Objects
        private void UpdateGameObjects(float fTimeSinceLastUpdateInSeconds, Graphics cBackBuffer)
        {
            // Loop through Midi Data and create new Targets if necessary
            int iMidiIndex = 0;
            int iNumberOfMidiNotes = CMidiInput.Instance().cMidiInfoList.Count;
            for (iMidiIndex = 0; iMidiIndex < iNumberOfMidiNotes; iMidiIndex++)
            {
                // Create a handle to the current Midi Info for readability
                CMidiInfo cMidiInfo = CMidiInput.Instance().cMidiInfoList[iMidiIndex];

                // Filter Midi Notes based on which Music is playing
                // If this Midi Note is not on the Channel we want
                if (cMidiInfo.iChannel != mcCurrentMusic.iChannelToUse && cMidiInfo.iChannel > 0)
                {
                    // Move to the next Midi Note (only keep Notes on the desired Channel)
                    continue;
                }

                // Create a new Target
                CTarget cNewTarget = new CTarget();

                // Specify Target Width and Height
                cNewTarget.rRect.Width = 70;
                cNewTarget.rRect.Height = 80;

                // Determine which Row the Target should be in
                // If it should be in the bottom Row
                if (cMidiInfo.iNote < mcCurrentMusic.iLowPitchCeiling)
                {
                    cNewTarget.rRect.Y = miBOTTOM_ROW_Y;
                }
                // Else if it should be in the middle Row
                else if (cMidiInfo.iNote < mcCurrentMusic.iMiddlePitchCeiling)
                {
                    cNewTarget.rRect.Y = miMIDDLE_ROW_Y;
                }
                // Else it should be in the top Row
                else
                {
                    cNewTarget.rRect.Y = miTOP_ROW_Y;
                }
                
                // Calculate the Targets velocity based on the Midi Note's Velocity
                cNewTarget.sVelocity.X = cMidiInfo.iVelocity + 140;
                cNewTarget.sVelocity.Y = 0.0f;

                // 50/50 chance of Target approaching from left/right side
                bool bApproachFromRight = (mcRandom.Next(0, 2) == 0) ? true : false;
                bApproachFromRight = false;

                // If this Target is approaching from the Right
                if (bApproachFromRight)
                {
                    // Reverse it's X Velocity
                    cNewTarget.sVelocity.X = -cNewTarget.sVelocity.X;

                    // Record that it should be Facing Left
                    cNewTarget.bFacingLeft = true;
                }

                // Save the Targets Starting Velocity
                cNewTarget.sStartingVelocity = cNewTarget.sVelocity;

                // Calculate the Time when the Target should reach the center of the screen
                cNewTarget.cTimeToBeAtCenter = cMidiInfo.cTime.AddSeconds(3.0);

                // Calculate how long between the current Time and the time the target 
                // should be in the middle of the screen
                TimeSpan cTimeSpanToReachCenter = cNewTarget.cTimeToBeAtCenter - DateTime.Now;
                float fTimeToReachCenter = (float)(cTimeSpanToReachCenter.TotalMilliseconds / 1000.0f);

                // Calculate where it should be placed to reach the middle of the screen at the desired time
                cNewTarget.rRect.X = (int)(miCENTER_OF_SCREEN_X - (cNewTarget.sVelocity.X * fTimeToReachCenter)) + (cNewTarget.rRect.Width / 2.0f);

                // Add the new Target to the Target List
                mcTargetList.Add(cNewTarget);
            }

            // Temp variable to keep track of which Row the Players Pitch is in
            ETargetRows ePlayersPitchRow;

            // If the Player is making a sound
            if (CSoundInput.Instance().bInputReceived)
            {
                // If the Song is not still loading
                if (mcWindowsMediaPlayer.status != "Connecting...")
                {
                    // Duduct from their Score (so they don't make sounds the whole time)
                    miScore--;
                }

                // Save which Row their Pitch corresponds to
                // If the Pitch corresponds to the Bottom Row
                if (CSoundInput.Instance().fPitch < CSoundInput.Instance().iOneThirdOfPitchRange)
                {
                    ePlayersPitchRow = ETargetRows.Bottom;
                }
                // Else if the Pitch corresponds to the Middle Row
                else if (CSoundInput.Instance().fPitch < CSoundInput.Instance().iTwoThirdsOfPitchRange)
                {
                    ePlayersPitchRow = ETargetRows.Middle;
                }
                // Else the Pitch corresponds to the Top Row
                else
                {
                    ePlayersPitchRow = ETargetRows.Top;
                }
            }
            // Else no input was recieved
            else
            {
                ePlayersPitchRow = ETargetRows.None;
            }

            // Loop though all Targets in the Target List
            int iTargetIndex = 0;
            int iNumberOfTargets = mcTargetList.Count;
            for (iTargetIndex = 0; iTargetIndex < iNumberOfTargets; iTargetIndex++)
            {
                // Save a handle to this Target for readability
                CTarget cTarget = mcTargetList[iTargetIndex];

                // Make sure the Targets Velocity will still put them at the 
                // center of the screen at the correct time

                // If this target has not passed the middle of the screen yet
                if (cTarget.cTimeToBeAtCenter > DateTime.Now)
                {
                    // Calculate how long between the current Time and the time the target 
                    // should be in the middle of the screen
                    TimeSpan cTimeSpanToReachCenter = cTarget.cTimeToBeAtCenter - DateTime.Now;
                    float fTimeToReachCenter = (float)(cTimeSpanToReachCenter.TotalMilliseconds / 1000.0f);

                    // Calculate the Distance this Target is from the Center of the screen
                    float fDistanceToCenter = miCENTER_OF_SCREEN_X - (cTarget.rRect.X + (cTarget.rRect.Width / 2.0f));

                    // Calculate where it should be placed to reach the middle of the screen at the desired time
                    cTarget.sVelocity.X = fDistanceToCenter / fTimeToReachCenter;

                    // If the Velocity should be negative and it isn't, or it should be positive and it isn't
                    if ((cTarget.sStartingVelocity.X < 0.0f && cTarget.sVelocity.X > 0.0f) ||
                        (cTarget.sStartingVelocity.X > 0.0f && cTarget.sVelocity.X < 0.0f))
                    {
                        // Make the Velocity 
                        cTarget.sVelocity.X *= -1;
                    }
                }
                // Else it has passed the middle of the screen
                else
                {
                    // So make sure it is using it's original velocity
                    cTarget.sVelocity = cTarget.sStartingVelocity;
                }

                // Update the Position of this Target
                cTarget.rRect.X += (cTarget.sVelocity.X * fTimeSinceLastUpdateInSeconds);
                cTarget.rRect.Y += (cTarget.sVelocity.Y * fTimeSinceLastUpdateInSeconds);

                // Store which Row this Target is in
                ETargetRows eTargetRow;

                // If the Target is in the Top Row
                if (cTarget.rRect.Y == miTOP_ROW_Y)
                {
                    eTargetRow = ETargetRows.Top;
                }
                // Else if the Target is in the Middle Row
                else if (cTarget.rRect.Y == miMIDDLE_ROW_Y)
                {
                    eTargetRow = ETargetRows.Middle;
                }
                // Else the Target is in the Bottom Row
                else
                {
                    eTargetRow = ETargetRows.Bottom;
                }

                // Calculate the middle position of the Target
                float fMiddlePosition = cTarget.rRect.X + (cTarget.rRect.Width / 2.0f);

                // If the Player is making a Pitch correspond to the Row this Target is in
                // AND the Target is in the shooting area
                if (ePlayersPitchRow == eTargetRow &&
                    fMiddlePosition >= miSHOOT_TARGET_AREA_LEFT && fMiddlePosition <= miSHOOT_TARGET_AREA_RIGHT)
                {
                    // Give the Player some Points for hitting this Target
                    miScore += 25;

                    // Kill this Target by moving it off the screen
                    cTarget.rRect.X += (cTarget.sVelocity.X * 10.0f);
                }
            }

            // Remove all Targets from the List which have moved off the screen
            mcTargetList.RemoveAll(delegate(CTarget cTarget)
            {
                // Return if the Target has moved off the screen or not yet
                return (cTarget.sVelocity.X > 0.0f && cTarget.rRect.X > 800) ||
                    (cTarget.sVelocity.X < 0.0f && (cTarget.rRect.X + cTarget.rRect.Width) < 0);
            });

            // If the Song is finished playing
            if (mcWindowsMediaPlayer.status == "Stopped")
            {
                // Sum the amount of time passed since the song finished playing
                mfSecondsSongHasBeenCompleteFor += fTimeSinceLastUpdateInSeconds;

                // If the Song has been finished for 7 seconds
                if (mfSecondsSongHasBeenCompleteFor >= 7.0f)
                {
                    // Check which Song is being played
                    switch (mcCurrentMusic.eMusic)
                    {
                        default:
                        case EMusic.Simple:
                            // Save the Players Score if it's better than their old one
                            if (miScore > CProfiles.Instance().mcCurrentProfile.iScoreShootingGallerySong1)
                            {
                                // Save the Players new High Score
                                CProfiles.Instance().mcCurrentProfile.iScoreShootingGallerySong1 = miScore;
                                CProfiles.Instance().SaveProfilesToFile();
                            }
                        break;

                        case EMusic.DansMario:
                            // Save the Players Score if it's better than their old one
                            if (miScore > CProfiles.Instance().mcCurrentProfile.iScoreShootingGallerySong2)
                            {
                                // Save the Players new High Score
                                CProfiles.Instance().mcCurrentProfile.iScoreShootingGallerySong2 = miScore;
                                CProfiles.Instance().SaveProfilesToFile();
                            }
                        break;

                        case EMusic.Mario:
                            // Save the Players Score if it's better than their old one
                            if (miScore > CProfiles.Instance().mcCurrentProfile.iScoreShootingGallerySong3)
                            {
                                // Save the Players new High Score
                                CProfiles.Instance().mcCurrentProfile.iScoreShootingGallerySong3 = miScore;
                                CProfiles.Instance().SaveProfilesToFile();
                            }
                        break;
                    }

                    // Restart the game
                    Reset();
                }
            }
        }

        // Function to Draw the Game in it's current state to the Back Buffer
        private void DrawGame(float fTimeSinceLastUpdateInSeconds, Graphics cBackBuffer)
        {
            // Brush and Pen to draw with
            Brush cBrush;
            Pen cPen;

            // Clear the screen to Black
            cBackBuffer.Clear(Color.Black);

            // Loop though all Targets in the Target List
            int iTargetIndex = 0;
            int iNumberOfTargets = mcTargetList.Count;
            for (iTargetIndex = 0; iTargetIndex < iNumberOfTargets; iTargetIndex++)
            {
                // Save a handle to this Target for readability
                CTarget cTarget = mcTargetList[iTargetIndex];

                // Rectangle used to draw Target to the screen
                Rectangle rRect = new Rectangle((int)cTarget.rRect.X, (int)cTarget.rRect.Y, (int)cTarget.rRect.Width, (int)cTarget.rRect.Height);

                // If the Target is Facing Left
                if (cTarget.bFacingLeft)
                {
                    // Draw this Target to the screen facing Left
                    cBackBuffer.DrawImage(mcTargetFacingLeftImage, rRect, 0, 0, mcTargetFacingLeftImage.Width, mcTargetFacingLeftImage.Height, GraphicsUnit.Pixel, mcTargetImageAttributes);
                }
                // Else the Target is Facing Right
                else
                {
                    // Draw this Target to the screen facing Right
                    cBackBuffer.DrawImage(mcTargetFacingRightImage, rRect, 0, 0, mcTargetFacingLeftImage.Width, mcTargetFacingLeftImage.Height, GraphicsUnit.Pixel, mcTargetImageAttributes);
                }
            }

            // Draw the Pitch Meter
            mcPitchMeter.AutoDetectPitchAndUpdateMeter();
            mcPitchMeter.Draw(cBackBuffer);

            // Draw the area in which the Targets can be "hit"
            cPen = new Pen(Color.White);
            cBackBuffer.DrawRectangle(cPen, miSHOOT_TARGET_AREA_LEFT, miSHOOT_TARGET_AREA_TOP, (miSHOOT_TARGET_AREA_RIGHT - miSHOOT_TARGET_AREA_LEFT), (miSHOOT_TARGET_AREA_BOTTOM - miSHOOT_TARGET_AREA_TOP));
            
            // Display to press Space Bar to Pause the game
            cBrush = new SolidBrush(Color.LightBlue);
            cBackBuffer.DrawString("Pause - Space Bar", mcFontText, cBrush, 475, 530);

            // Display to press R to Restart the game
            cBackBuffer.DrawString("Restart - R", mcFontText, cBrush, 150, 530);

            // Display the players Score
            cBackBuffer.DrawString("Score " + miScore, mcFontText, cBrush, 10, 10);

            // Display the instructions
            cBackBuffer.DrawString("Try and hit as many ducks as possible", mcFontText, cBrush, 190, 10);

            // If the Song is still loading
            if (mcWindowsMediaPlayer.status == "Connecting...")
            {
                cBackBuffer.DrawString("Loading Tune", mcFontChooseMusicTitle, cBrush, 230, 200);
            }

            // Clean up the Brush and Pen resources
            cBrush.Dispose();
            cPen.Dispose();
        }

        // Function to handle the Game when Paused
        private void DisplayPaused(float fTimeSinceLastUpdateInSeconds, Graphics cBackBuffer)
        {
            // Display that the Game is Paused
            Brush cBrush = new SolidBrush(Color.White);
            cBackBuffer.DrawString("Paused", mcFontPause, cBrush, 100, 150);
            cBrush.Dispose();
        }

        // Function to display the Songs to choose from
        private void ChooseSong(float fTimeSinceLastUpdateInSeconds, Graphics cBackBuffer)
        {
            // Display the Songs to choose from, with the Selected song highlighted

            Brush cBrush;                               // Temp Brush used to draw the text
            Color cNormalColor = Color.WhiteSmoke;      // Color of the normal text
            Color cHighlightedColor = Color.Red;        // Color of the Hightlighted text

            // Clear the Back Buffer first
            cBackBuffer.Clear(Color.Black);


            // Display the players Score
            cBrush = new SolidBrush(Color.LightBlue);
            cBackBuffer.DrawString("Score " + miScore, mcFontText, cBrush, 10, 10);
            
            // Display the Choose A Tune text
            cBrush = new SolidBrush(Color.RoyalBlue);
            cBackBuffer.DrawString("Choose A Tune", mcFontChooseMusicTitle, cBrush, 200, 100);


            // If Simple should be Highlighted
            if (meHighlightedMusic == EMusic.Simple)
            {
                cBrush = new SolidBrush(cHighlightedColor);
            }
            // Else Simple is not Highlighted
            else
            {
                cBrush = new SolidBrush(cNormalColor);
            }

            // Display Simple
            cBackBuffer.DrawString("Simple", mcFontText, cBrush, 330, 200);


            // If DansMario should be Highlighted
            if (meHighlightedMusic == EMusic.DansMario)
            {
                cBrush = new SolidBrush(cHighlightedColor);
            }
            // Else DansMario is not Highlighted
            else
            {
                cBrush = new SolidBrush(cNormalColor);
            }

            // Display DansMario
            cBackBuffer.DrawString("Dans Mario", mcFontText, cBrush, 330, 250);


            // If Mario should be Highlighted
            if (meHighlightedMusic == EMusic.Mario)
            {
                cBrush = new SolidBrush(cHighlightedColor);
            }
            // Else Mario is not Highlighted
            else
            {
                cBrush = new SolidBrush(cNormalColor);
            }

            // Display Mario
            cBackBuffer.DrawString("Mario", mcFontText, cBrush, 330, 300);


            // Clean up the Brush
            cBrush.Dispose();
        }

        // Function loads the currently Highlighted song and starts the game
        private void StartPlayingWithCurrentlyHightlightedSong()
        {
            FileInfo cMidiFileInfo;

            // Use the Highlighted Song as the Song to Use
            switch (meHighlightedMusic)
            {
                default:
                case EMusic.Simple:
                    // Set the Music to Play
                    mcCurrentMusic = mcSIMPLE_MUSIC;

                    // Store the Path of the Music to play
                    cMidiFileInfo = new FileInfo("../../Data/Music/Simple.mid");
                break;

                case EMusic.DansMario:
                    // Set the Music to Play
                    mcCurrentMusic = mcDANS_MARIO_MUSIC;

                    // Store the Path of the Music to play
                    cMidiFileInfo = new FileInfo("../../Data/Music/DansMario.mid");
                break;

                case EMusic.Mario:
                    // Set the Music to Play
                    mcCurrentMusic = mcMARIO_MUSIC;

                    // Store the Path of the Music to play
                    cMidiFileInfo = new FileInfo("../../Data/Music/Mario.mid");
                break;
            }

            // Reset the Players Score to zero
            miScore = 0;

            // Start Playing the Music
            mcWindowsMediaPlayer.URL = cMidiFileInfo.FullName;
            mcWindowsMediaPlayer.Ctlcontrols.play();

            // Start Playing the Game
            meShootingGalleryState = EShootingGalleryGameStates.Play;
        }

        // Handle key presses
        public override EGames KeyDown(KeyEventArgs e)
        {
            // If the Pause button is pressed
            if (e.KeyCode == Keys.Space)
            {
                // If we are currently Playing the Game
                if (meShootingGalleryState == EShootingGalleryGameStates.Play)
                {
                    // Pause the music
                    mcWindowsMediaPlayer.Ctlcontrols.pause();

                    // Pause the Game
                    meShootingGalleryState = EShootingGalleryGameStates.Pause;
                }
                // Else if the Game is currently Paused
                else if (meShootingGalleryState == EShootingGalleryGameStates.Pause)
                {
                    // Start the music
                    mcWindowsMediaPlayer.Ctlcontrols.play();

                    // Play the Game
                    meShootingGalleryState = EShootingGalleryGameStates.Play;
                }
                // Don't Pause if currently choosing a Song
            }

            // If we are Choosing a Song
            if (meShootingGalleryState == EShootingGalleryGameStates.ChoosingSong)
            {
                // If the Player wants to play with the Highlighted Song
                if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space)
                {
                    StartPlayingWithCurrentlyHightlightedSong();
                }
            }

            // If the Player wants to change the Highlighted Song
            // If Up was pressed
            if (e.KeyCode == Keys.Up)
            {
                // Highlight the Previous song
                meHighlightedMusic--;

                // Wrap around
                if (meHighlightedMusic < 0)
                {
                    meHighlightedMusic = EMusic.DansMario;
                }
            }
            // Else if Down was pressed
            else if (e.KeyCode == Keys.Down)
            {
                // Highlight the Next song
                meHighlightedMusic++;

                // Wrap around
                if (meHighlightedMusic > EMusic.DansMario)
                {
                    meHighlightedMusic = 0;
                }
            }

            // If the Restart button is pressed
            if (e.KeyCode == Keys.R)
            {
                // Return to Choosing a Song
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
                // Switch to the Main Menu
                eGameToPlay = EGames.MainMenu;
            }

            // If we are Choosing a Song
            if (meShootingGalleryState == EShootingGalleryGameStates.ChoosingSong)
            {
                // If the Player wants to play with the Highlighted Song
                if (e.Button == MouseButtons.Left)
                {
                    StartPlayingWithCurrentlyHightlightedSong();
                }
            }

            return eGameToPlay;
        }

        // If the Mouse was Moved
        public override void MouseMove(MouseEventArgs e)
        { 
            // If Mario should be Highlighted
            if (e.Y < 240)
            {
                meHighlightedMusic = EMusic.Simple;
                
            }
            // Else if Simple should be Highlighted
            else if (e.Y < 290)
            {
                meHighlightedMusic = EMusic.DansMario;
            }
            // Else DansMario should be Highlighted
            else
            {
                meHighlightedMusic = EMusic.Mario;
            }
        }
    }
}
