using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.View;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using ObeeNetwork.Activities.MyPhoto.Adapters;
using ObeeNetwork.Activities.NativePost.Post;
using ObeeNetwork.Activities.PostData;
using ObeeNetwork.Helpers.Ads;
using ObeeNetwork.Helpers.Controller;
using ObeeNetwork.Helpers.Model;
using ObeeNetwork.Helpers.Utils;
using ObeeNetwork.Library.Anjo;
using ObeeNetworkClient.Classes.Posts;
using ObeeNetworkClient.Requests;
using Exception = System.Exception;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace ObeeNetwork.Activities.MyPhoto
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MyPhotoViewActivity : AppCompatActivity 
    {
        #region Variables Basic

        private ViewPager ViewPager;
        private ImageView ImgLike, ImgObeeNetwork, ImgWonder;
        private TextView TxtDescription, TxtCountLike, TxtCountObeeNetwork, TxtWonder, ShareText;
        private LinearLayout MainSectionButton, BtnCountLike, BtnCountObeeNetwork, BtnLike, BtnComment, BtnShare, BtnWonder, InfoImageLiner;
        private RelativeLayout MainLayout;
        private PostDataObject PostData;
        private ReactButton LikeButton;
        private PostClickListener ClickListener;
        private int IndexImage;
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
                SetContentView(Resource.Layout.MultiImagesPostViewerLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                Get_DataImage();

                ClickListener = new PostClickListener(this);

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

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.ImagePost, menu);

            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;

                case Resource.Id.download:
                    Download_OnClick();
                    break;

                case Resource.Id.ic_action_comment:
                    Copy_OnClick();
                    break;
              
                case Resource.Id.action_More:
                    More_OnClick();
                    break;

            }

            return base.OnOptionsItemSelected(item);
        }

        //Event Download Image  
        private void Download_OnClick()
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                }
                else
                {
                    var photos = PostData.PhotoMulti ?? PostData.PhotoAlbum;
                    IndexImage = ViewPager.CurrentItem;

                    Methods.MultiMedia.DownloadMediaTo_GalleryAsync(Methods.Path.FolderDcimImage, photos[IndexImage].Image);
                    Toast.MakeText(this, GetText(Resource.String.Lbl_ImageSaved), ToastLength.Short).Show();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //Event Copy link image 
        private void Copy_OnClick()
        {
            try
            {
                var clipboardManager = (ClipboardManager)GetSystemService(ClipboardService);

                var clipData = ClipData.NewPlainText("text", PostData.Url);
                clipboardManager.PrimaryClip = clipData;

                Toast.MakeText(this, GetText(Resource.String.Lbl_Copied), ToastLength.Short).Show();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //Event More 
        private void More_OnClick()
        {
            try
            {
                ClickListener.MorePostIconClick(new GlobalClickEventArgs { NewsFeedClass = PostData });
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
                ViewPager = (ViewPager)FindViewById(Resource.Id.view_pager);

                TxtDescription = FindViewById<TextView>(Resource.Id.tv_description);
                ImgLike = FindViewById<ImageView>(Resource.Id.image_like1);
                ImgObeeNetwork = FindViewById<ImageView>(Resource.Id.image_ObeeNetwork);
                TxtCountLike = FindViewById<TextView>(Resource.Id.LikeText1);
                TxtCountObeeNetwork = FindViewById<TextView>(Resource.Id.ObeeNetworkTextCount);

                MainLayout = FindViewById<RelativeLayout>(Resource.Id.main);
                InfoImageLiner = FindViewById<LinearLayout>(Resource.Id.infoImageLiner);
                InfoImageLiner.Visibility = ViewStates.Visible;

                BtnCountLike = FindViewById<LinearLayout>(Resource.Id.linerlikeCount);
                BtnCountObeeNetwork = FindViewById<LinearLayout>(Resource.Id.linerObeeNetworkCount);

                BtnLike = FindViewById<LinearLayout>(Resource.Id.linerlike);
                BtnComment = FindViewById<LinearLayout>(Resource.Id.linercomment);
                BtnShare = FindViewById<LinearLayout>(Resource.Id.linershare);

                MainSectionButton = FindViewById<LinearLayout>(Resource.Id.mainsection);
                BtnWonder = FindViewById<LinearLayout>(Resource.Id.linerSecondReaction);
                ImgWonder = FindViewById<ImageView>(Resource.Id.image_SecondReaction);
                TxtWonder = FindViewById<TextView>(Resource.Id.SecondReactionText);

                LikeButton = FindViewById<ReactButton>(Resource.Id.beactButton);

                ShareText = FindViewById<TextView>(Resource.Id.ShareText);

                if (!AppSettings.ShowTextShareButton && ShareText != null)
                    ShareText.Visibility = ViewStates.Gone;

                if (AppSettings.PostButton == PostButtonSystem.Reaction || AppSettings.PostButton == PostButtonSystem.Like)
                {
                    MainSectionButton.WeightSum = 3;
                    BtnWonder.Visibility = ViewStates.Gone;

                    TxtCountObeeNetwork.Visibility = ViewStates.Gone;
                    BtnCountObeeNetwork.Visibility = ViewStates.Gone;
                    ImgObeeNetwork.Visibility = ViewStates.Gone;

                }
                else if (AppSettings.PostButton == PostButtonSystem.Wonder)
                {
                    MainSectionButton.WeightSum = 4;
                    BtnWonder.Visibility = ViewStates.Visible;

                    TxtCountObeeNetwork.Visibility = ViewStates.Visible;
                    BtnCountObeeNetwork.Visibility = ViewStates.Visible;
                    ImgObeeNetwork.Visibility = ViewStates.Visible;

                    ImgObeeNetwork.SetImageResource(Resource.Drawable.ic_action_ObeeNetwork);
                    ImgWonder.SetImageResource(Resource.Drawable.ic_action_ObeeNetwork);
                    TxtWonder.Text = Application.Context.GetText(Resource.String.Btn_Wonder);
                }
                else if (AppSettings.PostButton == PostButtonSystem.DisLike)
                {
                    MainSectionButton.WeightSum = 4;
                    BtnWonder.Visibility = ViewStates.Visible;

                    TxtCountObeeNetwork.Visibility = ViewStates.Visible;
                    BtnCountObeeNetwork.Visibility = ViewStates.Visible;
                    ImgObeeNetwork.Visibility = ViewStates.Visible;

                    ImgObeeNetwork.SetImageResource(Resource.Drawable.ic_action_dislike);
                    ImgWonder.SetImageResource(Resource.Drawable.ic_action_dislike);
                    TxtWonder.Text = Application.Context.GetText(Resource.String.Btn_Dislike);
                }

                if (!AppSettings.ShowShareButton && BtnShare != null)
                    BtnShare.Visibility = ViewStates.Gone;
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
                    toolbar.Title = " ";
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
                    BtnComment.Click += BtnCommentOnClick;
                    BtnShare.Click += BtnShareOnClick;
                    BtnCountLike.Click += BtnCountLikeOnClick;
                    BtnCountObeeNetwork.Click += BtnCountObeeNetworkOnClick;
                    InfoImageLiner.Click += MainLayoutOnClick;
                    MainLayout.Click += MainLayoutOnClick;

                    if (AppSettings.PostButton == PostButtonSystem.Wonder || AppSettings.PostButton == PostButtonSystem.DisLike)
                        BtnWonder.Click += BtnWonderOnClick;

                    LikeButton.Click += (sender, args) => LikeButton.ClickLikeAndDisLike(new GlobalClickEventArgs()
                    {
                        NewsFeedClass = PostData,
                        View = TxtCountLike,
                    }, null, "MultiImagesPostViewerActivity");

                    if (AppSettings.PostButton == PostButtonSystem.Reaction)
                        LikeButton.LongClick += (sender, args) => LikeButton.LongClickDialog(new GlobalClickEventArgs()
                        {
                            NewsFeedClass = PostData,
                            View = TxtCountLike,
                        }, null, "MultiImagesPostViewerActivity");
                }
                else
                {
                    BtnComment.Click -= BtnCommentOnClick;
                    BtnShare.Click -= BtnShareOnClick;
                    BtnCountLike.Click -= BtnCountLikeOnClick;
                    BtnCountObeeNetwork.Click -= BtnCountObeeNetworkOnClick;
                    InfoImageLiner.Click -= MainLayoutOnClick;
                    MainLayout.Click -= MainLayoutOnClick;

                    if (AppSettings.PostButton == PostButtonSystem.Wonder || AppSettings.PostButton == PostButtonSystem.DisLike)
                        BtnWonder.Click -= BtnWonderOnClick;

                    LikeButton.Click += null;
                    if (AppSettings.PostButton == PostButtonSystem.Reaction)
                        LikeButton.LongClick -= null;
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
                TxtDescription = null;
                ImgLike = null;
                ImgObeeNetwork = null;
                TxtCountLike = null;
                TxtCountObeeNetwork = null; 
                MainLayout = null;
                InfoImageLiner = null;
                BtnCountLike = null;
                BtnCountObeeNetwork = null;
                BtnLike = null;
                BtnComment = null;
                BtnShare = null;
                MainSectionButton = null;
                BtnWonder = null;
                ImgWonder = null;
                TxtWonder = null;
                LikeButton= null;
                ShareText = null;
                ClickListener = null;
                RewardedVideoAd = null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        #endregion

        #region Events

        private void MainLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                InfoImageLiner.Visibility = InfoImageLiner.Visibility != ViewStates.Visible ? ViewStates.Visible : ViewStates.Invisible;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Event Add Wonder
        private void BtnWonderOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(Application.Context, Application.Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                    return;
                }

                if (PostData.IsWondered != null && (bool)PostData.IsWondered)
                {
                    var x = Convert.ToInt32(PostData.PostWonders);
                    if (x > 0)
                        x--;
                    else
                        x = 0;

                    ImgWonder.SetColorFilter(Color.White);
                    ImgObeeNetwork.SetColorFilter(Color.White);

                    PostData.IsWondered = false;
                    PostData.PostWonders = Convert.ToString(x, CultureInfo.InvariantCulture);

                    TxtCountObeeNetwork.Text = Methods.FunString.FormatPriceValue(x);

                    if (AppSettings.PostButton == PostButtonSystem.Wonder)
                        TxtWonder.Text = GetText(Resource.String.Btn_Wonder);
                    else if (AppSettings.PostButton == PostButtonSystem.DisLike)
                        TxtWonder.Text = GetText(Resource.String.Btn_Dislike);

                    BtnWonder.Tag = "false";
                }
                else
                {
                    var x = Convert.ToInt32(PostData.PostWonders);
                    x++;

                    PostData.PostWonders = Convert.ToString(x, CultureInfo.InvariantCulture);

                    PostData.IsWondered = true;

                    ImgWonder.SetColorFilter(Color.ParseColor("#f89823"));
                    ImgObeeNetwork.SetColorFilter(Color.ParseColor("#f89823"));

                    TxtCountObeeNetwork.Text = Methods.FunString.FormatPriceValue(x);

                    if (AppSettings.PostButton == PostButtonSystem.Wonder)
                        TxtWonder.Text = GetText(Resource.String.Lbl_wondered);
                    else if (AppSettings.PostButton == PostButtonSystem.DisLike)
                        TxtWonder.Text = GetText(Resource.String.Lbl_disliked);

                    BtnWonder.Tag = "true";
                }

                TxtCountObeeNetwork.Text = Methods.FunString.FormatPriceValue(int.Parse(PostData.PostWonders));

                if (AppSettings.PostButton == PostButtonSystem.Wonder)
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.Post_Actions(PostData.PostId, "wonder") });
                else if (AppSettings.PostButton == PostButtonSystem.DisLike)
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.Post_Actions(PostData.PostId, "dislike") });
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }


        //Event Show all users ObeeNetwork >> Open Post PostData_Activity
        private void BtnCountObeeNetworkOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(PostDataActivity));
                intent.PutExtra("PostId", PostData.PostId);
                intent.PutExtra("PostType", "post_wonders");
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Event Show all users liked >> Open Post PostData_Activity
        private void BtnCountLikeOnClick(object sender, EventArgs e)
        {
            try
            {
                if (AppSettings.PostButton == PostButtonSystem.Reaction)
                {
                    if (PostData.Reaction.Count > 0)
                    {
                        var intent = new Intent(this, typeof(ReactionPostTabbedActivity));
                        intent.PutExtra("PostObject", JsonConvert.SerializeObject(PostData));
                        StartActivity(intent);
                    }
                }
                else
                {
                    var intent = new Intent(this, typeof(PostDataActivity));
                    intent.PutExtra("PostId", PostData.PostId);
                    intent.PutExtra("PostType", "post_likes");
                    StartActivity(intent);
                } 
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Event Share
        private void BtnShareOnClick(object sender, EventArgs e)
        {
            try
            {
                ClickListener.SharePostClick(new GlobalClickEventArgs() {NewsFeedClass = PostData,}, PostModelType.ImagePost);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Event Add Comment
        private void BtnCommentOnClick(object sender, EventArgs e)
        {
            try
            {
                ClickListener.CommentPostClick(new GlobalClickEventArgs()
                {
                    NewsFeedClass = PostData,
                });
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        //Get Data 
        private void Get_DataImage()
        {
            try
            {
                IndexImage = int.Parse(Intent.GetStringExtra("itemIndex"));
                PostData = JsonConvert.DeserializeObject<PostDataObject>(Intent.GetStringExtra("AlbumObject"));
                if (PostData != null)
                {
                    ViewPager.Adapter = new TouchMyPhotoAdapter(this, ListUtils.ListCachedDataMyPhotos);
                    ViewPager.CurrentItem = IndexImage;
                    ViewPager.Adapter.NotifyDataSetChanged();
                    ViewPager.PageScrolled += ViewPagerOnPageScrolled;

                    SetDataPost();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void ViewPagerOnPageScrolled(object sender, ViewPager.PageScrolledEventArgs e)
        {
            try
            {
                if (e.Position >= 0 && ListUtils.ListCachedDataMyPhotos.Count > e.Position)
                {
                    PostData = ListUtils.ListCachedDataMyPhotos[e.Position];
                    if (PostData != null)
                        SetDataPost();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
         
        private void SetDataPost()
        {
            try
            {
                TxtDescription.Text = Methods.FunString.DecodeString(PostData.Orginaltext);

                if (PostData.IsLiked != null && (bool)PostData.IsLiked)
                {
                    BtnLike.Tag = "true";
                    ImgLike.SetColorFilter(Color.ParseColor(AppSettings.MainColor));
                }
                else
                {
                    BtnLike.Tag = "false";
                    ImgLike.SetColorFilter(Color.White);
                }

                if (PostData.IsWondered != null && (bool)PostData.IsWondered)
                {
                    BtnWonder.Tag = "true";
                    ImgWonder.SetColorFilter(Color.ParseColor("#f89823"));
                    ImgObeeNetwork.SetColorFilter(Color.ParseColor("#f89823"));

                    TxtWonder.Text = GetText(Resource.String.Lbl_wondered);
                }
                else
                {
                    BtnWonder.Tag = "false";
                    ImgWonder.SetColorFilter(Color.White);
                    ImgObeeNetwork.SetColorFilter(Color.White);
                    TxtWonder.Text = GetText(Resource.String.Btn_Wonder);
                }

                TxtCountObeeNetwork.Text = Methods.FunString.FormatPriceValue(int.Parse(PostData.PostWonders));

                if (AppSettings.PostButton == PostButtonSystem.Reaction)
                {
                    if (PostData.Reaction == null)
                        PostData.Reaction = new ObeeNetworkClient.Classes.Posts.Reaction();

                    TxtCountLike.Text = Methods.FunString.FormatPriceValue(PostData.Reaction.Count);

                    if ((bool)(PostData.Reaction != null & PostData.Reaction?.IsReacted))
                    {
                        if (!string.IsNullOrEmpty(PostData.Reaction.Type))
                        {
                            switch (PostData.Reaction.Type)
                            {
                                case "1":
                                case "Like":
                                    LikeButton.SetReactionPack(ReactConstants.Like);
                                    break;
                                case "2":
                                case "Love":
                                    LikeButton.SetReactionPack(ReactConstants.Love);
                                    break;
                                case "3":
                                case "HaHa":
                                    LikeButton.SetReactionPack(ReactConstants.HaHa);
                                    break;
                                case "4":
                                case "WoW":
                                    LikeButton.SetReactionPack(ReactConstants.Wow);
                                    break;
                                case "5":
                                case "Sad":
                                    LikeButton.SetReactionPack(ReactConstants.Sad);
                                    break;
                                case "6":
                                case "Angry":
                                    LikeButton.SetReactionPack(ReactConstants.Angry);
                                    break;
                                default:
                                    LikeButton.SetReactionPack(ReactConstants.Default);
                                    break;
                            } 
                        }
                    }
                    else
                    {
                        LikeButton.SetDefaultReaction(XReactions.GetDefaultReact());
                        LikeButton.SetTextColor(Color.White);
                    }
                }
                else
                {
                    if (PostData.IsLiked != null && (bool)PostData.IsLiked)
                        LikeButton.SetReactionPack(ReactConstants.Like);

                    TxtCountLike.Text = Methods.FunString.FormatPriceValue(int.Parse(PostData.PostLikes));

                    if (AppSettings.PostButton == PostButtonSystem.Wonder)
                    {
                        if (PostData.IsWondered != null && (bool)PostData.IsWondered)
                        {
                            ImgWonder.SetImageResource(Resource.Drawable.ic_action_ObeeNetwork);
                            ImgWonder.SetColorFilter(Color.ParseColor(AppSettings.MainColor));

                            TxtWonder.Text = GetString(Resource.String.Lbl_wondered);
                            TxtWonder.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                        }
                        else
                        {
                            ImgWonder.SetImageResource(Resource.Drawable.ic_action_ObeeNetwork);
                            ImgWonder.SetColorFilter(Color.White);

                            TxtWonder.Text = GetString(Resource.String.Btn_Wonder);
                            TxtWonder.SetTextColor(Color.ParseColor("#444444"));
                        }
                    }
                    else if (AppSettings.PostButton == PostButtonSystem.DisLike)
                    {
                        if (PostData.IsWondered != null && (bool)PostData.IsWondered)
                        {
                            ImgWonder.SetImageResource(Resource.Drawable.ic_action_dislike);
                            ImgWonder.SetColorFilter(Color.ParseColor(AppSettings.MainColor));

                            TxtWonder.Text = GetString(Resource.String.Lbl_disliked);
                            TxtWonder.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                        }
                        else
                        {
                            ImgWonder.SetImageResource(Resource.Drawable.ic_action_dislike);
                            ImgWonder.SetColorFilter(Color.White);

                            TxtWonder.Text = GetString(Resource.String.Btn_Dislike);
                            TxtWonder.SetTextColor(Color.ParseColor("#444444"));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

    }
}