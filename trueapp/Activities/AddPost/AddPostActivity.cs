using System;
using System.Collections.Generic;
using System.Linq;
using AFollestad.MaterialDialogs;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Com.Luseen.Autolinklibrary;
using Com.Sothree.Slidinguppanel;
using Com.Theartofdev.Edmodo.Cropper;
using Java.Lang;
using Newtonsoft.Json;
using ObeeNetwork.Activities.AddPost.Adapters;
using ObeeNetwork.Activities.AddPost.Service;
using ObeeNetwork.Helpers.Ads;
using ObeeNetwork.Helpers.CacheLoaders;
using ObeeNetwork.Helpers.Controller;
using ObeeNetwork.Helpers.Model;
using ObeeNetwork.Helpers.Utils;
using ObeeNetworkClient.Classes.Event;
using ObeeNetworkClient.Classes.Global;
using ObeeNetworkClient.Classes.Posts;
using Xamarin.Facebook.Ads;
using Console = System.Console;
using Exception = System.Exception;
using File = Java.IO.File;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using Uri = Android.Net.Uri;

namespace ObeeNetwork.Activities.AddPost
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class AddPostActivity : AppCompatActivity , SlidingPaneLayout.IPanelSlideListener, SlidingUpPanelLayout.IPanelSlideListener, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback, MaterialDialog.IInputCallback
    {
        #region Variables Basic

        private Toolbar TopToolBar;
        public SlidingUpPanelLayout SlidingUpPanel;
        private ImageView PostSectionImage;
        private TextView TxtAddPost, TxtUserName;
        private EditText TxtContentPost;
        private RecyclerView PostTypeRecyclerView, AttachmentRecyclerView, PollRecyclerView, ColorBoxRecyclerView;
        private MainPostAdapter MainPostAdapter;
        public AttachmentsAdapter AttachmentsAdapter;
        private ImageView IconHappy, IconTag, IconImage, ColoredImage;
        private AddPollAdapter AddPollAnswerAdapter;
        private ColorBoxAdapter ColorBoxAdapter;
        private NestedScrollView ScrollView;
        private View ImportPanel;
        private Button AddAnswerButton, PostPrivacyButton;
        public Button NameAlbumButton;
        private AutoLinkTextView MentionTextView;
        private string MentionText = "", PlaceText = "", FeelingText = "";
        private readonly string ActivityText = "";
        private string ListeningText = "", PlayingText = "", WatchingText = "", TravelingText = "", GifFile = "" , AlbumName = "";
        private string PagePost = "", IdPost = "", PostPrivacy = "", IdColor = "";
        private string PostFeelingType = "", PostFeelingText = "";
        private readonly string PostActivityType = "";
        private string TypeDialog = "", PermissionsType = "";
        private TextSanitizer TextSanitizer; 
        private EventDataObject DataEvent;
        private GroupClass DataGroup;
        private PageClass DataPage;
        private static AddPostActivity Instance;
        private InterstitialAd InterstitialAd;
        private UserDataObject DataUser;
        private VoiceRecorder VoiceRecorder;
        
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
                SetContentView(Resource.Layout.AddPost_Layout);

                var postdate = Intent.GetStringExtra("Type") ?? "Data not available";
                if (postdate != "Data not available" && !string.IsNullOrEmpty(postdate)) PagePost = postdate;

                var id = Intent.GetStringExtra("PostId") ?? "Data not available";
                if (id != "Data not available" && !string.IsNullOrEmpty(id)) IdPost = id;

                Instance = this;

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();

                GetPrivacyPost();

                TextSanitizer = new TextSanitizer(MentionTextView, this, "AddPost");

                InterstitialAd = AdsFacebook.InitInterstitial(this);

                Methods.Path.Chack_MyFolder();
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
                InterstitialAd?.Destroy();
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

        public static AddPostActivity GetInstance()
        {
            return Instance;
        }

        private void InitComponent()
        {
            try
            {
                TxtAddPost = FindViewById<TextView>(Resource.Id.toolbar_title);
                TxtContentPost = FindViewById<EditText>(Resource.Id.editTxtEmail);
                SlidingUpPanel = FindViewById<SlidingUpPanelLayout>(Resource.Id.sliding_layout);
                PostSectionImage = FindViewById<ImageView>(Resource.Id.postsectionimage);
                PostTypeRecyclerView = FindViewById<RecyclerView>(Resource.Id.Recyler);
                AttachmentRecyclerView = FindViewById<RecyclerView>(Resource.Id.AttachementRecyler);
                TxtUserName = FindViewById<TextView>(Resource.Id.card_name);
                IconImage = FindViewById<ImageView>(Resource.Id.ImageIcon);
                IconHappy = FindViewById<ImageView>(Resource.Id.Activtyicon);
                IconTag = FindViewById<ImageView>(Resource.Id.TagIcon);
                ScrollView = FindViewById<NestedScrollView>(Resource.Id.scroll_View);
                ColorBoxRecyclerView = FindViewById<RecyclerView>(Resource.Id.ColorboxRecyler);
                ColoredImage = FindViewById<ImageView>(Resource.Id.ColorImage);
                NameAlbumButton = FindViewById<Button>(Resource.Id.nameAlbumButton);

                IconTag.Tag = "Close";

                Methods.SetColorEditText(TxtContentPost, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                MentionTextView = FindViewById<AutoLinkTextView>(Resource.Id.MentionTextview);
                PostPrivacyButton = FindViewById<Button>(Resource.Id.cont);

                TxtContentPost.ClearFocus();
                SlidingUpPanel.SetPanelState(SlidingUpPanelLayout.PanelState.Collapsed);
                SlidingUpPanel.AddPanelSlideListener(this); 
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
                TopToolBar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (TopToolBar != null)
                {
                    TopToolBar.Title = GetText(Resource.String.Lbl_AddPost);
                    TopToolBar.SetTitleTextColor(Color.White);
                    SetSupportActionBar(TopToolBar);
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
                PostTypeRecyclerView.SetLayoutManager(new LinearLayoutManager(this));
                MainPostAdapter = new MainPostAdapter(this);
                PostTypeRecyclerView.SetAdapter(MainPostAdapter);

                AttachmentsAdapter = new AttachmentsAdapter(this);
                AttachmentRecyclerView.SetLayoutManager(new LinearLayoutManager(this, LinearLayoutManager.Horizontal, false));
                AttachmentRecyclerView.SetAdapter(AttachmentsAdapter);
                AttachmentRecyclerView.NestedScrollingEnabled = false;

                if (AppSettings.ShowColor)
                {
                    ColorBoxAdapter = new ColorBoxAdapter(this, ColorBoxRecyclerView);
                    ColorBoxRecyclerView.NestedScrollingEnabled = false;

                    ColorBoxRecyclerView.Visibility = ViewStates.Visible;
                }
                else
                {
                    ColorBoxRecyclerView.Visibility = ViewStates.Invisible;
                }
                 
                if (ColorBoxAdapter.ColorsList.Count == 0)
                    ColorBoxRecyclerView.Visibility = ViewStates.Invisible;
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
                    AttachmentsAdapter.DeleteItemClick += AttachmentsAdapterOnDeleteItemClick;
                    PostPrivacyButton.Click += PostPrivacyButton_Click;
                    MainPostAdapter.ItemClick += MainPostAdapterOnItemClick;
                    NameAlbumButton.Click += NameAlbumButtonOnClick;
                    TxtAddPost.Click += TxtAddPostOnClick;
                    if (AppSettings.ShowColor)
                        ColorBoxAdapter.ItemClick += ColorBoxAdapter_ItemClick;
                }
                else
                {
                    AttachmentsAdapter.DeleteItemClick -= AttachmentsAdapterOnDeleteItemClick;
                    PostPrivacyButton.Click -= PostPrivacyButton_Click;
                    MainPostAdapter.ItemClick -= MainPostAdapterOnItemClick;
                    TxtAddPost.Click -= TxtAddPostOnClick;
                    NameAlbumButton.Click -= NameAlbumButtonOnClick;
                    if (AppSettings.ShowColor)
                        ColorBoxAdapter.ItemClick -= ColorBoxAdapter_ItemClick;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Events

        private void AttachmentsAdapterOnDeleteItemClick(object sender, AttachmentsAdapterClickEventArgs e)
        {
            try
            {
                var position = e.Position;
                if (position >= 0)
                {
                    var item = AttachmentsAdapter.GetItem(position);
                    if (item != null)
                    {
                        AttachmentsAdapter.Remove(item);

                        //remove file the type
                        var listAttach = AttachmentsAdapter.AttachmentList.Where(a => a.TypeAttachment.Contains("postPhotos")).ToList();
                        if (listAttach.Count > 1)
                        {
                            NameAlbumButton.Visibility = ViewStates.Visible;

                            foreach (var attachments in listAttach)
                                attachments.TypeAttachment = "postPhotos[]";
                        }
                        else
                        {
                            NameAlbumButton.Visibility = ViewStates.Gone;

                            foreach (var attachments in listAttach.Where(attachments => attachments.TypeAttachment.Contains("postPhotos")))
                            {
                                attachments.TypeAttachment = "postPhotos";
                            }
                        }  
                    }
                } 
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
         
        private void NameAlbumButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                TypeDialog = "AddPicturesToAlbumName";

                var dialog = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                dialog.Title(GetText(Resource.String.Lbl_AddPicturesToAlbum));
                dialog.Input(Resource.String.Lbl_AlbumName, 0, false, this);
                dialog.InputType(InputTypes.TextFlagImeMultiLine);
                dialog.PositiveText(GetText(Resource.String.Lbl_Submit)).OnPositive(this);
                dialog.NegativeText(GetText(Resource.String.Lbl_Cancel)).OnNegative(this);
                dialog.AlwaysCallSingleChoiceCallback();
                dialog.ItemsCallback(this).Build().Show();  
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
         
        //Event Add post 
        private void TxtAddPostOnClick(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(TxtContentPost.Text) && string.IsNullOrEmpty(MentionTextView.Text) && AttachmentsAdapter.AttachmentList.Count == 0)
                {
                    Toast.MakeText(this, GetString(Resource.String.Lbl_YouCannot_PostanEmptyPost), ToastLength.Long).Show();
                }
                else
                {
                    if (!Methods.CheckConnectivity())
                    {
                        Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                        return;
                    }

                    string content = !string.IsNullOrEmpty(MentionText) ? TxtContentPost.Text + " " + GetText(Resource.String.Lbl_With) + " " + MentionText.Remove(MentionText.Length - 1, 1) : TxtContentPost.Text;

                    if (ListUtils.SettingsSiteList?.MaxCharacters != null)
                    {
                        int max = int.Parse(ListUtils.SettingsSiteList?.MaxCharacters);
                        if (max < content.Length)
                        {
                            //You have exceeded the text limit, must be less than ListUtils.SettingsSiteList?.MaxCharacters
                            Toast.MakeText(this, GetString(Resource.String.Lbl_Error_MaxCharacters) + " " + ListUtils.SettingsSiteList?.MaxCharacters, ToastLength.Short).Show();
                            return;
                        }
                    }

                    //Show a progress
                    //AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

                    var item = new FileUpload()
                    {
                        IdPost = IdPost,
                        PagePost = PagePost,
                        Content = content,
                        PostPrivacy = PostPrivacy,
                        PostFeelingType = PostFeelingType,
                        PostFeelingText = PostFeelingText,
                        PlaceText = PlaceText,
                        AttachmentList = AttachmentsAdapter.AttachmentList,
                        AnswersList = AddPollAnswerAdapter?.AnswersList,
                        IdColor = IdColor,
                        AlbumName = AlbumName,
                    };

                    Intent intent = new Intent(this, typeof(PostService));
                    intent.SetAction(PostService.ActionPost);
                    intent.PutExtra("DataPost", JsonConvert.SerializeObject(item));
                    StartService(intent);

                    Finish();

                    //var (apiStatus, respond) = await ApiRequest.AddNewPost_Async(IdPost, PagePost, content, PostPrivacy, PostFeelingType, PostFeelingText, PlaceText, AttachmentsAdapter.AttachmentList, AddPollAnswerAdapter?.AnswersList, IdColor);
                    //if (apiStatus == 200)
                    //{
                    //    if (respond is AddPostObject postObject)
                    //    {
                    //        //AndHUD.Shared.Dismiss(this);
                    //        Toast.MakeText(this, GetText(Resource.String.Lbl_Post_Added), ToastLength.Short).Show();

                    //        // put the String to pass back into an Intent and close this activity
                    //        var resultIntent = new Intent();
                    //        if (postObject.PostData != null)
                    //        {
                    //            resultIntent.PutExtra("itemObject", JsonConvert.SerializeObject(postObject.PostData));
                    //        }
                    //        SetResult(Result.Ok, resultIntent);

                    //        if (UserDetails.SoundControl)
                    //            Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("PopNotificationPost.mp3");

                    //        RemoveNotification();

                    //        Finish();
                    //    }
                    //}
                    //else
                    //{
                    //    Methods.DisplayReportResult(this, respond);
                    //    //Show a Error image with a message
                    //    //AndHUD.Shared.ShowError(this, GetText(Resource.String.Lbl_Post_Failed), MaskType.Clear, TimeSpan.FromSeconds(2));
                    //}

                    //AndHUD.Shared.Dismiss(this);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                //AndHUD.Shared.ShowError(this, GetText(Resource.String.Lbl_Post_Failed), MaskType.Clear, TimeSpan.FromSeconds(2));
            }
        }
         
        private void MainPostAdapterOnItemClick(object sender, MainPostAdapterClickEventArgs e)
        {
            try
            {
                if (ImportPanel != null)
                    ImportPanel.Visibility = ViewStates.Gone;

                if (MainPostAdapter.PostTypeList[e.Position] != null)
                {
                    if (MainPostAdapter.PostTypeList[e.Position].Id == 1) //Image Gallery
                    {
                        PermissionsType = "Image";

                        // Check if we're running on Android 5.0 or higher 
                        if ((int)Build.VERSION.SdkInt < 23)
                        {
                            //if (AppSettings.ImageCropping)
                            //    OpenDialogGallery(); //requestCode >> 500 => Image Gallery
                            //else
                                new IntentController(this).OpenIntentImageGallery(GetText(Resource.String.Lbl_SelectPictures)); //requestCode >> 500 => Image Gallery
                        }
                        else
                        {
                            if (CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted && CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted
                                                                                                      && CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted)
                            {
                                //if (AppSettings.ImageCropping)
                                //    OpenDialogGallery(); //requestCode >> 500 => Image Gallery
                                //else
                                    new IntentController(this).OpenIntentImageGallery(GetText(Resource.String.Lbl_SelectPictures)); //requestCode >> 500 => Image Gallery
                            }
                            else
                            {
                                new PermissionsController(this).RequestPermission(108);
                            }
                        }
                    }
                    else if (MainPostAdapter.PostTypeList[e.Position].Id == 2) //video Gallery
                    {
                        OpenDialogVideo();
                    }
                    else if (MainPostAdapter.PostTypeList[e.Position].Id == 3) // Mention
                    {
                        StartActivityForResult(new Intent(this, typeof(MentionActivity)), 3);
                    }
                    else if (MainPostAdapter.PostTypeList[e.Position].Id == 4) // Location
                    {
                        // Check if we're running on Android 5.0 or higher
                        if ((int)Build.VERSION.SdkInt < 23)
                        {
                            //Open intent Location when the request code of result is 502
                            new IntentController(this).OpenIntentLocation();
                        }
                        else
                        {
                            if (CheckSelfPermission(Manifest.Permission.AccessFineLocation) == Permission.Granted && CheckSelfPermission(Manifest.Permission.AccessCoarseLocation) == Permission.Granted)
                            {
                                //Open intent Location when the request code of result is 502
                                new IntentController(this).OpenIntentLocation();
                            }
                            else
                            {
                                new PermissionsController(this).RequestPermission(105);
                            }
                        }
                    }
                    else if (MainPostAdapter.PostTypeList[e.Position].Id == 5) // Feeling
                    {
                        //StartActivityForResult(new Intent(this, typeof(Feelings_Activity)), 5);
                        try
                        {
                            TypeDialog = "Feelings";

                            var arrayAdapter = new List<string>();
                            var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);
                              
                            if (AppSettings.ShowFeeling)
                                arrayAdapter.Add(GetText(Resource.String.Lbl_Feeling));
                            if (AppSettings.ShowListening)
                                arrayAdapter.Add(GetText(Resource.String.Lbl_Listening));
                            if (AppSettings.ShowPlaying)
                                arrayAdapter.Add(GetText(Resource.String.Lbl_Playing));
                            if (AppSettings.ShowWatching)
                                arrayAdapter.Add(GetText(Resource.String.Lbl_Watching));
                            if (AppSettings.ShowTraveling)
                                arrayAdapter.Add(GetText(Resource.String.Lbl_Traveling));

                            dialogList.Title(GetString(Resource.String.Lbl_What_Are_You_Doing));
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
                    else if (MainPostAdapter.PostTypeList[e.Position].Id == 6) // Camera
                    {
                        PermissionsType = "Camera";

                        // Check if we're running on Android 5.0 or higher
                        if ((int)Build.VERSION.SdkInt < 23)
                        {
                            //requestCode >> 503 => Camera
                            new IntentController(this).OpenIntentCamera();
                        }
                        else
                        {
                            if (CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted && CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted
                                && CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted)
                            {
                                //requestCode >> 503 => Camera
                                new IntentController(this).OpenIntentCamera();
                            }
                            else
                            { 
                                new PermissionsController(this).RequestPermission(108);
                            }
                        }
                    }
                    else if (MainPostAdapter.PostTypeList[e.Position].Id == 7) // Gif
                    {
                        StartActivityForResult(new Intent(this, typeof(GifActivity)), 7);
                    }
                    else if (MainPostAdapter.PostTypeList[e.Position].Id == 8) // File
                    {
                        PermissionsType = "File";

                        // Check if we're running on Android 5.0 or higher
                        if ((int)Build.VERSION.SdkInt < 23)
                        {
                            //requestCode >> 504 => File
                            new IntentController(this).OpenIntentFile(GetText(Resource.String.Lbl_SelectFile));
                        }
                        else
                        {
                            if (CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted &&
                                CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted)
                            {
                                //requestCode >> 504 => File
                                new IntentController(this).OpenIntentFile(GetText(Resource.String.Lbl_SelectFile));
                            }
                            else
                            {
                                new PermissionsController(this).RequestPermission(108);
                            }
                        }
                    }
                    else if (MainPostAdapter.PostTypeList[e.Position].Id == 9) // Music
                    {
                        PermissionsType = "Music";

                        // Check if we're running on Android 5.0 or higher
                        if ((int)Build.VERSION.SdkInt < 23)
                            new IntentController(this).OpenIntentAudio(); //505
                        else
                        {
                            if (CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted && CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted)
                                new IntentController(this).OpenIntentAudio(); //505
                            else
                                new PermissionsController(this).RequestPermission(100);
                        }
                    }
                    else if (MainPostAdapter.PostTypeList[e.Position].Id == 10) // VoiceRecorder
                    {
                        PermissionsType = "Music";

                        // Check if we're running on Android 5.0 or higher
                        if ((int)Build.VERSION.SdkInt < 23)
                        { 
                            VoiceRecorder = new VoiceRecorder();
                            VoiceRecorder.Show(SupportFragmentManager, VoiceRecorder.Tag);  
                        }  
                        else
                        {
                            if (CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted && CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted)
                            {
                                VoiceRecorder = new VoiceRecorder();
                                VoiceRecorder.Show(SupportFragmentManager, VoiceRecorder.Tag);
                            }
                            else
                                new PermissionsController(this).RequestPermission(102);
                        }
                    }
                    else if (MainPostAdapter.PostTypeList[e.Position].Id == 11) // Polls
                    { 
                        if (ColoredImage.Visibility != ViewStates.Gone)
                        {
                            ColoredImage.Visibility = ViewStates.Gone;

                            TxtContentPost.SetTextColor(new Color(ContextCompat.GetColor(TxtContentPost.Context, Resource.Color.textDark_color)));
                            TxtContentPost.SetHintTextColor(new Color(ContextCompat.GetColor(TxtContentPost.Context, Resource.Color.textDark_color)));
                        }

                        TxtContentPost.ClearFocus();
                        SlidingUpPanel.SetPanelState(SlidingUpPanelLayout.PanelState.Collapsed);

                        if (ImportPanel == null)
                            ImportPanel = ((ViewStub)FindViewById(Resource.Id.stub_import)).Inflate();

                        if (PollRecyclerView == null)
                            PollRecyclerView = (RecyclerView)ImportPanel.FindViewById(Resource.Id.Recyler);

                        AttachmentsAdapter?.AttachmentList.Clear();
                        ImportPanel.Visibility = ViewStates.Visible;
                        AddPollAnswerAdapter = new AddPollAdapter(this);
                        PollRecyclerView.SetLayoutManager(new LinearLayoutManager(this, LinearLayoutManager.Vertical, false));
                        PollRecyclerView.SetAdapter(AddPollAnswerAdapter);
                        AddPollAnswerAdapter.AnswersList.Add(new PollAnswers { Answer = GetText(Resource.String.Lbl2_Polls) + " 1", Id = 1 });
                        AddPollAnswerAdapter.AnswersList.Add(new PollAnswers { Answer = GetText(Resource.String.Lbl2_Polls) + " 2", Id = 2 });
                        AddPollAnswerAdapter.NotifyDataSetChanged();

                        AddAnswerButton = (Button)ImportPanel.FindViewById(Resource.Id.addanswer);

                        if (!AddAnswerButton.HasOnClickListeners)
                            AddAnswerButton.Click += AddAnswerButtonOnClick;

                        PollRecyclerView.NestedScrollingEnabled = false;
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void ColorBoxAdapter_ItemClick(object sender, ColorBoxAdapterClickEventArgs e)
        {
            try
            {
                var item = ColorBoxAdapter.ColorsList[e.Position];
                if (item == null)
                    return;

                if (AttachmentsAdapter.AttachmentList.Count > 0)
                {
                    AttachmentsAdapter.AttachmentList.Clear();
                    AttachmentsAdapter.NotifyDataSetChanged();
                }

                IdColor = item.Id.ToString();
                if (item.Color1 == "#ffffff" && item.Color2 == "#efefef")
                {
                    ColoredImage.Visibility = ViewStates.Gone;

                    TxtContentPost.SetTextColor(new Color(ContextCompat.GetColor(TxtContentPost.Context, Resource.Color.textDark_color)));
                    TxtContentPost.SetHintTextColor(new Color(ContextCompat.GetColor(TxtContentPost.Context, Resource.Color.textDark_color)));

                    return;
                }

                ColoredImage.Visibility = ViewStates.Visible;
                if (!string.IsNullOrEmpty(item.Image))
                {
                    Glide.With(this).Load(item.Image).Apply(new RequestOptions()).Into(ColoredImage);
                    //GlideImageLoader.LoadImage(this, item.Image, ColoredImage, ImageStyle.FitCenter, ImagePlaceholders.Color, false);
                }
                else
                {
                    var colorsList = new List<int>();

                    if (!string.IsNullOrEmpty(item.Color1))
                        colorsList.Add(Color.ParseColor(item.Color1));

                    if (!string.IsNullOrEmpty(item.Color2))
                        colorsList.Add(Color.ParseColor(item.Color2));
                     
                    GradientDrawable gd = new GradientDrawable(GradientDrawable.Orientation.TopBottom, colorsList.ToArray());
                    gd.SetCornerRadius(0f);
                    ColoredImage.Background = (gd);
                }

                if (!string.IsNullOrEmpty(item.TextColor))
                {
                    TxtContentPost.SetTextColor(Color.ParseColor(item.TextColor));
                    TxtContentPost.SetHintTextColor(Color.ParseColor(item.TextColor));
                }

                TxtContentPost.Gravity = GravityFlags.CenterVertical; 
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        #region Permissions && Result

        private Uri UriData;

        //Result
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);

                SlidingUpPanel.SetPanelState(SlidingUpPanelLayout.PanelState.Collapsed);

                if (ColoredImage.Visibility != ViewStates.Gone)
                {
                    ColoredImage.Visibility = ViewStates.Gone;

                    TxtContentPost.SetTextColor(new Color(ContextCompat.GetColor(TxtContentPost.Context, Resource.Color.textDark_color)));
                    TxtContentPost.SetHintTextColor(new Color(ContextCompat.GetColor(TxtContentPost.Context, Resource.Color.textDark_color)));
                }
                  
                if (requestCode == 500 && resultCode == Result.Ok) // Add image 
                { 
                    if (data.ClipData != null)
                    {
                        var mClipData = data.ClipData;
                        for (var i = 0; i < mClipData.ItemCount; i++)
                        {
                            var item = mClipData.GetItemAt(i);
                            Uri uri = item.Uri;
                            var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, uri);
                            PickiTonCompleteListener(filepath);
                        }
                    }
                    else
                    {
                        Uri uri = data.Data;
                        var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, uri);
                        PickiTonCompleteListener(filepath);
                    }
                }
                else if (requestCode == 501 && resultCode == Result.Ok) // Add video 
                {
                    NameAlbumButton.Visibility = ViewStates.Gone;

                    AttachmentsAdapter.RemoveAll();

                    UriData = data.Data;
                    //PickiT.GetPath(uriData, (int)Build.VERSION.SdkInt);

                    var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, data.Data);
                    if (filepath != null)
                    {
                        var type = Methods.AttachmentFiles.Check_FileExtension(filepath);
                        if (type == "Video")
                        {
                            var fileName = filepath.Split('/').Last();
                            var fileNameWithoutExtension = fileName.Split('.').First();
                            var pathWithoutFilename = Methods.Path.FolderDcimImage;
                            var fullPathFile = new File(Methods.Path.FolderDcimImage, fileNameWithoutExtension + ".png");

                            var videoPlaceHolderImage = Methods.MultiMedia.GetMediaFrom_Gallery(pathWithoutFilename, fileNameWithoutExtension + ".png");
                            if (videoPlaceHolderImage == "File Dont Exists")
                            {
                                var bitmapImage = Methods.MultiMedia.Retrieve_VideoFrame_AsBitmap(this, data.Data.ToString());
                                Methods.MultiMedia.Export_Bitmap_As_Image(bitmapImage, fileNameWithoutExtension, pathWithoutFilename);
                            }

                            //remove file the type
                            var imageAttach = AttachmentsAdapter.AttachmentList.Where(a => a.TypeAttachment != "postVideo").ToList();
                            if (imageAttach.Count > 0)
                                foreach (var image in imageAttach)
                                    AttachmentsAdapter.Remove(image);
                              //wael
                            //File f = new File(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryMovies) + "/Silicompressor/videos");

                            //if (!Directory.Exists(f.Path))
                            //    Directory.CreateDirectory(f.Path);

                            //Uri videoContentUri = data.Data;
                            //new VideoCompressAsyncTask(this).Execute("false", filepath, f.Path);
                             
                            var attach = new Attachments
                            {
                                Id = AttachmentsAdapter.AttachmentList.Count + 1,
                                TypeAttachment = "postVideo",
                                FileSimple = fullPathFile.AbsolutePath,
                                Thumb = new Attachments.VideoThumb()
                                {
                                    FileUrl = fullPathFile.AbsolutePath
                                },
                                FileUrl = filepath
                            };

                            AttachmentsAdapter.Add(attach);
                        }
                    }
                }
                else if (requestCode == 513 && resultCode == Result.Ok) // Add video Camera 
                {
                    NameAlbumButton.Visibility = ViewStates.Gone;

                    AttachmentsAdapter.RemoveAll();

                    var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, data.Data);
                    if (filepath != null)
                    {
                        var type = Methods.AttachmentFiles.Check_FileExtension(filepath);
                        if (type == "Video")
                        {
                            var fileName = filepath.Split('/').Last();
                            var fileNameWithoutExtension = fileName.Split('.').First();
                            var pathWithoutFilename = Methods.Path.FolderDcimImage;
                            var fullPathFile = new File(Methods.Path.FolderDcimImage, fileNameWithoutExtension + ".png");

                            var videoPlaceHolderImage = Methods.MultiMedia.GetMediaFrom_Gallery(pathWithoutFilename, fileNameWithoutExtension + ".png");
                            if (videoPlaceHolderImage == "File Dont Exists")
                            {
                                var bitmapImage = Methods.MultiMedia.Retrieve_VideoFrame_AsBitmap(this, data.Data.ToString());
                                Methods.MultiMedia.Export_Bitmap_As_Image(bitmapImage, fileNameWithoutExtension, pathWithoutFilename);
                            }

                            //remove file the type
                            var imageAttach = AttachmentsAdapter.AttachmentList.Where(a => a.TypeAttachment != "postVideo").ToList();
                            if (imageAttach.Count > 0)
                                foreach (var image in imageAttach)
                                    AttachmentsAdapter.Remove(image);

                            var attach = new Attachments
                            {
                                Id = AttachmentsAdapter.AttachmentList.Count + 1,
                                TypeAttachment = "postVideo",
                                FileSimple = fullPathFile.AbsolutePath,
                                Thumb = new Attachments.VideoThumb()
                                {
                                    FileUrl = fullPathFile.AbsolutePath
                                },
                                FileUrl = filepath
                            };

                            AttachmentsAdapter.Add(attach);
                        }
                    }
                    else
                    {
                        UriData = data.Data;
                        var filepath2 = Methods.AttachmentFiles.GetActualPathFromFile(this, UriData);
                        PickiTonCompleteListener(filepath2);
                    } 
                }
                else if (requestCode == 3 && resultCode == Result.Ok) // Mention
                {
                    try
                    {
                        var dataUser = MentionActivity.MAdapter.MentionList.Where(a => a.Selected).ToList();
                        if (dataUser.Count > 0)
                        { 
                            foreach (var item in dataUser) MentionText += " @" + item.Username + " ,";

                            TextSanitizer.Load(LoadPostStrings());
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
                else if (requestCode == 502 && resultCode == Result.Ok) // Location
                {
                    var placeAddress = data.GetStringExtra("Address") ?? "";
                    //var placeLatLng = data.GetStringExtra("latLng") ?? "";
                    if (!string.IsNullOrEmpty(placeAddress))
                    {
                        if (!string.IsNullOrEmpty(PlaceText))
                            PlaceText = string.Empty;


                        PlaceText = " /" + placeAddress;
                        TextSanitizer.Load(LoadPostStrings());
                    }

                }
                else if (requestCode == 5 && resultCode == Result.Ok) // Feeling
                {
                    var feelings = data.GetStringExtra("FeelingName") ?? "Data not available";
                    var feelingsDisplayText = data.GetStringExtra("Feelings") ?? "Data not available";
                    if (feelings != "Data not available" && !string.IsNullOrEmpty(feelings))
                    {

                        FeelingText = feelingsDisplayText; //This Will be displayed And translated
                        PostFeelingType = "feelings"; //Type Of feeling
                        PostFeelingText = feelings.ToLower(); //This will be send via API
                        TextSanitizer.Load(LoadPostStrings());
                    }
                }
                else if (requestCode == 503 && resultCode == Result.Ok) // Add image using camera
                { 
                    //remove file the type
                    var videoAttach = AttachmentsAdapter.AttachmentList.Where(a => !a.TypeAttachment.Contains("postPhotos")).ToList();
                    if (videoAttach.Count > 0)
                        foreach (var video in videoAttach)
                            AttachmentsAdapter.Remove(video);

                    if (string.IsNullOrEmpty(IntentController.CurrentPhotoPath))
                    {
                        Uri uri = data.Data;
                        var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, uri);
                        PickiTonCompleteListener(filepath);
                    }
                    else
                    {
                        if (Methods.MultiMedia.CheckFileIfExits(IntentController.CurrentPhotoPath) != "File Dont Exists")
                        { 
                            var attach = new Attachments
                            {
                                Id = AttachmentsAdapter.AttachmentList.Count + 1,
                                TypeAttachment = "postPhotos",
                                FileSimple = IntentController.CurrentPhotoPath,
                                FileUrl = IntentController.CurrentPhotoPath
                            };

                            AttachmentsAdapter.Add(attach);

                            if (AttachmentsAdapter.AttachmentList.Count > 1)
                            {
                                NameAlbumButton.Visibility = ViewStates.Visible;

                                foreach (var item in AttachmentsAdapter.AttachmentList)
                                    item.TypeAttachment = "postPhotos[]";
                            }
                            else
                            {
                                NameAlbumButton.Visibility = ViewStates.Gone;

                                foreach (var item in AttachmentsAdapter.AttachmentList)
                                    item.TypeAttachment = "postPhotos";
                            }
                        }
                        else
                        {
                            //Toast.MakeText(this, GetText(Resource.String.Lbl_Failed_to_load), ToastLength.Short).Show();
                        }
                    }
                }
                else if (requestCode == 7 && resultCode == Result.Ok) // Gif
                {
                    var giflink = data.GetStringExtra("gif") ?? "Data not available";
                    if (giflink != "Data not available" && !string.IsNullOrEmpty(giflink))
                    {
                        GifFile = giflink;

                        //remove file the type
                        AttachmentsAdapter.RemoveAll();

                        var attach = new Attachments
                        {
                            Id = AttachmentsAdapter.AttachmentList.Count + 1,
                            TypeAttachment = "postPhotos",
                            FileSimple = GifFile,
                            FileUrl = GifFile
                        };

                        AttachmentsAdapter.Add(attach);
                    }
                }
                else if (requestCode == 504 && resultCode == Result.Ok) // File
                {
                    Uri uri = data.Data;
                    var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, uri);
                    PickiTonCompleteListener(filepath);
                }
                else if (requestCode == 505 && resultCode == Result.Ok) // Music
                {
                    Uri uri = data.Data;
                    var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, uri);
                    PickiTonCompleteListener(filepath);
                }
                else if (requestCode == CropImage.CropImageActivityRequestCode && resultCode == Result.Ok) // Add image 
                { 
                    var videoAttach = AttachmentsAdapter.AttachmentList.Where(a => !a.TypeAttachment.Contains("postPhotos")).ToList();

                    if (videoAttach.Count > 0)
                        foreach (var video in videoAttach)
                            AttachmentsAdapter.Remove(video);

                    var result = CropImage.GetActivityResult(data);

                    if (result.IsSuccessful)
                    {
                        var resultUri = result.Uri;

                        if (!string.IsNullOrEmpty(resultUri.Path))
                        {
                            var attach = new Attachments
                            {
                                Id = AttachmentsAdapter.AttachmentList.Count + 1,
                                TypeAttachment = "postPhotos",
                                FileSimple = resultUri.Path,
                                FileUrl = resultUri.Path
                            };

                            AttachmentsAdapter.Add(attach);

                            if (AttachmentsAdapter.AttachmentList.Count > 1)
                            {
                                NameAlbumButton.Visibility = ViewStates.Visible;

                                foreach (var item in AttachmentsAdapter.AttachmentList)
                                {
                                    item.TypeAttachment = "postPhotos[]";
                                }
                            }
                            else
                            {
                                NameAlbumButton.Visibility = ViewStates.Gone;

                                foreach (var item in AttachmentsAdapter.AttachmentList)
                                    item.TypeAttachment = "postPhotos";
                            }
                        }
                        else
                        {
                            Toast.MakeText(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long).Show();
                        }
                    }
                }

                TxtContentPost.ClearFocus();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //Permissions
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                if (requestCode == 108)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        switch (PermissionsType)
                        {
                            //requestCode >> 500 => Image Gallery
                            //case "Image" when AppSettings.ImageCropping:
                            //    OpenDialogGallery();
                            //    break;
                            case "Image": //requestCode >> 500 => Image Gallery
                                new IntentController(this).OpenIntentImageGallery(GetText(Resource.String.Lbl_SelectPictures));
                                break;
                            case "VideoGallery":
                                //requestCode >> 501 => video Gallery
                                new IntentController(this).OpenIntentVideoGallery();
                                break;
                            case "VideoCamera":
                                //requestCode >> 513 => video Camera
                                new IntentController(this).OpenIntentVideoCamera();
                                break;
                            case "Camera":
                                //requestCode >> 503 => Camera
                                new IntentController(this).OpenIntentCamera();
                                break;
                            case "File":
                                //requestCode >> 504 => File
                                new IntentController(this).OpenIntentFile(GetText(Resource.String.Lbl_SelectFile));
                                break;
                            case "Music":
                                //requestCode >> 505 => Music
                                new IntentController(this).OpenIntentAudio();
                                break;
                        }
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long).Show();
                    }
                }
                else if (requestCode == 105)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        //Open intent Location when the request code of result is 502
                        new IntentController(this).OpenIntentLocation();
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long).Show();
                    }
                }
                else if (requestCode == 102)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        VoiceRecorder = new VoiceRecorder();
                        VoiceRecorder.Show(SupportFragmentManager, VoiceRecorder.Tag);
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long).Show();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Panel Item Post

        public void OnPanelClosed(View panel)
        {
            try
            {
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void OnPanelOpened(View panel)
        {
            try
            {
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        void SlidingPaneLayout.IPanelSlideListener.OnPanelSlide(View panel, float slideOffset)
        {
            try
            {
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void OnPanelStateChanged(View p0, SlidingUpPanelLayout.PanelState p1, SlidingUpPanelLayout.PanelState p2)
        {
            try
            {
                if (p1 == SlidingUpPanelLayout.PanelState.Expanded && p2 == SlidingUpPanelLayout.PanelState.Dragging)
                {
                    if (IconTag.Tag.ToString() == "Open")
                    {
                        IconTag.SetImageResource(Resource.Drawable.icon_mention_contact_vector);
                        IconTag.Tag = "Close";
                        IconImage.Visibility = ViewStates.Visible;
                        IconHappy.Visibility = ViewStates.Visible;
                    }
                }
                else if (p1 == SlidingUpPanelLayout.PanelState.Collapsed && p2 == SlidingUpPanelLayout.PanelState.Dragging)
                {
                    if (IconTag.Tag.ToString() == "Close")
                    {
                        IconTag.SetImageResource(Resource.Drawable.ic_action_arrow_down_sign);
                        IconTag.Tag = "Open";
                        IconImage.Visibility = ViewStates.Invisible;
                        IconHappy.Visibility = ViewStates.Invisible;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        void SlidingUpPanelLayout.IPanelSlideListener.OnPanelSlide(View p0, float p1)
        {
            try
            {
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Privacy

        private void LoadDataUser()
        {
            try
            {
                if (DataUser != null)
                {
                    GlideImageLoader.LoadImage(this, DataUser.Avatar, PostSectionImage, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                    TxtUserName.Text = ObeeNetworkTools.GetNameFinal(DataUser);

                    PostPrivacyButton.Text = GetString(Resource.String.Lbl_Everyone);

                    //if (dataUser.post_privacy.Contains("0"))
                    //    PostPrivacyButton.Text = GetString(Resource.String.Lbl_Everyone);
                    //else if (dataUser.post_privacy.Contains("ifollow"))
                    //    PostPrivacyButton.Text = GetString(Resource.String.Lbl_People_i_Follow);
                    //else if (dataUser.post_privacy.Contains("me"))
                    //    PostPrivacyButton.Text = GetString(Resource.String.Lbl_People_Follow_Me);
                    //else
                    //    PostPrivacyButton.Text = GetString(Resource.String.Lbl_No_body);

                    PostPrivacy = "0";
                }
                else
                {
                    TxtUserName.Text = UserDetails.Username;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
         
        private void GetPrivacyPost()
        {
            try
            {
                DataUser = ListUtils.MyProfileList?.FirstOrDefault();
                if (PagePost == "Normal" || PagePost == "Normal_More" || PagePost == "Normal_Gallery")
                {
                    LoadDataUser();

                    switch (PagePost)
                    {
                        case "Normal_More":
                            SlidingUpPanel.SetPanelState(SlidingUpPanelLayout.PanelState.Expanded);
                            break;
                        case "Normal_Gallery":
                            {
                                PermissionsType = "Image";

                                // Check if we're running on Android 5.0 or higher 
                                if ((int)Build.VERSION.SdkInt < 23)
                                {
                                    //if (AppSettings.ImageCropping)
                                    //    OpenDialogGallery(); //requestCode >> 500 => Image Gallery
                                    //else
                                        new IntentController(this).OpenIntentImageGallery(GetText(Resource.String.Lbl_SelectPictures)); //requestCode >> 500 => Image Gallery
                                }
                                else
                                {
                                    if (CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted && CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted
                                                                                                              && CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted)
                                    {
                                        //if (AppSettings.ImageCropping)
                                        //    OpenDialogGallery(); //requestCode >> 500 => Image Gallery
                                        //else
                                            new IntentController(this).OpenIntentImageGallery(GetText(Resource.String.Lbl_SelectPictures)); //requestCode >> 500 => Image Gallery
                                    }
                                    else
                                    {
                                        new PermissionsController(this).RequestPermission(108);
                                    }
                                }
                                break;
                            }
                    }
                }
                else if (PagePost == "SocialGroup")
                {
                    DataGroup = JsonConvert.DeserializeObject<GroupClass>(Intent.GetStringExtra("itemObject"));
                    if (DataGroup != null)
                    {
                        PostPrivacyButton.SetBackgroundResource(0);
                        PostPrivacyButton.Enabled = false;
                        PostPrivacyButton.Text = GetText(Resource.String.Lbl_PostingAs) + " " + ObeeNetworkTools.GetNameFinal(DataUser);

                        GlideImageLoader.LoadImage(this, DataGroup.Avatar, PostSectionImage, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                        TxtUserName.Text = DataGroup.GroupName;
                    }
                    else
                    {
                        LoadDataUser();
                    }
                }
                else if (PagePost == "SocialPage")
                {
                    DataPage = JsonConvert.DeserializeObject<PageClass>(Intent.GetStringExtra("itemObject"));
                    if (DataPage != null)
                    {
                        PostPrivacyButton.SetBackgroundResource(0);
                        PostPrivacyButton.Enabled = false;
                        PostPrivacyButton.Text = GetText(Resource.String.Lbl_PostingAs) + " " + ObeeNetworkTools.GetNameFinal(DataUser);

                        GlideImageLoader.LoadImage(this, DataPage.Avatar, PostSectionImage, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                        TxtUserName.Text = DataPage.PageName;
                    }
                    else
                    {
                        LoadDataUser();
                    }
                }
                else if (PagePost == "SocialEvent")
                {
                    DataEvent = JsonConvert.DeserializeObject<EventDataObject>(Intent.GetStringExtra("itemObject"));
                    if (DataEvent != null)
                    {
                        PostPrivacyButton.SetBackgroundResource(0);
                        PostPrivacyButton.Enabled = false;
                        PostPrivacyButton.Text = GetText(Resource.String.Lbl_PostingAs) + " " + ObeeNetworkTools.GetNameFinal(DataUser);

                        GlideImageLoader.LoadImage(this, DataEvent.Cover, PostSectionImage, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                        TxtUserName.Text = DataEvent.Name;
                    }
                    else
                    {
                        LoadDataUser();
                    }
                }
                else
                {
                    LoadDataUser();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void PostPrivacyButton_Click(object sender, EventArgs e)
        {
            try
            {
                TypeDialog = "PostPrivacy";

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                arrayAdapter.Add(GetString(Resource.String.Lbl_Everyone));// > 0

                if (AppSettings.ConnectivitySystem == 1)
                { 
                    arrayAdapter.Add(GetString(Resource.String.Lbl_People_i_Follow));// > 1
                    arrayAdapter.Add(GetText(Resource.String.Lbl_People_Follow_Me));// > 2 
                }
                else
                {
                    arrayAdapter.Add(GetString(Resource.String.Lbl_MyFriends)); // > 1 
                }
                arrayAdapter.Add(GetString(Resource.String.Lbl_No_body)); // > 3
                arrayAdapter.Add(GetText(Resource.String.Lbl_Anonymous)); // > 4

                dialogList.Title(GetText(Resource.String.Lbl_PostPrivacy));
                dialogList.Items(arrayAdapter);
                dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(this);
                dialogList.ItemsCallback(this).Build().Show();
                dialogList.AlwaysCallSingleChoiceCallback();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void AddAnswerButtonOnClick(object sender, EventArgs eventArgs)
        {
            try
            {
                if (AddPollAnswerAdapter.AnswersList.Count < 8)
                {
                    AddPollAnswerAdapter.AnswersList.Add(new PollAnswers { Answer = "", Id = AddPollAnswerAdapter.AnswersList.Count });
                    AddPollAnswerAdapter.NotifyItemInserted(AddPollAnswerAdapter.AnswersList.Count);
                    PollRecyclerView.ScrollToPosition(AddPollAnswerAdapter.AnswersList.Count);
                    ScrollView.ScrollTo(0, ScrollView.Bottom + 500);
                    ScrollView.SmoothScrollTo(0, ScrollView.Bottom + 200);
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl2_PollsLimitError), ToastLength.Long).Show();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region MaterialDialog

        public void OnSelection(MaterialDialog p0, View p1, int itemid, ICharSequence itemString)
        {
            try
            {
                if (TypeDialog == "PostPrivacy")
                {
                    PostPrivacyButton.Text = itemString.ToString();

                    if (itemString.ToString() == GetString(Resource.String.Lbl_Everyone))
                        PostPrivacy = "0";
                    else if (itemString.ToString() == GetString(Resource.String.Lbl_People_i_Follow) || itemString.ToString() == GetString(Resource.String.Lbl_MyFriends))
                        PostPrivacy = "1";
                    else if (itemString.ToString() == GetString(Resource.String.Lbl_People_Follow_Me))
                        PostPrivacy = "2";
                    else if (itemString.ToString() == GetString(Resource.String.Lbl_No_body))
                        PostPrivacy = "3";
                    else if (itemString.ToString() == GetString(Resource.String.Lbl_Anonymous))
                        PostPrivacy = "4";
                    else
                        PostPrivacy = "0";
                }
                else if (TypeDialog == "PostVideos")
                {
                    if (itemString.ToString() == GetText(Resource.String.Lbl_VideoGallery))
                    {
                        PermissionsType = "VideoGallery";
                        // Check if we're running on Android 5.0 or higher
                        if ((int)Build.VERSION.SdkInt < 23)
                        {
                            //requestCode >> 501 => video Gallery
                            new IntentController(this).OpenIntentVideoGallery();
                        }
                        else
                        {
                            if (CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted
                                && CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted
                                && CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted)
                            {
                                //requestCode >> 501 => video Gallery
                                new IntentController(this).OpenIntentVideoGallery();
                            }
                            else
                            {
                                new PermissionsController(this).RequestPermission(108);
                            }
                        }
                    }
                    else if (itemString.ToString() == GetText(Resource.String.Lbl_RecordVideoFromCamera))
                    {
                        PermissionsType = "VideoCamera";

                        // Check if we're running on Android 5.0 or higher
                        if ((int)Build.VERSION.SdkInt < 23)
                        {
                            //requestCode >> 513 => video Camera
                            new IntentController(this).OpenIntentVideoCamera();
                        }
                        else
                        {
                            if (CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted
                                && CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted
                                && CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted)
                            {
                                //requestCode >> 513 => video Camera
                                new IntentController(this).OpenIntentVideoCamera();
                            }
                            else
                            {
                                new PermissionsController(this).RequestPermission(108);
                            }
                        }
                    }
                }
                else if (TypeDialog == "Feelings")
                {
                    if (itemid == 0) // Feelings
                    {
                        StartActivityForResult(new Intent(this, typeof(FeelingsActivity)), 5);
                    }
                    else if (itemid == 1) //Listening
                    {
                        TypeDialog = "Listening";

                        var dialog = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                        dialog.Title(Resource.String.Lbl_Listening);
                        dialog.Input(Resource.String.Lbl_Comment_Hint_Listening, 0, false, this);
                        dialog.InputType(InputTypes.TextFlagImeMultiLine);
                        dialog.PositiveText(GetText(Resource.String.Lbl_Submit)).OnPositive(this);
                        dialog.NegativeText(GetText(Resource.String.Lbl_Cancel)).OnNegative(this);
                        dialog.AlwaysCallSingleChoiceCallback();
                        dialog.Build().Show();
                    }
                    else if (itemid == 2) //Playing
                    {
                        TypeDialog = "Playing";

                        var dialog = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                        dialog.Title(Resource.String.Lbl_Playing);
                        dialog.Input(Resource.String.Lbl_Comment_Hint_Playing, 0, false, this);
                        dialog.InputType(InputTypes.TextFlagImeMultiLine);
                        dialog.PositiveText(GetText(Resource.String.Lbl_Submit)).OnPositive(this);
                        dialog.NegativeText(GetText(Resource.String.Lbl_Cancel)).OnNegative(this);
                        dialog.AlwaysCallSingleChoiceCallback();
                        dialog.Build().Show();
                    }
                    else if (itemid == 3) //Watching
                    {
                        TypeDialog = "Watching";

                        var dialog = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                        dialog.Title(Resource.String.Lbl_Watching);
                        dialog.Input(Resource.String.Lbl_Comment_Hint_Watching, 0, false, this);
                        dialog.InputType(InputTypes.TextFlagImeMultiLine);
                        dialog.PositiveText(GetText(Resource.String.Lbl_Submit)).OnPositive(this);
                        dialog.NegativeText(GetText(Resource.String.Lbl_Cancel)).OnNegative(this);
                        dialog.AlwaysCallSingleChoiceCallback();
                        dialog.Build().Show();
                    }
                    else if (itemid == 4) //Traveling
                    {
                        TypeDialog = "Traveling";

                        var dialog = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                        dialog.Title(Resource.String.Lbl_Traveling);
                        dialog.Input(Resource.String.Lbl_Comment_Hint_Traveling, 0, false, this);
                        dialog.InputType(InputTypes.TextFlagImeMultiLine);
                        dialog.PositiveText(GetText(Resource.String.Lbl_Submit)).OnPositive(this);
                        dialog.NegativeText(GetText(Resource.String.Lbl_Cancel)).OnNegative(this);
                        dialog.AlwaysCallSingleChoiceCallback();
                        dialog.Build().Show();
                    }
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
                if (TypeDialog == "PostPrivacy")
                {
                    if (p1 == DialogAction.Positive) p0.Dismiss();
                }
                else if (TypeDialog == "PostBack")
                {
                    if (p1 == DialogAction.Positive)
                    {
                        p0.Dismiss();

                         
                        Finish();
                    }
                    else if (p1 == DialogAction.Negative)
                    {
                        p0.Dismiss();
                    }
                }
                else
                {
                    if (p1 == DialogAction.Positive)
                    {
                    }
                    else if (p1 == DialogAction.Negative)
                    {
                        p0.Dismiss();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void OnInput(MaterialDialog p0, ICharSequence p1)
        {
            try
            {
                if (TypeDialog == "AddPicturesToAlbumName")
                {
                    if (p1.Length() > 0)
                    {
                        var strName = p1.ToString();
                        AlbumName = strName;
                        NameAlbumButton.Text = Methods.FunString.SubStringCutOf(strName, 30);
                    }
                }
                else if (TypeDialog == "Listening")
                {
                    if (p1.Length() > 0)
                    {
                        var strName = p1.ToString();
                        ListeningText = strName;
                        PostFeelingText = strName;
                        PostFeelingType = "listening"; //Type Of listening
                    }
                }
                else if (TypeDialog == "Playing")
                {
                    if (p1.Length() > 0)
                    {
                        var strName = p1.ToString();
                        PlayingText = strName;
                        PostFeelingText = strName;
                        PostFeelingType = "playing"; //Type Of playing
                    }
                }
                else if (TypeDialog == "Watching")
                {
                    if (p1.Length() > 0)
                    {
                        var strName = p1.ToString();
                        WatchingText = strName;
                        PostFeelingText = strName;
                        PostFeelingType = "watching"; //Type Of watching
                    }
                }
                else if (TypeDialog == "Traveling")
                {
                    if (p1.Length() > 0)
                    {
                        var strName = p1.ToString();
                        TravelingText = strName;
                        PostFeelingText = strName;
                        PostFeelingType = "traveling"; //Type Of traveling
                    }
                } 
                 
                TextSanitizer.Load(LoadPostStrings());

                var inputManager = (InputMethodManager)GetSystemService(InputMethodService);
                inputManager.HideSoftInputFromWindow(TopToolBar.WindowToken, 0);

                TopToolBar.ClearFocus();

                SlidingUpPanel.SetPanelState(SlidingUpPanelLayout.PanelState.Collapsed);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        // Event Back
        public override void OnBackPressed()
        {
            try
            {
                if (!string.IsNullOrEmpty(TxtContentPost.Text) || !string.IsNullOrEmpty(MentionText) || AttachmentsAdapter.AttachmentList.Count > 0)
                {
                    TypeDialog = "PostBack";

                    var dialog = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                    dialog.Title(GetText(Resource.String.Lbl_Title_Back));
                    dialog.Content(GetText(Resource.String.Lbl_Content_Back));
                    dialog.PositiveText(GetText(Resource.String.Lbl_PositiveText_Back)).OnPositive(this);
                    dialog.NegativeText(GetText(Resource.String.Lbl_NegativeText_Back)).OnNegative(this);
                    dialog.AlwaysCallSingleChoiceCallback();
                    dialog.ItemsCallback(this).Build().Show();
                }
                else
                {
                    
                    base.OnBackPressed();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //private void OpenDialogGallery()
        //{
        //    try
        //    {
        //        // Check if we're running on Android 5.0 or higher
        //        if ((int)Build.VERSION.SdkInt < 23)
        //        {
        //            Methods.Path.Chack_MyFolder();

        //            //Open Image 
        //            var myUri = Uri.FromFile(new File(Methods.Path.FolderDiskImage, Methods.GetTimestamp(DateTime.Now) + ".jpeg"));
        //            CropImage.Builder()
        //                .SetInitialCropWindowPaddingRatio(0)
        //                .SetAutoZoomEnabled(true)
        //                .SetMaxZoom(4)
        //                .SetGuidelines(CropImageView.Guidelines.On)
        //                .SetCropMenuCropButtonTitle(GetText(Resource.String.Lbl_Crop))
        //                .SetOutputUri(myUri).Start(this);
        //        }
        //        else
        //        {
        //            if (!CropImage.IsExplicitCameraPermissionRequired(this) && CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted &&
        //                CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted && CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted)
        //            {
        //                Methods.Path.Chack_MyFolder();

        //                //Open Image 
        //                var myUri = Uri.FromFile(new File(Methods.Path.FolderDiskImage, Methods.GetTimestamp(DateTime.Now) + ".jpeg"));
        //                CropImage.Builder()
        //                    .SetInitialCropWindowPaddingRatio(0)
        //                    .SetAutoZoomEnabled(true)
        //                    .SetMaxZoom(4)
        //                    .SetGuidelines(CropImageView.Guidelines.On)
        //                    .SetCropMenuCropButtonTitle(GetText(Resource.String.Lbl_Crop))
        //                    .SetOutputUri(myUri).Start(this);
        //            }
        //            else
        //            {
        //                new PermissionsController(this).RequestPermission(108);
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e);
        //    }
        //}
       
        private string LoadPostStrings()
        {
            try
            {
                var newActivityText = string.Empty;
                var newFeelingText = string.Empty;
                var newMentionText = string.Empty;
                var newPlaceText = string.Empty;

                if (!string.IsNullOrEmpty(ActivityText))
                    newActivityText = PostActivityType + " " + ActivityText;

                if (!string.IsNullOrEmpty(ListeningText))
                    newFeelingText = GetText(Resource.String.Lbl_ListeningTo) + " " + ListeningText;

                if (!string.IsNullOrEmpty(PlayingText))
                    newFeelingText = GetText(Resource.String.Lbl_Playing) + " " + PlayingText;

                if (!string.IsNullOrEmpty(WatchingText))
                    newFeelingText = GetText(Resource.String.Lbl_Watching) + " " + WatchingText;

                if (!string.IsNullOrEmpty(TravelingText))
                    newFeelingText = GetText(Resource.String.Lbl_Traveling) + " " + TravelingText;

                if (!string.IsNullOrEmpty(FeelingText))
                    newFeelingText = GetText(Resource.String.Lbl_Feeling) + " " + FeelingText;

                if (!string.IsNullOrEmpty(MentionText))
                    newMentionText += " " + GetText(Resource.String.Lbl_With) + " " + MentionText.Remove(MentionText.Length - 1, 1);

                if (!string.IsNullOrEmpty(PlaceText))
                    newPlaceText += " " + GetText(Resource.String.Lbl_At) + " " + PlaceText;

                var mainString = newActivityText + newFeelingText + newMentionText + newPlaceText;
                return mainString;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return "";
            }
        }

        private void OpenDialogVideo()
        {
            try
            {
                TypeDialog = "PostVideos";

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                arrayAdapter.Add(GetText(Resource.String.Lbl_VideoGallery));
                arrayAdapter.Add(GetText(Resource.String.Lbl_RecordVideoFromCamera));

                dialogList.Title(GetText(Resource.String.Lbl_SelectVideoFrom));
                dialogList.Items(arrayAdapter);
                dialogList.PositiveText(GetText(Resource.String.Lbl_Close)).OnPositive(this);
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void RemoveLocation()
        {
            try
            {
                RunOnUiThread(() =>
                {
                    MentionTextView.Text = "";

                    PlaceText = string.Empty;
                    TextSanitizer.Load(LoadPostStrings());
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
         
        #region Path
          
        public void PickiTonCompleteListener(string path)
        {
            //Dismiss dialog and return the path
            try
            {
                //  Check if it was a Drive/local/unknown provider file and display a Toast
                //if (wasDriveFile)
                //{
                //    // "Drive file was selected"
                //}
                //else if (wasUnknownProvider)
                //{
                //    // "File was selected from unknown provider"
                //}
                //else
                //{
                //    // "Local file was selected"
                //}

                //  Chick if it was successful
                var check = ObeeNetworkTools.CheckMimeTypesWithServer(path);
                if (!check)
                {
                    //this file not supported on the server , please select another file 
                    Toast.MakeText(this, GetString(Resource.String.Lbl_ErrorFileNotSupported), ToastLength.Short).Show();
                    return;
                }

                var type = Methods.AttachmentFiles.Check_FileExtension(path);
                if (type == "File")
                {
                    NameAlbumButton.Visibility = ViewStates.Gone;

                    //remove file the type
                    AttachmentsAdapter.RemoveAll();

                    var attach = new Attachments
                    {
                        Id = AttachmentsAdapter.AttachmentList.Count + 1,
                        TypeAttachment = "postFile",
                        FileSimple = "Image_File",
                        FileUrl = path
                    };

                    AttachmentsAdapter.Add(attach);
                }
                else if (type == "Video")
                {
                    NameAlbumButton.Visibility = ViewStates.Gone;

                    AttachmentsAdapter.RemoveAll();

                    var fileName = path.Split('/').Last();
                    var fileNameWithoutExtenion = fileName.Split('.').First();

                    var pathImage = Methods.Path.FolderDcimImage + "/" + fileNameWithoutExtenion + ".png";

                    var vidoPlaceHolderImage = Methods.MultiMedia.GetMediaFrom_Gallery(Methods.Path.FolderDcimImage, fileNameWithoutExtenion + ".png");
                    if (vidoPlaceHolderImage == "File Dont Exists")
                    {
                        var bitmapImage = Methods.MultiMedia.Retrieve_VideoFrame_AsBitmap(this, UriData.ToString());
                        Methods.MultiMedia.Export_Bitmap_As_Image(bitmapImage, fileNameWithoutExtenion, Methods.Path.FolderDcimImage);
                    }

                    var attach = new Attachments
                    {
                        Id = AttachmentsAdapter.AttachmentList.Count + 1,
                        TypeAttachment = "postVideo",
                        FileSimple = pathImage,
                        Thumb = new Attachments.VideoThumb()
                        {
                            FileUrl = pathImage
                        },

                        FileUrl = path
                    };

                    AttachmentsAdapter.Add(attach);
                }
                else if (type == "Audio")
                {
                    NameAlbumButton.Visibility = ViewStates.Gone;
                    //var fileName = filepath.Split('/').Last();
                    //var fileNameWithoutExtension = fileName.Split('.').First();

                    //remove file the type
                    AttachmentsAdapter.RemoveAll();

                    var attach = new Attachments
                    {
                        Id = AttachmentsAdapter.AttachmentList.Count + 1,
                        TypeAttachment = "postMusic",
                        FileSimple = "Audio_File",
                        FileUrl = path
                    };

                    AttachmentsAdapter.Add(attach);
                }
                else if (type == "Image")
                {
                    //remove file the type
                    var videoAttach = AttachmentsAdapter.AttachmentList.Where(a => !a.TypeAttachment.Contains("postPhotos")).ToList();
                    if (videoAttach.Count > 0)
                        foreach (var video in videoAttach)
                            AttachmentsAdapter.Remove(video);
                   
                    var attach = new Attachments
                    {
                        Id = AttachmentsAdapter.AttachmentList.Count + 1,
                        TypeAttachment = "postPhotos",
                        FileSimple = path,
                        FileUrl = path
                    };

                    AttachmentsAdapter.Add(attach);

                    if (AttachmentsAdapter.AttachmentList.Count > 1)
                    {
                        NameAlbumButton.Visibility = ViewStates.Visible;

                        foreach (var item in AttachmentsAdapter.AttachmentList)
                            item.TypeAttachment = "postPhotos[]";
                    }
                    else
                    {
                        NameAlbumButton.Visibility = ViewStates.Gone;

                        foreach (var item in AttachmentsAdapter.AttachmentList)
                            item.TypeAttachment = "postPhotos";
                    }
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_Failed_to_load), ToastLength.Short).Show();
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