using System;
using Android.Gms.Ads;
using Android.OS;
using Android.Views;
using Android.Widget;
using Com.Luseen.Autolinklibrary;
using ObeeNetwork.Helpers.Ads;
using ObeeNetwork.Helpers.Fonts;
using ObeeNetwork.Helpers.Utils;
using ObeeNetworkClient.Classes.Movies;

namespace ObeeNetwork.Activities.Videos.Fragment
{
    public class VideosAboutFragment : Android.Support.V4.App.Fragment
    {
        #region  Variables Basic

        private VideoViewerActivity GlobalContext;
        public AdView MAdView;
        private TextView QualityIcon, ViewsIcon, ShareIcon, MoreIcon, ShowMoreDescriptionIcon, VideoVideoCategory, VideoStars, VideoTag, VideoQualityTextView, VideoViewsNumber, VideoVideoDate, VideoTittle;
        private LinearLayout VideoDescriptionLayout, ShareButton, MoreButton;
        private AutoLinkTextView VideoVideoDescription;
        private TextSanitizer TextSanitizerAutoLink;

        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
            GlobalContext = (VideoViewerActivity)Activity;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                View view = inflater.Inflate(Resource.Layout.VideoAboutLayout, container, false);

                InitComponent(view);
                AddOrRemoveEvent(true);

                GlobalContext.GetDataVideo();
                return view;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
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
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                QualityIcon = view.FindViewById<TextView>(Resource.Id.Qualityicon);
                ViewsIcon = view.FindViewById<TextView>(Resource.Id.Viewsicon);
                ShareIcon = view.FindViewById<TextView>(Resource.Id.Shareicon);
                MoreIcon = view.FindViewById<TextView>(Resource.Id.Moreicon);
                ShowMoreDescriptionIcon = view.FindViewById<TextView>(Resource.Id.video_ShowDiscription);
                VideoDescriptionLayout = view.FindViewById<LinearLayout>(Resource.Id.videoDescriptionLayout);

                ShareButton = view.FindViewById<LinearLayout>(Resource.Id.ShareButton);
                ShareButton.Click +=  GlobalContext.VideoActionsController.ShareIcon_Click;

                MoreButton = view.FindViewById<LinearLayout>(Resource.Id.moreButton);
                MoreButton.Click += GlobalContext.VideoActionsController.MoreButton_OnClick;

                VideoTittle = view.FindViewById<TextView>(Resource.Id.video_Titile);
                VideoQualityTextView = view.FindViewById<TextView>(Resource.Id.QualityTextView);
                VideoViewsNumber = view.FindViewById<TextView>(Resource.Id.ViewsNumber);
                VideoVideoDate = view.FindViewById<TextView>(Resource.Id.videoDate);
                VideoVideoDescription = view.FindViewById<AutoLinkTextView>(Resource.Id.videoDescriptionTextview);
                VideoVideoCategory = view.FindViewById<TextView>(Resource.Id.videoCategorytextview);

                VideoStars = view.FindViewById<TextView>(Resource.Id.videoStarstextview);
                VideoTag = view.FindViewById<TextView>(Resource.Id.videoTagtextview);

                TextSanitizerAutoLink = new TextSanitizer(VideoVideoDescription, Activity);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, QualityIcon, IonIconsFonts.RibbonA);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, ViewsIcon, IonIconsFonts.Eye);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, ShareIcon, IonIconsFonts.ReplyAll);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, MoreIcon, IonIconsFonts.PlusCircled);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, ShowMoreDescriptionIcon, IonIconsFonts.ArrowDownB);

                ShowMoreDescriptionIcon.Visibility = ViewStates.Gone;

                VideoDescriptionLayout.Visibility = ViewStates.Visible;

                MAdView = view.FindViewById<AdView>(Resource.Id.adView);
                AdsGoogle.InitAdView(MAdView, null); 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
           
        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                // true +=  // false -=
                if (addEvent)
                {

                }
                else
                {

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Events


        #endregion

        public void LoadVideo_Data(GetMoviesObject.Movie videoObject)
        {
            try
            {
                if (videoObject != null)
                {
                    VideoTittle.Text = Methods.FunString.DecodeString(videoObject.Name);

                    VideoQualityTextView.Text = videoObject.Quality.ToUpperInvariant();
                    VideoViewsNumber.Text = videoObject.Views + " " + Activity.GetText(Resource.String.Lbl_Views);
                    VideoVideoDate.Text = Activity.GetText(Resource.String.Lbl_Published_on) + " " + videoObject.Release;

                    VideoVideoCategory.Text = videoObject.Genre;
                    VideoStars.Text = videoObject.Stars;
                    VideoTag.Text = videoObject.Producer;

                    TextSanitizerAutoLink.Load(Methods.FunString.DecodeString(videoObject.Description));
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

    }
}