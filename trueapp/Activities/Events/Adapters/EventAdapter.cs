using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Android.App;
using Android.Support.V4.Text;
using Android.Support.V4.Widget;
using Bumptech.Glide;
using Java.Util;
using ObeeNetwork.Helpers.CacheLoaders;
using ObeeNetwork.Helpers.Model;
using ObeeNetwork.Helpers.Utils;
using ObeeNetworkClient.Classes.Event;
using IList = System.Collections.IList;
using Object = Java.Lang.Object;

namespace ObeeNetwork.Activities.Events.Adapters
{
    public class EventAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<EventAdapterClickEventArgs> ItemClick;
        public event EventHandler<EventAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext; 
        public ObservableCollection<EventDataObject> EventList = new ObservableCollection<EventDataObject>();

        public EventAdapter(Activity context)
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

        public override int ItemCount => EventList?.Count ?? 0;


        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_Event_Cell
                var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Style_Event_Cell, parent, false);
                var vh = new EventAdapterViewHolder(itemView, Click, LongClick);
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
                if (viewHolder is EventAdapterViewHolder holder)
                {
                    var item = EventList[position];
                    if (item != null) Initialize(holder, item);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void Initialize(EventAdapterViewHolder holder, EventDataObject item)
        {
            try
            {
                GlideImageLoader.LoadImage(ActivityContext, item.Cover, holder.Image, ImageStyle.RoundedCrop, ImagePlaceholders.Color);

                holder.TxtEventTitle.SetTextFuture(PrecomputedTextCompat.GetTextFuture(Methods.FunString.DecodeString(item.Name), holder.TxtEventTitleTextMetrics, null));
                holder.TxtEventDescription.SetTextFuture(PrecomputedTextCompat.GetTextFuture(Methods.FunString.DecodeString(item.Description), holder.TxtEventDescriptionTextMetrics, null));
                holder.TxtEventLocation.SetTextFuture(PrecomputedTextCompat.GetTextFuture(item.Location, holder.TxtEventLocationTextMetrics, null));
                holder.TxtEventTime.Text = item.EndDate; 

                item.IsOwner = item.UserData.UserId == UserDetails.UserId;
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
                    if (holder is EventAdapterViewHolder viewHolder)
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
        public EventDataObject GetItem(int position)
        {
            return EventList[position];
        }

        public override long GetItemId(int position)
        {
            try
            {
                return int.Parse(EventList[position].Id);
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

        private void Click(EventAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void LongClick(EventAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = EventList[p0];
                if (item == null)
                    return Collections.SingletonList(p0);

                if (!string.IsNullOrEmpty(item.Cover))
                    d.Add(item.Cover);
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
           return GlideImageLoader.GetPreLoadRequestBuilder(ActivityContext,p0.ToString(),ImageStyle.RoundedCrop);
        }
    }

    public class EventAdapterViewHolder : RecyclerView.ViewHolder
    {
        public EventAdapterViewHolder(View itemView, Action<EventAdapterClickEventArgs> clickListener,Action<EventAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;
                Image = itemView.FindViewById<ImageView>(Resource.Id.Image);
                TxtEventTitle = itemView.FindViewById<AppCompatTextView>(Resource.Id.event_titile);
                TxtEventDescription = itemView.FindViewById<AppCompatTextView>(Resource.Id.event_description);
                TxtEventTime = itemView.FindViewById<TextView>(Resource.Id.event_time);
                TxtEventLocation = itemView.FindViewById<AppCompatTextView>(Resource.Id.event_location);
                PostLinkLinearLayout = itemView.FindViewById<CardView>(Resource.Id.card_view);

                if (TxtEventTitle != null)
                    TxtEventTitleTextMetrics = TextViewCompat.GetTextMetricsParams(TxtEventTitle);
                if (TxtEventDescription != null)
                    TxtEventDescriptionTextMetrics = TextViewCompat.GetTextMetricsParams(TxtEventDescription);
                if (TxtEventLocation != null)
                    TxtEventLocationTextMetrics = TextViewCompat.GetTextMetricsParams(TxtEventLocation);

                //Event
                itemView.Click += (sender, e) => clickListener(new EventAdapterClickEventArgs{ View = itemView, Position = AdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new EventAdapterClickEventArgs{ View = itemView, Position = AdapterPosition });
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #region Variables Basic

        public View MainView { get; }
        public ImageView Image { get; private set; }
        public AppCompatTextView TxtEventTitle { get; private set; }
        public AppCompatTextView TxtEventDescription { get; private set; }
        public TextView TxtEventTime { get; private set; } 
        public AppCompatTextView TxtEventLocation { get; private set; }
        public CardView PostLinkLinearLayout { get; private set; }
        public PrecomputedTextCompat.Params TxtEventTitleTextMetrics { get; private set; }
        public PrecomputedTextCompat.Params TxtEventDescriptionTextMetrics { get; private set; }
        public PrecomputedTextCompat.Params TxtEventLocationTextMetrics { get; private set; }

        #endregion Variables Basic

    }

    public class EventAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}