using System;
using System.Collections.ObjectModel;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Ads;
using Android.Graphics;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Bumptech.Glide.Integration.RecyclerView;
using Bumptech.Glide.Util;
using Newtonsoft.Json;
using ObeeNetwork.Activities.Album.Adapters;
using ObeeNetwork.Activities.UsersPages;
using ObeeNetwork.Helpers.Ads;
using ObeeNetwork.Helpers.Utils;
using ObeeNetworkClient.Classes.Album;
using ObeeNetworkClient.Classes.Posts;

namespace ObeeNetwork.Activities.Album
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class ImageByAlbumActivity : AppCompatActivity
    {
        #region Variables Basic

        private PhotosAdapter MAdapter;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private RecyclerView MRecycler;
        private GridLayoutManager LayoutManager;
        private ViewStub EmptyStateLayout;
        private PostDataObject ImageData;
        private Toolbar Toolbar;
        private AdView MAdView;
        private FloatingActionButton ActionButton;

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
                SetContentView(Resource.Layout.RecyclerDefaultLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();

                Get_DataImage();
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
                MAdView?.Resume();
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
                MAdView?.Pause();
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

        private void InitComponent()
        {
            try
            {
                MRecycler = (RecyclerView)FindViewById(Resource.Id.recyler);
                EmptyStateLayout = FindViewById<ViewStub>(Resource.Id.viewStub);
                 
                SwipeRefreshLayout = (SwipeRefreshLayout)FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = false;
                SwipeRefreshLayout.Enabled = false;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));

                ActionButton = FindViewById<FloatingActionButton>(Resource.Id.floatingActionButtonView);
                ActionButton.Visibility = ViewStates.Visible;
                ActionButton.SetImageResource(Resource.Drawable.ic_add);

                MAdView = FindViewById<AdView>(Resource.Id.adView);
                AdsGoogle.InitAdView(MAdView, MRecycler);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void InitToolbar()
        {
            try
            {
                Toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (Toolbar != null)
                {
                    Toolbar.Title = " ";
                    Toolbar.SetTitleTextColor(Color.White);
                    SetSupportActionBar(Toolbar);
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

        private void SetRecyclerViewAdapters()
        {
            try
            {
                MAdapter = new PhotosAdapter(this)
                {
                    PhotosList = new ObservableCollection<PhotoAlbumObject>()
                };
                LayoutManager = new GridLayoutManager(this, 2);
                LayoutManager.SetSpanSizeLookup(new MySpanSizeLookup(4, 1, 1)); //5, 1, 2
                MRecycler.SetLayoutManager(LayoutManager); 
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<PhotoAlbumObject>(this, MAdapter, sizeProvider, 10);
                MRecycler.AddOnScrollListener(preLoader);
                MRecycler.SetAdapter(MAdapter);
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
                    MAdapter.ItemClick += MAdapterOnItemClick;
                    ActionButton.Click += ActionButtonOnClick;
                }
                else
                {
                    MAdapter.ItemClick -= MAdapterOnItemClick;
                    ActionButton.Click -= ActionButtonOnClick;
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
                MAdView?.Destroy();

                MAdapter = null;
                SwipeRefreshLayout = null;
                MRecycler = null;
                EmptyStateLayout = null;
                ImageData = null;
                Toolbar = null;
                ActionButton = null;
                MAdView = null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Events

        //Add New Image to this album 
        private void ActionButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(AddImageToAlbumActivity));
                intent.PutExtra("AlbumObject", JsonConvert.SerializeObject(ImageData)); // PostDataObject
                StartActivityForResult(intent, 2020);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
  
        private void MAdapterOnItemClick(object sender, PhotosAdapterClickEventArgs e)
        {
            try
            {
                PhotoAlbumObject item = MAdapter.GetItem(e.Position);
                if (item != null)
                {
                    var index = ImageData.PhotoAlbum.IndexOf(item);
                    //var intent = new Intent(this, typeof(ImagePostViewerActivity));
                    //intent.PutExtra("itemIndex", index.ToString()); //PhotoAlbumObject
                    //intent.PutExtra("AlbumObject", JsonConvert.SerializeObject(ImageData)); // PostDataObject
                    //StartActivity(intent);

                    var intent = new Intent(this, typeof(MultiImagesPostViewerActivity));
                    intent.PutExtra("indexImage", index.ToString()); // Index Image Show
                    intent.PutExtra("AlbumObject", JsonConvert.SerializeObject(ImageData)); // PostDataObject
                    StartActivity(intent);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
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

                if (requestCode == 2020 && resultCode == Result.Ok) // Add image 
                {
                   var dataObject = JsonConvert.DeserializeObject<PostDataObject>(data.GetStringExtra("AlbumItem"));
                   if (dataObject != null)
                   {
                       ImageData = dataObject;

                       MAdapter.PhotosList = new ObservableCollection<PhotoAlbumObject>(dataObject.PhotoAlbum);
                       MAdapter.NotifyDataSetChanged();

                       MRecycler.Visibility = ViewStates.Visible;
                       EmptyStateLayout.Visibility = ViewStates.Gone;
                   }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
          
        #endregion

        //Get Data 
        private void Get_DataImage()
        {
            try
            {
                ImageData = JsonConvert.DeserializeObject<PostDataObject>(Intent.GetStringExtra("ItemData"));
                if (ImageData != null)
                {
                    Toolbar.Title = Methods.FunString.DecodeString(ImageData.AlbumName);

                    MAdapter.PhotosList =  new ObservableCollection<PhotoAlbumObject>(ImageData.PhotoAlbum);
                    MAdapter.NotifyDataSetChanged();

                    MRecycler.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

    }
}