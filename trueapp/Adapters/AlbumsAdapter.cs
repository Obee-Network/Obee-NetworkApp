using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Android.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Request;
using Java.Util;
using ObeeNetwork.Helpers.CacheLoaders;
using ObeeNetwork.Helpers.Utils;
using ObeeNetworkClient.Classes.Posts;
using IList = System.Collections.IList;

namespace ObeeNetwork.Activities.Album.Adapters
{
    public class AlbumsAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<AlbumsAdapterClickEventArgs> OnItemClick;
        public event EventHandler<AlbumsAdapterClickEventArgs> OnItemLongClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<PostDataObject> AlbumList = new ObservableCollection<PostDataObject>();

        public AlbumsAdapter(Activity activity)
        {
            try
            {
                ActivityContext = activity;
                HasStableIds = true; 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_HorizontalSound
                View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Style_HAlbumsView, parent, false);
                var vh = new AlbumsAdapterViewHolder(itemView, Click, LongClick);
                return vh;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {
                if (viewHolder is AlbumsAdapterViewHolder holder)
                {
                    var item = AlbumList[position];
                    if (item != null)
                    {
                        GlideImageLoader.LoadImage(ActivityContext, item.PhotoAlbum[0]?.Image, holder.Image, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                        holder.TxtTitle.Text = Methods.FunString.DecodeString(item.AlbumName); 
                        holder.TxtCounter.Text = item.PhotoAlbum.Count.ToString();
                    }
                }
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
                    if (holder is AlbumsAdapterViewHolder viewHolder)
                        Glide.With(ActivityContext).Clear(viewHolder.Image);
                }
                base.OnViewRecycled(holder);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        public override int ItemCount => AlbumList?.Count ?? 0;
 
        public PostDataObject GetItem(int position)
        {
            return AlbumList[position];
        }

        public override long GetItemId(int position)
        {
            try
            {
                return int.Parse(AlbumList[position].Id);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 0;
            }
        }

        public override int GetItemViewType(int position)
        {
            try
            {
                return position;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 0;
            }
        }

        void Click(AlbumsAdapterClickEventArgs args) => OnItemClick?.Invoke(this, args);
        void LongClick(AlbumsAdapterClickEventArgs args) => OnItemLongClick?.Invoke(this, args);

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = AlbumList[p0];

                if (item == null)
                    return Collections.SingletonList(p0);

                if (item.PhotoAlbum[0]?.Image != "")  
                {
                    d.Add(item.PhotoAlbum[0]?.Image);
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

        public RequestBuilder GetPreloadRequestBuilder(Java.Lang.Object p0)
        {
            return Glide.With(ActivityContext).Load(p0.ToString())
                .Apply(new RequestOptions().CircleCrop().SetDiskCacheStrategy(DiskCacheStrategy.All));
        } 
    }

    public class AlbumsAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; set; }
        public ImageView Image { get; private set; }
        public TextView TxtTitle { get; private set; }
        public TextView TxtCounter { get; private set; }

        #endregion

        public AlbumsAdapterViewHolder(View itemView, Action<AlbumsAdapterClickEventArgs> clickListener, Action<AlbumsAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                //Get values
                Image = (ImageView)MainView.FindViewById(Resource.Id.image);
                TxtTitle = MainView.FindViewById<TextView>(Resource.Id.name);
                TxtCounter = MainView.FindViewById<TextView>(Resource.Id.counter);

                //Event
                itemView.Click += (sender, e) => clickListener(new AlbumsAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new AlbumsAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    public class AlbumsAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}