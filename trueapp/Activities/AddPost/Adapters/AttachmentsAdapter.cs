using System;
using System.Collections.ObjectModel;
using System.Linq;
using Android.App;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using AT.Markushi.UI;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Bumptech.Glide.Request.Target;
using Bumptech.Glide.Request.Transition;
using Java.IO;
using ObeeNetwork.Helpers.CacheLoaders;
using ObeeNetwork.Helpers.Utils;
using ObeeNetworkClient.Classes.Posts;
using Console = System.Console;
using Exception = System.Exception;
using Object = Java.Lang.Object;

namespace ObeeNetwork.Activities.AddPost.Adapters
{ 
    public class AttachmentsAdapter : RecyclerView.Adapter
    {
        public event EventHandler<AttachmentsAdapterClickEventArgs> DeleteItemClick;
        public event EventHandler<AttachmentsAdapterClickEventArgs> ItemClick;
        public event EventHandler<AttachmentsAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<Attachments> AttachmentList = new ObservableCollection<Attachments>();
        public AttachmentsAdapter(Activity context)
        {
            try
            {
                ActivityContext = context;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override int ItemCount => AttachmentList?.Count ?? 0;
         
        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_Attachment_View
                var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Style_Attachment_View, parent, false);
                var vh = new AttachmentsAdapterViewHolder(itemView, DeleteClick, Click, LongClick);
                return vh;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return null;
            }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            { 
                if (viewHolder is AttachmentsAdapterViewHolder holder)
                {
                    var item = AttachmentList[position];
                    if (item != null)
                    {
                        if (item.TypeAttachment == "Default")
                            Glide.With(ActivityContext).Load(Resource.Drawable.addImage).Apply(new RequestOptions().Placeholder(Resource.Drawable.ImagePlacholder)).Into(holder.Image);
                        else
                        {
                            if (item.FileSimple.Contains("http") || item.FileSimple == "Image_File" || item.FileSimple == "Audio_File")
                            {
                                GlideImageLoader.LoadImage(ActivityContext, item.FileSimple, holder.Image, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                            }
                            else
                            {
                                if (item.TypeAttachment == "postVideo" && string.IsNullOrEmpty(item.FileSimple) && !new File(item.FileSimple).Exists())
                                {
                                    File file2 = new File(item.FileUrl);
                                    var photoUri = FileProvider.GetUriForFile(ActivityContext, ActivityContext.PackageName + ".fileprovider", file2);

                                    Glide.With(ActivityContext)
                                        .AsBitmap()
                                        .Placeholder(Resource.Drawable.blackdefault)
                                        .Error(Resource.Drawable.blackdefault)
                                        .Load(photoUri) // or URI/path
                                        .Into(new MySimpleTarget(this, holder, position));  //image view to set thumbnail to
                                }
                                else
                                {
                                    Glide.With(ActivityContext).Load(new File(item.FileUrl)).Apply(new RequestOptions().Placeholder(Resource.Drawable.Blue_Color).Error(Resource.Drawable.Blue_Color)).Into(holder.Image);
                                }
                            }
                        }
                         
                        switch (item.TypeAttachment)
                        {
                            case "postVideo":
                                holder.AttachType.Visibility = ViewStates.Visible;
                                break;
                            case "postPhotos":
                            case "postMusic":
                            case "postFile":
                                holder.AttachType.Visibility = ViewStates.Gone;
                                break;
                            case "Default":
                                holder.ImageDelete.Visibility = ViewStates.Invisible;
                                break;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private class MySimpleTarget : CustomTarget
        {
            private readonly AttachmentsAdapter MAdapter;
            private readonly AttachmentsAdapterViewHolder ViewHolder;
            private readonly int Position;
            public MySimpleTarget(AttachmentsAdapter adapter, AttachmentsAdapterViewHolder viewHolder, int position)
            {
                try
                {
                    MAdapter = adapter;
                    ViewHolder = viewHolder;
                    Position = position;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            public override void OnResourceReady(Object resource, ITransition transition)
            {
                try
                {
                    if (MAdapter.AttachmentList?.Count > 0)
                    {
                        var item = MAdapter.AttachmentList[Position];
                        if (item != null && string.IsNullOrEmpty(item.Thumb?.FileUrl))
                        {
                            var fileName = item.FileUrl.Split('/').Last();
                            var fileNameWithoutExtension = fileName.Split('.').First();

                            var pathImage = Methods.Path.FolderDcimImage + "/" + fileNameWithoutExtension + ".png";

                            var videoImage = Methods.MultiMedia.GetMediaFrom_Gallery(Methods.Path.FolderDcimImage, fileNameWithoutExtension + ".png");
                            if (videoImage == "File Dont Exists")
                            {
                                if (resource is Bitmap bitmap)
                                {
                                    Methods.MultiMedia.Export_Bitmap_As_Image(bitmap, fileNameWithoutExtension, Methods.Path.FolderDcimImage);
                                     
                                    File file2 = new File(pathImage);
                                    var photoUri = FileProvider.GetUriForFile(MAdapter.ActivityContext, MAdapter.ActivityContext.PackageName + ".fileprovider", file2);

                                    Glide.With(MAdapter.ActivityContext).Load(photoUri).Apply(new RequestOptions()).Into(ViewHolder.Image);

                                    item.Thumb = new Attachments.VideoThumb()
                                    {
                                        FileUrl = photoUri.ToString()
                                    };
                                } 
                            }
                            else
                            { 
                                File file2 = new File(pathImage);
                                var photoUri = FileProvider.GetUriForFile(MAdapter.ActivityContext, MAdapter.ActivityContext.PackageName + ".fileprovider", file2);

                                Glide.With(MAdapter.ActivityContext).Load(photoUri).Apply(new RequestOptions()).Into(ViewHolder.Image);
                             
                                item.Thumb = new Attachments.VideoThumb()
                                {
                                    FileUrl = photoUri.ToString()
                                };
                            }
                        } 
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            public override void OnLoadCleared(Drawable p0) { }
        }

        public override void OnViewRecycled(Object holder)
        {
            try
            {
                if (holder != null)
                {
                    if (holder is AttachmentsAdapterViewHolder viewHolder)
                        Glide.With(ActivityContext).Clear(viewHolder.Image); 
                }
                base.OnViewRecycled(holder);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        // Function 
        public void Add(Attachments item)
        {
            try
            {
                AttachmentList.Add(item);
                NotifyItemInserted(AttachmentList.IndexOf(AttachmentList.Last()));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public void Remove(Attachments item)
        {
            try
            {
                var index = AttachmentList.IndexOf(AttachmentList.FirstOrDefault(a => a.Id == item.Id));
                if (index != -1)
                {
                    AttachmentList.Remove(item);
                    NotifyItemRemoved(index);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }


        public void RemoveAll()
        {
            try
            {
                AttachmentList.Clear();
                NotifyDataSetChanged();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
        
        public Attachments GetItem(int position)
        {
            return AttachmentList[position];
        }

        public override long GetItemId(int position)
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

        private void DeleteClick(AttachmentsAdapterClickEventArgs args)
        {
            DeleteItemClick?.Invoke(this, args);
        }

        private void Click(AttachmentsAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void LongClick(AttachmentsAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }
    }

    public class AttachmentsAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; set; }
         
        public ImageView AttachType { get; private set; }
        public ImageView Image { get; private set; }
        public CircleButton ImageDelete { get; private set; }

        #endregion

        public AttachmentsAdapterViewHolder(View itemView, Action<AttachmentsAdapterClickEventArgs> clickDeleteListener,Action<AttachmentsAdapterClickEventArgs> clickListener,Action<AttachmentsAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                //Get values         
                AttachType = (ImageView) MainView.FindViewById(Resource.Id.AttachType);
                Image = (ImageView) MainView.FindViewById(Resource.Id.Image);

                ImageDelete = MainView.FindViewById<CircleButton>(Resource.Id.ImageCircle);

                //Create an Event
                ImageDelete.Click += (sender, e) => clickDeleteListener(new AttachmentsAdapterClickEventArgs{View = itemView, Position = AdapterPosition});
                itemView.Click += (sender, e) => clickListener(new AttachmentsAdapterClickEventArgs{View = itemView, Position = AdapterPosition});
                itemView.LongClick += (sender, e) => longClickListener(new AttachmentsAdapterClickEventArgs{View = itemView, Position = AdapterPosition});
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        } 
    }

    public class AttachmentsAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}