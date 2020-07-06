using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using AutoMapper;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Com.Theartofdev.Edmodo.Cropper;
using Java.IO;
using Newtonsoft.Json;
using ObeeNetwork.Activities.Comment.Adapters;
using ObeeNetwork.Activities.NativePost.Extra;
using ObeeNetwork.Helpers.Controller;
using ObeeNetwork.Helpers.Model;
using ObeeNetwork.Helpers.Utils;
using ObeeNetworkClient.Classes.Comments;
using ObeeNetworkClient.Requests;
using Console = System.Console;
using Uri = Android.Net.Uri;

namespace ObeeNetwork.Activities.Comment
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class ReplyCommentActivity : AppCompatActivity
    {
        #region Variables Basic

        private static ReplyCommentActivity Instance;
        public ReplyCommentAdapter MAdapter;
        private RecyclerView MainRecyclerView;
        private ViewStub CommentLayout;
        private View CommentLayoutView;
        private TextView ReplyCountTextView;
        public EditText TxtComment;
        private ImageView ImgBack, ImgSent, ImgGallery;
        private CommentObjectExtra CommentObject;
        private string CommentId , PathImage;
        

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                Window.SetSoftInputMode(SoftInput.AdjustResize);

                base.OnCreate(savedInstanceState);

                Methods.App.FullScreenApp(this);
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);

                // Create your application here
                SetContentView(Resource.Layout.Native_Reply_Comment_Layout);

                Instance = this;

                CommentId = Intent.GetStringExtra("CommentId") ?? string.Empty;

                var data = Intent.GetStringExtra("CommentObject");
                if (!string.IsNullOrEmpty(data))
                    CommentObject = JsonConvert.DeserializeObject<CommentObjectExtra>(data);

                //Get Value And Set Toolbar
                InitComponent();
                SetRecyclerViewAdapters();

                ReplyCountTextView.Text = CommentObject?.Replies + " " + GetString(Resource.String.Lbl_Replies);

                StartApiService("0");
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
                MainRecyclerView = FindViewById<RecyclerView>(Resource.Id.recycler_view);
                CommentLayout = FindViewById<ViewStub>(Resource.Id.comment_layout);
                ReplyCountTextView = FindViewById<TextView>(Resource.Id.replyCountTextview);

                TxtComment = FindViewById<EditText>(Resource.Id.commenttext);
                ImgBack = FindViewById<ImageView>(Resource.Id.back);

                ImgSent = FindViewById<ImageView>(Resource.Id.send);
                ImgGallery = FindViewById<ImageView>(Resource.Id.image);

                Methods.SetColorEditText(TxtComment, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
 
                if (AppSettings.FlowDirectionRightToLeft)
                    ImgBack.SetImageResource(Resource.Drawable.ic_action_ic_back_rtl);

                TxtComment.Text = "";
                PathImage = "";

                ImgGallery.SetImageDrawable(AppSettings.SetTabDarkTheme ? GetDrawable(Resource.Drawable.ic_action_addpost_Ligth) : GetDrawable(Resource.Drawable.ic_action_AddPost));

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
                MAdapter = new ReplyCommentAdapter(this, MainRecyclerView, CommentId);

                if (CommentObject != null )
                {
                    CommentLayout.LayoutResource = string.IsNullOrEmpty(CommentObject.CFile) ? Resource.Layout.Style_Comment : Resource.Layout.Style_Comment_Image ; 
                } 
                CommentLayoutView = CommentLayout.Inflate();

                var holder = new CommentAdapterViewHolder(CommentLayoutView, "Light")
                {
                    ReplyTextView = { Visibility = ViewStates.Gone }
                };

                //Load data same as comment adapter
                CommentActivity.GetInstance()?.CommentsAdapter?.LoadCommentData(CommentObject, holder);
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
                    ImgBack.Click += ImgBackOnClick;
                    ImgSent.Click += ImgSentOnClick;
                    ImgGallery.Click += ImgGalleryOnClick;
                }
                else
                {
                    ImgBack.Click -= ImgBackOnClick;
                    ImgSent.Click -= ImgSentOnClick;
                    ImgGallery.Click -= ImgGalleryOnClick;
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
                Instance = null;
                MAdapter = null;
                MainRecyclerView = null;
                CommentLayout = null;
                CommentLayoutView = null;
                ReplyCountTextView = null;
                ImgBack = null;
                ImgSent = null;
                ImgGallery = null;
                CommentObject = null;
                CommentId = null;
                PathImage = null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static ReplyCommentActivity GetInstance()
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

        private void ImgBackOnClick(object sender, EventArgs e)
        {
            Finish();
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
                if (string.IsNullOrEmpty(TxtComment.Text) && string.IsNullOrEmpty(PathImage))
                    return;

                if (Methods.CheckConnectivity())
                {
                    CommentObject.Replies ??= "0";
                     
                    //Comment Code 
                    var dataUser = ListUtils.MyProfileList.FirstOrDefault();

                    var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                    CommentObjectExtra comment = new CommentObjectExtra
                    {
                        Id = unixTimestamp.ToString(),
                        PostId = CommentObject.PostId,
                        UserId = UserDetails.UserId,
                        Text = TxtComment.Text,
                        Time = unixTimestamp.ToString(),
                        CFile = PathImage,
                        Record = "",
                        Publisher = dataUser,
                        Url = dataUser?.Url,
                        Fullurl = CommentObject?.Fullurl,
                        Orginaltext = TxtComment.Text,
                        Owner = true,
                        CommentLikes = "0",
                        CommentWonders = "0",
                        IsCommentLiked = false,
                        Replies = "0"
                    };

                    MAdapter.ReplyCommentList.Add(comment);

                    var index = MAdapter.ReplyCommentList.IndexOf(comment);
                    if (index > -1)
                    {
                        MAdapter.NotifyItemInserted(index);
                    }

                    MainRecyclerView.Visibility = ViewStates.Visible;

                    var dd = MAdapter.ReplyCommentList.FirstOrDefault();
                    if (dd?.Text == MAdapter.EmptyState)
                    {
                        MAdapter.ReplyCommentList.Remove(dd);
                        MAdapter.NotifyItemRemoved(MAdapter.ReplyCommentList.IndexOf(dd));
                    }

                    //CommentLayout.Visibility = ViewStates.Gone;
                    bool success = int.TryParse(CommentObject.Replies, out var number);
                    if (success)
                    {
                        Console.WriteLine("Converted '{0}' to {1}.", CommentObject.Replies, number);
                        var x = number + 1;
                        ReplyCountTextView.Text = x + " " + GetString(Resource.String.Lbl_Replies);
                    }
                    else
                    {
                        Console.WriteLine("Attempted conversion of '{0}' failed.", CommentObject.Replies ?? "<null>");
                        ReplyCountTextView.Text = 1 + " " + GetString(Resource.String.Lbl_Replies);
                    }

                    ImgGallery.SetImageDrawable(AppSettings.SetTabDarkTheme ? GetDrawable(Resource.Drawable.ic_action_addpost_Ligth) : GetDrawable(Resource.Drawable.ic_action_AddPost));

                    var text = TxtComment.Text;

                    //Hide keyboard
                    TxtComment.Text = "";

                    (int apiStatus, var respond) =await RequestsAsync.Comment.CreatePostComments(CommentId, text, PathImage , "","create_reply");
                    if (apiStatus == 200)
                    {
                        if (respond is CreateComments result)
                        {
                            var date = MAdapter.ReplyCommentList.FirstOrDefault(a => a.Id == comment.Id) ?? MAdapter.ReplyCommentList.FirstOrDefault(x => x.Id == result.Data.Id);
                            if (date != null)
                            {
                                var db = Mapper.Map<CommentObjectExtra>(result.Data);

                                date = db;
                                date.Id = result.Data.Id;

                                index = MAdapter.ReplyCommentList.IndexOf(MAdapter.ReplyCommentList.FirstOrDefault(a => a.Id == unixTimestamp.ToString()));
                                if (index > -1)
                                {
                                    MAdapter.ReplyCommentList[index] = db;

                                    //MAdapter.NotifyItemChanged(index, !string.IsNullOrEmpty(PathImage) ? 1 : 0);
                                    //MainRecyclerView.ScrollToPosition(index);
                                }

                                var postFeedAdapter = WRecyclerView.GetInstance()?.NativeFeedAdapter;
                                var dataGlobal = postFeedAdapter?.ListDiffer?.Where(a => a.PostData?.Id == CommentObject?.PostId).ToList();
                                if (dataGlobal?.Count > 0)
                                {
                                    foreach (var dataClass in from dataClass in dataGlobal let indexCom = postFeedAdapter.ListDiffer.IndexOf(dataClass) where indexCom > -1 select dataClass)
                                    {
                                        if (dataClass.PostData.GetPostComments?.Count > 0)
                                        {
                                            var dataComment = dataClass.PostData.GetPostComments.FirstOrDefault(a => a.Id == date.Id);
                                            if (dataComment != null)
                                            {
                                                dataComment.Replies = MAdapter.ReplyCommentList.Count.ToString();
                                            }
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
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        #endregion
         
        private void StartApiService(string offset)
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => MAdapter.FetchPostApiComments(offset, CommentId) });
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

        #region PickiT >> Gert path file

        public void PickiTonCompleteListener(string path)
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
    }
}