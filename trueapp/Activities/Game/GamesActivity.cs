using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V7.App;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using ObeeNetwork.Activities.Games.Fragment;
using ObeeNetwork.Adapters;
using ObeeNetwork.Helpers.Ads;
using ObeeNetwork.Helpers.Controller;
using ObeeNetwork.Helpers.Utils;
using ObeeNetworkClient.Classes.Games;
using ObeeNetworkClient.Requests;
using SearchView = Android.Support.V7.Widget.SearchView;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace ObeeNetwork.Activities.Games
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class GamesActivity : AppCompatActivity
    {
        #region Variables Basic

        private ViewPager ViewPager;
        private GamesFragment GamesTab;
        private MyGamesFragment MyGamesTab;
        private TabLayout TabLayout;
        private FloatingActionButton FloatingActionButtonView;
        private SearchView SearchView;
        private Toolbar ToolBar;
        public string SearchKey;

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
                SetContentView(Resource.Layout.EventMain_Layout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();

                LoadDataApi();
                AdsGoogle.Ad_Interstitial(this);
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
                AddOrRemoveGames(true);
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
                AddOrRemoveGames(false);
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
                if (GamesTab.MAdapter.GamesList.Count > 0)
                    ListUtils.ListCachedDataGames = GamesTab.MAdapter.GamesList;

                if (MyGamesTab.MAdapter.GamesList.Count > 0)
                    ListUtils.ListCachedDataMyGames = MyGamesTab.MAdapter.GamesList;

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

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.SearchGif_Menu, menu);

            try
            {
                var item = menu.FindItem(Resource.Id.searchUserBar);
                SearchView searchItem = (SearchView)item.ActionView;

                SearchView = searchItem.JavaCast<SearchView>();
                SearchView.SetQuery("", false);
                SearchView.SetIconifiedByDefault(false);
                SearchView.OnActionViewExpanded();
                SearchView.Iconified = false;
                SearchView.QueryTextChange += SearchViewOnQueryTextChange;
                SearchView.QueryTextSubmit += SearchViewOnQueryTextSubmit;
                SearchView.ClearFocus();

                //Change text colors
                var editText = (EditText)SearchView.FindViewById(Resource.Id.search_src_text);
                editText.SetHintTextColor(Color.White);
                editText.SetTextColor(Color.White);

                //Remove Icon Search
                ImageView searchViewIcon = (ImageView)SearchView.FindViewById(Resource.Id.search_mag_icon);
                ViewGroup linearLayoutSearchView = (ViewGroup)searchViewIcon.Parent;
                linearLayoutSearchView.RemoveView(searchViewIcon);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
             
            return base.OnCreateOptionsMenu(menu);
        }

        private void SearchViewOnQueryTextSubmit(object sender, SearchView.QueryTextSubmitEventArgs e)
        {
            try
            {
                SearchKey = e.NewText;

                GamesTab.MAdapter.GamesList.Clear();
                GamesTab.MAdapter.NotifyDataSetChanged();

                GamesTab.SwipeRefreshLayout.Refreshing = true;

                if (!Methods.CheckConnectivity())
                    Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                else
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => SearchGames() });

                //Hide keyboard programmatically in MonoDroid
                e.Handled = true;

                SearchView.ClearFocus();

                var inputManager = (InputMethodManager)GetSystemService(InputMethodService);
                inputManager.HideSoftInputFromWindow(ToolBar.WindowToken, 0);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void SearchViewOnQueryTextChange(object sender, SearchView.QueryTextChangeEventArgs e)
        {
            try
            {
                SearchKey = e.NewText;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                ViewPager = FindViewById<ViewPager>(Resource.Id.viewpager);
                TabLayout = FindViewById<TabLayout>(Resource.Id.tabs);

                FloatingActionButtonView = FindViewById<FloatingActionButton>(Resource.Id.floatingActionButtonView);
                FloatingActionButtonView.Visibility = ViewStates.Gone;

                ViewPager.OffscreenPageLimit = 2;
                SetUpViewPager(ViewPager);
                TabLayout.SetupWithViewPager(ViewPager);
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
                ToolBar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (ToolBar != null)
                {
                    ToolBar.Title = GetText(Resource.String.Lbl_Games);
                    ToolBar.SetTitleTextColor(Color.White);
                    SetSupportActionBar(ToolBar);
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

        private void AddOrRemoveGames(bool addGames)
        {
            try
            {
                // true +=  // false -=
                if (addGames)
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
        private void DestroyBasic()
        {
            try
            {
                ViewPager = null;
                TabLayout = null;
                FloatingActionButtonView = null;
                ToolBar = null;
                SearchKey = null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Set Tap

        private void SetUpViewPager(ViewPager viewPager)
        {
            try
            {
                GamesTab = new GamesFragment();
                MyGamesTab = new MyGamesFragment();

                var adapter = new MainTabAdapter(SupportFragmentManager);
                adapter.AddFragment(GamesTab, GetText(Resource.String.Lbl_Games));
                adapter.AddFragment(MyGamesTab, GetText(Resource.String.Lbl_MyGames));

                viewPager.CurrentItem = 2;
                viewPager.Adapter = adapter;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        #region Get Games Api 

        private void LoadDataApi()
        {
            try
            {
                string offsetGames = "0", offsetMyGames = "0";

                if (GamesTab.MAdapter != null && ListUtils.ListCachedDataGames.Count > 0)
                {
                    GamesTab.MAdapter.GamesList = ListUtils.ListCachedDataGames;
                    GamesTab.MAdapter.NotifyDataSetChanged();

                    var item = GamesTab.MAdapter.GamesList.LastOrDefault();
                    if (item != null && !string.IsNullOrEmpty(item.Id))
                        offsetGames = item.Id;
                }

                if (MyGamesTab.MAdapter != null && ListUtils.ListCachedDataMyGames.Count > 0)
                {
                    MyGamesTab.MAdapter.GamesList = ListUtils.ListCachedDataMyGames;
                    MyGamesTab.MAdapter.NotifyDataSetChanged();

                    var item = MyGamesTab.MAdapter.GamesList.LastOrDefault();
                    if (item != null && !string.IsNullOrEmpty(item.Id))
                        offsetMyGames = (item.Id);
                }

                StartApiService(offsetGames, offsetMyGames);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void StartApiService(string offsetGames = "0", string offsetMyGames = "0")
        {
            if (Methods.CheckConnectivity())
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => GetGames(offsetGames), () => GetMyGames(offsetMyGames) });
            else
                Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long).Show();
        }

        public async Task GetGames(string offset = "0")
        {
            if (GamesTab.MainScrollEvent.IsLoading)
                return;

            if (Methods.CheckConnectivity())
            {
                GamesTab.MainScrollEvent.IsLoading = true;
                var countList = GamesTab.MAdapter.GamesList.Count;

                var (respondCode, respondString) = await RequestsAsync.Games.FetchGames("10", offset);
                if (respondCode.Equals(200))
                {
                    if (respondString is FetchGamesObject result)
                    {
                        var respondList = result.Data.Count;
                        if (respondList > 0)
                        {
                            if (countList > 0)
                            {
                                foreach (var item in from item in result.Data let check = GamesTab.MAdapter.GamesList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                {
                                    GamesTab.MAdapter.GamesList.Add(item);
                                }

                                RunOnUiThread(() => { GamesTab.MAdapter.NotifyItemRangeInserted(countList, GamesTab.MAdapter.GamesList.Count - countList); });
                            }
                            else
                            {
                                GamesTab.MAdapter.GamesList = new ObservableCollection<GamesDataObject>(result.Data);
                                RunOnUiThread(() => { GamesTab.MAdapter.NotifyDataSetChanged(); });
                            }
                        }
                        else
                        {
                            if (GamesTab.MAdapter.GamesList.Count > 10 && !GamesTab.MRecycler.CanScrollVertically(1))
                                Toast.MakeText(this, GetText(Resource.String.Lbl_NoMoreGames), ToastLength.Short).Show();
                        }
                    }
                }
                else Methods.DisplayReportResult(this, respondString);

                RunOnUiThread(() => ShowEmptyPage("GetGames"));
            }
            else
            {
                GamesTab.Inflated = GamesTab.EmptyStateLayout.Inflate();
                EmptyStateInflater x = new EmptyStateInflater();
                x.InflateLayout(GamesTab.Inflated, EmptyStateInflater.Type.NoConnection);
                if (!x.EmptyStateButton.HasOnClickListeners)
                {
                    x.EmptyStateButton.Click += null;
                    x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                }

                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                GamesTab.MainScrollEvent.IsLoading = false;
            }
        }

        public async Task GetMyGames(string offset = "0")
        {
            if (MyGamesTab.MainScrollEvent.IsLoading)
                return;

            if (Methods.CheckConnectivity())
            {
                MyGamesTab.MainScrollEvent.IsLoading = true;
                var countList = MyGamesTab.MAdapter.GamesList.Count;

                var (respondCode, respondString) = await RequestsAsync.Games.FetchMyhGames("10", offset);
                if (respondCode.Equals(200))
                {
                    if (respondString is FetchGamesObject result)
                    {
                        var respondList = result.Data.Count;
                        if (respondList > 0)
                        {
                            if (countList > 0)
                            {
                                foreach (var item in from item in result.Data let check = MyGamesTab.MAdapter.GamesList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                {
                                    MyGamesTab.MAdapter.GamesList.Add(item);
                                }

                                RunOnUiThread(() => { MyGamesTab.MAdapter.NotifyItemRangeInserted(countList, MyGamesTab.MAdapter.GamesList.Count - countList); });
                            }
                            else
                            {
                                MyGamesTab.MAdapter.GamesList = new ObservableCollection<GamesDataObject>(result.Data);
                                RunOnUiThread(() => { MyGamesTab.MAdapter.NotifyDataSetChanged(); });
                            }
                        }
                        else
                        {
                            if (MyGamesTab.MAdapter.GamesList.Count > 10 && !MyGamesTab.MRecycler.CanScrollVertically(1))
                                Toast.MakeText(this, GetText(Resource.String.Lbl_NoMoreGames), ToastLength.Short).Show();
                        }
                    }
                }
                else Methods.DisplayReportResult(this, respondString);

                RunOnUiThread(()=> ShowEmptyPage("GetMyGames"));
            }
            else
            {
                MyGamesTab.Inflated = MyGamesTab.EmptyStateLayout.Inflate();
                EmptyStateInflater x = new EmptyStateInflater();
                x.InflateLayout(MyGamesTab.Inflated, EmptyStateInflater.Type.NoConnection);
                if (!x.EmptyStateButton.HasOnClickListeners)
                {
                    x.EmptyStateButton.Click += null;
                    x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                }

                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                MyGamesTab.MainScrollEvent.IsLoading = false;
            }
        }

        public async Task SearchGames(string offset = "0")
        {
            if (GamesTab.MainScrollEvent.IsLoading)
                return;

            if (Methods.CheckConnectivity())
            {
                GamesTab.MainScrollEvent.IsLoading = true;
                var countList = GamesTab.MAdapter.GamesList.Count;

                var (respondCode, respondString) = await RequestsAsync.Games.SearchGames(SearchKey,"15", offset);
                if (respondCode.Equals(200))
                {
                    if (respondString is FetchGamesObject result)
                    {
                        var respondList = result.Data.Count;
                        if (respondList > 0)
                        {
                            if (countList > 0)
                            {
                                foreach (var item in from item in result.Data let check = GamesTab.MAdapter.GamesList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                {
                                    GamesTab.MAdapter.GamesList.Add(item);
                                }

                                RunOnUiThread(() => { GamesTab.MAdapter.NotifyItemRangeInserted(countList, GamesTab.MAdapter.GamesList.Count - countList); });
                            }
                            else
                            {
                                GamesTab.MAdapter.GamesList = new ObservableCollection<GamesDataObject>(result.Data);
                                RunOnUiThread(() => { GamesTab.MAdapter.NotifyDataSetChanged(); });
                            }
                        }
                        else
                        {
                            if (GamesTab.MAdapter.GamesList.Count > 10 && !GamesTab.MRecycler.CanScrollVertically(1))
                                Toast.MakeText(this, GetText(Resource.String.Lbl_NoMoreGames), ToastLength.Short).Show();
                        }
                    }
                }
                else Methods.DisplayReportResult(this, respondString);

                RunOnUiThread(() => ShowEmptyPage("GetGames"));
            }
            else
            {
                GamesTab.Inflated = GamesTab.EmptyStateLayout.Inflate();
                EmptyStateInflater x = new EmptyStateInflater();
                x.InflateLayout(GamesTab.Inflated, EmptyStateInflater.Type.NoConnection);
                if (!x.EmptyStateButton.HasOnClickListeners)
                {
                    x.EmptyStateButton.Click += null;
                    x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                }

                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                GamesTab.MainScrollEvent.IsLoading = false;
            }
        }

        private void ShowEmptyPage(string type)
        {
            try
            { 
                if (type == "GetGames")
                {
                    GamesTab.MainScrollEvent.IsLoading = false;
                    GamesTab.SwipeRefreshLayout.Refreshing = false;

                    if (GamesTab.MAdapter.GamesList.Count > 0)
                    {
                        GamesTab.MRecycler.Visibility = ViewStates.Visible;
                        GamesTab.EmptyStateLayout.Visibility = ViewStates.Gone;
                    }
                    else
                    {
                        GamesTab.MRecycler.Visibility = ViewStates.Gone;

                        if (GamesTab.Inflated == null)
                            GamesTab.Inflated = GamesTab.EmptyStateLayout.Inflate();

                        EmptyStateInflater x = new EmptyStateInflater();
                        x.InflateLayout(GamesTab.Inflated, EmptyStateInflater.Type.NoGames);
                        if (!x.EmptyStateButton.HasOnClickListeners)
                        {
                            x.EmptyStateButton.Click += null; 
                        }
                        GamesTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                    }
                }
                else if (type == "GetMyGames")
                {
                    MyGamesTab.MainScrollEvent.IsLoading = false;
                    MyGamesTab.SwipeRefreshLayout.Refreshing = false;

                    if (MyGamesTab.MAdapter.GamesList.Count > 0)
                    {
                        MyGamesTab.MRecycler.Visibility = ViewStates.Visible;
                        MyGamesTab.EmptyStateLayout.Visibility = ViewStates.Gone;
                    }
                    else
                    {
                        MyGamesTab.MRecycler.Visibility = ViewStates.Gone;

                        if (MyGamesTab.Inflated == null)
                            MyGamesTab.Inflated = MyGamesTab.EmptyStateLayout.Inflate();

                        EmptyStateInflater x = new EmptyStateInflater();
                        x.InflateLayout(MyGamesTab.Inflated, EmptyStateInflater.Type.NoGames);
                        if (!x.EmptyStateButton.HasOnClickListeners)
                        {
                            x.EmptyStateButton.Click += null; 
                        }
                        MyGamesTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                    }
                }
            }
            catch (Exception e)
            {
                GamesTab.MainScrollEvent.IsLoading = false;
                GamesTab.SwipeRefreshLayout.Refreshing = false;
                MyGamesTab.MainScrollEvent.IsLoading = false;
                MyGamesTab.SwipeRefreshLayout.Refreshing = false;
                Console.WriteLine(e);
            }
        }

        //No Internet Connection 
        private void EmptyStateButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                StartApiService();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion
    }
}