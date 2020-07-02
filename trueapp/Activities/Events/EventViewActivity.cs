using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AFollestad.MaterialDialogs;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Android.Locations;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Java.Lang;
using Newtonsoft.Json;
using Plugin.Share;
using Plugin.Share.Abstractions;
using ObeeNetwork.Activities.AddPost;
using ObeeNetwork.Activities.NativePost.Extra;
using ObeeNetwork.Activities.NativePost.Post;
using ObeeNetwork.Helpers.Controller;
using ObeeNetwork.Helpers.Utils;
using ObeeNetwork.Library.Anjo.SuperTextLibrary;
using ObeeNetworkClient.Classes.Event;
using ObeeNetworkClient.Classes.Posts;
using ObeeNetworkClient.Classes.Product;
using ObeeNetworkClient.Requests;
using Exception = System.Exception;
using Uri = Android.Net.Uri;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using static Android.Support.Design.Widget.AppBarLayout;
using String = Java.Lang.String;

namespace ObeeNetwork.Activities.Events
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class EventViewActivity : AppCompatActivity, IOnMapReadyCallback, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback, IOnOffsetChangedListener
    {
        #region Variables Basic

        private CollapsingToolbarLayout ToolbarLayout;
        private GoogleMap Map;
        private double CurrentLongitude, CurrentLatitude;
        private TextView TxtName, TxtGoing, TxtInterested, TxtStartDate, TxtEndDate, TxtLocation, TxtDescription;
        private SuperTextView TxtDescriptionText;
        private FloatingActionButton FloatingActionButtonView;
        private ImageView ImageEventCover;
        private Button BtnGo, BtnInterested;
        private WRecyclerView MainRecyclerView;
        private NativePostAdapter PostFeedAdapter;
        private ImageButton BtnMore;
        private EventDataObject EventData;

        private SwipeRefreshLayout SwipeRefreshLayout;
        private AppBarLayout AppBarLayout;
        private string Name;
        private bool IsShow = true;
        private int ScrollRange = -1;
        private FeedCombiner Combiner;
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
                SetContentView(Resource.Layout.EventView_Layout);

                var eventObject = Intent.GetStringExtra("EventView");
                if (!string.IsNullOrEmpty(eventObject))
                    EventData = JsonConvert.DeserializeObject<EventDataObject>(eventObject);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();

                Get_Data_Event();
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
                ToolbarLayout = FindViewById<CollapsingToolbarLayout>(Resource.Id.collapsingToolbar);
                ToolbarLayout.Title = "";
                AppBarLayout = FindViewById<AppBarLayout>(Resource.Id.appbar_ptwo);
                TxtName = FindViewById<TextView>(Resource.Id.tvName_ptwo);

                TxtGoing = FindViewById<TextView>(Resource.Id.GoingTextview);

                TxtInterested = FindViewById<TextView>(Resource.Id.InterestedTextview);
                TxtStartDate = FindViewById<TextView>(Resource.Id.txtStartDate);
                TxtEndDate = FindViewById<TextView>(Resource.Id.txtEndDate);


                TxtLocation = FindViewById<TextView>(Resource.Id.LocationTextview);
                TxtDescription = FindViewById<TextView>(Resource.Id.tv_about);
                TxtDescriptionText = FindViewById<SuperTextView>(Resource.Id.tv_aboutdescUser);
                 
                ImageEventCover = FindViewById<ImageView>(Resource.Id.EventCover);

                BtnGo = FindViewById<Button>(Resource.Id.ButtonGoing);
                BtnInterested = FindViewById<Button>(Resource.Id.ButtonIntersted);

                FloatingActionButtonView = FindViewById<FloatingActionButton>(Resource.Id.floatingActionButtonView);
                FloatingActionButtonView.Visibility = ViewStates.Visible;

                MainRecyclerView = FindViewById<WRecyclerView>(Resource.Id.newsfeedRecyler);
                BtnMore = (ImageButton)FindViewById(Resource.Id.morebutton);

                SwipeRefreshLayout = FindViewById<SwipeRefreshLayout>(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = false;
                SwipeRefreshLayout.Enabled = false;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));

                AppBarLayout.AddOnOffsetChangedListener(this);

                var mapFrag = SupportMapFragment.NewInstance();
                SupportFragmentManager.BeginTransaction().Add(Resource.Id.map, mapFrag, mapFrag.Tag).Commit();
                mapFrag.GetMapAsync(this);
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
                PostFeedAdapter = new NativePostAdapter(this, EventData.Id, MainRecyclerView, NativeFeedType.Event, SupportFragmentManager);
                MainRecyclerView.SetXAdapter(PostFeedAdapter, null);
                Combiner = new FeedCombiner(null, PostFeedAdapter.ListDiffer, this);
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
                    FloatingActionButtonView.Click += AddPostOnClick;
                    BtnGo.Click += BtnGoOnClick;
                    BtnInterested.Click += BtnInterestedOnClick;
                    BtnMore.Click += BtnMoreOnClick;
                }
                else
                {
                    FloatingActionButtonView.Click -= AddPostOnClick;
                    BtnGo.Click -= BtnGoOnClick;
                    BtnInterested.Click -= BtnInterestedOnClick;
                    BtnMore.Click -= BtnMoreOnClick;
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
                ToolbarLayout = null;
                AppBarLayout = null;
                TxtName = null;
                TxtGoing = null;
                TxtInterested = null;
                TxtStartDate  = null;
                TxtEndDate = null;
                TxtLocation = null;
                TxtDescription = null;
                TxtDescriptionText = null;
                ImageEventCover = null;
                BtnGo = null;
                BtnInterested = null;
                FloatingActionButtonView = null;
                MainRecyclerView = null;
                BtnMore = null;
                SwipeRefreshLayout = null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        #endregion

        #region Events

        //Event Show More : Copy Link , Share , Edit (If user isOwner_Event)
        private void BtnMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                arrayAdapter.Add(GetString(Resource.String.Lbl_CopeLink));
                arrayAdapter.Add(GetString(Resource.String.Lbl_Share));
                if (EventData.IsOwner)
                    arrayAdapter.Add(GetString(Resource.String.Lbl_Edit));

                dialogList.Title(GetString(Resource.String.Lbl_More));
                dialogList.Items(arrayAdapter);
                dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(this);
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void BtnInterestedOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                    return;
                }

                if (BtnInterested.Tag.ToString() == "false")
                {
                    BtnInterested.SetBackgroundResource(Resource.Drawable.follow_button_profile_friends_pressed);
                    BtnInterested.SetTextColor(Color.ParseColor("#ffffff"));
                    BtnInterested.Text = GetText(Resource.String.Lbl_Interested);
                    BtnInterested.Tag = "true";
                }
                else
                {
                    BtnInterested.SetBackgroundResource(Resource.Drawable.follow_button_profile_friends);
                    BtnInterested.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                    BtnInterested.Text = GetText(Resource.String.Lbl_Interested);
                    BtnInterested.Tag = "false";
                }

                var dataEvent = EventMainActivity.GetInstance()?.EventTab.MAdapter.EventList?.FirstOrDefault(a => a.Id == EventData.Id);
                if (dataEvent != null)
                {
                    dataEvent.IsInterested = Convert.ToBoolean(BtnInterested.Tag.ToString());
                }

                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Event.Interest_Event(EventData.Id) });
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void BtnGoOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                    return;
                }

                if (BtnGo.Tag.ToString() == "false")
                {
                    BtnGo.SetBackgroundResource(Resource.Drawable.follow_button_profile_friends_pressed);
                    BtnGo.SetTextColor(Color.ParseColor("#ffffff"));
                    BtnGo.Text = GetText(Resource.String.Lbl_Going);
                    BtnGo.Tag = "true";
                }
                else
                {
                    BtnGo.SetBackgroundResource(Resource.Drawable.follow_button_profile_friends);
                    BtnGo.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                    BtnGo.Text = GetText(Resource.String.Lbl_Go);
                    BtnGo.Tag = "false";
                }

                var list = EventMainActivity.GetInstance()?.EventTab.MAdapter?.EventList;
                var dataEvent = list?.FirstOrDefault(a => a.Id == EventData.Id);
                if (dataEvent != null)
                {
                    dataEvent.IsGoing = Convert.ToBoolean(BtnGo.Tag.ToString());
                    EventMainActivity.GetInstance()?.EventTab.MAdapter.NotifyItemChanged(list.IndexOf(dataEvent));
                }

                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Event.Go_To_Event(EventData.Id) });
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void AddPostOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(AddPostActivity));
                intent.PutExtra("Type", "SocialEvent");
                intent.PutExtra("PostId", EventData.Id);
                intent.PutExtra("itemObject", JsonConvert.SerializeObject(EventData));
                StartActivityForResult(intent, 2500);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        #region Location

        public async void OnMapReady(GoogleMap googleMap)
        {
            try
            {
                var latLng = await GetLocationFromAddress(EventData.Location);
                if (latLng != null)
                {
                    CurrentLatitude = latLng.Latitude;
                    CurrentLongitude = latLng.Longitude;
                }

                Map = googleMap;

                //Optional
                googleMap.UiSettings.ZoomControlsEnabled = false;
                googleMap.UiSettings.CompassEnabled = false;

                googleMap.MoveCamera(CameraUpdateFactory.ZoomIn());

                var makerOptions = new MarkerOptions();
                makerOptions.SetPosition(new LatLng(CurrentLatitude, CurrentLongitude));
                makerOptions.SetTitle(GetText(Resource.String.Lbl_EventPlace));

                Map.AddMarker(makerOptions);
                Map.MapType = GoogleMap.MapTypeNormal;

                if (AppSettings.SetTabDarkTheme)
                {
                    MapStyleOptions style = MapStyleOptions.LoadRawResourceStyle(this, Resource.Raw.map_dark);
                    Map.SetMapStyle(style);
                }

                CameraPosition.Builder builder = CameraPosition.InvokeBuilder();
                builder.Target(new LatLng(CurrentLatitude, CurrentLongitude));
                builder.Zoom(10);
                builder.Bearing(155);
                builder.Tilt(65);

                CameraPosition cameraPosition = builder.Build();

                CameraUpdate cameraUpdate = CameraUpdateFactory.NewCameraPosition(cameraPosition);
                googleMap.MoveCamera(cameraUpdate);

                Map.MapClick += MapOnMapClick;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void MapOnMapClick(object sender, GoogleMap.MapClickEventArgs e)
        {
            try
            {
                // Create a Uri from an intent string. Use the result to create an Intent. 
                var uri = Uri.Parse("geo:" + CurrentLatitude + "," + CurrentLongitude);
                var intent = new Intent(Intent.ActionView, uri);
                intent.SetPackage("com.google.android.apps.maps");
                intent.AddFlags(ActivityFlags.NewTask);
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private async Task<LatLng> GetLocationFromAddress(string strAddress)
        {
            var locale = Resources.Configuration.Locale;
            Geocoder coder = new Geocoder(this, locale);

            try
            {
                var address = await coder.GetFromLocationNameAsync(strAddress, 2);
                if (address == null)
                    return null;

                Address location = address[0];
                var lat = location.Latitude;
                var lng = location.Longitude;

                LatLng p1 = new LatLng(lat, lng);
                return p1;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
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

                if (requestCode == 2500 && resultCode == Result.Ok) //add post
                {
                    if (!string.IsNullOrEmpty(data.GetStringExtra("itemObject")))
                    {
                        var postData = JsonConvert.DeserializeObject<PostDataObject>(data.GetStringExtra("itemObject"));
                        if (postData != null)
                        {
                            var countList = PostFeedAdapter.ItemCount;

                            var combine = new FeedCombiner(postData, PostFeedAdapter.ListDiffer, this);
                            combine.CombineDefaultPostSections("Top");

                            int countIndex = 1;
                            var model1 = PostFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.Story);
                            var model2 = PostFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.AddPostBox);
                            var model3 = PostFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.AlertBox);
                            var model4 = PostFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.SearchForPosts);

                            if (model4 != null)
                                countIndex += PostFeedAdapter.ListDiffer.IndexOf(model4) + 1;
                            else if (model3 != null)
                                countIndex += PostFeedAdapter.ListDiffer.IndexOf(model3) + 1;
                            else if (model2 != null)
                                countIndex += PostFeedAdapter.ListDiffer.IndexOf(model2) + 1;
                            else if (model1 != null)
                                countIndex += PostFeedAdapter.ListDiffer.IndexOf(model1) + 1;
                            else
                                countIndex = 0;

                            PostFeedAdapter.NotifyItemRangeInserted(countIndex, PostFeedAdapter.ListDiffer.Count - countList);
                        }
                    }
                    else
                    {
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => MainRecyclerView.FetchNewsFeedApiPosts() });
                    }
                }
                else if (requestCode == 3950 && resultCode == Result.Ok) //Edit post
                {
                    var postId = data.GetStringExtra("PostId") ?? "";
                    var postText = data.GetStringExtra("PostText") ?? "";
                    var diff = PostFeedAdapter.ListDiffer;
                    List<AdapterModelsClass> dataGlobal = diff.Where(a => a.PostData?.Id == postId).ToList();
                    if (dataGlobal.Count > 0)
                    {
                        foreach (var postData in dataGlobal)
                        {
                            postData.PostData.Orginaltext = postText;
                            var index = diff.IndexOf(postData);
                            if (index > -1)
                            {
                                PostFeedAdapter.NotifyItemChanged(index);
                            }
                        }

                        var checkTextSection = dataGlobal.FirstOrDefault(w => w.TypeView == PostModelType.TextSectionPostPart);
                        if (checkTextSection == null)
                        {
                            var collection = dataGlobal.FirstOrDefault()?.PostData;
                            var item = new AdapterModelsClass
                            {
                                TypeView = PostModelType.TextSectionPostPart,
                                Id = int.Parse((int)PostModelType.TextSectionPostPart + collection?.Id),
                                PostData = collection,
                                IsDefaultFeedPost = true
                            };

                            var headerPostIndex = diff.IndexOf(dataGlobal.FirstOrDefault(w => w.TypeView == PostModelType.HeaderPost));
                            if (headerPostIndex > -1)
                            {
                                diff.Insert(headerPostIndex + 1, item);
                                PostFeedAdapter.NotifyItemInserted(headerPostIndex + 1);
                            }
                        }
                    }
                }
                else if (requestCode == 3500 && resultCode == Result.Ok) //Edit post product 
                {
                    if (string.IsNullOrEmpty(data.GetStringExtra("itemData"))) return;
                    var item = JsonConvert.DeserializeObject<ProductDataObject>(data.GetStringExtra("itemData"));
                    if (item != null)
                    {
                        var diff = PostFeedAdapter.ListDiffer;
                        var dataGlobal = diff.Where(a => a.PostData?.Id == item.PostId).ToList();
                        if (dataGlobal.Count > 0)
                        {
                            foreach (var postData in dataGlobal)
                            {
                                var index = diff.IndexOf(postData);
                                if (index > -1)
                                {
                                    var productUnion = postData.PostData.Product?.ProductClass;
                                    if (productUnion != null) productUnion.Id = item.Id;
                                    productUnion = item;
                                    Console.WriteLine(productUnion);

                                    PostFeedAdapter.NotifyItemChanged(PostFeedAdapter.ListDiffer.IndexOf(postData));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region MaterialDialog

        public void OnSelection(MaterialDialog p0, View p1, int itemId, ICharSequence itemString)
        {
            try
            {
                string text = itemString.ToString();
                if (text == GetString(Resource.String.Lbl_CopeLink))
                {
                    CopyLinkEvent();
                }
                else if (text == GetString(Resource.String.Lbl_Share))
                {
                    ShareEvent();
                }
                else if (text == GetString(Resource.String.Lbl_Edit))
                {
                    EditInfoEvent_OnClick();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void OnClick(MaterialDialog p0, DialogAction p1)
        {
            try
            {
                if (p1 == DialogAction.Positive)
                {

                }
                else if (p1 == DialogAction.Negative)
                {
                    p0.Dismiss();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //Event Menu >> Copy Link
        private void CopyLinkEvent()
        {
            try
            {
                var clipboardManager = (ClipboardManager)GetSystemService(ClipboardService);

                var clipData = ClipData.NewPlainText("text", EventData.Url);
                clipboardManager.PrimaryClip = clipData;

                Toast.MakeText(this, GetText(Resource.String.Lbl_Copied), ToastLength.Short).Show();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //Event Menu >> Share
        private async void ShareEvent()
        {
            try
            {
                //Share Plugin  
                if (!CrossShare.IsSupported) return;

                await CrossShare.Current.Share(new ShareMessage
                {
                    Title = EventData.Name,
                    Text = EventData.Description,
                    Url = EventData.Url
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
         
        //Event Menu >> Edit Info Event if user == is_owner
        private void EditInfoEvent_OnClick()
        {
            try
            {
                if (EventData.IsOwner)
                {
                    var intent = new Intent(this, typeof(EditEventActivity));
                    intent.PutExtra("EventData", JsonConvert.SerializeObject(EventData));
                    intent.PutExtra("EventId", EventData.Id);
                    StartActivity(intent);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion
         
        private void Get_Data_Event()
        {
            try
            {
                if (EventData != null)
                {
                    Glide.With(this).Load(EventData.Cover).Apply(new RequestOptions()).Into(ImageEventCover);

                    Name = Methods.FunString.DecodeString(EventData.Name);

                    TxtName.Text = Name;
                    ToolbarLayout.Title = Name;
                    SupportActionBar.Title = Name;

                    if (string.IsNullOrEmpty(EventData.GoingCount))
                        EventData.GoingCount = "0";

                    if (string.IsNullOrEmpty(EventData.InterestedCount))
                        EventData.InterestedCount = "0";

                    TxtGoing.Text = EventData.GoingCount + " " + GetText(Resource.String.Lbl_GoingPeople);
                    TxtInterested.Text = EventData.InterestedCount + " " + GetText(Resource.String.Lbl_InterestedPeople);
                    TxtLocation.Text = EventData.Location;

                    TxtStartDate.Text = EventData.StartDate;
                    TxtEndDate.Text = EventData.EndDate;


                    if (!string.IsNullOrEmpty(EventData.Description))
                    {
                        var description = Methods.FunString.DecodeString(EventData.Description);
                        var readMoreOption = new StReadMoreOption.Builder()
                            .TextLength(250, StReadMoreOption.TypeCharacter)
                            .MoreLabel(GetText(Resource.String.Lbl_ReadMore))
                            .LessLabel(GetText(Resource.String.Lbl_ReadLess))
                            .MoreLabelColor(Color.ParseColor(AppSettings.MainColor))
                            .LessLabelColor(Color.ParseColor(AppSettings.MainColor))
                            .LabelUnderLine(true)
                            .Build();
                         readMoreOption.AddReadMoreTo(TxtDescriptionText, new String(description)); 
                    }
                    else
                    {
                        TxtDescription.Visibility = ViewStates.Gone;
                        TxtDescriptionText.Visibility = ViewStates.Gone; 
                    }

                    if (EventData.IsGoing != null && EventData.IsGoing.Value)
                    {
                        BtnGo.SetBackgroundResource(Resource.Drawable.follow_button_profile_friends_pressed);
                        BtnGo.SetTextColor(Color.ParseColor("#ffffff"));
                        BtnGo.Text = GetText(Resource.String.Lbl_Going);
                        BtnGo.Tag = "true";
                    }
                    else
                    {
                        BtnGo.SetBackgroundResource(Resource.Drawable.follow_button_profile_friends);
                        BtnGo.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                        BtnGo.Text = GetText(Resource.String.Lbl_Go);
                        BtnGo.Tag = "false";
                    }

                    if (EventData.IsInterested != null && EventData.IsInterested.Value)
                    {
                        BtnInterested.SetBackgroundResource(Resource.Drawable.follow_button_profile_friends_pressed);
                        BtnInterested.SetTextColor(Color.ParseColor("#ffffff"));
                        BtnInterested.Text = GetText(Resource.String.Lbl_Interested);
                        BtnInterested.Tag = "true";
                    }
                    else
                    {
                        BtnInterested.SetBackgroundResource(Resource.Drawable.follow_button_profile_friends);
                        BtnInterested.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                        BtnInterested.Text = GetText(Resource.String.Lbl_Interested);
                        BtnInterested.Tag = "false";
                    }

                    //add post  
                    var checkSection = PostFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.AddPostBox);
                    if (checkSection == null)
                    {
                        Combiner.AddPostBoxPostView("Event", -1, new PostDataObject() { Event = new EventUnion() { EventClass = EventData } });
                         
                        PostFeedAdapter.NotifyItemInserted(PostFeedAdapter.ListDiffer.Count -1);
                    }

                    StartApiService();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => MainRecyclerView.FetchNewsFeedApiPosts() });
        }
        public void OnOffsetChanged(AppBarLayout appBarLayout, int verticalOffset)
        {
            if (ScrollRange == -1)
            {
                ScrollRange = appBarLayout.TotalScrollRange;
            }
            if (ScrollRange + verticalOffset == 0)
            {
                ToolbarLayout.Title = Name;
                IsShow = true;
            }
            else if (IsShow)
            {
                ToolbarLayout.Title = " ";
                IsShow = false;
            }
        }
    }
}