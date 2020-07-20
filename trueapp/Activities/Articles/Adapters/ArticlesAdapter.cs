using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Android.App;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Request;
using Java.Util;
using ObeeNetwork.Helpers.Controller;
using ObeeNetwork.Helpers.Utils;
using ObeeNetworkClient.Classes.Global;
using IList = System.Collections.IList;
using Object = Java.Lang.Object;

namespace ObeeNetwork.Activities.Articles.Adapters
{
    public class ArticlesAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        
        private readonly Activity ActivityContext;

        public ObservableCollection<ArticleDataObject> ArticlesList =
            new ObservableCollection<ArticleDataObject>();

        public ArticlesAdapter(Activity context)
        {
            try
            {
                HasStableIds = true;
                ActivityContext = context;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override int ItemCount => ArticlesList?.Count ?? 0;
 
        public event EventHandler<ArticlesAdapterClickEventArgs> ItemClick;
        public event EventHandler<ArticlesAdapterClickEventArgs> ItemLongClick;

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_Article_View
                var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Style_Article_View, parent, false);
                var vh = new ArticlesAdapterViewHolder(itemView, Click, LongClick);
                return vh;
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
               
                if (viewHolder is ArticlesAdapterViewHolder holder)
                {
                    var item = ArticlesList[position];
                    if (item != null) Initialize(holder, item);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void Initialize(ArticlesAdapterViewHolder holder, ArticleDataObject item)
        {
            try
            {

                var colorImage = Color.ParseColor(Methods.FunString.RandomColor());

                Glide.With(ActivityContext)
                    .Load(item.Thumbnail)
                    .Apply(RequestOptions.CenterCropTransform().Placeholder(new ColorDrawable(colorImage)).Fallback(new ColorDrawable(colorImage)).SetPriority(Priority.High))
                    .Into(holder.Image);
                 
                Glide.With(ActivityContext)
                    .Load(item.Author.Avatar)
                    .Apply(RequestOptions.CircleCropTransform())
                    .Into(holder.UserImageProfile);
              
                holder.Category.SetBackgroundColor(colorImage);

                CategoriesController cat = new CategoriesController();
                string id = item.CategoryLink.Split('/').Last();
                holder.Category.Text = cat.Get_Translate_Categories_Communities(id, item.CategoryName);
                 
                holder.Description.Text = Methods.FunString.DecodeString(item.Description);
                holder.Title.Text = Methods.FunString.DecodeString(item.Title);
                holder.Username.Text = ObeeNetworkTools.GetNameFinal(item.Author);
                holder.ViewMore.Text = ActivityContext.GetText(Resource.String.Lbl_ReadMore) + " >"; //READ MORE &gt; 
                holder.Time.Text = item.Posted;
                
                if (!holder.UserItem.HasOnClickListeners)
                    holder.UserItem.Click += (sender, args) =>
                    {
                        try
                        {
                            ObeeNetworkTools.OpenProfile(ActivityContext, item.Author.UserId, item.Author); 
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    };
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        public override void OnViewRecycled(Java.Lang.Object holder)
        {
            try
            {
                if (holder != null)
                {
                    if (holder is ArticlesAdapterViewHolder viewHolder)
                    {
                        Glide.With(ActivityContext).Clear(viewHolder.Image);
                        Glide.With(ActivityContext).Clear(viewHolder.UserImageProfile);
                    }
                }
                base.OnViewRecycled(holder);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        public ArticleDataObject GetItem(int position)
        {
            return ArticlesList[position];
        }

        public override long GetItemId(int position)
        {
            try
            {
                return int.Parse(ArticlesList[position].Id);
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
                return position;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return 0;
            }
        }

        private void Click(ArticlesAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void LongClick(ArticlesAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = ArticlesList[p0];

                if (item == null)
                   return Collections.SingletonList(p0);

                if (item.Thumbnail != "")
                {
                    d.Add(item.Thumbnail);
                    return d;
                }

                return d;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Collections.SingletonList(p0);
            }
        }

        public RequestBuilder GetPreloadRequestBuilder(Object p0)
        {
            return Glide.With(ActivityContext).Load(p0.ToString())
                .Apply(new RequestOptions().CircleCrop().SetDiskCacheStrategy(DiskCacheStrategy.All));
        }
    }

    public class ArticlesAdapterViewHolder : RecyclerView.ViewHolder
    {
        public ArticlesAdapterViewHolder(View itemView, Action<ArticlesAdapterClickEventArgs> clickListener,Action<ArticlesAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;


                UserItem = MainView.FindViewById<RelativeLayout>(Resource.Id.UserItem_Layout);

                Image = MainView.FindViewById<ImageView>(Resource.Id.Image);
                Category = MainView.FindViewById<TextView>(Resource.Id.Category);
                Title = MainView.FindViewById<TextView>(Resource.Id.Title);
                Description = MainView.FindViewById<TextView>(Resource.Id.description);
                UserImageProfile = MainView.FindViewById<ImageView>(Resource.Id.UserImageProfile);
                Username = MainView.FindViewById<TextView>(Resource.Id.Username);
                Time = MainView.FindViewById<TextView>(Resource.Id.card_dist);
                ViewMore = MainView.FindViewById<TextView>(Resource.Id.View_more);

                //Event
                itemView.Click += (sender, e) => clickListener(new ArticlesAdapterClickEventArgs
                    {View = itemView, Position = AdapterPosition});
                itemView.LongClick += (sender, e) => longClickListener(new ArticlesAdapterClickEventArgs
                    {View = itemView, Position = AdapterPosition});
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #region Variables Basic

        private View MainView { get; }

        public ImageView Image { get; private set; }
        public TextView Title { get; private set; }
        public TextView Description { get; private set; }
        public ImageView UserImageProfile { get; private set; }
        public TextView Category { get; private set; }
        public TextView Username { get; private set; }
        public TextView Time { get; private set; }
        public TextView ViewMore { get; private set; }
        public RelativeLayout UserItem { get; private set; }

        #endregion
    }

    public class ArticlesAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}