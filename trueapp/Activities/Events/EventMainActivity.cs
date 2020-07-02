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
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using ObeeNetwork.Activities.Events.Fragment;
using ObeeNetwork.Adapters;
using ObeeNetwork.Helpers.Ads;
using ObeeNetwork.Helpers.Controller;
using ObeeNetwork.Helpers.Utils;
using ObeeNetworkClient.Classes.Event;
using ObeeNetworkClient.Requests;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace ObeeNetwork.Activities.Events
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class EventMainActivity : AppCompatActivity
    {
        #region Variables Basic

        private static EventMainActivity Instance;
        private ViewPager ViewPager;
        public EventFragment EventTab;
        public MyEventFragment MyEventTab;
        private GoingFragment GoingTab;
        private InterestedFragment InterestedTab;
        private InvitedFragment InvitedTab;
        private PastFragment PastTab;
        private TabLayout TabLayout;
        private FloatingActionButton FloatingActionButtonView;
        private AdsGoogle.AdMobRewardedVideo RewardedVideoAd;

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

                Instance = this;

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();

                RewardedVideoAd = AdsGoogle.Ad_RewardedVideo(this);
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
                RewardedVideoAd?.OnResume(this);
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
                ViewPager = FindViewById<ViewPager>(Resource.Id.viewpager);
                TabLayout = FindViewById<TabLayout>(Resource.Id.tabs);

                FloatingActionButtonView = FindViewById<FloatingActionButton>(Resource.Id.floatingActionButtonView);

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
                var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolbar != null)
                {
                    toolbar.Title = GetText(Resource.String.Lbl_Events);
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

        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                // true +=  // false -=
                if (addEvent)
                {
                    FloatingActionButtonView.Click += BtnCreateEventsOnClick;
                }
                else
                {
                    FloatingActionButtonView.Click -= BtnCreateEventsOnClick;
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
                RewardedVideoAd?.OnDestroy(this);

                ViewPager = null;
                EventTab = null;
                MyEventTab = null;
                GoingTab = null;
                InterestedTab = null;
                InvitedTab = null;
                PastTab = null;
                TabLayout = null;
                FloatingActionButtonView = null;
                Instance = null;
                RewardedVideoAd = null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        public static EventMainActivity GetInstance()
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
         
        #endregion

        #region Events

        private void BtnCreateEventsOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(this, typeof(CreateEventActivity)));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }


        #endregion

        #region Set Tap

        private void SetUpViewPager(ViewPager viewPager)
        {
            try
            {
                EventTab = new EventFragment();
                GoingTab = new GoingFragment();
                InvitedTab = new InvitedFragment();
                InterestedTab = new InterestedFragment();
                PastTab = new PastFragment();
                MyEventTab = new MyEventFragment();

                var adapter = new MainTabAdapter(SupportFragmentManager);

                adapter.AddFragment(EventTab, GetText(Resource.String.Lbl_All_Events));

                if (AppSettings.ShowEventGoing)
                    adapter.AddFragment(GoingTab, GetText(Resource.String.Lbl_Going));

                if (AppSettings.ShowEventInvited)
                    adapter.AddFragment(InvitedTab, GetText(Resource.String.Lbl_Invited));

                if (AppSettings.ShowEventInterested)
                    adapter.AddFragment(InterestedTab, GetText(Resource.String.Lbl_Interested));

                if (AppSettings.ShowEventPast)
                    adapter.AddFragment(PastTab, GetText(Resource.String.Lbl_Past));

                adapter.AddFragment(MyEventTab, GetText(Resource.String.Lbl_My_Events));
                
                viewPager.CurrentItem = adapter.Count;
                viewPager.OffscreenPageLimit = adapter.Count;
                viewPager.Adapter = adapter;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        #region Get Event Api 

        public void StartApiService(string offset = "0", string typeEvent = "events")
        {
            if (Methods.CheckConnectivity())
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => GetEvent(offset, typeEvent) });
            else
                Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long).Show();
        }

        private async Task GetEvent(string offset = "0", string typeEvent = "events")
        {
            if (typeEvent == "events" && EventTab.MainScrollEvent != null && EventTab.MainScrollEvent.IsLoading)
                return;
       
            if (Methods.CheckConnectivity())
            { 
                var dictionary = new Dictionary<string, string>();
                switch (typeEvent)
                {
                    case "events":
                        dictionary.Add("offset", offset);
                        dictionary.Add("fetch", "events");
                        if (EventTab?.MainScrollEvent != null)
                            EventTab.MainScrollEvent.IsLoading = true;
                        break;
                    case "going":
                        dictionary.Add("going_offset", offset);
                        dictionary.Add("fetch", "going");
                        if (AppSettings.ShowEventGoing && GoingTab?.MainScrollEvent != null)
                            GoingTab.MainScrollEvent.IsLoading = true;
                        break;
                    case "past":
                        dictionary.Add("past_offset", offset);
                        dictionary.Add("fetch", "past");
                        if (AppSettings.ShowEventPast && PastTab?.MainScrollEvent != null)
                            PastTab.MainScrollEvent.IsLoading = true;
                        break;
                    case "myEvent":
                        dictionary.Add("my_offset", offset);
                        dictionary.Add("fetch", "my_events");
                        if (MyEventTab?.MainScrollEvent != null)
                            MyEventTab.MainScrollEvent.IsLoading = true;
                        break;
                    case "interested":
                        dictionary.Add("interested_offset", offset);
                        dictionary.Add("fetch", "interested");
                        if (AppSettings.ShowEventInterested && InterestedTab?.MainScrollEvent != null)
                            InterestedTab.MainScrollEvent.IsLoading = true;
                        break;
                    case "invited":
                        dictionary.Add("invited_offset", offset);
                        dictionary.Add("fetch", "invited");
                        if (AppSettings.ShowEventInvited && InvitedTab?.MainScrollEvent != null)
                            InvitedTab.MainScrollEvent.IsLoading = true;
                        break;
                }
                 
                (int apiStatus, var respond) = await RequestsAsync.Event.Get_Events(dictionary);
                if (apiStatus.Equals(200))
                {
                    if (respond is GetEventsObject result)
                    {
                        //Events
                        //==============================================================
                        if (typeEvent == "events" && EventTab != null)
                        { 
                            int countList = EventTab.MAdapter.EventList.Count;
                            var respondList = result.Events.Count;
                            if (respondList > 0)
                            {
                                if (countList > 0)
                                {
                                    foreach (var item in from item in result.Events let check = EventTab.MAdapter.EventList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                    {
                                        EventTab.MAdapter.EventList.Add(item);
                                    }

                                    RunOnUiThread(() => { EventTab.MAdapter.NotifyItemRangeInserted(countList, EventTab.MAdapter.EventList.Count - countList); });
                                }
                                else
                                {
                                    EventTab.MAdapter.EventList = new ObservableCollection<EventDataObject>(result.Events);
                                    RunOnUiThread(() => { EventTab.MAdapter.NotifyDataSetChanged(); });
                                }
                            }
                            else
                            {
                                if (EventTab.MAdapter.EventList.Count > 10 && !EventTab.MRecycler.CanScrollVertically(1))
                                    Toast.MakeText(this, GetText(Resource.String.Lbl_NoMoreEvent), ToastLength.Short).Show();
                            }

                            RunOnUiThread(() => { ShowEmptyPage("Event"); });
                        }
                         
                        //Going 
                        //==============================================================
                        if (AppSettings.ShowEventGoing && typeEvent == "going" && GoingTab != null)
                        {
                            int countGoingList = GoingTab.MAdapter.EventList.Count;

                            var respondGoingList = result.Going.Count;
                            if (respondGoingList > 0)
                            {
                                if (countGoingList > 0)
                                {
                                    foreach (var item in from item in result.Going let check = GoingTab.MAdapter.EventList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                    {
                                        GoingTab.MAdapter.EventList.Add(item);
                                    }

                                    RunOnUiThread(() => { GoingTab.MAdapter.NotifyItemRangeInserted(countGoingList - 1, GoingTab.MAdapter.EventList.Count - countGoingList); });
                                }
                                else
                                {
                                    GoingTab.MAdapter.EventList = new ObservableCollection<EventDataObject>(result.Going);
                                    RunOnUiThread(() => { GoingTab.MAdapter.NotifyDataSetChanged(); });
                                }
                            }
                            else
                            {
                                if (GoingTab.MAdapter.EventList.Count > 10 && !GoingTab.MRecycler.CanScrollVertically(1))
                                    Toast.MakeText(this, GetText(Resource.String.Lbl_NoMoreEvent), ToastLength.Short).Show();
                            }

                            RunOnUiThread(() => { ShowEmptyPage("Going"); });
                        }
                         
                        //Invited 
                        //==============================================================
                        if (AppSettings.ShowEventInvited && typeEvent == "invited" && InvitedTab != null)
                        {
                            int countInvitedList = InvitedTab.MAdapter.EventList.Count;

                            var respondInvitedList = result.Invited.Count;
                            if (respondInvitedList > 0)
                            {
                                if (countInvitedList > 0)
                                {
                                    foreach (var item in from item in result.Invited let check = InvitedTab.MAdapter.EventList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                    {
                                        InvitedTab.MAdapter.EventList.Add(item);
                                    }

                                    RunOnUiThread(() => { InvitedTab.MAdapter.NotifyItemRangeInserted(countInvitedList - 1, InvitedTab.MAdapter.EventList.Count - countInvitedList); });
                                }
                                else
                                {
                                    InvitedTab.MAdapter.EventList = new ObservableCollection<EventDataObject>(result.Invited);
                                    RunOnUiThread(() => { InvitedTab.MAdapter.NotifyDataSetChanged(); });
                                }
                            }
                            else
                            {
                                if (InvitedTab.MAdapter.EventList.Count > 10 && !InvitedTab.MRecycler.CanScrollVertically(1))
                                    Toast.MakeText(this, GetText(Resource.String.Lbl_NoMoreEvent), ToastLength.Short).Show();
                            }

                            RunOnUiThread(() => { ShowEmptyPage("Invited"); });
                        }

                        //Interested 
                        //==============================================================
                        if (AppSettings.ShowEventInterested && typeEvent == "interested" && InterestedTab != null)
                        {
                            int countInterestedList = InterestedTab.MAdapter.EventList.Count;

                            var respondInterestedList = result.Interested.Count;
                            if (respondInterestedList > 0)
                            {
                                if (countInterestedList > 0)
                                {
                                    foreach (var item in from item in result.Interested let check = InterestedTab.MAdapter.EventList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                    {
                                        InterestedTab.MAdapter.EventList.Add(item);
                                    }

                                    RunOnUiThread(() => { InterestedTab.MAdapter.NotifyItemRangeInserted(countInterestedList - 1, InterestedTab.MAdapter.EventList.Count - countInterestedList); });
                                }
                                else
                                {
                                    InterestedTab.MAdapter.EventList = new ObservableCollection<EventDataObject>(result.Interested);
                                    RunOnUiThread(() => { InterestedTab.MAdapter.NotifyDataSetChanged(); });
                                }
                            }
                            else
                            {
                                if (InterestedTab.MAdapter.EventList.Count > 10 && !InterestedTab.MRecycler.CanScrollVertically(1))
                                    Toast.MakeText(this, GetText(Resource.String.Lbl_NoMoreEvent), ToastLength.Short).Show();
                            }

                            RunOnUiThread(() => { ShowEmptyPage("Interested"); });
                        }

                        //Past 
                        //==============================================================
                        if (AppSettings.ShowEventPast && typeEvent == "past" && PastTab != null)
                        {
                            int countPastList = PastTab.MAdapter.EventList.Count;

                            var respondPastList = result.Past.Count;
                            if (respondPastList > 0)
                            {
                                if (countPastList > 0)
                                {
                                    foreach (var item in from item in result.Past let check = PastTab.MAdapter.EventList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                    {
                                        PastTab.MAdapter.EventList.Add(item);
                                    }

                                    RunOnUiThread(() => { PastTab.MAdapter.NotifyItemRangeInserted(countPastList - 1, PastTab.MAdapter.EventList.Count - countPastList); });
                                }
                                else
                                {
                                    PastTab.MAdapter.EventList = new ObservableCollection<EventDataObject>(result.Past);
                                    RunOnUiThread(() => { PastTab.MAdapter.NotifyDataSetChanged(); });
                                }
                            }
                            else
                            {
                                if (PastTab.MAdapter.EventList.Count > 10 && !PastTab.MRecycler.CanScrollVertically(1))
                                    Toast.MakeText(this, GetText(Resource.String.Lbl_NoMoreEvent), ToastLength.Short).Show();
                            }

                            RunOnUiThread(() => { ShowEmptyPage("Past"); });
                        }

                        //My Event 
                        //==============================================================
                        if (typeEvent == "myEvent" && MyEventTab != null)
                        {
                            int myEventsCountList = MyEventTab.MAdapter.EventList.Count;
                            var myEventsList = result.MyEvents.Count;
                            if (myEventsList > 0)
                            {
                                if (myEventsCountList > 0)
                                {
                                    foreach (var item in from item in result.MyEvents let check = MyEventTab.MAdapter.EventList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                    {
                                        MyEventTab.MAdapter.EventList.Add(item);
                                    }

                                    RunOnUiThread(() => { MyEventTab.MAdapter.NotifyItemRangeInserted(myEventsCountList - 1, MyEventTab.MAdapter.EventList.Count - myEventsCountList); });
                                }
                                else
                                {
                                    MyEventTab.MAdapter.EventList = new ObservableCollection<EventDataObject>(result.MyEvents);
                                    RunOnUiThread(() => { MyEventTab.MAdapter.NotifyDataSetChanged(); });
                                }
                            }
                            else
                            {
                                if (MyEventTab.MAdapter.EventList.Count > 10 && !MyEventTab.MRecycler.CanScrollVertically(1))
                                    Toast.MakeText(this, GetText(Resource.String.Lbl_NoMoreEvent), ToastLength.Short).Show();
                            }

                            RunOnUiThread(() => { ShowEmptyPage("MyEvent"); });
                        } 
                    }
                }
                else Methods.DisplayReportResult(this, respond);
            }
            else
            {
                if (MyEventTab != null)
                {
                    if (EventTab.Inflated == null)
                        EventTab.Inflated = EventTab.EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(EventTab.Inflated, EmptyStateInflater.Type.NoConnection);
                    if (!x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click += null;
                        x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                    }

                    Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                    if (EventTab?.MainScrollEvent != null) EventTab.MainScrollEvent.IsLoading = false;
                }
               
            }
        }
         

        private void ShowEmptyPage(string type)
        {
            try
            { 
                if (type == "Event")
                {
                    EventTab.MainScrollEvent.IsLoading = false;
                    EventTab.SwipeRefreshLayout.Refreshing = false;

                    if (EventTab.MAdapter.EventList.Count > 0)
                    {
                        EventTab.MRecycler.Visibility = ViewStates.Visible;
                        EventTab.EmptyStateLayout.Visibility = ViewStates.Gone;
                    }
                    else
                    {
                        EventTab.MRecycler.Visibility = ViewStates.Gone;

                        if (EventTab.Inflated == null)
                            EventTab.Inflated = EventTab.EmptyStateLayout.Inflate();

                        EmptyStateInflater x = new EmptyStateInflater();
                        x.InflateLayout(EventTab.Inflated, EmptyStateInflater.Type.NoEvent);
                        if (!x.EmptyStateButton.HasOnClickListeners)
                        {
                            x.EmptyStateButton.Click += null;
                            x.EmptyStateButton.Click += BtnCreateEventsOnClick;
                        }
                        EventTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                    }
                }
                else if (type == "Going")
                {
                    GoingTab.MainScrollEvent.IsLoading = false;
                    GoingTab.SwipeRefreshLayout.Refreshing = false;

                    if (GoingTab.MAdapter.EventList.Count > 0)
                    {
                        GoingTab.MRecycler.Visibility = ViewStates.Visible;
                        GoingTab.EmptyStateLayout.Visibility = ViewStates.Gone;
                    }
                    else
                    {
                        GoingTab.MRecycler.Visibility = ViewStates.Gone;

                        if (GoingTab.Inflated == null)
                            GoingTab.Inflated = GoingTab.EmptyStateLayout.Inflate();

                        EmptyStateInflater x = new EmptyStateInflater();
                        x.InflateLayout(GoingTab.Inflated, EmptyStateInflater.Type.NoEvent);
                        if (!x.EmptyStateButton.HasOnClickListeners)
                        {
                            x.EmptyStateButton.Click += null;
                            x.EmptyStateButton.Click += BtnCreateEventsOnClick;
                        }
                        GoingTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                    }
                }
                else if (type == "Invited")
                {
                    InvitedTab.MainScrollEvent.IsLoading = false;
                    InvitedTab.SwipeRefreshLayout.Refreshing = false;

                    if (InvitedTab.MAdapter.EventList.Count > 0)
                    {
                        InvitedTab.MRecycler.Visibility = ViewStates.Visible;
                        InvitedTab.EmptyStateLayout.Visibility = ViewStates.Gone;
                    }
                    else
                    {
                        InvitedTab.MRecycler.Visibility = ViewStates.Gone;

                        if (InvitedTab.Inflated == null)
                            InvitedTab.Inflated = InvitedTab.EmptyStateLayout.Inflate();

                        EmptyStateInflater x = new EmptyStateInflater();
                        x.InflateLayout(InvitedTab.Inflated, EmptyStateInflater.Type.NoEvent);
                        if (!x.EmptyStateButton.HasOnClickListeners)
                        {
                            x.EmptyStateButton.Click += null;
                            x.EmptyStateButton.Click += BtnCreateEventsOnClick;
                        }
                        InvitedTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                    }
                }
                else if (type == "Interested")
                {
                    InterestedTab.MainScrollEvent.IsLoading = false;
                    InterestedTab.SwipeRefreshLayout.Refreshing = false;

                    if (InterestedTab.MAdapter.EventList.Count > 0)
                    {
                        InterestedTab.MRecycler.Visibility = ViewStates.Visible;
                        InterestedTab.EmptyStateLayout.Visibility = ViewStates.Gone;
                    }
                    else
                    {
                        InterestedTab.MRecycler.Visibility = ViewStates.Gone;

                        if (InterestedTab.Inflated == null)
                            InterestedTab.Inflated = InterestedTab.EmptyStateLayout.Inflate();

                        EmptyStateInflater x = new EmptyStateInflater();
                        x.InflateLayout(InterestedTab.Inflated, EmptyStateInflater.Type.NoEvent);
                        if (!x.EmptyStateButton.HasOnClickListeners)
                        {
                            x.EmptyStateButton.Click += null;
                            x.EmptyStateButton.Click += BtnCreateEventsOnClick;
                        }
                        InterestedTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                    }
                }
                else if (type == "Past")
                {
                    PastTab.MainScrollEvent.IsLoading = false;
                    PastTab.SwipeRefreshLayout.Refreshing = false;

                    if (PastTab.MAdapter.EventList.Count > 0)
                    {
                        PastTab.MRecycler.Visibility = ViewStates.Visible;
                        PastTab.EmptyStateLayout.Visibility = ViewStates.Gone;
                    }
                    else
                    {
                        PastTab.MRecycler.Visibility = ViewStates.Gone;

                        if (PastTab.Inflated == null)
                            PastTab.Inflated = PastTab.EmptyStateLayout.Inflate();

                        EmptyStateInflater x = new EmptyStateInflater();
                        x.InflateLayout(PastTab.Inflated, EmptyStateInflater.Type.NoEvent);
                        if (!x.EmptyStateButton.HasOnClickListeners)
                        {
                            x.EmptyStateButton.Click += null;
                            x.EmptyStateButton.Click += BtnCreateEventsOnClick;
                        }
                        PastTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                    }
                } 
                else if (type == "MyEvent")
                {
                    MyEventTab.MainScrollEvent.IsLoading = false;
                    MyEventTab.SwipeRefreshLayout.Refreshing = false;

                    if (MyEventTab.MAdapter.EventList.Count > 0)
                    {
                        MyEventTab.MRecycler.Visibility = ViewStates.Visible;
                        MyEventTab.EmptyStateLayout.Visibility = ViewStates.Gone;
                    }
                    else
                    {
                        MyEventTab.MRecycler.Visibility = ViewStates.Gone;

                        if (MyEventTab.Inflated == null)
                            MyEventTab.Inflated = MyEventTab.EmptyStateLayout.Inflate();

                        EmptyStateInflater x = new EmptyStateInflater();
                        x.InflateLayout(MyEventTab.Inflated, EmptyStateInflater.Type.NoEvent);
                        if (!x.EmptyStateButton.HasOnClickListeners)
                        {
                            x.EmptyStateButton.Click += null;
                            x.EmptyStateButton.Click += BtnCreateEventsOnClick;
                        }
                        MyEventTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                    } 
                } 
            }
            catch (Exception e)
            {
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