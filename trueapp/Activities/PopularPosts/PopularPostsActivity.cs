using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using ObeeNetwork.Activities.NativePost.Extra;
using ObeeNetwork.Activities.NativePost.Post;
using ObeeNetwork.Helpers.Ads;
using ObeeNetwork.Helpers.Controller;
using ObeeNetwork.Helpers.Utils;
using Xamarin.Facebook.Ads;
using Exception = System.Exception;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace ObeeNetwork.Activities.PopularPosts
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class PopularPostsActivity : AppCompatActivity
    {
        #region Variables Basic

        private WRecyclerView MainRecyclerView;
        private NativePostAdapter PostFeedAdapter;
        private SwipeRefreshLayout SwipeRefreshLayout;
        
        private InterstitialAd InterstitialAd;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);

                Methods.App.FullScreenApp(this);

                // Create your application here
                SetContentView(Resource.Layout.MainRecylerViewLayout);

                //Get Value And Set Toolbar 
                InitToolbar();
                SetRecyclerViewAdapters();

                StartApiService();

                InterstitialAd = AdsFacebook.InitInterstitial(this);
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
                AddOrRemoveEvent(true);
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
                base.OnPause();
                AddOrRemoveEvent(false);
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
                MainRecyclerView.ReleasePlayer();
                DestroyBasic(); 
                base.OnDestroy(); 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
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
         
        private void InitToolbar()
        {
            try
            {
                var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolbar != null)
                {
                    toolbar.Title = GetText(Resource.String.Lbl_Popular_Posts);
                    toolbar.SetTitleTextColor(Color.White);
                    SetSupportActionBar(toolbar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);
                    
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        private void DestroyBasic()
        {
            try
            {
                InterstitialAd?.Destroy();

                MainRecyclerView = null;
                SwipeRefreshLayout = null;
                PostFeedAdapter = null;
                InterstitialAd = null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        #endregion

        #region Event

        //Refresh
        private void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
        {
            try
            {
                PostFeedAdapter.ListDiffer = PostFeedAdapter.ListDiffer;

                PostFeedAdapter.ListDiffer.Clear();
                PostFeedAdapter.NotifyDataSetChanged();

                PostFeedAdapter.NativePostType = NativeFeedType.Popular;

                StartApiService();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        private void StartApiService(string offset = "0")
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
            else
              PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => MainRecyclerView.FetchNewsFeedApiPosts(offset) });
        }  
    }
}