using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CS_827_Project_Source_Code
{
    public partial class formProfileSettings : Form
    {
        // Class Variables
        bool mbDetectingPitch = false;      // Tells if the player is currently detecting their Pitch
        Timer mcUpdateTimer;                // Timer to update the form
        int[] miaPitchHits = new int[130];  // Keeps track of how many times each pitch was hit (128 pitches)
        CGame mcGameHandle = null;          // Handle to the Game object
        CPitchMeter mcPitchMeter;           // The Pitch Meter

        // Sounds
        FMOD.Sound msSoundPitchTransitionExample = null;


        // Form Constructor
        public formProfileSettings()
        {
            // Initializes form controls
            InitializeComponent();

            try
            {
                // Pre-load images

                // Pre-load Sounds
                CFMOD.CheckResult(CFMOD.GetSystem().createSound("../../Data/Sounds/ProfileSettingsExample.wav", FMOD.MODE.DEFAULT, ref msSoundPitchTransitionExample), "FMOD Create Sound Error");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Exception caught");
            }

            // Setup the Update Timer
            mcUpdateTimer = new Timer();
            mcUpdateTimer.Tick += new EventHandler(UpdateTimer_Tick);
            mcUpdateTimer.Interval = 16;
            mcUpdateTimer.Enabled = true;
            mcUpdateTimer.Start();            

            // Set all of the Pitch Hits to zero
            for (int i = 0; i < miaPitchHits.Length; i++)
            {
                miaPitchHits[i] = 0;
            }

            // Display the Profiles in the Profiles combo box
            UpdateShownProfilesAndCurrentSettings();

            // Set the initial Pitch Meter Indicators position
            mcPitchMeter = new CPitchMeter(350, 125);
            mcPitchMeter.ShowHalfPointMarker(true);
        }

        // Set a handle to the Game object so this class can update it
        public void SetGameObjectHandle(ref CGame cGame)
        {
            mcGameHandle = cGame;
            mcGameHandle.meGame = EGames.ProfileSettings;
        }

        // Callback function of the UpdateTimer to update this form
        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            // Redraw this form, causing the UpdateForm function to be called
            this.Invalidate();
        }

        // Update this form
        private void UpdateForm(object sender, PaintEventArgs e)
        {
            // Get a handle to this forms Graphics object so we can draw to it
            Graphics cGraphics = e.Graphics;

            // Get input from PD (audio input from player), without keeping the Pitch within range
            CSoundInput.Instance().GetPDInput(false);

            // If we should be Detecting the Pitch
            if (mbDetectingPitch)
            {              
                // Get the Players current Pitch
                int iPitch = (int)CSoundInput.Instance().fPitch;

                // Increment the Hits on the Players current Pitch
                miaPitchHits[iPitch]++;

                // If this is the lowest Pitch heard so far and it's been hit at least 10 times
                if ((CSoundInput.Instance().fPitch < CSoundInput.Instance().iLowerPitchRange ||
                    CSoundInput.Instance().iLowerPitchRange <= 10) && CSoundInput.Instance().fPitch > 1.0f &&
                    miaPitchHits[iPitch] >= 10)
                {
                    // Record this as the new lowest Pitch
                    CSoundInput.Instance().iLowerPitchRange = iPitch;

                    // If the Pitch is too low
                    if (CSoundInput.Instance().iLowerPitchRange < 0)
                    {
                        CSoundInput.Instance().iLowerPitchRange = 0;
                    }
                }

                // If this is the highest Pitch heard so far
                if ((CSoundInput.Instance().fPitch > CSoundInput.Instance().iUpperPitchRange ||
                    CSoundInput.Instance().iUpperPitchRange <= 10) && CSoundInput.Instance().fPitch > 1.0f &&
                    miaPitchHits[iPitch] >= 10)
                {
                    // Record this as the new highest Pitch
                    CSoundInput.Instance().iUpperPitchRange = iPitch;

                    // If the High Pitch isn't at least 10 Pitches louder than the low
                    if ((CSoundInput.Instance().iUpperPitchRange - CSoundInput.Instance().iLowerPitchRange) < 10)
                    {
                        // Increase the High Pitch to 10 Pitches louder
                        CSoundInput.Instance().iUpperPitchRange = CSoundInput.Instance().iLowerPitchRange + 10;

                        // If the Pitch is too high
                        if (CSoundInput.Instance().iUpperPitchRange > 130)
                        {
                            CSoundInput.Instance().iUpperPitchRange = 130;
                        }
                    }
                }
            }
            // Else we are not Detecting the Pitch
            else
            {
                // Draw the Pitch Meter    
                mcPitchMeter.AutoDetectPitchAndUpdateMeter();
                mcPitchMeter.Draw(cGraphics);
            }

            // Show the Current Pitch
            textCurrentPitch.Text = CSoundInput.Instance().fPitch.ToString();

            // Show the High and Low Pitch Values
            numericHighPitch.Value = CSoundInput.Instance().iUpperPitchRange;
            numericLowPitch.Value = CSoundInput.Instance().iLowerPitchRange;
        }

        // Function to update the combo-box list of Profiles
        private void UpdateShownProfilesAndCurrentSettings()
        {
            // Clear the combo box of all entries
            comboProfiles.Items.Clear();

            // Loop through each of the Profiles
            CProfiles.Instance().mcProfileList.ForEach(delegate(CProfile sProfile)
            {
                // Add this Profiles Name to the combo box
                comboProfiles.Items.Add(sProfile.sName);
            });

            // Get the Name of the Profile which should be Selected
            string sName = CProfiles.Instance().mcCurrentProfile.sName;

            // Make sure the Current Profile is selected
            // If we couldn't find the Current Profile in the combo box
            if (comboProfiles.Items.IndexOf(sName) < 0)
            {
                MessageBox.Show("Could not find current profile in comboProfiles", "Error finding profile in combo box");
            }
            // Else we found which Profile Name should be selected
            else
            {
                // Select the Current Profiles Name
                comboProfiles.SelectedItem = sName;
            }

            // Use the Current Profiles Settings
            CProfiles.Instance().CopyCurrentProfileSettingsToCurrentSoundInputSettings();
        }        

        // Copy the Current Settings to the Current Profiles Settings
        private void CopyCurrentSettingsToCurrentProfilesSettings()
        {
            // Update the Current Profiles Settings
            CProfiles.Instance().mcCurrentProfile.iLowerPitch = CSoundInput.Instance().iLowerPitchRange;
            CProfiles.Instance().mcCurrentProfile.iUpperPitch = CSoundInput.Instance().iUpperPitchRange;
            CProfiles.Instance().mcCurrentProfile.sName = CSoundInput.Instance().sProfileName;
        }


        //===========================================================
        // Event Functions
        //===========================================================

        // When the form should be re-drawn
        private void formProfileSettings_Paint(object sender, PaintEventArgs e)
        {
            // Update the form
            UpdateForm(sender, e);
        }

        // When the Player presses the Close button
        private void buttonClose_Click(object sender, EventArgs e)
        {
            // Copy the Current Settings to this Profiles Settings
            CopyCurrentSettingsToCurrentProfilesSettings();

            // Save the Profiles to File
            CProfiles.Instance().SaveProfilesToFile();

            // Reset the Game type to the Main Menu
            mcGameHandle.meGame = EGames.MainMenu;

            // Close this form
            this.Close();
        }

        // When the Detect Pitch button is pressed
        private void buttonDetectPitch_Click(object sender, EventArgs e)
        {
            // If we are not currently Detecting Pitch
            if (!mbDetectingPitch)
            {
                // Start Detecting Pitch
                mbDetectingPitch = true;

                // Reset the High and Low Pitch Values
                CSoundInput.Instance().iUpperPitchRange = 10;
                CSoundInput.Instance().iLowerPitchRange = 0;

                // Change Button Text
                buttonDetectPitch.Text = "Stop Detecting Pitch";

                // Make the other controls inaccessible
                numericLowPitch.Enabled = false;
                numericHighPitch.Enabled = false;
                comboProfiles.Enabled = false;
                buttonClose.Enabled = false;
                buttonNewProfile.Enabled = false;
                buttonDeleteProfile.Enabled = false;
                buttonPitchTransitionExample.Enabled = false;

                // Reset the Pitch array to all zeros
                for (int i = 0; i < miaPitchHits.Length; i++)
                {
                    miaPitchHits[i] = 0;
                }
            }
            // Else we are currently Detecting Pitch already
            else
            {
                // So stop Detecting Pitch
                mbDetectingPitch = false;

                // Change Button Text
                buttonDetectPitch.Text = "Detect Pitch";

                // Make High and Low Pitch values accessible
                numericLowPitch.Enabled = true;
                numericHighPitch.Enabled = true;
                comboProfiles.Enabled = true;
                buttonClose.Enabled = true;
                buttonNewProfile.Enabled = true;
                buttonDeleteProfile.Enabled = true;
                buttonPitchTransitionExample.Enabled = true;
            }
        }

        // If the create a New Profile button was pressed
        private void buttonNewProfile_Click(object sender, EventArgs e)
        {
            string sName = "";
            bool bUniqueNameFound = false;
            int iNameNumber = 0;

            // Save changes to the Current Profile before creating a new one
            CopyCurrentSettingsToCurrentProfilesSettings();

            // Create a temporary Profile
            CProfile cProfile = new CProfile();

            // Find a name not already taken for this Profile
            while (!bUniqueNameFound)
            {
                // Assume that the Name we look for will be unique
                bUniqueNameFound = true;

                // Create the name to check for
                sName = "Profile" + ++iNameNumber;

                // Loop through each of the Profiles in the List
                CProfiles.Instance().mcProfileList.ForEach(delegate(CProfile sProfileToCheck)
                {
                    // If this Profile already has the Name
                    if (sProfileToCheck.sName.Equals(sName))
                    {
                        // Mark that the Name is not unique
                        bUniqueNameFound = false;
                    }
                });
            }

            // Set initial values
            cProfile.sName = sName;
            cProfile.iLowerPitch = 45;
            cProfile.iUpperPitch = 70;

            // Add this Profile to the List
            CProfiles.Instance().AddProfileToListAndSetItAsTheCurrentProfile(cProfile);

            // Update comboProfiles to show the new Profile
            UpdateShownProfilesAndCurrentSettings();
        }

        // If the Delete Profile button was pressed
        private void buttonDeleteProfile_Click(object sender, EventArgs e)
        {
            // Delete the current Profile
            CProfiles.Instance().DeleteCurrentProfileFromTheProfileList();

            // Update comboProfiles to show the new Profile List
            UpdateShownProfilesAndCurrentSettings();
        }

        // If the Profile to use was changed
        private void comboProfiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Get the new Current Profile to use
            CProfiles.Instance().mcCurrentProfile = CProfiles.Instance().mcProfileList[CProfiles.Instance().mcProfileList.IndexOf(new CProfile((string)comboProfiles.SelectedItem))];

            // Copy this Profiles Settings to the Current Settings
            CProfiles.Instance().CopyCurrentProfileSettingsToCurrentSoundInputSettings();
        }

        // If the Profiles List gets focus
        private void comboProfiles_Enter(object sender, EventArgs e)
        {
            // Save changes to the Current Profile before switching Profiles
            CopyCurrentSettingsToCurrentProfilesSettings();
        }

        // If the Profiles combo box has lost focus, check to see if the Profile Name was changed
        private void comboProfiles_Leave(object sender, EventArgs e)
        {
            // Get the New Profile Name
            string sNewName = (string)comboProfiles.Text;
            string sOldName = CProfiles.Instance().mcCurrentProfile.sName;

            // If a valid name was entered and the Name of the Profile was changed
            if (sNewName != null && !sNewName.Equals("") && !sNewName.Equals(sOldName))
            {
                // Give this Profile it's new Name
                CProfiles.Instance().mcCurrentProfile.sName = sNewName;
            }

            // Update comboProfiles to show the new Profile
            UpdateShownProfilesAndCurrentSettings();
        }

        // If the High Pitch value is changed
        private void numericHighPitch_ValueChanged(object sender, EventArgs e)
        {
            // Get the New High Pitch value
            int iNewHighPitch = (int)numericHighPitch.Value;

            // If the New High Pitch is valid
            if ((iNewHighPitch - CSoundInput.Instance().iLowerPitchRange) >= 10)
            {
                // Record the change
                CSoundInput.Instance().iUpperPitchRange = iNewHighPitch;
            }
        }

        // If the Low Pitch value is changed
        private void numericLowPitch_ValueChanged(object sender, EventArgs e)
        {
            // Get the New Low Pitch value
            int iNewLowPitch = (int)numericLowPitch.Value;

            // If the New Low Pitch is valid
            if ((CSoundInput.Instance().iUpperPitchRange - iNewLowPitch) >= 10)
            {
                // Record the change
                CSoundInput.Instance().iLowerPitchRange = iNewLowPitch;
            }
        }

        // If the Pitch Transition Example button was pressed
        private void buttonPitchTransitionExample_Click(object sender, EventArgs e)
        {
            // Play the Pitch Transition Example sound
            CFMOD.PlaySound(msSoundPitchTransitionExample, true);
        }
    }
}