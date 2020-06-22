using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Android.App;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Request;
using Java.IO;
using Java.Util;
using ObeeNetwork.Helpers.CacheLoaders;
using ObeeNetworkClient.Classes.Album;
using Console = System.Console;
using IList = System.Collections.IList;
using Object = Java.Lang.Object;

namespace ObeeNetwork.Activities.Album.Adapters
{
    public class PhotosAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<PhotosAdapterClickEventArgs> ItemClick;
        public event EventHandler<PhotosAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext; 
        public ObservableCollection<PhotoAlbumObject> PhotosList = new ObservableCollection<PhotoAlbumObject>();

        public PhotosAdapter(Activity context)
        {
            try
            {
                ActivityContext = context;
                HasStableIds = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override int ItemCount => PhotosList?.Count ?? 0;
         
        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_ImageAlbum_view
                var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Style_ImageAlbum_view, parent, false);
                var vh = new PhotosAdapterViewHolder(itemView, Click, LongClick);
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
                if (viewHolder is PhotosAdapterViewHolder holder)
                {
                    var item = PhotosList[position];
                    if (item != null)
                    {
                        if (!string.IsNullOrEmpty(item.Image) && (item.Image.Contains("file://") || item.Image.Contains("content://") || item.Image.Contains("storage") || item.Image.Contains("/data/user/0/")))
                        {
                            File file2 = new File(item.Image);
                            var photoUri = FileProvider.GetUriForFile(ActivityContext, ActivityContext.PackageName + ".fileprovider", file2);
                            Glide.With(ActivityContext).Load(photoUri).Apply(new RequestOptions()).Into(holder.Image);
                        }
                        else
                        {
                            GlideImageLoader.LoadImage(ActivityContext, item.Image, holder.Image, ImageStyle.CenterCrop, ImagePlaceholders.Drawable); 
                        }
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
                    if (holder is PhotosAdapterViewHolder viewHolder)
                        Glide.With(ActivityContext).Clear(viewHolder.Image);
                }
                base.OnViewRecycled(holder);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        public PhotoAlbumObject GetItem(int position)
        {
            return PhotosList[position];
        }

        public override long GetItemId(int position)
        {
            try
            {
                return Convert.ToInt32(PhotosList[position].Id);
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

        private void Click(PhotosAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void LongClick(PhotosAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }


        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = PhotosList[p0];

                if (item == null)
                    return Collections.SingletonList(p0);

                if (item.Image != "")
                {
                    d.Add(item.Image);
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
            return Glide.With(ActivityContext).Load(p0.ToString()).Apply(new RequestOptions().CenterCrop().SetDiskCacheStrategy(DiskCacheStrategy.All));
        }
    }

    public class PhotosAdapterViewHolder : RecyclerView.ViewHolder
    {
        public PhotosAdapterViewHolder(View itemView, Action<PhotosAdapterClickEventArgs> clickListener,Action<PhotosAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;
                Image = MainView.FindViewById<ImageView>(Resource.Id.image);

                //Event
                itemView.Click += (sender, e) => clickListener(new PhotosAdapterClickEventArgs{View = itemView, Position = AdapterPosition});
                itemView.LongClick += (sender, e) => longClickListener(new PhotosAdapterClickEventArgs{View = itemView, Position = AdapterPosition});
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #region Variables Basic

        public View MainView { get; }

        public ImageView Image { get; private set; }

        #endregion
    }

    public class PhotosAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}