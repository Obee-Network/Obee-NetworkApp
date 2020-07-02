using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V4.View.Animation;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using AT.Markushi.UI;
using AutoMapper;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Com.Theartofdev.Edmodo.Cropper;
using Java.IO;
using Newtonsoft.Json;
using ObeeNetwork.Activities.Comment.Adapters;
using ObeeNetwork.Activities.Comment.Fragment;
using ObeeNetwork.Activities.NativePost.Extra;
using ObeeNetwork.Helpers.Controller;
using ObeeNetwork.Helpers.Model;
using ObeeNetwork.Helpers.Utils;
using ObeeNetworkClient.Classes.Comments;
using ObeeNetworkClient.Classes.Posts;
using ObeeNetworkClient.Requests;
using Console = System.Console;
using Uri = Android.Net.Uri;
using SupportFragment = Android.Support.V4.App.Fragment;

namespace ObeeNetwork.Activities.Comment
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/RoundedTopActivityTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class CommentActivity : AppCompatActivity
    {
        #region Variables Basic

        private static CommentActivity Instance;
        public CommentAdapter CommentsAdapter;
        private RecyclerView MainRecyclerView;
        private EditText TxtComment;
        private TextView LikeCountBox;
        private ImageView ImgSent, ImgGallery, ImgBack;
        public CircleButton BtnVoice;
        private PostDataObject PostObject;
        public string PostId;
        private string PathImage, PathVoice, TextRecorder = "";
        private FrameLayout TopFragment;
        private RecordSoundFragment RecordSoundFragment;
        private bool IsRecording;
        private Methods.AudioRecorderAndPlayer RecorderService;
        private LinearLayout CommentLayout;
        public SwipeRefreshLayout SwipeRefreshLayout;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                Window.SetSoftInputMode(SoftInput.AdjustResize);

                base.OnCreate(savedInstanceState);
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);

                Methods.App.FullScreenApp(this);

                // Create your application here
                SetContentView(Resource.Layout.Native_Comment_Layout);

                Instance = this;

                PostId = Intent.GetStringExtra("PostId") ?? string.Empty;
                PostObject = JsonConvert.DeserializeObject<PostDataObject>(Intent.GetStringExtra("PostObject"));
                 
                //Get Value And Set Toolbar
                InitComponent();

                GetDataPost();
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
                ResetMediaPlayer();
                DestroyBasic(); 
                base.OnDestroy(); 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                LikeCountBox = FindViewById<TextView>(Resource.Id.like_box);
                MainRecyclerView = FindViewById<RecyclerView>(Resource.Id.recycler_view);
                TxtComment = FindViewById<EditText>(Resource.Id.commenttext);
                ImgSent = FindViewById<ImageView>(Resource.Id.send);
                ImgGallery = FindViewById<ImageView>(Resource.Id.image);
                ImgBack = FindViewById<ImageView>(Resource.Id.back);
                CommentLayout = FindViewById<LinearLayout>(Resource.Id.commentLayout);

                SwipeRefreshLayout = (SwipeRefreshLayout)FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = true;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
                 
                BtnVoice = FindViewById<CircleButton>(Resource.Id.voiceButton);
                BtnVoice.LongClickable = true;
                BtnVoice.Tag = "Free";
                BtnVoice.SetImageResource(Resource.Drawable.microphone);

                TopFragment = FindViewById<FrameLayout>(Resource.Id.TopFragmentHolder);
                 
                TxtComment.Text = "";
                PathImage = "";
                TextRecorder = "";

                RecordSoundFragment = new RecordSoundFragment();
                SupportFragmentManager.BeginTransaction().Add(TopFragment.Id, RecordSoundFragment, RecordSoundFragment.Tag);

                CommentsAdapter = new CommentAdapter(this, MainRecyclerView, "Light", PostId)
                {
                    CommentList = new ObservableCollection<CommentObjectExtra>()
                };
                 
                if (AppSettings.FlowDirectionRightToLeft)
                    ImgBack.SetImageResource(Resource.Drawable.ic_action_ic_back_rtl);

                ImgGallery.SetImageDrawable(AppSettings.SetTabDarkTheme ? GetDrawable(Resource.Drawable.ic_action_addpost_Ligth) : GetDrawable(Resource.Drawable.ic_action_AddPost));
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
                    ImgSent.Click += ImgSentOnClick;
                    ImgGallery.Click += ImgGalleryOnClick;
                    ImgBack.Click += ImgBackOnClick;
                    BtnVoice.LongClick += BtnVoiceOnLongClick;
                    BtnVoice.Touch += BtnVoiceOnTouch;
                    SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
                }
                else
                {
                    ImgSent.Click -= ImgSentOnClick;
                    ImgGallery.Click -= ImgGalleryOnClick;
                    ImgBack.Click -= ImgBackOnClick;
                    BtnVoice.LongClick -= BtnVoiceOnLongClick;
                    BtnVoice.Touch -= BtnVoiceOnTouch;
                    SwipeRefreshLayout.Refresh -= SwipeRefreshLayoutOnRefresh;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static CommentActivity GetInstance()
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
                Instance = null;
                CommentsAdapter = null;
                SwipeRefreshLayout = null; 
                MainRecyclerView = null;
                TxtComment = null;
                LikeCountBox = null;
                ImgSent = null; ImgGallery = null; ImgBack = null;
                BtnVoice = null;
                PostObject = null;
                PostId = null;
                PathImage = null; PathVoice = null; TextRecorder = null;
                TopFragment = null;
                RecordSoundFragment = null;
                RecorderService = null;
                CommentLayout = null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Events

        private void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
        {
            try
            {
                CommentsAdapter.CommentList.Clear();
                CommentsAdapter.NotifyDataSetChanged();

                StartApiService();

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
                            RecorderService.StopRecourding();
                            PathVoice = RecorderService.GetRecorded_Sound_Path();

                            BtnVoice.SetColorFilter(Color.ParseColor(AppSettings.MainColor));
                            BtnVoice.SetImageResource(Resource.Drawable.microphone);

                            if (TextRecorder == "Recording")
                            {
                                if (!string.IsNullOrEmpty(PathVoice))
                                {
                                    Bundle bundle = new Bundle();
                                    bundle.PutString("FilePath", PathVoice);
                                    RecordSoundFragment.Arguments = bundle;
                                    ReplaceTopFragment(RecordSoundFragment);
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
                    if (CheckSelfPermission(Manifest.Permission.RecordAudio) == Permission.Granted)
                    {
                        StartRecording();
                    }
                    else
                    {
                        new PermissionsController(this).RequestPermission(102);
                    }
                }
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

                    RecorderService = new Methods.AudioRecorderAndPlayer(PostId);
                    //Start Audio record
                    await Task.Delay(600);
                    RecorderService.StartRecourding();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //Back
        private void ImgBackOnClick(object sender, EventArgs e)
        {
            try
            {
                Finish();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Open Gallery
        private void ImgGalleryOnClick(object sender, EventArgs e)
        {
            try
            {
                // Check if we're running on Android 5.0 or higher
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    if (AppSettings.ImageCropping)
                        OpenDialogGallery(); //requestCode >> 500 => Image Gallery
                    else
                        new IntentController(this).OpenIntentImageGallery(GetText(Resource.String.Lbl_SelectPictures)); //requestCode >> 500 => Image Gallery
                }
                else
                {
                    if (CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted && CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted
                                                                                                   && CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted)
                    {
                        if (AppSettings.ImageCropping)
                            OpenDialogGallery(); //requestCode >> 500 => Image Gallery
                        else
                            new IntentController(this).OpenIntentImageGallery(GetText(Resource.String.Lbl_SelectPictures)); //requestCode >> 500 => Image Gallery
                    }
                    else
                    {
                        new PermissionsController(this).RequestPermission(108);
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Api sent Comment
        private async void ImgSentOnClick(object sender, EventArgs e)
        {
            try
            {
                IsRecording = false;

                if (BtnVoice.Tag?.ToString() == "Audio")
                {
                    var interTortola = new FastOutSlowInInterpolator();
                    TopFragment.Animate().SetInterpolator(interTortola).TranslationY(1200).SetDuration(300);
                    SupportFragmentManager.BeginTransaction().Remove(RecordSoundFragment).Commit();

                    PathVoice = RecorderService.GetRecorded_Sound_Path();
                } 

                if (string.IsNullOrEmpty(TxtComment.Text) && string.IsNullOrEmpty(PathImage) && string.IsNullOrEmpty(PathVoice))
                    return;

                if (Methods.CheckConnectivity())
                {
                    var dataUser = ListUtils.MyProfileList.FirstOrDefault();
                    //Comment Code 

                    var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    string time2 = unixTimestamp.ToString(CultureInfo.InvariantCulture);

                    CommentObjectExtra comment = new CommentObjectExtra
                    {
                        Id = unixTimestamp.ToString(),
                        PostId = PostObject.Id,
                        UserId = UserDetails.UserId,
                        Text = TxtComment.Text,
                        Time = time2,
                        CFile = PathImage,
                        Record = PathVoice,
                        Publisher = dataUser,
                        Url = dataUser?.Url,
                        Fullurl = PostObject?.PostUrl,
                        Orginaltext = TxtComment.Text,
                        Owner = true,
                        CommentLikes = "0",
                        CommentWonders = "0",
                        IsCommentLiked = false,
                        Replies = "0"
                    };

                    CommentsAdapter.CommentList.Add(comment);

                    var index = CommentsAdapter.CommentList.IndexOf(comment);
                    if (index > -1)
                    {
                        CommentsAdapter.NotifyItemInserted(index);
                    }

                    MainRecyclerView.Visibility = ViewStates.Visible;

                    var dd = CommentsAdapter.CommentList.FirstOrDefault();
                    if (dd?.Text == CommentsAdapter.EmptyState)
                    {
                        CommentsAdapter.CommentList.Remove(dd);
                        CommentsAdapter.NotifyItemRemoved(CommentsAdapter.CommentList.IndexOf(dd));
                    }

                    ImgGallery.SetImageDrawable(AppSettings.SetTabDarkTheme ? GetDrawable(Resource.Drawable.ic_action_addpost_Ligth) : GetDrawable(Resource.Drawable.ic_action_AddPost));
                    var text = TxtComment.Text;

                    //Hide keyboard
                    TxtComment.Text = "";

                    (int apiStatus, var respond) = await RequestsAsync.Comment.CreatePostComments(PostObject.PostId, text, PathImage, PathVoice);
                    if (apiStatus == 200)
                    {
                        if (respond is CreateComments result)
                        {
                            var date = CommentsAdapter.CommentList.FirstOrDefault(a => a.Id == comment.Id) ?? CommentsAdapter.CommentList.FirstOrDefault(x => x.Id == result.Data.Id);
                            if (date != null)
                            {
                                var db = Mapper.Map<CommentObjectExtra>(result.Data);

                                date = db;
                                date.Id = result.Data.Id;

                                index = CommentsAdapter.CommentList.IndexOf(CommentsAdapter.CommentList.FirstOrDefault(a => a.Id == unixTimestamp.ToString()));
                                if (index > -1)
                                {
                                    CommentsAdapter.CommentList[index] = db;

                                    //CommentsAdapter.NotifyItemChanged(index);
                                    //MainRecyclerView.ScrollToPosition(index);
                                }

                                var postFeedAdapter = WRecyclerView.GetInstance()?.NativeFeedAdapter;
                                var dataGlobal = postFeedAdapter?.ListDiffer?.Where(a => a.PostData?.Id == PostObject?.PostId).ToList();
                                if (dataGlobal?.Count > 0)
                                {
                                    foreach (var dataClass in from dataClass in dataGlobal let indexCom = postFeedAdapter.ListDiffer.IndexOf(dataClass) where indexCom > -1 select dataClass)
                                    {
                                        if (dataClass.PostData.GetPostComments?.Count > 0)
                                        {
                                            var dataComment = dataClass.PostData.GetPostComments.FirstOrDefault(a => a.Id == date.Id);
                                            if (dataComment == null)
                                            {
                                                dataClass.PostData.GetPostComments.Add(date);
                                            }
                                        }
                                        else
                                        {
                                            dataClass.PostData.GetPostComments = new List<GetCommentObject>() { date };
                                        }

                                        postFeedAdapter.NotifyItemChanged(postFeedAdapter.ListDiffer.IndexOf(dataClass));
                                    }
                                } 
                            }
                        }
                    }
                    else Methods.DisplayReportResult(this, respond);

                    //Hide keyboard
                    TxtComment.Text = "";
                    PathImage = "";
                    PathVoice = "";

                    BtnVoice.Tag = "Free";
                    BtnVoice.SetImageResource(Resource.Drawable.microphone);
                    BtnVoice.ClearColorFilter();
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        #region Permissions && Result

        //Result
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);

                //If its from Camera or Gallery  
                if (requestCode == 500)
                {
                    Uri uri = data.Data;
                    var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, uri);
                    PickiTonCompleteListener(filepath);
                }
                else if (requestCode == CropImage.CropImageActivityRequestCode)
                {
                    var result = CropImage.GetActivityResult(data);

                    if (resultCode == Result.Ok)
                    {
                        if (result.IsSuccessful)
                        {
                            var resultUri = result.Uri;

                            if (!string.IsNullOrEmpty(resultUri.Path))
                            {
                                PathImage = resultUri.Path;

                                File file2 = new File(resultUri.Path);
                                var photoUri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file2);
                                Glide.With(this).Load(photoUri).Apply(new RequestOptions()).Into(ImgGallery);
                                 
                                //GlideImageLoader.LoadImage(this, PathImage, ImgGallery, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                            }
                            else
                            {
                                Toast.MakeText(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long).Show();
                            }
                        }
                        else
                        {
                            Toast.MakeText(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long)
                                .Show();
                        }
                    }
                }
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
                        if (AppSettings.ImageCropping)
                            OpenDialogGallery(); //requestCode >> 500 => Image Gallery
                        else
                            new IntentController(this).OpenIntentImageGallery(GetText(Resource.String.Lbl_SelectPictures)); //requestCode >> 500 => Image Gallery
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
                        StartRecording();
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

        private void GetDataPost()
        {
            try
            {
                if (PostObject != null)
                {
                    if (PostObject.Reaction == null)
                        PostObject.Reaction = new Reaction();

                    if (PostObject.Reaction != null)
                        LikeCountBox.Text = PostObject.Reaction.Count + " " + GetString(Resource.String.Btn_Likes);
                    else
                        LikeCountBox.Text = "0 " + GetString(Resource.String.Btn_Likes);
                     
                    if (PostObject.CommentsStatus == "0")
                    {
                        CommentsAdapter.CommentList.Clear();
                        
                        CommentsAdapter.CommentList.Add(new CommentObjectExtra()
                        {
                            Id = CommentsAdapter.EmptyState,
                            Text = CommentsAdapter.EmptyState,
                            Orginaltext = GetText(Resource.String.Lbl_CommentsAreDisabledBy) + " " + ObeeNetworkTools.GetNameFinal(PostObject.Publisher),
                        });

                        CommentsAdapter.NotifyDataSetChanged();

                        CommentLayout.Visibility = ViewStates.Gone;
                    }
                    else
                    {
                        if (PostObject?.GetPostComments?.Count > 0)
                        {
                            var db = Mapper.Map<List<CommentObjectExtra>>(PostObject.GetPostComments);
                            CommentsAdapter.CommentList = new ObservableCollection<CommentObjectExtra>(db);
                        }
                        else
                        {
                            CommentsAdapter.CommentList = new ObservableCollection<CommentObjectExtra>();
                        }

                        Methods.SetColorEditText(TxtComment, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                        if (CommentsAdapter.CommentList.Count > 0)
                            CommentsAdapter?.NotifyDataSetChanged();


                        if (CommentsAdapter.CommentList.Count == 0)
                            StartApiService();
                    } 
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void StartApiService(string offset = "0")
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => CommentsAdapter.FetchPostApiComments(offset, PostId) });
        }

        private void OpenDialogGallery()
        {
            try
            {
                // Check if we're running on Android 5.0 or higher
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    Methods.Path.Chack_MyFolder();

                    //Open Image 
                    var myUri = Uri.FromFile(new File(Methods.Path.FolderDiskImage, Methods.GetTimestamp(DateTime.Now) + ".jpeg"));
                    CropImage.Builder()
                        .SetInitialCropWindowPaddingRatio(0)
                        .SetAutoZoomEnabled(true)
                        .SetMaxZoom(4)
                        .SetGuidelines(CropImageView.Guidelines.On)
                        .SetCropMenuCropButtonTitle(GetText(Resource.String.Lbl_Crop))
                        .SetOutputUri(myUri).Start(this);
                }
                else
                {
                    if (!CropImage.IsExplicitCameraPermissionRequired(this) && CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted &&
                        CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted && CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted)
                    {
                        Methods.Path.Chack_MyFolder();

                        //Open Image 
                         var myUri = Uri.FromFile(new File(Methods.Path.FolderDiskImage, Methods.GetTimestamp(DateTime.Now) + ".jpeg"));
                        CropImage.Builder()
                            .SetInitialCropWindowPaddingRatio(0)
                            .SetAutoZoomEnabled(true)
                            .SetMaxZoom(4)
                            .SetGuidelines(CropImageView.Guidelines.On)
                            .SetCropMenuCropButtonTitle(GetText(Resource.String.Lbl_Crop))
                            .SetOutputUri(myUri).Start(this);
                    }
                    else
                    {
                        new PermissionsController(this).RequestPermission(108);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void ReplaceTopFragment(SupportFragment fragmentView)
        {
            try
            {
                if (fragmentView.IsVisible)
                    return;

                var trans = SupportFragmentManager.BeginTransaction();
                trans.Replace(TopFragment.Id, fragmentView);

                if (SupportFragmentManager.BackStackEntryCount == 0)
                {
                    trans.AddToBackStack(null);
                }

                trans.Commit();

                TopFragment.TranslationY = 1200;
                TopFragment.Animate().SetInterpolator(new FastOutSlowInInterpolator()).TranslationYBy(-1200)
                    .SetDuration(500);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #region PickiT >> Gert path file

        private void PickiTonCompleteListener(string path)
        {
            //Dismiss dialog and return the path
            try
            {
                //  Check if it was a Drive/local/unknown provider file and display a Toast
                //if (wasDriveFile) => "Drive file was selected" 
                //else if (wasUnknownProvider)  => "File was selected from unknown provider" 
                //else => "Local file was selected"

                //  Chick if it was successful
                var check = ObeeNetworkTools.CheckMimeTypesWithServer(path);
                if (!check)
                {
                    //this file not supported on the server , please select another file 
                    Toast.MakeText(this, GetString(Resource.String.Lbl_ErrorFileNotSupported), ToastLength.Short).Show();
                    return;
                }

                var type = Methods.AttachmentFiles.Check_FileExtension(path);
                if (type == "Image")
                {
                    File file2 = new File(PathImage);
                    var photoUri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file2);
                    Glide.With(this).Load(photoUri).Apply(new RequestOptions()).Into(ImgGallery);

                    //GlideImageLoader.LoadImage(this, PathImage, ImgGallery, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
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

         
        private void ResetMediaPlayer()
        {
            try
            {
                var list = CommentsAdapter.CommentList.Where(a => !string.IsNullOrEmpty(a.Record) && a.MediaPlayer != null).ToList();
                if (list.Count > 0)
                {
                    foreach (var item in list)
                    {
                        if (item.MediaPlayer != null)
                        {
                            item.MediaPlayer.Stop();
                            item.MediaPlayer.Reset();
                        }
                        item.MediaPlayer = null;
                        item.MediaTimer = null;

                        item.MediaPlayer?.Release();
                        item.MediaPlayer = null;
                    }
                    CommentsAdapter.NotifyDataSetChanged();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


    }
}