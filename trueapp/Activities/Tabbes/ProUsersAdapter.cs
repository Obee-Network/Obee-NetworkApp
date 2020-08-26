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
using ObeeNetworkClient.Classes.Global;
using IList = System.Collections.IList;

namespace ObeeNetwork.Activities.Tabbes.Adapters
{
    public class ProUsersAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        private readonly Activity ActivityContext; 
        public ObservableCollection<UserDataObject> MProUsersList = new ObservableCollection<UserDataObject>();

        public ProUsersAdapter(Activity context)
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

        public override int ItemCount => MProUsersList?.Count ?? 0;
 
        public event EventHandler<ProUsersAdapterClickEventArgs> ItemClick;
        public event EventHandler<ProUsersAdapterClickEventArgs> ItemLongClick;

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_Pro_Users_view
                var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Style_Pro_Users_view, parent, false);
                var vh = new ProUsersAdapterViewHolder(itemView, Click, LongClick);
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
                if (viewHolder is ProUsersAdapterViewHolder holder)
                {
                    var item = MProUsersList[position];
                    if (item != null)
                    {
                        GlideImageLoader.LoadImage(ActivityContext, item.Avatar, holder.UserImage, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                        if (item.Type == "Your")
                        {
                            //holder.ViewIcon.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor("#444444"));
                            holder.ViewIcon.SetBackgroundResource(Resource.Drawable.circlegradient5);
                            holder.ImageIcon.SetImageResource(Resource.Drawable.ic_add);
                            holder.Name.Text = ActivityContext.GetText(Resource.String.Lbl_AddMe);
                            holder.Name.Visibility = ViewStates.Visible;
                        }
                        else
                        {
                            switch (item.ProType)
                            {
                                //  STAR
                                case "1":
                                    holder.ViewIcon.SetBackgroundResource(Resource.Drawable.circlegradient6);
                                    holder.ImageIcon.SetImageResource(Resource.Drawable.ic_plan_1);
                                    break;
                                // HOT
                                case "2":
                                    holder.ViewIcon.SetBackgroundResource(Resource.Drawable.circlegradient4);
                                    holder.ImageIcon.SetImageResource(Resource.Drawable.ic_plan_2);
                                    break;
                                // ULTIMA
                                case "3":
                                    holder.ViewIcon.SetBackgroundResource(Resource.Drawable.circlegradient);
                                    holder.ImageIcon.SetImageResource(Resource.Drawable.ic_plan_3);
                                    break;
                                // VIP
                                case "4":
                                    holder.ViewIcon.SetBackgroundResource(Resource.Drawable.circlegradient2);
                                    holder.ImageIcon.SetImageResource(Resource.Drawable.ic_plan_4);
                                    break;
                                default:
                                    holder.ViewIcon.SetBackgroundResource(Resource.Drawable.circlegradient2);
                                    holder.ImageIcon.SetImageResource(Resource.Drawable.ic_plan_4); 
                                    break;
                            }
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
                    if (holder is ProUsersAdapterViewHolder viewHolder)
                    {
                        Glide.With(ActivityContext).Clear(viewHolder.UserImage);
                    }
                }
                base.OnViewRecycled(holder);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        public UserDataObject GetItem(int position)
        {
            return MProUsersList[position];
        }

        public override long GetItemId(int position)
        {
            try
            {
                return int.Parse(MProUsersList[position].UserId);
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

        private void Click(ProUsersAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void LongClick(ProUsersAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }


        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = MProUsersList[p0];
                if (item == null)
                    return d;
                else
                {
                    if (!string.IsNullOrEmpty(item.Avatar))
                        d.Add(item.Avatar);

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

    public class ProUsersAdapterViewHolder : RecyclerView.ViewHolder
    {
        public ProUsersAdapterViewHolder(View itemView, Action<ProUsersAdapterClickEventArgs> clickListener,
            Action<ProUsersAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                Name = MainView.FindViewById<TextView>(Resource.Id.name);
                ImageIcon = MainView.FindViewById<ImageView>(Resource.Id.ImageIcon);
                UserImage = MainView.FindViewById<ImageView>(Resource.Id.ImageUser);
                ViewIcon = MainView.FindViewById<View>(Resource.Id.viewIcon);
                Name.Visibility = ViewStates.Invisible;

                //Create an Event
                itemView.Click += (sender, e) => clickListener(new ProUsersAdapterClickEventArgs {View = itemView, Position = AdapterPosition});
                itemView.LongClick += (sender, e) => longClickListener(new ProUsersAdapterClickEventArgs {View = itemView, Position = AdapterPosition});
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #region Variables Basic

        public View MainView { get; }

        public ImageView UserImage { get; private set; }
        public ImageView ImageIcon { get; private set; }
        public View ViewIcon { get; private set; }
        public TextView Name { get; private set; }

        #endregion
    }

    public class ProUsersAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}