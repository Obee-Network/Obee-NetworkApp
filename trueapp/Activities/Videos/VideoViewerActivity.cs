using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Com.Google.Android.Youtube.Player;
using Newtonsoft.Json;
using ObeeNetwork.Activities.Comment;
using ObeeNetwork.Activities.Tabbes;
using ObeeNetwork.Activities.Videos.Fragment;
using ObeeNetwork.Adapters;
using ObeeNetwork.Helpers.Ads;
using ObeeNetwork.Helpers.Utils;
using ObeeNetwork.SQLite;
using ObeeNetworkClient.Classes.Movies;
using ObeeNetworkClient.Requests;
using VideoController = ObeeNetwork.Helpers.Controller.VideoController;

namespace ObeeNetwork.Activities.Videos
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Keyboard | ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenLayout | ConfigChanges.ScreenSize | ConfigChanges.SmallestScreenSize | ConfigChanges.UiMode, ResizeableActivity = true)]
    public class VideoViewerActivity : AppCompatActivity, IYouTubePlayerOnInitializedListener
    {
        #region Variables Basic

        public VideoController VideoActionsController;
        private GetMoviesObject.Movie Video;
        private YouTubePlayerSupportFragment YouTubeFragment;
        private IYouTubePlayer YoutubePlayer { get; set; }
        private string VideoIdYoutube;
        private static VideoViewerActivity Instance;
        public ReplyCommentBottomSheet ReplyFragment;
        public string MoviesId;
        public VideosCommentFragment TabVideosComment;
        public VideosAboutFragment TabVideosAbout;
        private TabLayout TabLayoutView;
        private ViewPager ViewPagerView;
        private MainTabAdapter TabAdapter; 
        private AdsGoogle.AdMobRewardedVideo RewardedVideoAd;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                Window.SetSoftInputMode(SoftInput.AdjustResize);
                 
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);
                  
                SetContentView(Resource.Layout.Video_Viewer_Layout);

                Instance = this;

                MoviesId = Intent.GetStringExtra("VideoId") ?? "";

                VideoActionsController = new VideoController(this, "Viewer_Video");
                SetVideoPlayerFragmentAdapters();
               
                  
                RewardedVideoAd = AdsGoogle.Ad_RewardedVideo(this);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        protected override void OnPause()
        {
            try
            {
                TabVideosAbout?.MAdView?.Pause();
                base.OnPause();
                RewardedVideoAd?.OnResume(this);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        protected override void OnResume()
        {
            try
            {
                base.OnResume();
                TabVideosAbout?.MAdView?.Resume();
                RewardedVideoAd?.OnPause(this);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override void OnTrimMemory(TrimMemory level)
        {
            try
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                base.OnTrimMemory(level);
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
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        } 
        protected override void OnDestroy()
        {
            try
            {
                VideoActionsController.ReleaseVideo();

                if (YoutubePlayer != null && YoutubePlayer.IsPlaying)
                    YoutubePlayer?.Pause();

                TabbedMainActivity.GetInstance()?.SetOffWakeLock();

                TabVideosAbout?.MAdView?.Destroy();

                DestroyBasic();
                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
         
        #endregion
         
        #region Menu

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region Functions

        private void SetVideoPlayerFragmentAdapters()
        {
            try
            {
                TabLayoutView = FindViewById<TabLayout>(Resource.Id.tabs);
                ViewPagerView = FindViewById<ViewPager>(Resource.Id.viewPager);

                TabAdapter = new MainTabAdapter(SupportFragmentManager);

                TabVideosComment = new VideosCommentFragment();
                TabVideosAbout = new VideosAboutFragment();

                TabAdapter.AddFragment(TabVideosAbout, GetString(Resource.String.Lbl_About));
                TabAdapter.AddFragment(TabVideosComment, GetString(Resource.String.Lbl_Comment));

                ViewPagerView.CurrentItem = 2;
                ViewPagerView.OffscreenPageLimit = TabAdapter.Count;
                ViewPagerView.Adapter = TabAdapter;
                TabLayoutView.SetupWithViewPager(ViewPagerView);

                TabLayoutView.SetTabTextColors(AppSettings.SetTabDarkTheme ? Color.White : Color.Black, Color.ParseColor(AppSettings.MainColor));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static VideoViewerActivity GetInstance()
        {
            try
            {
                return Instance;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
        private void DestroyBasic()
        {
            try
            {
                RewardedVideoAd?.OnDestroy(this);
                TabLayoutView = null;
                ViewPagerView = null;
                TabAdapter = null;
                TabVideosComment = null;
                TabVideosAbout = null;
                RewardedVideoAd = null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        #endregion

        #region Back Pressed

        public override void OnBackPressed()
        {
            try
            {
                VideoActionsController.SetStopVideo();
                base.OnBackPressed();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion
         
        #region Result

        //Result
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);
                if (requestCode == 2000)
                    if (resultCode == Result.Ok)
                        VideoActionsController.RestartPlayAfterShrinkScreen();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        #region YouTube Player

        public void OnInitializationFailure(IYouTubePlayerProvider p0, YouTubeInitializationResult p1)
        {
            try
            {
                if (p1.IsUserRecoverableError)
                    p1.GetErrorDialog(this, 1).Show();
                else
                    Toast.MakeText(this, p1.ToString(), ToastLength.Short).Show();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void OnInitializationSuccess(IYouTubePlayerProvider p0, IYouTubePlayer player, bool wasRestored)
        {
            try
            {
                if (YoutubePlayer == null)
                    YoutubePlayer = player;

                if (!wasRestored)
                {
                    YoutubePlayer.LoadVideo(VideoIdYoutube);
                    //YoutubePlayer.AddFullscreenControlFlag(YouTubePlayer.FullscreenFlagControlOrientation  | YouTubePlayer.FullscreenFlagControlSystemUi  | YouTubePlayer.FullscreenFlagCustomLayout); 
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Event
          
        //Event Menu >> Reply
        public void CommentReplyClick(CommentsMoviesObject item)
        {
            try
            {
                // show dialog :
                ReplyFragment = new ReplyCommentBottomSheet();

                Bundle bundle = new Bundle();
                bundle.PutString("Type", "Movies");
                bundle.PutString("Id", MoviesId);
                bundle.PutString("Object", JsonConvert.SerializeObject(item));

                ReplyFragment.Arguments = bundle;

                ReplyFragment.Show(SupportFragmentManager, ReplyFragment.Tag);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void ReplyOnReplyClick(CommentsMoviesObject item)
        {
            try
            {
                if (ReplyFragment == null) return;
                ReplyFragment.TxtComment.Text = "";
                ReplyFragment.TxtComment.Text = "@" + item.UserData.Username + " ";
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
         

        #endregion
        public async void GetDataVideo()
        {
            try
            {
                if (!string.IsNullOrEmpty(Intent.GetStringExtra("Viewer_Video")))
                {
                    Video = JsonConvert.DeserializeObject<GetMoviesObject.Movie>(Intent.GetStringExtra("Viewer_Video"));
                    LoadDataVideo();
                }
                else
                {
                    if (!Methods.CheckConnectivity())
                    {
                        Toast.MakeText(Application.Context, Application.Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                        return;
                    }

                    (int apiStatus, var respond) = await RequestsAsync.Movies.Get_Movies("", "", MoviesId);
                    if (apiStatus == 200)
                    {
                        if (respond is GetMoviesObject result)
                        {
                            var respondList = result.Movies.Count;
                            if (respondList > 0)
                            {
                                Video = result.Movies.FirstOrDefault(w => w.Id == MoviesId);
                                LoadDataVideo();
                            } 
                        }
                    }
                    else Methods.DisplayReportResult(this, respond); 
                } 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void LoadDataVideo()
        {
            try
            {
                if (Video == null)
                    return;

                if (!string.IsNullOrEmpty(Video.Iframe))
                {
                    TabVideosAbout.LoadVideo_Data(Video);

                    if (Video.Iframe.Contains("Youtube") || Video.Iframe.Contains("youtu"))
                    {
                        var ft = SupportFragmentManager.BeginTransaction();

                        VideoIdYoutube = Video.Iframe.Split(new[] { "v=", "/" }, StringSplitOptions.None).LastOrDefault();
                         
                        if (YouTubeFragment == null)
                        {
                            YouTubeFragment = new YouTubePlayerSupportFragment();
                            YouTubeFragment.Initialize(AppSettings.YoutubeKey, this);
                            ft.Add(Resource.Id.root, YouTubeFragment, YouTubeFragment.Id.ToString() + DateTime.Now).Commit();

                            VideoActionsController.SimpleExoPlayerView.Visibility = ViewStates.Gone;
                            VideoActionsController.ReleaseVideo();
                        }
                        else
                        {
                            VideoActionsController.SimpleExoPlayerView.Visibility = ViewStates.Gone;
                            VideoActionsController.ReleaseVideo();

                            if (YouTubeFragment.IsAdded)
                                ft.Show(YouTubeFragment).Commit();
                            else
                            {
                                YouTubeFragment = new YouTubePlayerSupportFragment();
                                ft.Add(Resource.Id.root, YouTubeFragment, YouTubeFragment.Id.ToString() + DateTime.Now).Commit();
                            }
                            YouTubeFragment.View.Visibility = ViewStates.Visible;
                            YoutubePlayer?.LoadVideo(VideoIdYoutube);
                        }
                    }
                }
                else
                {
                    var dbDatabase = new SqLiteDatabase();
                    var dataVideos = dbDatabase.Get_WatchOfflineVideos_ById(Video.Id);
                    if (dataVideos != null)
                        VideoActionsController.PlayVideo(dataVideos.Source, dataVideos);
                    else
                        VideoActionsController.PlayVideo(Video.Source, Video);
                    dbDatabase.Dispose();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
          
    }
}