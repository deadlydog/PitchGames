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
    // Game States
    public enum EGames
    {
        MainMenu = 0,
        Spong = 1,
        ShootingGallery = 2,
        PitchMatcher = 3,
        TargetPractice = 4,
        ProfileSettings = 5,
        HighScores = 6
    }

    // Parent class of all Games and the Main Menu
    public class CGame
    {
        // Class variables
        public EGames meGame;

        // Constructor
        public CGame()
        {
            meGame = EGames.MainMenu;
        }

        virtual public void Update(float fTimeSinceLastUpdateInSeconds, Graphics cBackBuffer)
        {
            // This function should be overriden by the inheriting class
            // This is where the Game should update and draw itself
        }

        virtual public EGames KeyDown(KeyEventArgs e)
        {
            return meGame;
        }

        virtual public EGames MouseDown(MouseEventArgs e)
        {
            return meGame;
        }

        virtual public void MouseMove(MouseEventArgs e)
        { }
    }

    // Class to hold all sound input information
    public sealed class CSoundInput
    {
        // Sound info
        public float fPitch, fPitchLastFrame, fPitchChange;
        public float fAmplitude;
        public bool bAttack;

        // Player specific Pitch variables
        public int iLowerPitchRange;
        public int iUpperPitchRange;
        public int iMiddlePitchRange;
        public int iOneThirdOfPitchRange;
        public int iTwoThirdsOfPitchRange;

        public bool bInputReceived;         // Tells if Pitch input was received or not
        public string sProfileName;         // The Name of the current Profile

        Ventuz.OSC.UdpReader mcUDPReader;   // Used to get input from PD

        // Singleton class variables
        private static CSoundInput mcInstance = null;   // Handle to an instance of this class
        static readonly object Lock = new object();     // Lock for multi-thread safety

        // Constructor
        CSoundInput()
        {
            // Reset all data to default values
            iLowerPitchRange = iUpperPitchRange = 0;
            iMiddlePitchRange = iOneThirdOfPitchRange = iTwoThirdsOfPitchRange = 0;
            sProfileName = "";

            // Listen for incoming PD messages on port 5555
            mcUDPReader = new Ventuz.OSC.UdpReader(5555);

            // Reset commonly changed variables
            Reset();
        }

        // Return the singleton Instance of this class
        public static CSoundInput Instance()
        {
            // Lock the Instance so only one thread can use it at a time
            lock (Lock)
            {
                // If this class Instance hasn't been created yet
                if (mcInstance == null)
                {
                    // Create the Instance
                    mcInstance = new CSoundInput();
                }

                return mcInstance;
            }
        }
     
        // Reset all non Player specific variables
        public void Reset()
        {
            fPitch = fPitchLastFrame = fPitchChange = 0.0f;
            fAmplitude = 0.0f;
            bAttack = false;
            bInputReceived = false;
        }

        // Get input from the player
        public void GetPDInput(bool bKeepPitchInRange)
        {
            // Clear any previous sound input, but save the Pitch from the last frame
            float fPreviousPitch = CSoundInput.Instance().fPitch;
            CSoundInput.Instance().Reset();
            CSoundInput.Instance().fPitchLastFrame = fPreviousPitch;

            try
            {
                // Get any PD messages that might have been sent          
                Ventuz.OSC.OscBundle cOSCBundle = mcUDPReader.ReceiveBundle();

                // If there are no messages to retrieve
                if (cOSCBundle == null || cOSCBundle.Elements == null || cOSCBundle.Elements.Count <= 0)
                {
                    // Exit the function
                    return;
                }                
                
                // If valid pitch input was not received yet AND there are more OSC Bundles to read
                while (cOSCBundle != null && !CSoundInput.mcInstance.bInputReceived)
                {
                    // Read in and Store this OSC Bundles Contents
                    ReadAndStoreOSCBundleContents(cOSCBundle, bKeepPitchInRange);

                    // Get the next OSC Bundle in the queue
                    cOSCBundle = mcUDPReader.ReceiveBundle();
                } 

                // Read in the any remaining Bundles to empty the queue
                while (cOSCBundle != null)
                {
                    cOSCBundle = mcUDPReader.ReceiveBundle();
                }                
            }
            // Catch all exceptions
            catch (Exception e)
            {
                // Display the error message
                MessageBox.Show(e.ToString(), "ERROR");
            }

            // Calculate how much the Pitch changed between frames
            CSoundInput.Instance().fPitchChange = CSoundInput.Instance().fPitch - CSoundInput.Instance().fPitchLastFrame;
        }

        // Function to Read in and Store an OSC Bundles Contents
        private void ReadAndStoreOSCBundleContents(Ventuz.OSC.OscBundle cOSCBundle, bool bKeepPitchInRange)
        {
            string sInfo = "";  // Temp variable

            try
            {
                // Get the Elements from the PD Bundle
                System.Collections.ArrayList cElementList = (System.Collections.ArrayList)cOSCBundle.Elements;

                // Loop through all Elements recieved from PD
                foreach (Ventuz.OSC.OscElement cElement in cElementList)
                {
                    // Loop through each of the Elements Arguments
                    foreach (object cObject in cElement.Args)
                    {
                        // Check which type of message this is and store its value appropriately
                        switch (cElement.Address.ToString())
                        {
                            case "/Pitch":
                                // Read in the Pitch
                                float fPitch = float.Parse(cObject.ToString());

                                // If the Pitch is valid
                                if (fPitch > 1.0f)
                                {
                                    // Save that some input was received
                                    CSoundInput.Instance().bInputReceived = true;

                                    // If the Pitch should be kept in range
                                    if (bKeepPitchInRange)
                                    {
                                        // Make sure the given Pitch is within range
                                        if (fPitch < CSoundInput.Instance().iLowerPitchRange)
                                        {
                                            fPitch = CSoundInput.Instance().iLowerPitchRange;
                                        }
                                        else if (fPitch > CSoundInput.Instance().iUpperPitchRange)
                                        {
                                            fPitch = CSoundInput.Instance().iUpperPitchRange;
                                        }
                                    }

                                    // Save the Pitch value
                                    CSoundInput.Instance().fPitch = fPitch;
                                }
                                break;

                            case "/Amplitude":
                                // If this Elements Pitch was valid
                                if (CSoundInput.Instance().bInputReceived)
                                {
                                    // Read in and save the Amplitude value
                                    CSoundInput.Instance().fAmplitude = float.Parse(cObject.ToString());
                                }
                                break;

                            case "/Attack":
                                // If this Elements Pitch was valid
                                if (CSoundInput.Instance().bInputReceived)
                                {
                                    // Read in and save the Attack value
                                    CSoundInput.Instance().bAttack = (int.Parse(cObject.ToString()) == 1) ? true : false;
                                }
                                break;

                            default:
                                MessageBox.Show("Unknown PD Address: " + cElement.Address.ToString(), "ERROR");
                                break;
                        }
                        sInfo += cElement.Address.ToString() + ":" + cObject.ToString() + "   ";
                    }
                }
            }
            // Catch all exceptions
            catch (Exception e)
            {
                // Display the error message
                MessageBox.Show(e.ToString(), "ERROR");
            }

            // If valid pitch input was received
            if (CSoundInput.mcInstance.bInputReceived)
            {
                // Output the found values for debugging purposes
                //                int iRow = listBox1.Items.Add(sInfo);
                //                listBox1.SetSelected(iRow, true);
                Console.WriteLine(sInfo);
            }
        }
    }

    // Class to hold a Midi note's info
    public class CMidiInfo
    {
        // Sound info
        public int iNote;       // The Note/Pitch of the instrument
        public int iVelocity;   // The Amplitude/Volume of the Note played
        public int iChannel;    // The Channel/Instrument of the Note played
        public DateTime cTime;  // The Time the Note was played

        // Default constructor
        public CMidiInfo()
        {
            Purge();
        }

        // Reset all class variables to default values
        public void Purge()
        {
            iNote = iVelocity = iChannel = 0;
            cTime = new DateTime();
        }
    }

    // Class to hold all Midi input information
    public sealed class CMidiInput
    {
        public List<CMidiInfo> cMidiInfoList;   // Holds all of the Midi Info which is received

        private Ventuz.OSC.UdpReader mcUDPReader;       // Used to get input from PD

        // Singleton class variables
        private static CMidiInput mcInstance = null;    // Handle to an instance of this class
        static readonly object Lock = new object();     // Lock for multi-thread safety

        // Constructor
        CMidiInput()
        {            
            // Listen for incoming PD messages on port 4444
            mcUDPReader = new Ventuz.OSC.UdpReader(4444);

            // Reset class variables
            Reset();
        }

        // Return the singleton Instance of this class
        public static CMidiInput Instance()
        {
            // Lock the Instance so only one thread can use it at a time
            lock (Lock)
            {
                // If this class Instance hasn't been created yet
                if (mcInstance == null)
                {
                    // Create the Instance
                    mcInstance = new CMidiInput();
                }

                return mcInstance;
            }
        }

        // Reset all non Player specific variables
        public void Reset()
        {
            // Reset all data to default values
            cMidiInfoList = new List<CMidiInfo>();
        }

        // Get Midi input from PD
        public void GetPDInput()
        {
            // Clear any previous Midi input
            CMidiInput.Instance().cMidiInfoList.Clear();

            try
            {
                // Get any PD messages that might have been sent          
                Ventuz.OSC.OscBundle cOSCBundle = mcUDPReader.ReceiveBundle();

                // If there are no messages to retrieve
                if (cOSCBundle == null || cOSCBundle.Elements == null || cOSCBundle.Elements.Count <= 0)
                {
                    // Exit the function
                    return;
                }

                // Read in and Store all of the OSC Bundles contents
                while (cOSCBundle != null)
                {
                    // Read in and Store the Contents of the OSC Bundle
                    ReadAndStoreOSCBundleContents(cOSCBundle);

                    // Get the next OSC Bundle from the queue
                    cOSCBundle = mcUDPReader.ReceiveBundle();
                }             
            }
            // Catch all exceptions
            catch (Exception e)
            {
                // Display the error message
                MessageBox.Show(e.ToString(), "ERROR");
            }
        }

        // Function to Read and Store the OSC Bundles Contents
        private void ReadAndStoreOSCBundleContents(Ventuz.OSC.OscBundle cOSCBundle)
        {
            string sInfo = "";                      // Temp variable to display read in values
            CMidiInfo cMidiInfo = new CMidiInfo();  // Temp variable to hold the read in Midi Info

            // Initialize the variables used to hold the Notes Time
            int iHour = 0, iMinute = 0, iSecond = 0, iMillisecond = 0;

            try
            {
                // Get the Elements from the PD Bundle
                System.Collections.ArrayList cElementList = (System.Collections.ArrayList)cOSCBundle.Elements;

                // Loop through all Elements recieved from PD
                foreach (Ventuz.OSC.OscElement cElement in cElementList)
                {
                    // Loop through each of the Elements Arguments
                    foreach (object cObject in cElement.Args)
                    {
                        // Check which type of message this is and store its value appropriately
                        switch (cElement.Address.ToString())
                        {
                            case "/Hour":
                                // Read in and store the Hour
                                iHour = int.Parse(cObject.ToString());
                            break;

                            case "/Minute":
                                // Read in and store the Hour
                                iMinute = int.Parse(cObject.ToString());
                            break;

                            case "/Second":
                                // Read in and store the Hour
                                iSecond = int.Parse(cObject.ToString());
                            break;

                            case "/Millisecond":
                                // Read in and store the Hour
                                iMillisecond = int.Parse(cObject.ToString());
                            break;

                            case "/Velocity":
                                // Read in and store the Velocity
                                cMidiInfo.iVelocity = int.Parse(cObject.ToString());
                            break;

                            case "/Note":
                                // Read in and store the Note value
                                cMidiInfo.iNote = int.Parse(cObject.ToString());
                            break;

                            case "/Channel":
                                // Read in and store the Channel value
                                cMidiInfo.iChannel = int.Parse(cObject.ToString());
                            break;

                            default:
                                MessageBox.Show("Unknown PD Address: " + cElement.Address.ToString(), "ERROR");
                            break;
                        }
                        sInfo += cElement.Address.ToString() + ":" + cObject.ToString() + "   ";
                    }
                }
            }
            // Catch all exceptions
            catch (Exception e)
            {
                // Display the error message
                MessageBox.Show(e.ToString(), "ERROR");
            }

            // If this was not a Note Off message
            if (cMidiInfo.iVelocity > 0)
            {
                // Set the Time of this Midi Note
                DateTime cTempDate = DateTime.Now;
                cMidiInfo.cTime = new DateTime(cTempDate.Year, cTempDate.Month, cTempDate.Day, iHour, iMinute, iSecond, iMillisecond);

                // Store the read in Midi Info
                cMidiInfoList.Add(cMidiInfo);

                // Output the found values for debugging purposes
                //                int iRow = listBox1.Items.Add(sInfo);
                //                listBox1.SetSelected(iRow, true);
                Console.WriteLine(sInfo);
            }
        }        
    }

    // Class to hold a Profiles info
    public class CProfile
    {
        public string sName;
        public int iUpperPitch, iLowerPitch;
        public int iScoreSpong, iScorePitchMatcher, iScoreTargetPractice;
        public int iScoreShootingGallerySong1, iScoreShootingGallerySong2, iScoreShootingGallerySong3;

        // Default constructor
        public CProfile()
        {
            sName = "";
            iUpperPitch = iLowerPitch = 0;
            iScoreSpong = iScorePitchMatcher = iScoreTargetPractice = 0;
            iScoreShootingGallerySong1 = iScoreShootingGallerySong2 = iScoreShootingGallerySong3 = 0;
        }

        // Explicit constructor
        public CProfile(string _sName)
        {
            sName = _sName;
            iUpperPitch = iLowerPitch = 0;
            iScoreSpong = iScorePitchMatcher = iScoreTargetPractice = 0;
            iScoreShootingGallerySong1 = iScoreShootingGallerySong2 = iScoreShootingGallerySong3 = 0;
        }

        // Overload the == operator
        public static bool operator ==(CProfile s1, CProfile s2)
        {
            return s1.sName.Equals(s2.sName);
        }

        // Overload the != operator
        public static bool operator !=(CProfile s1, CProfile s2)
        {
            return !(s1 == s2);
        }

        // Override the Equals function
        public override bool Equals(object o)
        {
            if (!(o is CProfile))
            {
                return false;
            }
            return this == (CProfile)o;
        }

        // Override the GetHashCode function
        public override int GetHashCode()
        {
            return sName.GetHashCode();
        }
    }

    // Class to manage Profiles and stats
    public sealed class CProfiles
    {
        // Class Variables
        public List<CProfile> mcProfileList;    // Holds all of the Profiles
        public CProfile mcCurrentProfile;       // Handle to the Current Profile

        private const string msFILE_NAME = "../../Data/Profiles.dat";

        // Singleton class variables
        private static CProfiles mcInstance = null;   // Handle to an instance of this class
        static readonly object Lock = new object();     // Lock for multi-thread safety

        // Constructor
        CProfiles()
        {
            // Create an instance of the List
            mcProfileList = new List<CProfile>();

            // Set the initial Current Profile
            mcCurrentProfile = null;

            // Load Profiles from the file
            LoadProfilesFromFile();
        }

        // Return the singleton Instance of this class
        public static CProfiles Instance()
        {
            // Lock the Instance so only one thread can use it at a time
            lock (Lock)
            {
                // If this class Instance hasn't been created yet
                if (mcInstance == null)
                {
                    // Create the Instance
                    mcInstance = new CProfiles();
                }

                return mcInstance;
            }
        }

        // Function to Add a new Profile into the Profile List
        public void AddProfileToListAndSetItAsTheCurrentProfile(CProfile cProfile)
        {
            // Add the Profile into the List and use it as the new Current Profile
            mcProfileList.Add(cProfile);
            mcCurrentProfile = mcProfileList[mcProfileList.IndexOf(cProfile)];
        }

        // Function to Delete the Current Profile from the Profile List
        public void DeleteCurrentProfileFromTheProfileList()
        {
            // Delete the Current Profile from the Profile List
            mcProfileList.Remove(mcCurrentProfile);

            // If there are still some Profiles in the List
            if (mcProfileList.Count > 0)
            {
                // Use the first Profile as the new Profile
                mcCurrentProfile = mcProfileList[0];
            }
            // Else there are no more Profiles in the List
            else
            {
                // Create the Default Profile and use it as the Current Profile
                ClearAllProfilesExceptDefault();
            }
        }

        // Clear all Profiles except the default
        public void ClearAllProfilesExceptDefault()
        {
            // Clear the List
            mcProfileList.Clear();

            // Create the Default Profile
            CProfile sDefaultProfile = new CProfile();
            sDefaultProfile.sName = "Default Profile";
            sDefaultProfile.iLowerPitch = 45;
            sDefaultProfile.iUpperPitch = 60;

            // Insert it into the List
            mcProfileList.Add(sDefaultProfile);

            // Store the handle to the Current Profile
            mcCurrentProfile = mcProfileList[mcProfileList.IndexOf(sDefaultProfile)];

            // Use this Profiles Settings
            CopyCurrentProfileSettingsToCurrentSoundInputSettings();
        }

        // Copy a Profiles Settings to the Current Settings
        public void CopyCurrentProfileSettingsToCurrentSoundInputSettings()
        {
            // Copy the given Profiles info to the Current Settings
            CSoundInput.Instance().iLowerPitchRange = mcCurrentProfile.iLowerPitch;
            CSoundInput.Instance().iOneThirdOfPitchRange = mcCurrentProfile.iLowerPitch + (int)((mcCurrentProfile.iUpperPitch - mcCurrentProfile.iLowerPitch) / 3.0f);
            CSoundInput.Instance().iTwoThirdsOfPitchRange = mcCurrentProfile.iLowerPitch + (int)(((mcCurrentProfile.iUpperPitch - mcCurrentProfile.iLowerPitch) / 3.0f) * 2.0f);
            CSoundInput.Instance().iMiddlePitchRange = mcCurrentProfile.iLowerPitch + (int)((mcCurrentProfile.iUpperPitch - mcCurrentProfile.iLowerPitch) / 2.0f);
            CSoundInput.Instance().iUpperPitchRange = mcCurrentProfile.iUpperPitch;
            CSoundInput.Instance().sProfileName = mcCurrentProfile.sName;
        }

        // Load Profiles in from the file, overwriting any current ones
        public void LoadProfilesFromFile()
        {
            // Clear the List
            mcProfileList.Clear();

            try
            {
                // Open an Input File Stream
                FileStream cInStream = new FileStream(msFILE_NAME, FileMode.Open, FileAccess.Read);
                BinaryReader cBinaryIn = new BinaryReader(cInStream);

                // Read in the name of the Current Profile first
                string sCurrentProfileName = cBinaryIn.ReadString();

                // While we haven't reached the end of the file
                while (cBinaryIn.PeekChar() > 0)
                {
                    // Create a new temp Profile
                    CProfile sProfile = new CProfile();

                    // Read in the Profiles data
                    sProfile.sName = cBinaryIn.ReadString();
                    sProfile.iLowerPitch = cBinaryIn.ReadInt32();
                    sProfile.iUpperPitch = cBinaryIn.ReadInt32();
                    sProfile.iScoreSpong = cBinaryIn.ReadInt32();
                    sProfile.iScoreShootingGallerySong1 = cBinaryIn.ReadInt32();
                    sProfile.iScoreShootingGallerySong2 = cBinaryIn.ReadInt32();
                    sProfile.iScoreShootingGallerySong3 = cBinaryIn.ReadInt32();
                    sProfile.iScorePitchMatcher = cBinaryIn.ReadInt32();
                    sProfile.iScoreTargetPractice = cBinaryIn.ReadInt32();

                    // Store this Profile in the List
                    mcProfileList.Add(sProfile);
                }

                // Close the file
                cBinaryIn.Close();

                // Save the handle to the Current Profile
                mcCurrentProfile = mcProfileList[mcProfileList.IndexOf(new CProfile(sCurrentProfileName))];

                // Use the Current Profiles Settings
                CopyCurrentProfileSettingsToCurrentSoundInputSettings();
            }
            // If the end of the stream is reached before reading in a full Profile
            catch (EndOfStreamException)
            {
                MessageBox.Show("End of file reached before full Profile was read in. Using default profile", "Error Loading Profiles");
                
                // Load the Default Profile
                ClearAllProfilesExceptDefault();
            }
            // If the file was not found
            catch (FileNotFoundException)
            {
                MessageBox.Show("Could not find Profiles.dat to load profiles from. Using default profile", "Error locating file Profiles.dat");

                // Load the Default Profile
                ClearAllProfilesExceptDefault();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error Loading Profiles; Using Default Profile");

                // Load the Default Profile
                ClearAllProfilesExceptDefault();
            }
        }

        // Save the Profiles to the file
        public void SaveProfilesToFile()
        {
            try
            {
                // Open an Output File Stream
                FileStream cOutStream = new FileStream(msFILE_NAME, FileMode.Create, FileAccess.Write);
                BinaryWriter cBinaryOut = new BinaryWriter(cOutStream);

                // Save the Name of the Current Profile first
                cBinaryOut.Write(mcCurrentProfile.sName);

                // Loop through all of the Profiles and save them
                mcProfileList.ForEach(delegate(CProfile sProfile)
                {
                    // Write this profiles info to the file
                    cBinaryOut.Write(sProfile.sName);
                    cBinaryOut.Write(sProfile.iLowerPitch);
                    cBinaryOut.Write(sProfile.iUpperPitch);
                    cBinaryOut.Write(sProfile.iScoreSpong);
                    cBinaryOut.Write(sProfile.iScoreShootingGallerySong1);
                    cBinaryOut.Write(sProfile.iScoreShootingGallerySong2);
                    cBinaryOut.Write(sProfile.iScoreShootingGallerySong3);
                    cBinaryOut.Write(sProfile.iScorePitchMatcher);
                    cBinaryOut.Write(sProfile.iScoreTargetPractice);
                });

                // Close the file
                cBinaryOut.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error Loading Profiles");
            }
        }
    }

    // Class to Hold the Pitch Meter
    public class CPitchMeter
    {
        private int miWidth;                        // Width of the Pitch Meter
        private int miHeight;                       // Height of the Pitch Meter
        private Image mcPitchIndicatorImage;        // Image to use for the Pitch Indicator

        private int miX;                            // X Position of the Pitch Meter
        private int miY;                            // Y Position of the Pitch Meter

        private bool mbShowHalfPointMarker;         // If the Half Way Point should be shown on the Meter
        private bool mbShowThirdsMarkers;           // If the Thirds Points should be shown on the Meter
        private bool mbShowFourthsMarkers;          // If the Fourths Points should be shown on the Meter

        public float mfPitchIndicatorUnitPosition;  // The unit (0.0 - 1.0) Position of the Pitch Indicator
        private bool mbInputReceived;               // Tells if Pitch Input was Received or not

        // Default Constructor
        public CPitchMeter(int iXPosition, int iYPosition)
        {
            // Initialize class variables
            Initialize(iXPosition, iYPosition, 5, 300);          
        }

        // Explicit Constructor
        public CPitchMeter(int iXPosition, int iYPosition, int iWidth, int iHeight)
        {
            // Initialize class variables
            Initialize(iXPosition, iYPosition, iWidth, iHeight);
        }

        // Function to initialize the class variables
        private void Initialize(int iXPosition, int iYPosition, int iWidth, int iHeight)
        {
            // Initialize class variables
            miX = iXPosition;
            miY = iYPosition;
            miWidth = iWidth;
            miHeight = iHeight;
            mfPitchIndicatorUnitPosition = 0.5f;
            mbInputReceived = false;
            
            mbShowHalfPointMarker = false;
            mbShowThirdsMarkers = false;
            mbShowFourthsMarkers = false;

            try
            {
                // Load the Pitch Indicator Image
                mcPitchIndicatorImage = Image.FromFile("../../Data/Images/PitchMeterSlider.png");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Exception caught");
            }
        }

        // Function to Automatically Update the Pitch Meter
        public void AutoDetectPitchAndUpdateMeter()
        {
            // If some Pitch Input was received
            if (CSoundInput.Instance().bInputReceived)
            {
                // Update the Pitch Indicators Position
                mfPitchIndicatorUnitPosition = (float)(CSoundInput.Instance().fPitch - CSoundInput.Instance().iLowerPitchRange) / (float)(CSoundInput.Instance().iUpperPitchRange - CSoundInput.Instance().iLowerPitchRange);

                // Make sure the new value is valid
                if (mfPitchIndicatorUnitPosition < 0.0f)
                {
                    mfPitchIndicatorUnitPosition = 0.0f;
                }
                else if (mfPitchIndicatorUnitPosition > 1.0f)
                {
                    mfPitchIndicatorUnitPosition = 1.0f;
                }

                // Record that some Pitch Input was received
                mbInputReceived = true;
            }
            // Else no Pitch Input was received
            else
            {
                mbInputReceived = false;
            }
        }

        // Function to Draw the Pitch Meter to the given Graphics object
        public void Draw(Graphics cBackBuffer)
        {
            // Initialize the Pen to use for drawing
            Pen cPen = new Pen(Color.Gray);

            // If some Input was Received
            if (mbInputReceived)
            {
                // Draw the Pitch Meter in White
                cPen = new Pen(Color.White);
            }

            // Draw the Pitch Meter            
            cBackBuffer.DrawRectangle(cPen, miX, miY, miWidth, miHeight);

            // If the Half Point Marker should be shown
            if (mbShowHalfPointMarker || mbShowFourthsMarkers)
            {
                cBackBuffer.DrawRectangle(cPen, (miX - 8), miY + (miHeight / 2.0f), 20, 2);
            }

            // If the Thirds Markers should be shown
            if (mbShowThirdsMarkers)
            {
                cBackBuffer.DrawRectangle(cPen, (miX - 8), miY + (miHeight / 3.0f), 20, 2);
                cBackBuffer.DrawRectangle(cPen, (miX - 8), miY + ((miHeight / 3.0f) * 2.0f), 20, 2);
            }

            // If the Fourths Markers should be shown
            if (mbShowFourthsMarkers)
            {
                cBackBuffer.DrawRectangle(cPen, (miX - 8), miY + (miHeight / 4.0f), 20, 2);
                cBackBuffer.DrawRectangle(cPen, (miX - 8), miY + ((miHeight / 4.0f) * 3.0f), 20, 2);
            }

            // Calculate where the Pitch Indicator should appear
            int iIndicatorPixelPosition = (miY + miHeight) - (int)(miHeight * mfPitchIndicatorUnitPosition);

            // Draw the Pitch Indicator
            cBackBuffer.DrawImage(mcPitchIndicatorImage, (miX - 20), iIndicatorPixelPosition - 9, 48, 18);
        }

        // Function to update the Pitch Indicators Position
        public void UpdatePitchIndicatorsPosition(float fUnitPosition)
        {
            // Make sure the given Position is valid
            if (fUnitPosition < 0.0f)
            {
                fUnitPosition = 0.0f;
            }
            else if (fUnitPosition > 1.0f)
            {
                fUnitPosition = 1.0f;
            }

            // Update the Pitch Indicators Position
            mfPitchIndicatorUnitPosition = fUnitPosition;
        }

        // Set if the Pitch Meter should be shown as Receiving Pitch or not
        public void SetPitchInputReceived(bool bPitchInputReceived)
        {
            mbInputReceived = bPitchInputReceived;
        }

        // Function to display/hide the Half Point Marker
        public void ShowHalfPointMarker(bool bShow)
        {
            mbShowHalfPointMarker = bShow;
        }

        // Function to display/hide the Thirds Markers
        public void ShowThirdsMarkers(bool bShow)
        {
            mbShowThirdsMarkers = bShow;
        }

        // Function to display/hide the Fourths Markers
        public void ShowFourthsMarkers(bool bShow)
        {
            mbShowFourthsMarkers = bShow;
        }
    }

    // Singleton class used to access FMOD globally
    public sealed class CFMOD
    {
        private static FMOD.System mcFMODSystem = null; // Handle to the FMOD System object
        private static CFMOD mcInstance = new CFMOD();  // Create an instance of this class
        public static bool mbSoundOn;

        // Constructor
        CFMOD()
        {
            // Sound turned on by default
            mbSoundOn = true;

            // Create the FMOD System Object
            CheckResult(FMOD.Factory.System_Create(ref mcFMODSystem), "FMOD System Create Error");

            // Setup FMOD
            CheckResult(mcFMODSystem.init(32, FMOD.INITFLAG.NORMAL, (IntPtr)(0)), "FMOD Init Error");
        }

        // Deconstructor
        ~CFMOD()
        {
            // Shut FMOD down
            mcFMODSystem.release();
        }

        // Function to get a handle to the FMOD System object
        static public FMOD.System GetSystem()
        {
            return mcFMODSystem;
        }

        // Function to check a FMOD Result and display any error messages
        static public void CheckResult(FMOD.RESULT sFMODResult, string sErrorCaption)
        {
            // If there was a problem
            if (sFMODResult != FMOD.RESULT.OK)
            {
                // Display the error message
                MessageBox.Show(FMOD.Error.String(sFMODResult), sErrorCaption);
            }
        }

        // Function to Play a FMOD Sound
        static public FMOD.Channel PlaySound(FMOD.Sound sFMODSound, bool bForcePlaySound)
        {
            // Dummy Channel Handle
            FMOD.Channel sFMODChannelHandle = new FMOD.Channel();

            // If Sounds should be played
            if (mbSoundOn || bForcePlaySound)
            {
                // Play Sound
                CFMOD.CheckResult(CFMOD.GetSystem().playSound(FMOD.CHANNELINDEX.FREE, sFMODSound, false, ref sFMODChannelHandle), "FMOD Play Sound Error");
            }

            // Return the Channel the sound is playing on
            return sFMODChannelHandle;
        }
    }
}