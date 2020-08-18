using System;
using System.Threading.Tasks;
using System.Timers;
using Android;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using AT.Markushi.UI;
using Com.Sothree.Slidinguppanel;
using ObeeNetwork.Helpers.Controller;
using ObeeNetwork.Helpers.Fonts;
using ObeeNetwork.Helpers.Model;
using ObeeNetwork.Helpers.Utils;
using ObeeNetworkClient.Classes.Posts;

namespace ObeeNetwork.Activities.AddPost
{
    public class VoiceRecorder : BottomSheetDialogFragment
    {
        #region Variables Basic

        private AddPostActivity MainActivityContext;

        private TextView IconClose, IconMicrophone;
        private CircleButton RecordPlayButton, RecordCloseButton, SendRecordButton, BtnVoice;
        private LinearLayout RecordLayout;
        private SeekBar VoiceSeekBar;
        private string RecordFilePath, TextRecorder;
        private Methods.AudioRecorderAndPlayer AudioPlayerClass;
        private Timer TimerSound;
        private bool IsRecording;

        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
            MainActivityContext = (AddPostActivity)Activity;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                var contextThemeWrapper = AppSettings.SetTabDarkTheme ? new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Dark_Base) : new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Base);

                // clone the inflater using the ContextThemeWrapper 
                LayoutInflater localInflater = inflater.CloneInContext(contextThemeWrapper);

                View view = localInflater.Inflate(Resource.Layout.DialogVoiceRecorder, container, false);

                InitComponent(view);
                 
                return view;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
        
        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                IconClose = view.FindViewById<TextView>(Resource.Id.IconBack);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconClose, IonIconsFonts.Close);
                IconClose.Click += IconClose_Click;

                IconMicrophone = view.FindViewById<TextView>(Resource.Id.iconMicrophone);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconMicrophone, IonIconsFonts.AndroidMicrophone);

                RecordLayout = view.FindViewById<LinearLayout>(Resource.Id.recordLayout);
                
                RecordPlayButton = view.FindViewById<CircleButton>(Resource.Id.playButton);
                RecordPlayButton.Tag = "Stop";
                RecordPlayButton.Click += RecordPlayButton_Click;
                 
                RecordCloseButton = view.FindViewById<CircleButton>(Resource.Id.closeRecordButton);
                RecordCloseButton.Click += RecordCloseButtonClick;

                SendRecordButton = view.FindViewById<CircleButton>(Resource.Id.sendRecordButton);
                SendRecordButton.Visibility = ViewStates.Visible;
                SendRecordButton.Click += SendRecordButton_Click;

                VoiceSeekBar = view.FindViewById<SeekBar>(Resource.Id.voiceseekbar);
                VoiceSeekBar.Progress = 0;
                 
                BtnVoice = view.FindViewById<CircleButton>(Resource.Id.startRecordButton);
                BtnVoice.LongClickable = true;
                BtnVoice.Tag = "Free";
                BtnVoice.LongClick += BtnVoiceOnLongClick;
                BtnVoice.Touch += BtnVoiceOnTouch;
                 
                AudioPlayerClass = new Methods.AudioRecorderAndPlayer("");
                TimerSound = new Timer();
                TextRecorder = "";
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Event
         
        private void SendRecordButton_Click(object sender, EventArgs e)
        {
            try
            { 
                MainActivityContext.NameAlbumButton.Visibility = ViewStates.Gone;

                //remove file the type
                MainActivityContext.AttachmentsAdapter.RemoveAll();

                var attach = new Attachments
                {
                    Id = MainActivityContext.AttachmentsAdapter.AttachmentList.Count + 1,
                    TypeAttachment = "postMusic",
                    FileSimple = "Audio_File",
                    FileUrl = RecordFilePath
                };

                MainActivityContext.AttachmentsAdapter.Add(attach);

                MainActivityContext.SlidingUpPanel.SetPanelState(SlidingUpPanelLayout.PanelState.Collapsed);

                Dismiss();
            }
            catch (Exception ez)
            {
                Console.WriteLine(ez);
            }
        }

        //Back
        private void IconClose_Click(object sender, EventArgs e)
        {
            try
            {
                Dismiss();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
         
        private void RecordCloseButtonClick(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(RecordFilePath))
                    AudioPlayerClass.StopAudioPlay();

                if (UserDetails.SoundControl)
                    Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("Error.mp3");
                 
                AudioPlayerClass.Delete_Sound_Path(RecordFilePath);

                RecordLayout.Visibility = ViewStates.Gone;

                BtnVoice.SetImageResource(0);
                BtnVoice.Tag = "Free";

                RecordFilePath = ""; 
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void RecordPlayButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(RecordFilePath))
                {
                    if (RecordPlayButton.Tag.ToString() == "Stop")
                    {
                        RecordPlayButton.Tag = "Playing";
                        RecordPlayButton.SetColor(Color.ParseColor("#efefef"));
                        RecordPlayButton.SetImageResource(Resource.Drawable.ic_stop_dark_arrow);

                        AudioPlayerClass.PlayAudioFromPath(RecordFilePath);
                        VoiceSeekBar.Max = AudioPlayerClass.Player.Duration;
                        TimerSound.Interval = 1000;
                        TimerSound.Elapsed += TimerSound_Elapsed;
                        TimerSound.Start();
                    }
                    else
                    {
                        RestPlayButton();
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void TimerSound_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (AudioPlayerClass.Player.CurrentPosition + 50 >= AudioPlayerClass.Player.Duration &&
                    AudioPlayerClass.Player.CurrentPosition + 50 <= AudioPlayerClass.Player.Duration + 20)
                {
                    VoiceSeekBar.Progress = AudioPlayerClass.Player.Duration;
                    RestPlayButton();
                    TimerSound.Stop();
                }
                else if (VoiceSeekBar.Max != AudioPlayerClass.Player.Duration && AudioPlayerClass.Player.Duration == 0)
                {
                    RestPlayButton();
                    VoiceSeekBar.Max = AudioPlayerClass.Player.Duration;
                }
                else
                {
                    VoiceSeekBar.Progress = AudioPlayerClass.Player.CurrentPosition;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void RestPlayButton()
        {
            try
            {
                Activity.RunOnUiThread(() =>
                {
                    RecordPlayButton.Tag = "Stop";
                    RecordPlayButton.SetColor(Color.White);
                    RecordPlayButton.SetImageResource(Resource.Drawable.ic_play_dark_arrow);
                    AudioPlayerClass.Player.Stop();
                    VoiceSeekBar.Progress = 0;
                });

                TimerSound.Stop();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //record voices ( Permissions is 102 )
        private void BtnVoiceOnLongClick(object sender, View.LongClickEventArgs e)
        {
            try
            {
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    StartRecording();
                }
                else
                {
                    //Check to see if any permission in our group is available, if one, then all are
                    if (Activity.CheckSelfPermission(Manifest.Permission.RecordAudio) == Permission.Granted)
                    {
                        StartRecording();
                    }
                    else
                    {
                        new PermissionsController(Activity).RequestPermission(102);
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void BtnVoiceOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                var handled = false;

                if (e.Event.Action == MotionEventActions.Up)
                {
                    try
                    {
                        if (IsRecording)
                        {
                            AudioPlayerClass.StopRecourding();
                            RecordFilePath = AudioPlayerClass.GetRecorded_Sound_Path();

                            BtnVoice.SetColorFilter(Color.ParseColor(AppSettings.MainColor));
                            BtnVoice.SetImageResource(0);
                            BtnVoice.Tag = "tick";

                            if (TextRecorder == "Recording")
                            {
                                if (!string.IsNullOrEmpty(RecordFilePath))
                                { 
                                    Console.WriteLine("FilePath" + RecordFilePath);

                                    RecordLayout.Visibility = ViewStates.Visible; 
                                }

                                TextRecorder = "";
                            }

                            IsRecording = false;
                        }
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                    }

                    BtnVoice.Pressed = false;
                    handled = true;
                }

                e.Handled = handled;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private async void StartRecording()
        {
            try
            {
                if (BtnVoice.Tag?.ToString() == "Free")
                {
                    //Set Record Style
                    IsRecording = true;

                    if (UserDetails.SoundControl)
                        Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("RecourdVoiceButton.mp3");

                    if (TextRecorder != null && TextRecorder != "Recording")
                        TextRecorder = "Recording";

                    BtnVoice.SetColorFilter(Color.ParseColor("#FA3C4C"));
                    BtnVoice.SetImageResource(Resource.Drawable.ic_stop_white_24dp);

                    AudioPlayerClass = new Methods.AudioRecorderAndPlayer("");

                    //Start Audio record
                    await Task.Delay(600);
                    AudioPlayerClass.StartRecourding();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

    }
}