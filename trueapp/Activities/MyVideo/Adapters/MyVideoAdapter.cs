using Android.Support.V7.Widget;
using Android.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Android.App;
using Android.Widget;
using Bumptech.Glide;
using Java.Util;
using ObeeNetwork.Helpers.CacheLoaders;
using ObeeNetworkClient.Classes.Posts;
using IList = System.Collections.IList;

namespace ObeeNetwork.Activities.MyVideo.Adapters
{
    public class MyVideoAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<MyVideoAdapterClickEventArgs> ItemClick;
        public event EventHandler<MyVideoAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<PostDataObject> MyVideoList = new ObservableCollection<PostDataObject>();

        public MyVideoAdapter(Activity context)
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

        public override int ItemCount => MyVideoList?.Count ?? 0;

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_ImageAlbum_view
                var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Style_ImageAlbum_view, parent, false);
                var vh = new MyVideoAdapterViewHolder(itemView, Click, LongClick);
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
                if (viewHolder is MyVideoAdapterViewHolder holder)
                {
                    var item = MyVideoList[position];
                    if (item != null)
                    {
                        GlideImageLoader.LoadImage(ActivityContext, item.PostFileFull, holder.Image, ImageStyle.CenterCrop, ImagePlaceholders.Color);
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
        public override void OnViewRecycled(Java.Lang.Object holder)
        {
            try
            {
                if (holder != null)
                {
                    if (holder is MyVideoAdapterViewHolder viewHolder)
                    {
                        Glide.With(ActivityContext).Clear(viewHolder.Image);
                    }
                }
                base.OnViewRecycled(holder);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        public PostDataObject GetItem(int position)
        {
            return MyVideoList[position];
        }

        public override long GetItemId(int position)
        {
            try
            {
                return int.Parse(MyVideoList[position].Id);
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

        private void Click(MyVideoAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void LongClick(MyVideoAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }


        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = MyVideoList[p0];
                if (item == null)
                    return d;
                else
                {
                    if (!string.IsNullOrEmpty(item.PostFileFull))
                        d.Add(item.PostFileFull);

                    return d;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Collections.SingletonList(p0);
            }
        }

        public RequestBuilder GetPreloadRequestBuilder(Java.Lang.Object p0)
        {
            return GlideImageLoader.GetPreLoadRequestBuilder(ActivityContext, p0.ToString(), ImageStyle.CircleCrop);
        }
    }

    public class MyVideoAdapterViewHolder : RecyclerView.ViewHolder
    {
        public MyVideoAdapterViewHolder(View itemView, Action<MyVideoAdapterClickEventArgs> clickListener, Action<MyVideoAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                Image = MainView.FindViewById<ImageView>(Resource.Id.image);

                //Event
                itemView.Click += (sender, e) => clickListener(new MyVideoAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new MyVideoAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #region Variables Basic

        public View MainView { get; set; }

        public ImageView Image { get; private set; }

        #endregion
    }

    public class MyVideoAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}