using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
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

namespace ObeeNetwork.Activities.Comment.Adapters
{ 
    public class ReplyCommentAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    { 
        public string EmptyState = "Wo_Empty_State"; 
        private readonly ReplyCommentActivity ActivityContext; 
        public ObservableCollection<CommentObjectExtra> ReplyCommentList = new ObservableCollection<CommentObjectExtra>();
        private readonly RecyclerScrollListener MainScrollEvent;
        private string ApiIdParameter { get;  set; }

        public ReplyCommentAdapter(ReplyCommentActivity context, RecyclerView mainRecyclerView,string commentId)
        {
            try
            {
                HasStableIds = true;
                ActivityContext = context;
                var mainRecyclerView1 = mainRecyclerView;
              
                ApiIdParameter = commentId;

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

        public override int ItemCount => ReplyCommentList?.Count ?? 0;
         
        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                switch (viewType)
                {
                    case 0: return new ReplyCommentAdapterViewHolder(LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Style_Comment, parent, false));
                    case 1: return new ReplyCommentAdapterViewHolder(LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Style_Comment_Image, parent, false));
                    case 666: return new MainHolders.EmptyStateAdapterViewHolder(LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Style_EmptyState, parent, false));
                    default:
                        return new ReplyCommentAdapterViewHolder(LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Style_Comment, parent, false));
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return null;
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

                    emptyHolder.EmptyText.Text = "No Replies to be displayed";
                    return;
                }

                if (!(viewHolder is ReplyCommentAdapterViewHolder holder))
                    return;

                var item = ReplyCommentList[position];
                if (item == null)
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
                 
                if (holder.TimeTextView.Tag?.ToString() == "true")
                    return;

                holder.TimeTextView.Text = Methods.Time.TimeAgo(int.Parse(item.Time));
                holder.UserName.Text = item.Publisher.Name;
                GlideImageLoader.LoadImage(ActivityContext, item.Publisher.Avatar, holder.Image, ImageStyle.CircleCrop, ImagePlaceholders.Color);
                 
                var textHighLighter = item.Publisher.Name;
                var textIsPro = string.Empty;

                if (item.Publisher.Verified == "1")
                    textHighLighter += " " + IonIconsFonts.CheckmarkCircled;

                if (item.Publisher.IsPro == "1")
                {
                    textIsPro = " " + IonIconsFonts.Flash;
                    textHighLighter += textIsPro;
                }

                var decorator = TextDecorator.Decorate(holder.UserName, textHighLighter)
                    .SetTextStyle((int)TypefaceStyle.Bold, 0, item.Publisher.Name.Length);

                if (item.Publisher.Verified == "1")
                    decorator.SetTextColor(Resource.Color.Post_IsVerified, IonIconsFonts.CheckmarkCircled);

                if (item.Publisher.IsPro == "1")
                    decorator.SetTextColor(Resource.Color.text_color_in_between, textIsPro);

                decorator.Build();

                if (holder.ItemViewType == 1)
                    if (!string.IsNullOrEmpty(item.CFile) && (item.CFile.Contains("file://") || item.CFile.Contains("content://") || item.CFile.Contains("storage") || item.CFile.Contains("/data/user/0/")))
                    {
                        File file2 = new File(item.CFile);
                        var photoUri = FileProvider.GetUriForFile(ActivityContext, ActivityContext.PackageName + ".fileprovider", file2);
                        Glide.With(ActivityContext).Load(photoUri).Apply(new RequestOptions()).Into(holder.CommentImage);
                         
                        //GlideImageLoader.LoadImage(ActivityContext, item.CFile, holder.CommentImage, ImageStyle.CenterCrop, ImagePlaceholders.Color);
                    }
                    else
                    {
                        GlideImageLoader.LoadImage(ActivityContext, Client.WebsiteUrl + "/" + item.CFile, holder.CommentImage, ImageStyle.CenterCrop, ImagePlaceholders.Color);
                    }

                if (item.IsCommentLiked)
                {
                    holder.LikeTextView.Text = ActivityContext.GetText(Resource.String.Btn_Liked);
                    holder.LikeTextView.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                    holder.LikeTextView.Tag = "Liked";
                }
                else
                {
                    holder.LikeTextView.Text = ActivityContext.GetText(Resource.String.Btn_Like);
                    if (AppSettings.SetTabDarkTheme)
                    {
                        holder.LikeTextView.SetTextColor(Color.ParseColor("#ffffff"));
                    }
                    else
                    {
                        holder.LikeTextView.SetTextColor(Color.ParseColor("#000000"));
                    }
                    holder.LikeTextView.Tag = "Like";
                }

                holder.TimeTextView.Tag = "true";

                if (holder.Image.HasOnClickListeners)
                    return;
                 
                var postEventListener = new CommentClickListener(ActivityContext, "Reply");

                //Create an Event 
                holder.MainView.LongClick += (sender, e) => postEventListener.MoreCommentReplyPostClick(new CommentReplyClickEventArgs { CommentObject = item, Position = position, View = holder.MainView });

                holder.Image.Click += (sender, args) => postEventListener.ProfilePostClick(new ProfileClickEventArgs { Holder = holder, CommentClass = item, Position = position, View = holder.MainView });

                holder.ReplyTextView.Click += (sender, args) =>
                {
                    try
                    {
                        ActivityContext.TxtComment.Text = "";
                        ActivityContext.TxtComment.Text = "@" + item.Publisher.Username + " ";
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                };
                 
                holder.LikeTextView.Click += delegate
                {
                    try
                    {
                        if (holder.LikeTextView.Tag.ToString() == "Liked")
                        {
                            item.IsCommentLiked = false;

                            holder.LikeTextView.Text = ActivityContext.GetText(Resource.String.Btn_Like);
                            if (AppSettings.SetTabDarkTheme)
                            {
                                holder.LikeTextView.SetTextColor(Color.ParseColor("#ffffff"));
                            }
                            else
                            {
                                holder.LikeTextView.SetTextColor(Color.ParseColor("#000000"));
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

                holder.CommentImage.Click += (sender, args) => postEventListener.OpenImageLightBox(item);

            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

      
        public CommentObjectExtra GetItem(int position)
        {
            return ReplyCommentList[position];
        }

        public override long GetItemId(int position)
        {
            try
            {
                return int.Parse(ReplyCommentList[position].Id);
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
                if (string.IsNullOrEmpty(ReplyCommentList[position].CFile) && ReplyCommentList[position].Text != EmptyState)
                    return 0;
                else if (ReplyCommentList[position].Text == EmptyState)
                    return 666;
                else
                    return 1;
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
                var item = ReplyCommentList[p0];
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

        public class RecyclerScrollListener : RecyclerView.OnScrollListener
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
                    LayoutManager = (LinearLayoutManager)recyclerView.GetLayoutManager();

                var visibleItemCount = recyclerView.ChildCount;
                var totalItemCount = recyclerView.GetAdapter().ItemCount;

                var pastItems = LayoutManager.FindFirstVisibleItemPosition();

                if (visibleItemCount + pastItems + 4 < totalItemCount)
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
                if (ReplyCommentList.Count > 3)
                {
                    MainScrollEvent.IsLoading = true;
                     
                    var item = ReplyCommentList?.LastOrDefault()?.Id;

                    if (ReplyCommentList?.Count <= 3)
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

        public async Task FetchPostApiComments(string offset,string postId)
        {
            int countList = ReplyCommentList.Count;
            var (apiStatus, respond) = await RequestsAsync.Comment.GetPostComments(postId, "10", offset, "fetch_comments_reply");
            if (apiStatus == 200)
            {
                if (respond is CommentObject result)
                {
                    var respondList = result.CommentList?.Count;
                    if (respondList > 0)
                    {
                        foreach (var item in from item in result.CommentList let check = ReplyCommentList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                        {
                            var db = Mapper.Map<CommentObjectExtra>(item);
                            if (db != null) ReplyCommentList.Add(db);
                        }

                        if (countList > 0)
                        { 
                            ActivityContext.RunOnUiThread(() => { NotifyItemRangeInserted(countList, ReplyCommentList.Count - countList); });
                        }
                        else
                        { 
                            ActivityContext.RunOnUiThread(NotifyDataSetChanged);
                        }
                    }
                }
            }
            else Methods.DisplayReportResult(ActivityContext, respond);

            MainScrollEvent.IsLoading = false;

            if (ReplyCommentList.Count > 0)
            {
                var emptyStateChecker = ReplyCommentList.FirstOrDefault(a => a.Text == EmptyState);
                if (emptyStateChecker != null && ReplyCommentList.Count > 1)
                {
                    ReplyCommentList.Remove(emptyStateChecker);
                    ActivityContext.RunOnUiThread(NotifyDataSetChanged);
                }
            }
            else
            {
                ReplyCommentList.Clear();
                var d = new CommentObjectExtra { Text = EmptyState };
                ReplyCommentList.Add(d);
                ActivityContext.RunOnUiThread(NotifyDataSetChanged);
            } 
        }
    }

    public class ReplyCommentAdapterViewHolder : RecyclerView.ViewHolder
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
        #endregion

        public ReplyCommentAdapterViewHolder(View itemView) : base(itemView)
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
               
                var font = Typeface.CreateFromAsset(MainView.Context.Resources.Assets, "ionicons.ttf");
                UserName.SetTypeface(font, TypefaceStyle.Normal);
                ReplyTextView.Visibility = ViewStates.Visible; 

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }


}