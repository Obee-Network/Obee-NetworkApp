using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Android.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Bumptech.Glide;
using Java.Util;
using ObeeNetwork.Helpers.CacheLoaders;
using ObeeNetwork.Helpers.Fonts;
using ObeeNetwork.Helpers.Utils;
using ObeeNetworkClient.Classes.Global;

namespace ObeeNetwork.Activities.CommonThings.Adapters
{
    public class CommonThingsAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<CommonThingsAdapterClickEventArgs> ItemClick;
        public event EventHandler<CommonThingsAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;

        public ObservableCollection<CommonThingsObject> CommonThingsList = new ObservableCollection<CommonThingsObject>();

        public CommonThingsAdapter(Activity context)
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

        public override int ItemCount => CommonThingsList?.Count ?? 0;


        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_HPage_view
                var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Style_CommonThingsView, parent, false);
                var vh = new CommonThingsAdapterViewHolder(itemView, Click, LongClick);
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
                if (viewHolder is CommonThingsAdapterViewHolder holder)
                {
                    var item = CommonThingsList[position];
                    if (item != null)
                    {
                        GlideImageLoader.LoadImage(ActivityContext, item.UserData.Avatar, holder.Image, ImageStyle.FitCenter, ImagePlaceholders.Drawable);

                        holder.Name.Text = ObeeNetworkTools.GetNameFinal(item.UserData);
                        holder.CountCommon.Text = item.CommonThings + " " + ActivityContext.GetText(Resource.String.Lbl_ThingInCommon);
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
                    if (holder is CommonThingsAdapterViewHolder viewHolder)
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
        public CommonThingsObject GetItem(int position)
        {
            return CommonThingsList[position];
        }

        public override long GetItemId(int position)
        {
            try
            {
                return int.Parse(CommonThingsList[position].UserId);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return 0;
            }
        }
		
		protected override void OnPause()
        {
            try
            {
                base.OnPause();
                AddOrRemoveEvent(false);

                MAdView?.Pause();
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

        private void Click(CommonThingsAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void LongClick(CommonThingsAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }


        public RequestBuilder GetPreloadRequestBuilder(Java.Lang.Object p0)
        {
            return GlideImageLoader.GetPreLoadRequestBuilder(ActivityContext, p0.ToString(), ImageStyle.CircleCrop);
        }

        System.Collections.IList ListPreloader.IPreloadModelProvider.GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = CommonThingsList[p0];
                if (item == null)
                    return d;
                else
                {
                    if (!string.IsNullOrEmpty(item.UserData.Avatar))
                        d.Add(item.UserData.Avatar);

                    return d;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Collections.SingletonList(p0);
            }
        }
    }

    public class CommonThingsAdapterViewHolder : RecyclerView.ViewHolder
    {
        public CommonThingsAdapterViewHolder(View itemView, Action<CommonThingsAdapterClickEventArgs> clickListener, Action<CommonThingsAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                Image = MainView.FindViewById<ImageView>(Resource.Id.image);
                Name = MainView.FindViewById<TextView>(Resource.Id.name);
                Icon = MainView.FindViewById<TextView>(Resource.Id.icon);
                CountCommon = MainView.FindViewById<TextView>(Resource.Id.countCommon);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, Icon, FontAwesomeIcon.AmericanSignLanguageInterpreting);

                //Event  
                itemView.Click += (sender, e) => clickListener(new CommonThingsAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new CommonThingsAdapterClickEventArgs { View = itemView, Position = AdapterPosition });

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #region Variables Basic

        public View MainView { get; }

        public ImageView Image { get; private set; }
        public TextView Name { get; private set; }
        public TextView Icon { get; private set; } 
        public TextView CountCommon { get; private set; }

        #endregion
    }

    public class CommonThingsAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}