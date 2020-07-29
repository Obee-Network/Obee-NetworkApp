using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AFollestad.MaterialDialogs;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using Java.Lang;
using Newtonsoft.Json;
using Plugin.Share;
using Plugin.Share.Abstractions;
using ObeeNetwork.Activities.Articles.Adapters;
using ObeeNetwork.Activities.Comment;
using ObeeNetwork.Helpers.Ads;
using ObeeNetwork.Helpers.CacheLoaders;
using ObeeNetwork.Helpers.Controller;
using ObeeNetwork.Helpers.Model;
using ObeeNetwork.Helpers.Utils;
using ObeeNetworkClient.Classes.Articles;
using ObeeNetworkClient.Classes.Global;
using ObeeNetworkClient.Requests;
using Xamarin.Facebook.Ads;
using Exception = System.Exception;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace ObeeNetwork.Activities.Articles
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class ArticlesViewActivity : AppCompatActivity, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        #region Variables Basic

        private ImageView ImageUser, ImageBlog;
        private TextView TxtUsername,TxtTime,TxtTitle, TxtViews; 
        private WebView TxtHtml;
        private ImageButton BtnMore;
        private ArticleDataObject ArticleData;

        public ArticlesCommentAdapter MAdapter;
        private RecyclerView MRecycler;
        private EditText TxtComment;
        private ImageView ImgSent;
        private static string ArticlesId;
        private static ArticlesViewActivity Instance;
        public ReplyCommentBottomSheet ReplyFragment;
        private RewardedVideoAd RewardedVideo;
        private string DataWebHtml;
       
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
                SetContentView(Resource.Layout.ArticlesViewLayout);

                Instance = this;

                ArticlesId = Intent.GetStringExtra("Id") ?? "";

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();

                GetDataArticles();

                RewardedVideo = AdsFacebook.InitRewardVideo(this);
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
        #region Menu

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;

                case Resource.Id.action_share:
                    ShareEvent();
                    break;

                case Resource.Id.action_copy:
                    Methods.CopyToClipboard(this, ArticleData.Url);
                    break;

            }
            return base.OnOptionsItemSelected(item);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.MenuArticleShare, menu);
            ChangeMenuIconColor(menu, Color.White);

            return base.OnCreateOptionsMenu(menu);

        }

        private void ChangeMenuIconColor(IMenu menu, Color color)
        {
            for (int i = 0; i < menu.Size(); i++)
            {
                var drawable = menu.GetItem(i).Icon;
                if (drawable == null) continue;
                drawable.Mutate();
                drawable.SetColorFilter(new PorterDuffColorFilter(color, PorterDuff.Mode.SrcAtop));
            }
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                ImageUser = FindViewById<ImageView>(Resource.Id.imageAvatar);
                ImageBlog = FindViewById<ImageView>(Resource.Id.imageBlog); 
                TxtUsername = FindViewById<TextView>(Resource.Id.username);
                TxtTime = FindViewById<TextView>(Resource.Id.time); 
                TxtTitle = FindViewById<TextView>(Resource.Id.title); 
                TxtHtml = FindViewById<WebView>(Resource.Id.LocalWebView); 
                TxtViews = FindViewById<TextView>(Resource.Id.views);
                BtnMore = FindViewById<ImageButton>(Resource.Id.more);

                MRecycler = FindViewById<RecyclerView>(Resource.Id.recycler_view);
                TxtComment = FindViewById<EditText>(Resource.Id.commenttext);
                ImgSent = FindViewById<ImageView>(Resource.Id.send);

                TxtComment.Text = "";
                Methods.SetColorEditText(TxtComment, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
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
                    toolbar.Title = "";
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

        private void SetRecyclerViewAdapters()
        {
            try
            {
                MAdapter = new ArticlesCommentAdapter(this, MRecycler, "Light", ArticlesId, "Comment")
                {
                    CommentList = new ObservableCollection<CommentsArticlesObject>()
                };

                if (!Methods.CheckConnectivity())
                    Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                else
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => MAdapter.FetchBlogsApiComments(ArticlesId, "0") });
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
                    BtnMore.Click += BtnMoreOnClick;
                    ImgSent.Click += ImgSentOnClick;
                }
                else
                {
                    BtnMore.Click -= BtnMoreOnClick;
                    ImgSent.Click -= ImgSentOnClick;
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
                RewardedVideo?.Destroy();

                MAdapter = null;
                ImageUser = null;
                ImageBlog = null;
                MRecycler = null;
                TxtUsername = null;
                TxtTime = null;
                TxtTitle = null;
                TxtViews = null;
                TxtHtml = null;
                BtnMore = null;
                ArticleData = null;
                MAdapter = null;
                MRecycler = null;
                TxtComment = null;
                ArticlesId = null;
                Instance = null;
                ReplyFragment = null;
                DataWebHtml = null;
                RewardedVideo = null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        public static ArticlesViewActivity GetInstance()
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

        //Api sent Comment
        private async void ImgSentOnClick(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(TxtComment.Text))
                    return;

                if (Methods.CheckConnectivity())
                {
                    var dataUser = ListUtils.MyProfileList.FirstOrDefault();
                    //Comment Code 

                    var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                    CommentsArticlesObject comment = new CommentsArticlesObject
                    {
                        Id = unixTimestamp.ToString(),
                        BlogId = ArticlesId,
                        UserId = UserDetails.UserId,
                        Text = TxtComment.Text,
                        Likes = "0",
                        Posted = unixTimestamp.ToString(),
                        UserData = dataUser,
                        IsOwner = true,
                        Dislikes = "0",
                        IsCommentLiked = false,
                        Replies = new List<CommentsArticlesObject>()
                    };

                    MAdapter.CommentList.Add(comment);

                    var index = MAdapter.CommentList.IndexOf(comment);
                    if (index > -1)
                    {
                        MAdapter.NotifyItemInserted(index);
                    }

                    MRecycler.Visibility = ViewStates.Visible;

                    var dd = MAdapter.CommentList.FirstOrDefault();
                    if (dd?.Text == MAdapter.EmptyState)
                    {
                        MAdapter.CommentList.Remove(dd);
                        MAdapter.NotifyItemRemoved(MAdapter.CommentList.IndexOf(dd));
                    }

                    var text = TxtComment.Text;

                    //Hide keyboard
                    TxtComment.Text = "";

                    (int apiStatus, var respond) = await RequestsAsync.Article.CreateComments(ArticlesId, text);
                    if (apiStatus == 200)
                    {
                        if (respond is GetCommentsArticlesObject result)
                        {
                            var date = MAdapter.CommentList.FirstOrDefault(a => a.Id == comment.Id) ?? MAdapter.CommentList.FirstOrDefault(x => x.Id == result.Data[0]?.Id);
                            if (date != null)
                            {
                                date = result.Data[0];
                                date.Id = result.Data[0].Id;

                                index = MAdapter.CommentList.IndexOf(MAdapter.CommentList.FirstOrDefault(a => a.Id == unixTimestamp.ToString()));
                                if (index > -1)
                                {
                                    MAdapter.CommentList[index] = result.Data[0];

                                    //MAdapter.NotifyItemChanged(index);
                                    MRecycler.ScrollToPosition(index);
                                }
                            }
                        }
                    }
                    else Methods.DisplayReportResult(this, respond);

                    //Hide keyboard
                    TxtComment.Text = "";
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

        //More 
        private void BtnMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                arrayAdapter.Add(GetString(Resource.String.Lbl_CopeLink));
                arrayAdapter.Add(GetString(Resource.String.Lbl_Share));
                
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
         
        //Event Menu >> Share
        private async void ShareEvent()
        {
            try
            {
                //Share Plugin same as video
                if (!CrossShare.IsSupported) return;

                await CrossShare.Current.Share(new ShareMessage
                {
                    Title = ArticleData.Title,
                    Text = " ",
                    Url = ArticleData.Url
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        //Event Menu >> Reply
        public void CommentReplyClick(CommentsArticlesObject item)
        {
            try
            {
                // show dialog :
                ReplyFragment = new ReplyCommentBottomSheet();

                Bundle bundle = new Bundle();
                bundle.PutString("Type", "Article");
                bundle.PutString("Id", ArticlesId);
                bundle.PutString("Object", JsonConvert.SerializeObject(item));

                ReplyFragment.Arguments = bundle;

                ReplyFragment.Show(SupportFragmentManager, ReplyFragment.Tag);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void ReplyOnReplyClick(CommentsArticlesObject item)
        {
            try
            {
                if (ReplyFragment == null) return;
                ReplyFragment.TxtComment.Text = "";
                ReplyFragment.TxtComment.Text = "@" + item.UserData.Username + " ";
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
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
                    Methods.CopyToClipboard(this, ArticleData.Url);
                }
                else if (text == GetString(Resource.String.Lbl_Share))
                {
                    ShareEvent();
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
         
        #endregion

        private void GetDataArticles()
        {
            try
            {
                ArticleData = JsonConvert.DeserializeObject<ArticleDataObject>(Intent.GetStringExtra("ArticleObject"));
                if (ArticleData != null)
                {
                    GlideImageLoader.LoadImage(this, ArticleData.Author.Avatar, ImageUser, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                    GlideImageLoader.LoadImage(this, ArticleData.Thumbnail, ImageBlog, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

                    TxtUsername.Text = ObeeNetworkTools.GetNameFinal(ArticleData.Author);

                    TxtTitle.Text = Methods.FunString.DecodeString(ArticleData.Title);
                    TxtViews.Text = ArticleData.View + " " + GetText(Resource.String.Lbl_Views);

                    string style = AppSettings.SetTabDarkTheme ? "<style type='text/css'>body{color: #fff; background-color: #282828;}</style>" : "<style type='text/css'>body{color: #444; background-color: #FFFAFA;}</style>";
                    string imageFullWidthStyle = "<style>img{display: inline;height: auto;max-width: 100%;}</style>";

                    string content;
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
                    {
                        content = Html.FromHtml(ArticleData.Content, FromHtmlOptions.ModeCompact).ToString();
                    }
                    else
                    {
                        // This method is deprecated but need to use for old os devices
#pragma warning disable CS0618 // Type or member is obsolete
                        content = Html.FromHtml(ArticleData.Content).ToString();
#pragma warning restore CS0618 // Type or member is obsolete
                    }

                    //string content = Html.FromHtml(ArticleData.Content, FromHtmlOptions.ModeCompact).ToString();
                    DataWebHtml = "<!DOCTYPE html>";
                    DataWebHtml += "<head><title></title>" + style + imageFullWidthStyle + "</head>";
                    DataWebHtml += "<body>" + content + "</body>";
                    DataWebHtml += "</html>";
                    // <meta name='viewport' content='width=device-width, user-scalable=no' />
                    TxtHtml.SetWebViewClient(new MyWebViewClient(this));
                    TxtHtml.Settings.LoadsImagesAutomatically = true;
                    TxtHtml.Settings.JavaScriptEnabled = true;
                    TxtHtml.Settings.JavaScriptCanOpenWindowsAutomatically = true;
                    TxtHtml.Settings.SetLayoutAlgorithm(WebSettings.LayoutAlgorithm.NarrowColumns);
                    TxtHtml.Settings.DomStorageEnabled = true;
                    TxtHtml.Settings.AllowFileAccess = true;
                    TxtHtml.Settings.DefaultTextEncodingName = "utf-8";

                    TxtHtml.Settings.UseWideViewPort = (true);
                    TxtHtml.Settings.LoadWithOverviewMode = (true);

                    TxtHtml.Settings.SetSupportZoom(false);
                    TxtHtml.Settings.BuiltInZoomControls = (false);
                    TxtHtml.Settings.DisplayZoomControls = (false);

                    int fontSize = (int)TypedValue.ApplyDimension(ComplexUnitType.Sp, 18, Resources.DisplayMetrics);
                    TxtHtml.Settings.DefaultFontSize = fontSize;

                    TxtHtml.LoadDataWithBaseURL(null, DataWebHtml, "text/html", "UTF-8", null);

                    bool success = int.TryParse(ArticleData.Posted, out var number);
                    string Timedate = "";
                    if (success)
                    {
                        TxtTime.Text = Timedate;
                        //TxtTime.Text = Methods.Time.TimeAgo(Convert.ToInt32(number));
                    }
                    else
                    {
                        TxtTime.Text = Timedate;
                        //TxtTime.Text = ArticleData.Posted;
                    }

                    if (Methods.CheckConnectivity())
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Article.GetBlogById(ArticlesId) });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private class MyWebViewClient : WebViewClient
        {
            private readonly ArticlesViewActivity Activity;
            public MyWebViewClient(ArticlesViewActivity mActivity)
            {
                Activity = mActivity;
            }

            public override bool ShouldOverrideUrlLoading(WebView view, IWebResourceRequest request)
            {
                Methods.App.OpenbrowserUrl(Activity, request.Url.ToString());
                view.LoadDataWithBaseURL(null, Activity.DataWebHtml, "text/html", "UTF-8", null);
                return true;
            }
        }
    }
}