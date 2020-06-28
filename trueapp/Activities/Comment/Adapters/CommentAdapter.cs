using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Graphics;
using Android.Media;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using AT.Markushi.UI;
using AutoMapper;
using Bumptech.Glide;
using Bumptech.Glide.Integration.RecyclerView;
using Bumptech.Glide.Request;
using Bumptech.Glide.Util;
using Com.Github.Library.Bubbleview;
using Com.Luseen.Autolinklibrary;
using Com.Tuyenmonkey.Textdecorator;
using Java.IO;
using Java.Util;
using Refractored.Controls;
using ObeeNetwork.Activities.NativePost.Holders;
using ObeeNetwork.Activities.NativePost.Post;
using ObeeNetwork.Helpers.CacheLoaders;
using ObeeNetwork.Helpers.Controller;
using ObeeNetwork.Helpers.Fonts;
using ObeeNetwork.Helpers.Utils;
using ObeeNetworkClient;
using ObeeNetworkClient.Classes.Comments;
using ObeeNetworkClient.Requests;
using Console = System.Console;
using IList = System.Collections.IList;
using Object = Java.Lang.Object;
using Timer = System.Timers.Timer;
using Uri = Android.Net.Uri;

namespace ObeeNetwork.Activities.Comment.Adapters
{
    public class CommentObjectExtra : GetCommentObject 
    {
        public new Android.Media.MediaPlayer MediaPlayer { get; set; }
        public new Timer MediaTimer { get; set; }
        public new CommentAdapterViewHolder SoundViewHolder { get; set; }
    }

    public class CommentAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public string EmptyState = "Wo_Empty_State";
        private static Activity ActivityContext;

        public ObservableCollection<CommentObjectExtra> CommentList = new ObservableCollection<CommentObjectExtra>();
        private static string ThemeColor;
        private readonly RecyclerScrollListener MainScrollEvent;
        private string ApiIdParameter { get; }

        public CommentAdapter(Activity context, RecyclerView mainRecyclerView, string themeColor, string postId )
        {
            try
            {
                HasStableIds = true;
                ActivityContext = context;
                var mainRecyclerView1 = mainRecyclerView;
                ThemeColor = themeColor;
                ApiIdParameter = postId;

                var mainLinearLayoutManager = new LinearLayoutManager(context);
                mainRecyclerView1.SetLayoutManager(mainLinearLayoutManager);

                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<CommentObjectExtra>(context, this, sizeProvider, 8);
                mainRecyclerView1.AddOnScrollListener(preLoader);

                mainRecyclerView1.SetAdapter(this);
                mainRecyclerView1.HasFixedSize = true;
                mainRecyclerView1.SetItemViewCacheSize(10);
                mainRecyclerView1.ClearAnimation();
                mainRecyclerView1.GetLayoutManager().ItemPrefetchEnabled = true;
                mainRecyclerView1.SetItemViewCacheSize(10);

                MainScrollEvent = new RecyclerScrollListener();
                mainRecyclerView1.AddOnScrollListener(MainScrollEvent);
                MainScrollEvent.LoadMoreEvent += MainScrollEvent_LoadMoreEvent;
                MainScrollEvent.IsLoading = false; 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override int ItemCount => CommentList?.Count ?? 0;
         
        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            { 
                switch (viewType)
                {
                    case 0:
                        return new CommentAdapterViewHolder(LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Style_Comment, parent, false), ThemeColor);
                    case 1:
                        return new CommentAdapterViewHolder(LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Style_Comment_Image, parent, false), ThemeColor);
                    case 2:
                        return new CommentAdapterViewHolder(LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Style_Comment_Voice, parent, false), ThemeColor);
                        case 666:
                        return new MainHolders.EmptyStateAdapterViewHolder(LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Style_EmptyState, parent, false));
                    default:
                        return new CommentAdapterViewHolder(LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Style_Comment, parent, false),ThemeColor);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return null;
            }
        }

        public void LoadCommentData(CommentObjectExtra item, RecyclerView.ViewHolder viewHolder, int position = 0, bool hasClickEvents = true)
        {
            try
            {
                if (!(viewHolder is CommentAdapterViewHolder holder))
                    return;

                if (AppSettings.FlowDirectionRightToLeft)
                    holder.BubbleLayout.LayoutDirection = LayoutDirection.Rtl;
              
                if (!string.IsNullOrEmpty(item.Text) || !string.IsNullOrWhiteSpace(item.Text))
                {
                    var changer = new TextSanitizer(holder.CommentText, ActivityContext);
                    changer.Load(Methods.FunString.DecodeString(item.Text));
                }
                else
                {
                    holder.CommentText.Visibility = ViewStates.Gone;
                }
                 
                holder.TimeTextView.Text = Methods.Time.TimeAgo(int.Parse(item.Time));
                holder.UserName.Text = item.Publisher.Name;

                GlideImageLoader.LoadImage(ActivityContext, item.Publisher.Avatar, holder.Image, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
             
                var textHighLighter = item.Publisher.Name;
                var textIsPro = string.Empty;

                if (item.Publisher.Verified == "1")
                    textHighLighter += " " + IonIconsFonts.CheckmarkCircled;

                if (item.Publisher.IsPro == "1")
                {
                    textIsPro = " " + IonIconsFonts.Flash;
                    textHighLighter += textIsPro;
                }

                var decorator = TextDecorator.Decorate(holder.UserName, textHighLighter).SetTextStyle((int) TypefaceStyle.Bold, 0, item.Publisher.Name.Length);

                if (item.Publisher.Verified == "1")
                    decorator.SetTextColor(Resource.Color.Post_IsVerified, IonIconsFonts.CheckmarkCircled);

                if (item.Publisher.IsPro == "1")
                    decorator.SetTextColor(Resource.Color.text_color_in_between, textIsPro);

                decorator.Build();
              
                //Image
                if (holder.ItemViewType == 1 || holder.CommentImage != null)
                {
                    if(!string.IsNullOrEmpty(item.CFile) && (item.CFile.Contains("file://") || item.CFile.Contains("content://") || item.CFile.Contains("storage") || item.CFile.Contains("/data/user/0/")))
                    {
                        File file2 = new File(item.CFile);
                        var photoUri = FileProvider.GetUriForFile(ActivityContext, ActivityContext.PackageName + ".fileprovider", file2);
                        Glide.With(ActivityContext).Load(photoUri).Apply(new RequestOptions()).Into(holder.CommentImage);
                         
                        //GlideImageLoader.LoadImage(ActivityContext,item.CFile, holder.CommentImage, ImageStyle.CenterCrop, ImagePlaceholders.Color);
                    }
                    else
                    {
                        if (!item.CFile.Contains(Client.WebsiteUrl))
                            item.CFile = ObeeNetworkTools.GetTheFinalLink(item.CFile);

                        GlideImageLoader.LoadImage(ActivityContext, item.CFile, holder.CommentImage, ImageStyle.CenterCrop, ImagePlaceholders.Color);
                    } 
                }

                //Voice
                if (holder.VoiceLayout != null && !string.IsNullOrEmpty(item.Record))
                {
                    LoadAudioItem(holder, position, item);
                }
                 
                if (item.Replies != "0" && item.Replies != null)
                    holder.ReplyTextView.Text = ActivityContext.GetText(Resource.String.Lbl_Reply) + " " + "(" + item.Replies + ")";
                
                if (item.IsCommentLiked)
                {
                    holder.LikeTextView.Text = ActivityContext.GetText(Resource.String.Btn_Liked);
                    holder.LikeTextView.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                    holder.LikeTextView.Tag = "Liked";
                }
                else
                {
                    holder.LikeTextView.Text = ActivityContext.GetText(Resource.String.Btn_Like);
                   
                    if (AppSettings.SetTabDarkTheme || ThemeColor == "Dark")
                    {
                        holder.ReplyTextView.SetTextColor(Color.White);
                        holder.LikeTextView.SetTextColor(Color.White);
                    }
                    else
                    {
                        holder.ReplyTextView.SetTextColor(Color.Black);
                        holder.LikeTextView.SetTextColor(Color.Black);
                    }
                     
                    holder.LikeTextView.Tag = "Like";
                }

                holder.TimeTextView.Tag = "true";

                if (holder.Image.HasOnClickListeners)
                    return;

                var postEventListener = new CommentClickListener(ActivityContext, "Comment"); 

                //Create an Event 
                holder.MainView.LongClick += (sender, e) => postEventListener.MoreCommentReplyPostClick(new CommentReplyClickEventArgs { CommentObject = item, Position = position, View = holder.MainView });

                holder.Image.Click += (sender, args) => postEventListener.ProfilePostClick(new ProfileClickEventArgs{Holder = holder, CommentClass = item, Position = position, View = holder.MainView});

                if (hasClickEvents)
                    holder.ReplyTextView.Click += (sender, args) => postEventListener.CommentReplyPostClick(new CommentReplyClickEventArgs { CommentObject = item, Position = position, View = holder.MainView });

                holder.LikeTextView.Click += delegate
                {
                    try
                    {
                        if (holder.LikeTextView.Tag.ToString() == "Liked")
                        {
                            item.IsCommentLiked = false;

                            holder.LikeTextView.Text = ActivityContext.GetText(Resource.String.Btn_Like);
                            if (AppSettings.SetTabDarkTheme || ThemeColor == "Dark")
                            {
                                holder.LikeTextView.SetTextColor(Color.White);
                            }
                            else
                            {
                                holder.LikeTextView.SetTextColor(Color.Black);
                            }
                             
                            holder.LikeTextView.Tag = "Like";

                            //sent api Dislike comment 
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Comment.LikeUnLikeCommentAsync(item.Id, false) });
                        }
                        else
                        {
                            item.IsCommentLiked = true;

                            holder.LikeTextView.Text = ActivityContext.GetText(Resource.String.Btn_Liked);
                            holder.LikeTextView.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                            holder.LikeTextView.Tag = "Liked";

                            //sent api like comment 
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Comment.LikeUnLikeCommentAsync(item.Id, true) }); 
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                };

                if (holder.CommentImage != null)
                    holder.CommentImage.Click += (sender, args) => postEventListener.OpenImageLightBox(item);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
         
        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {
                if (viewHolder.ItemViewType == 666)
                {
                    if (!(viewHolder is MainHolders.EmptyStateAdapterViewHolder emptyHolder))
                        return;

                    var itemEmpty = CommentList.FirstOrDefault(a => a.Id == EmptyState);
                    if (itemEmpty != null && !string.IsNullOrEmpty(itemEmpty.Orginaltext)) 
                    {
                        emptyHolder.EmptyText.Text = itemEmpty.Orginaltext;
                    }
                    else
                    { 
                        emptyHolder.EmptyText.Text = ActivityContext.GetText(Resource.String.Lbl_NoComments);
                    }
                    return;
                }

                if (!(viewHolder is CommentAdapterViewHolder holder))
                    return;

                var item = CommentList[position];
                if (item == null)
                    return;

                LoadCommentData(item, holder, position);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private int PositionSound;

        private void LoadAudioItem(CommentAdapterViewHolder soundViewHolder, int position, CommentObjectExtra item)
        {
            try
            {
                item.SoundViewHolder ??= soundViewHolder;

                soundViewHolder.VoiceLayout.Visibility = ViewStates.Visible;

                var fileName = item.Record.Split('/').Last();

                var mediaFile = ObeeNetworkTools.GetFile(item.PostId, Methods.Path.FolderDcimSound, fileName, item.Record);
                soundViewHolder.DurationVoice.Text = string.IsNullOrEmpty(item.MediaDuration)
                    ? Methods.AudioRecorderAndPlayer.GetTimeString(Methods.AudioRecorderAndPlayer.Get_MediaFileDuration(mediaFile))
                    : item.MediaDuration;

                soundViewHolder.PlayButton.Visibility = ViewStates.Visible;

                if (!soundViewHolder.PlayButton.HasOnClickListeners)
                {
                    soundViewHolder.PlayButton.Click += (o, args) =>
                    {
                        try
                        {
                            if (PositionSound != position)
                            {
                                var list = CommentList.Where(a => a.MediaPlayer != null).ToList();
                                if (list.Count > 0)
                                {
                                    foreach (var extra in list)
                                    {
                                        if (extra.MediaPlayer != null)
                                        {
                                            extra.MediaPlayer.Stop();
                                            extra.MediaPlayer.Reset();
                                        }
                                        extra.MediaPlayer = null;
                                        extra.MediaTimer = null;

                                        extra.MediaPlayer?.Release();
                                        extra.MediaPlayer = null;
                                    }
                                }
                            }

                            if (mediaFile.Contains("http"))
                                mediaFile = ObeeNetworkTools.GetFile(item.PostId, Methods.Path.FolderDcimSound, fileName, item.Record);

                            if (item.MediaPlayer == null)
                            {
                                PositionSound = position;
                                item.MediaPlayer = new Android.Media.MediaPlayer();
                                item.MediaPlayer.SetAudioAttributes(new AudioAttributes.Builder().SetUsage(AudioUsageKind.Media).SetContentType(AudioContentType.Music).Build());

                                item.MediaPlayer.Completion += (sender, e) =>
                                {
                                    try
                                    {
                                        soundViewHolder.PlayButton.Tag = "Play";
                                        //soundViewHolder.PlayButton.SetImageResource(item.ModelType == MessageModelType.LeftAudio ? Resource.Drawable.ic_play_dark_arrow : Resource.Drawable.ic_play_arrow);
                                        soundViewHolder.PlayButton.SetImageResource(Resource.Drawable.ic_play_dark_arrow);

                                        item.MediaIsPlaying = false;

                                        item.MediaPlayer.Stop();
                                        item.MediaPlayer.Reset();
                                        item.MediaPlayer = null;

                                        item.MediaTimer.Enabled = false;
                                        item.MediaTimer.Stop();
                                        item.MediaTimer = null;
                                    }
                                    catch (Exception exception)
                                    {
                                        Console.WriteLine(exception);
                                    }
                                };

                                item.MediaPlayer.Prepared += (s, ee) =>
                                {
                                    try
                                    {
                                        item.MediaIsPlaying = true;
                                        soundViewHolder.PlayButton.Tag = "Pause";
                                        soundViewHolder.PlayButton.SetImageResource(AppSettings.SetTabDarkTheme ? Resource.Drawable.ic_media_pause_light : Resource.Drawable.ic_media_pause_dark);

                                        if (item.MediaTimer == null)
                                            item.MediaTimer = new Timer { Interval = 1000 };

                                        item.MediaPlayer.Start();

                                        //var durationOfSound = item.MediaPlayer.Duration;

                                        item.MediaTimer.Elapsed += (sender, eventArgs) =>
                                        {
                                            ActivityContext.RunOnUiThread(() =>
                                            {
                                                try
                                                {
                                                    if (item.MediaTimer.Enabled)
                                                    {
                                                        if (item.MediaPlayer.CurrentPosition <= item.MediaPlayer.Duration)
                                                        {
                                                            soundViewHolder.DurationVoice.Text = Methods.AudioRecorderAndPlayer.GetTimeString(item.MediaPlayer.CurrentPosition);
                                                        }
                                                        else
                                                        {
                                                            soundViewHolder.DurationVoice.Text = Methods.AudioRecorderAndPlayer.GetTimeString(item.MediaPlayer.Duration);

                                                            soundViewHolder.PlayButton.Tag = "Play";
                                                            //soundViewHolder.PlayButton.SetImageResource(item.ModelType == MessageModelType.LeftAudio ? Resource.Drawable.ic_play_dark_arrow : Resource.Drawable.ic_play_arrow);
                                                            soundViewHolder.PlayButton.SetImageResource(Resource.Drawable.ic_play_dark_arrow);
                                                        }
                                                    }
                                                }
                                                catch (Exception e)
                                                {
                                                    Console.WriteLine(e);
                                                    soundViewHolder.PlayButton.Tag = "Play";
                                                }
                                            });
                                        };
                                        item.MediaTimer.Start();
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine(e);
                                    }
                                };

                                if (mediaFile.Contains("http"))
                                {
                                    item.MediaPlayer.SetDataSource(ActivityContext, Uri.Parse(mediaFile));
                                    item.MediaPlayer.PrepareAsync();
                                }
                                else
                                {
                                    File file2 = new File(mediaFile);
                                    var photoUri = FileProvider.GetUriForFile(ActivityContext, ActivityContext.PackageName + ".fileprovider", file2);

                                    item.MediaPlayer.SetDataSource(ActivityContext, photoUri);
                                    item.MediaPlayer.Prepare();
                                }

                                item.SoundViewHolder = soundViewHolder;
                            }
                            else
                            {
                                if (soundViewHolder.PlayButton.Tag.ToString() == "Play")
                                {
                                    soundViewHolder.PlayButton.Tag = "Pause";
                                    soundViewHolder.PlayButton.SetImageResource(AppSettings.SetTabDarkTheme ? Resource.Drawable.ic_media_pause_light : Resource.Drawable.ic_media_pause_dark);

                                    item.MediaIsPlaying = true;
                                    item.MediaPlayer?.Start();

                                    if (item.MediaTimer != null)
                                    {
                                        item.MediaTimer.Enabled = true;
                                        item.MediaTimer.Start();
                                    }
                                }
                                else if (soundViewHolder.PlayButton.Tag.ToString() == "Pause")
                                {
                                    soundViewHolder.PlayButton.Tag = "Play";
                                    //soundViewHolder.PlayButton.SetImageResource(item.ModelType == MessageModelType.LeftAudio ? Resource.Drawable.ic_play_dark_arrow : Resource.Drawable.ic_play_arrow);
                                    soundViewHolder.PlayButton.SetImageResource(Resource.Drawable.ic_play_dark_arrow);

                                    item.MediaIsPlaying = false;
                                    item.MediaPlayer?.Pause();

                                    if (item.MediaTimer != null)
                                    {
                                        item.MediaTimer.Enabled = false;
                                        item.MediaTimer.Stop();
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    };
                }

                if (item.SoundViewHolder == null)
                    item.SoundViewHolder = soundViewHolder;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }



        public CommentObjectExtra GetItem(int position)
        {
            return CommentList[position];
        }

        public override long GetItemId(int position)
        {
            try
            {
                return int.Parse(CommentList[position].Id);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return 0;
            }
        }

        public override int GetItemViewType(int position)
        {
            try
            {
                var item = CommentList[position];

                if (string.IsNullOrEmpty(item.CFile) && string.IsNullOrEmpty(item.Record) && item.Text != EmptyState)
                    return 0;

                if ((!string.IsNullOrEmpty(item.CFile) && !string.IsNullOrEmpty(item.Record)) || !string.IsNullOrEmpty(item.CFile))
                    return 1;

                if (!string.IsNullOrEmpty(item.Record))
                    return 2;

                if (item.Text == EmptyState)
                    return 666;

                return 0;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return 0;
            }
        }

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = CommentList[p0];
                if (item == null)
                    return d;
                else
                {
                    if (item.Text != EmptyState)
                    {
                        if (!string.IsNullOrEmpty(item.CFile))
                            d.Add(item.CFile);

                        if (!string.IsNullOrEmpty(item.Publisher.Avatar))
                            d.Add(item.Publisher.Avatar);

                        return d;
                    }

                    return Collections.SingletonList(p0);  
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Collections.SingletonList(p0);
            }
        }

        public RequestBuilder GetPreloadRequestBuilder(Object p0)
        {
            return GlideImageLoader.GetPreLoadRequestBuilder(ActivityContext, p0.ToString(), ImageStyle.CenterCrop);
        }

        private class RecyclerScrollListener : RecyclerView.OnScrollListener
        {
            public delegate void LoadMoreEventHandler(object sender, EventArgs e);

            public event LoadMoreEventHandler LoadMoreEvent;

            public bool IsLoading { get; set; }

           
            private LinearLayoutManager LayoutManager;

            public RecyclerScrollListener()
            {
                IsLoading = false;
            }
             
            public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
            {
                base.OnScrolled(recyclerView, dx, dy);


                if (LayoutManager == null)
                    LayoutManager = (LinearLayoutManager) recyclerView.GetLayoutManager();

                var visibleItemCount = recyclerView.ChildCount;
                var totalItemCount = recyclerView.GetAdapter().ItemCount;

                var pastItems = LayoutManager.FindFirstVisibleItemPosition();

                if (visibleItemCount + pastItems + 6 < totalItemCount)
                    return;

                if (IsLoading)
                    return;

                LoadMoreEvent?.Invoke(this, null);
            }
        }

        private void MainScrollEvent_LoadMoreEvent(object sender, EventArgs e)
        {
            try
            {
                if (CommentList.Count > 0)
                {
                    MainScrollEvent.IsLoading = true;

                    var item = CommentList?.LastOrDefault()?.Id;

                    if (CommentList?.Count <= 3)
                        item = "";

                    if (item == null)
                        return;

                    if (!Methods.CheckConnectivity())
                        Toast.MakeText(ActivityContext, ActivityContext.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                    else
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => FetchPostApiComments(item, ApiIdParameter) });
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public async Task FetchPostApiComments(string offset, string postId)
        {
            int countList = CommentList.Count;
            var (apiStatus, respond) = await RequestsAsync.Comment.GetPostComments(postId, "10", offset);
            if (apiStatus == 200)
            {
                if (respond is CommentObject result)
                {
                    var respondList = result.CommentList?.Count;
                    if (respondList > 0)
                    {
                        foreach (var item in from item in result.CommentList let check = CommentList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                        {
                            var db = Mapper.Map<CommentObjectExtra>(item);
                            if (db != null) CommentList.Add(db);
                        }

                        if (countList > 0)
                        { 
                            ActivityContext.RunOnUiThread(() => { NotifyItemRangeInserted(countList, CommentList.Count - countList); });
                        }
                        else
                        { 
                            ActivityContext.RunOnUiThread(NotifyDataSetChanged);
                        }
                    }
                }
            }
            else Methods.DisplayReportResult(ActivityContext, respond);

            CommentActivity.GetInstance().SwipeRefreshLayout.Refreshing = false;
            MainScrollEvent.IsLoading = false;

            if (CommentList.Count > 0)
            {
                var emptyStateChecker = CommentList.FirstOrDefault(a => a.Text == EmptyState);
                if (emptyStateChecker != null && CommentList.Count > 1)
                {
                    CommentList.Remove(emptyStateChecker);
                    ActivityContext.RunOnUiThread(NotifyDataSetChanged);
                }
            }
            else
            {
                CommentList.Clear();
                var d = new CommentObjectExtra { Text = EmptyState };
                CommentList.Add(d);
                ActivityContext.RunOnUiThread(NotifyDataSetChanged);
            } 
        } 
    }

    public class CommentAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; private set; }
        public BubbleLinearLayout BubbleLayout { get; private set; }
        public CircleImageView Image { get; private set; }
        public AutoLinkTextView CommentText { get; private set; }
        public TextView TimeTextView { get; private set; }
        public TextView UserName { get; private set; }
        public TextView ReplyTextView { get; private set; }
        public TextView LikeTextView { get; private set; }

        public ImageView CommentImage { get; private set; }

        public LinearLayout VoiceLayout { get; private set; }
        public CircleButton PlayButton { get; private set; }
        public TextView DurationVoice { get; private set; } 
        public TextView TimeVoice { get; private set; } 

        #endregion

        public CommentAdapterViewHolder(View itemView, string themeColor) : base(itemView)
        {
            try
            {
                MainView = itemView;

                BubbleLayout = MainView.FindViewById<BubbleLinearLayout>(Resource.Id.bubble_layout);
                Image = MainView.FindViewById<CircleImageView>(Resource.Id.card_pro_pic);
                CommentText = MainView.FindViewById<AutoLinkTextView>(Resource.Id.active);
                UserName = MainView.FindViewById<TextView>(Resource.Id.username);
                TimeTextView = MainView.FindViewById<TextView>(Resource.Id.time);
                ReplyTextView = MainView.FindViewById<TextView>(Resource.Id.reply);
                LikeTextView = MainView.FindViewById<TextView>(Resource.Id.Like);
                CommentImage = MainView.FindViewById<ImageView>(Resource.Id.image);

                try
                {
                    VoiceLayout = MainView.FindViewById<LinearLayout>(Resource.Id.voiceLayout);
                    PlayButton = MainView.FindViewById<CircleButton>(Resource.Id.playButton);
                    DurationVoice = MainView.FindViewById<TextView>(Resource.Id.Duration);
                    TimeVoice = MainView.FindViewById<TextView>(Resource.Id.timeVoice);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                 
                var font = Typeface.CreateFromAsset(MainView.Context.Resources.Assets, "ionicons.ttf");
                UserName.SetTypeface(font, TypefaceStyle.Normal);

                if (AppSettings.SetTabDarkTheme || themeColor == "Dark")
                {
                    ReplyTextView.SetTextColor(Color.White);
                    LikeTextView.SetTextColor(Color.White);
                }
                else
                {
                    ReplyTextView.SetTextColor(Color.Black);
                    LikeTextView.SetTextColor(Color.Black);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
