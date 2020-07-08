using System;
using System.Timers;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.View.Animation;
using Android.Views;
using Android.Widget;
using AT.Markushi.UI;
using ObeeNetwork.Helpers.Model;
using ObeeNetwork.Helpers.Utils;
using Exception = System.Exception;

namespace ObeeNetwork.Activities.Comment.Fragment
{
    public class RecordSoundFragment : Android.Support.V4.App.Fragment
    {
        private View RecordSoundFragmentView;
        private CircleButton RecordPlayButton, RecordCloseButton;
        private SeekBar VoiceSeekBar;
        public string RecordFilePath;
        private Methods.AudioRecorderAndPlayer AudioPlayerClass;
        private CommentActivity MainActivityView;
        private Timer TimerSound;
         
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                RecordSoundFragmentView = inflater.Inflate(Resource.Layout.RecourdSoundFragment, container, false);

                RecordFilePath = Arguments.GetString("FilePath");

                MainActivityView = ((CommentActivity)Activity);
                MainActivityView.BtnVoice.SetImageResource(Resource.Drawable.microphone);
                MainActivityView.BtnVoice.Tag = "Audio";

                RecordPlayButton = RecordSoundFragmentView.FindViewById<CircleButton>(Resource.Id.playButton);
                RecordCloseButton = RecordSoundFragmentView.FindViewById<CircleButton>(Resource.Id.closeRecordButton);

                VoiceSeekBar = RecordSoundFragmentView.FindViewById<SeekBar>(Resource.Id.voiceseekbar);
               
                VoiceSeekBar.Progress = 0;
                RecordCloseButton.Click += RecordCloseButtonClick;
                RecordPlayButton.Click += RecordPlayButton_Click;
                RecordPlayButton.Tag = "Stop";

                AudioPlayerClass = new Methods.AudioRecorderAndPlayer(MainActivityView.PostId);
                TimerSound = new Timer();

                return RecordSoundFragmentView;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
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

                var fragmentHolder = Activity.FindViewById<FrameLayout>(Resource.Id.TopFragmentHolder);

                AudioPlayerClass.Delete_Sound_Path(RecordFilePath);
                var interpolator = new FastOutSlowInInterpolator();
                fragmentHolder.Animate().SetInterpolator(interpolator).TranslationY(1200).SetDuration(300);
                Activity.SupportFragmentManager.BeginTransaction().Remove(this).Commit();

                MainActivityView.BtnVoice.SetImageResource(Resource.Drawable.microphone);
                MainActivityView.BtnVoice.Tag = "Free";
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
                MainActivityView.RunOnUiThread(() =>
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

        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

    }
}