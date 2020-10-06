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
    public partial class formMain : Form
    {
        //===========================================================
        // Structures
        //===========================================================

        // Structure to hold a transitions info
        struct STransition
        {
            // Fade parameters
            public Color sColor;                    // Color to fade out to
            public float fEndOpacity;               // How much to fade out
            public int iDurationInMilliseconds;     // How long the fade out should take (fade in will take same amount of time)
            public EGames eGameToSwitchTo;          // Game to switch to once fade out is complete

            // "Internal" variables used to control the fading
            public int iElapsedTime;                // Tells how much time has passed since the fade started
            public bool bPerfromingTransition;      // Tells if the transition is currently being performed
            public bool bFadeOutComplete;           // Tells if we are done fading out or not
            public bool bSwitchGamesNow;            // Tells if we should switch the game now or not

            // Function to Reset variables to their default values
            public void Reset()
            {
                sColor = Color.Black;
                iDurationInMilliseconds = 0;
                eGameToSwitchTo = EGames.MainMenu;
                fEndOpacity = 1.0f;

                iElapsedTime = 0;
                bPerfromingTransition = false;
                bFadeOutComplete = false;
                bSwitchGamesNow = false;
            }
        }


        //===========================================================
        // Class Variables
        //===========================================================

        CGame mcGame;                       // Keeps track of which Game we are currently playing
        Stopwatch mcUpdateStopwatch;        // Used to determine how much time has elapsed between frames
        STransition msTransition;           // Keeps track of transitions between Games

        // Offscreen devices to draw to (double buffering)
        Bitmap mcBackBufferImage;           // Image that the BackBuffer draws to
        Graphics mcBackBuffer;              // BackBuffer object

        // FPS (Frames Per Second) variables        
        int miFPSFrames;                    // Used to count the number of frames in a second
        int miFPSTime;                      // Used to tell when a second has passed
        int miFPS;                          // Stores the Frames Per Second
        bool mbShowFPS;                     // Tells whether the FPS should be shown or not
        Font mcFontFPS;                     // The Font used for the FPS


        //===========================================================
        // Class Functions
        //===========================================================

        // Main entry point of Application
        [STAThread]
        static void Main()
        {
            // Enable styles to make the form and it's controls look nice
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Get the Path to the PD Patch to open
            FileInfo cPDPatchFileInfo = new FileInfo("../../Data/PDPatches/GetPitchAndMidi.pd");

            // Load the PD Patch
            Process cPDProcess = new Process();
            cPDProcess.StartInfo.FileName = "C:\\Program Files\\pd\\bin\\pd.exe";
//            cPDProcess.StartInfo.Arguments = "-open \"" + cPDPatchFileInfo.FullName + "\"" + " -midiindev 2 -midioutdev 2 -nogui";
            cPDProcess.StartInfo.Arguments = "-open \"" + cPDPatchFileInfo.FullName + "\"" + " -midiindev 2 -midioutdev 2";
//            cPDProcess.StartInfo.Arguments = "-open \"" + cPDPatchFileInfo.FullName + "\"" + " -midiindev 2";
            cPDProcess.Start();

            // Wait a second for the PD Patch to load
            System.Threading.Thread.Sleep(1000);

            // Create the Form
            formMain cForm = new formMain();

            // Display the Form
            cForm.Show();

            // Loop until the user closes the Form
            while (cForm.Created)
            {
                cForm.UpdateGame();     // Update the Game Data and redraw the screen
                Application.DoEvents(); // Process events like keyboard input
            }
            
            // If the PD Patch Process is running
            if (!cPDProcess.StartInfo.FileName.Equals("") && !cPDProcess.HasExited)
            {
                // Stop the PD Patch Process
                cPDProcess.Kill();
            }
        }

        // Class constructor
        public formMain()
        {
            // Initialize all form controls
            InitializeComponent();

            // Create the initial Game object (default is the Main Menu)
            mcGame = new CMainMenu();

            // Offscreen devices to draw to (double buffering)
            // NOTE: Can only draw to 790x560 area though because the windows title bar and 
            //       side bars are included in the 800x600 area
            mcBackBufferImage = new Bitmap(800, 600);   // Create a 800x600 pixel area to draw to
            mcBackBuffer = Graphics.FromImage(mcBackBufferImage);

            // Enable double-buffering to help avoid flicker
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);

            // Start the Update Stopwatch
            mcUpdateStopwatch = new Stopwatch();
            mcUpdateStopwatch.Reset();
            mcUpdateStopwatch.Start();

            // Setup FPS variables
            miFPSFrames = 0;
            miFPSTime = 0;
            miFPS = 0;
            mbShowFPS = true;
            mcFontFPS = new Font("Arial", 10, FontStyle.Regular);

            // Load the Profiles from the File, and use the last Selected one
            CProfiles.Instance().LoadProfilesFromFile();

            // Initialize the Transition structure
            msTransition.Reset();
        }

        // Update Game Information
        void UpdateGame()
        {
            // Calculate how many milliseconds have passed since the last Update
            int iElapsedTime = (int)mcUpdateStopwatch.ElapsedMilliseconds;
            mcUpdateStopwatch.Reset();
            mcUpdateStopwatch.Start();

            // If we should switch Games
            if (msTransition.bSwitchGamesNow)
            {
                // Mark that the Game has been Switched
                msTransition.bSwitchGamesNow = false;

                // Switch Games
                PlayGame(msTransition.eGameToSwitchTo);
            }

            // If we should calculate the FPS
            if (mbShowFPS)
            {
                // Update the Time and Frame count
                miFPSTime += iElapsedTime;
                miFPSFrames++;

                // If a second has passed
                if (miFPSTime > 1000)
                {
                    // Subtract a second from the FPS Time
                    miFPSTime -= 1000;

                    // Record the FPS for the last second and reset the FPS counter
                    miFPS = miFPSFrames;
                    miFPSFrames = 0;
                }
            }

            // Get input from PD (audio input from player)
            CSoundInput.Instance().GetPDInput(true);

            // Get midi input from PD
            CMidiInput.Instance().GetPDInput();

            // Calculate how much time has passed in seconds since the last Update
            float fElapsedTimeInSeconds = iElapsedTime / 1000.0f;

            // Update the Game
            mcGame.Update(fElapsedTimeInSeconds, mcBackBuffer);

            // If we are in a screen Transition
            if (msTransition.bPerfromingTransition)
            {
                float fProgress = 0.0f;
                
                // Calculate the Elapsed Time
                msTransition.iElapsedTime += iElapsedTime;

                // If the Transition should happen instantly
                if (msTransition.iDurationInMilliseconds == 0)
                {
                    // Record that we should be at the end of the fade in/out
                    fProgress = 1.0f;
                }
                else
                {
                    // Calculate how far into the Transition we are
                    fProgress = (float)msTransition.iElapsedTime / (float)msTransition.iDurationInMilliseconds;
                    if (fProgress > 1.0f)
                    {
                        fProgress = 1.0f;
                    }
                    else if (fProgress == 0.0f)
                    {
                        fProgress = 0.0001f;
                    }
                }

                // If we are now fading back in
                if (msTransition.bFadeOutComplete)
                {
                    // Bug Workaround - If we are not in the Profile Settings
                    if (mcGame.meGame != EGames.ProfileSettings)
                    {
                        // Reverse the progress so that we fade in instead of out
                        fProgress = 1.0f - fProgress;
                    }
                }

                // Calculate the Transparency to use
                int iOpacity = (int)(fProgress * (msTransition.fEndOpacity * 255.0f));

                // Draw the Transition
                Brush cBrush = new SolidBrush(Color.FromArgb(iOpacity, msTransition.sColor));
                mcBackBuffer.FillRectangle(cBrush, 0, 0, mcBackBufferImage.Width, mcBackBufferImage.Height);
                cBrush.Dispose();

                // If we have faded out all the way
                if (fProgress == 1.0f)
                {
                    // Mark that we have faded out all the way
                    msTransition.bFadeOutComplete = true;

                    // Reset the Elapsed time
                    msTransition.iElapsedTime = 0;

                    // Mark that we should Switch Games Now
                    msTransition.bSwitchGamesNow = true;
                }
                // Else if we haved faded in all the way
                else if (fProgress == 0.0f)
                {
                    // Stop doing the Transition
                    msTransition.Reset();
                }
            }            

            // If the FPS should be shown
            if (mbShowFPS)
            {
                // Display the FPS
                Brush cBrush = new SolidBrush(Color.WhiteSmoke);
                mcBackBuffer.DrawString("FPS: " + miFPS.ToString(), mcFontFPS, cBrush, 1, 550);
                cBrush.Dispose();
            }

            // Update FMOD
            CFMOD.CheckResult(CFMOD.GetSystem().update(), "FMOD Update Error");

            // Update the Display by invalidating the Form
            this.Invalidate();
        }

        // If eNewGame is different than the current Game, start playing the New Game
        private void PlayGame(EGames eNewGame)
        {
            // If the Game type should be changed
            if (mcGame.meGame != eNewGame)
            {
                // If the screen Transition hasn't started yet
                if (!msTransition.bPerfromingTransition)
                {
                    // Reset the Transition variables
                    msTransition.Reset();

                    // Record that we are now performing a Transition
                    msTransition.bPerfromingTransition = true;
                    
                    // Save which Game to switch to during the Transition
                    msTransition.eGameToSwitchTo = eNewGame;

                    // Set the color to fade with
                    msTransition.sColor = Color.Black;

                    // Set Duration based on which Game we are switching to
                    switch (eNewGame)
                    {
                        case EGames.ProfileSettings:
                            msTransition.iDurationInMilliseconds = 0;
                            msTransition.fEndOpacity = 0.7f;
                        break;

                        default:
                            msTransition.iDurationInMilliseconds = 500;
                        break;
                    }
                }
                // Else we are in the middle of the Transition
                else
                {
                    // Check which Game to switch to
                    switch (eNewGame)
                    {
                        default:
                        case EGames.MainMenu:
                            // Make sure the Windows Media Player controls are hidden and not playing
                            playerWindowsMediaPlayer.Visible = false;
                            playerWindowsMediaPlayer.Ctlcontrols.stop();
                            playerWindowsMediaPlayer.URL = "";

                            mcGame = new CMainMenu();
                        break;

                        case EGames.Spong:
                            mcGame = new CSpong();
                        break;

                        case EGames.ShootingGallery:
                            mcGame = new CShootingGallery(playerWindowsMediaPlayer);
                        break;

                        case EGames.PitchMatcher:
                            mcGame = new CPitchMatcher();
                        break;

                        case EGames.TargetPractice:
                            mcGame = new CTargetPractice();
                        break;

                        case EGames.HighScores:
                            mcGame = new CHighScores();
                        break;

                        case EGames.ProfileSettings:
                            try
                            {
                                // Open and show the Profile Settings Form as a modal form
                                formProfileSettings cProfileSettingsForm = new formProfileSettings();
                                cProfileSettingsForm.SetGameObjectHandle(ref mcGame);
                                cProfileSettingsForm.ShowDialog();
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.ToString(), "Error generating Profile Settings Form");
                            }
                        break;
                    }
                }
            }
        }


        //===========================================================
        // Event Functions
        //===========================================================

        // If the Form should be redrawn (i.e. When it is invalidated)
        private void formMain_Paint(object sender, PaintEventArgs e)
        {
            // Draw the image to the screen
            e.Graphics.DrawImageUnscaled(mcBackBufferImage, 0, 0);
        }

        // If a Keyboard Key is pressed
        private void formMain_KeyDown(object sender, KeyEventArgs e)
        {
            // If Escape was pressed
            if (e.KeyCode == Keys.Escape)
            {
                // If we are in the Main Menu
                if (mcGame.meGame == EGames.MainMenu)
                {
                    // Exit the game
                    Application.Exit();
                }
                // Else we are not in the Main Menu
                else
                {
                    // So go to the Main Menu
                    PlayGame(EGames.MainMenu);
                }
            }
            // Else if Tab was pressed
            else if (e.KeyCode == Keys.Tab)
            {
                // If the FPS are being shown
                if (mbShowFPS)
                {
                    // Hide the FPS
                    mbShowFPS = false;
                }
                // Else the FPS are hidden
                else
                {
                    // Show the FPS
                    mbShowFPS = true;

                    // Reset FPS data
                    miFPSFrames = 0;
                    miFPSTime = 0;
                    miFPS = 0;
                }
            }
            // Else Escape and Tab were not pressed
            else
            {
                // Pass the key on to the Game to be handled
                // Also make sure we're playing the returned Game
                PlayGame(mcGame.KeyDown(e));                
            }
        }

        // If a Mouse Button is pressed
        private void formMain_MouseDown(object sender, MouseEventArgs e)
        {
            // Pass the mouse info on to the Game to be handled
            // Also make sure we're playing the returned Game
            PlayGame(mcGame.MouseDown(e));
        }

        // If the Mouse is Moved
        private void formMain_MouseMove(object sender, MouseEventArgs e)
        {
            // Pass the mouse info on to the Game to be handled
            mcGame.MouseMove(e);
        }            
    }
}