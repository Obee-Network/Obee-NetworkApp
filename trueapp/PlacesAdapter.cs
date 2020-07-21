using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Android.App;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Google.Places;
using Java.Lang;
using AmulyaKhare.TextDrawableLib;
using Double = Java.Lang.Double;
using Exception = System.Exception;
using Uri = Android.Net.Uri;

namespace ObeeNetwork.PlacesAsync.Adapters
{
    public class MyPlace  
    { 
        public string Address { get; set; }
        public AddressComponents AddressComponents { get; set; }
        public IList<string> Attributions { get; set; }
        public string Id { get; set; }
        public LatLng LatLng { get; set; }
        public string Name { get; set; }
        public OpeningHours OpeningHours { get; set; }
        public string PhoneNumber { get; set; }
        public IList<PhotoMetadata> PhotoMetadatas { get; set; }
        public PlusCode PlusCode { get; set; }
        public Integer PriceLevel { get; set; }
        public Double Rating { get; set; }
        public Integer UserRatingsTotal { get; set; }
        public LatLngBounds Viewport { get; set; }
        public Uri WebsiteUri { get; set; }
    }
    
    public class PlacesAdapter : RecyclerView.Adapter 
    {

        //private readonly Activity ActivityContext;

        public ObservableCollection<MyPlace> PlacesList = new ObservableCollection<MyPlace>();

        public PlacesAdapter(Activity context)
        {
            try
            {
                HasStableIds = true;
                //ActivityContext = context;
                Console.WriteLine(context);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override int ItemCount => PlacesList?.Count ?? 0;
 
        public event EventHandler<PlacesAdapterClickEventArgs> ItemClick;
        public event EventHandler<PlacesAdapterClickEventArgs> ItemLongClick;

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_Article_View
                var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Style_PlacesView, parent, false);
                var vh = new PlacesAdapterViewHolder(itemView, Click, LongClick);
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
                if (viewHolder is PlacesAdapterViewHolder holder)
                {
                    var item = PlacesList[position];
                    if (item != null)
                    {
                        var drawable = TextDrawable.InvokeBuilder().BeginConfig().FontSize(36).EndConfig().BuildRound(item.Name.Substring(0, 2), Color.ParseColor(AppSettings.MainColor));
                        holder.Image.SetImageDrawable(drawable);
                         
                        holder.Title.Text = item.Name;  
                        holder.Description.Text = item.Address; 
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        } 

        public MyPlace GetItem(int position)
        {
            return PlacesList[position];
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

        private void Click(PlacesAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void LongClick(PlacesAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }
         
    }

    public class PlacesAdapterViewHolder : RecyclerView.ViewHolder
    {
        public PlacesAdapterViewHolder(View itemView, Action<PlacesAdapterClickEventArgs> clickListener,Action<PlacesAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                Image = MainView.FindViewById<ImageView>(Resource.Id.card_pro_pic);
                Title = MainView.FindViewById<TextView>(Resource.Id.card_name);
                Description = MainView.FindViewById<TextView>(Resource.Id.card_dist); 

                //Event
                itemView.Click += (sender, e) => clickListener(new PlacesAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new PlacesAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
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

        #endregion
    }

    public class PlacesAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}