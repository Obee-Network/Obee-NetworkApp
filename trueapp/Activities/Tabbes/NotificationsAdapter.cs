using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Android.App;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Bumptech.Glide;
using Java.Util;
using ObeeNetwork.Helpers.CacheLoaders;
using ObeeNetwork.Helpers.Fonts;
using ObeeNetwork.Helpers.Utils;
using AmulyaKhare.TextDrawableLib;
using Bumptech.Glide.Request;
using IList = System.Collections.IList;
using Notification = ObeeNetworkClient.Classes.Global.Notification;

namespace ObeeNetwork.Activities.Tabbes.Adapters
{
    public class NotificationsAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<NotificationsAdapterClickEventArgs> ItemClick;
        public event EventHandler<NotificationsAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext; 
        public ObservableCollection<Notification> NotificationsList = new ObservableCollection<Notification>();

        public NotificationsAdapter(Activity context)
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

        public override int ItemCount => NotificationsList?.Count ?? 0;
 
        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Notifications_view
                var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Style_Notifications_view, parent, false);
                var vh = new NotificationsAdapterViewHolder(itemView, Click, LongClick);
                return vh;
            }
            catch (Exception exception)
            {
                Console.WriteLine("EX:ALLEN Notifications >> " + exception);
                return null;
            }
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {
               
                if (viewHolder is NotificationsAdapterViewHolder holder)
                {
                    var item = NotificationsList[position];
                    if (item != null)
                    { 
                        Initialize(holder, item);

                        if (AppSettings.FlowDirectionRightToLeft)
                        {
                            holder.LayoutMain.LayoutDirection = LayoutDirection.Rtl;
                            holder.ImageUser.LayoutDirection = LayoutDirection.Rtl;
                            holder.Image.LayoutDirection = LayoutDirection.Rtl;
                            holder.IconNotfy.LayoutDirection = LayoutDirection.Rtl;
                            holder.UserNameNotfy.LayoutDirection = LayoutDirection.Rtl;
                            holder.TextNotfy.LayoutDirection = LayoutDirection.Rtl;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void Initialize(NotificationsAdapterViewHolder holder, Notification notify)
        {
            try
            {
                if (notify.Type == "memory")
                {
                    Glide.With(ActivityContext).Load(Resource.Mipmap.icon).Apply(new RequestOptions().CircleCrop()).Into(holder.ImageUser);
                    holder.UserNameNotfy.Text = AppSettings.ApplicationName;
                }
                else
                {
                    GlideImageLoader.LoadImage(ActivityContext, notify.Notifier?.Avatar, holder.ImageUser, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                    holder.UserNameNotfy.Text = ObeeNetworkTools.GetNameFinal(notify.Notifier);
                }

                AddIconFonts(holder, notify.Type, notify.Icon);

                var drawable = TextDrawable.InvokeBuilder().BeginConfig().FontSize(30).EndConfig().BuildRound("", Color.ParseColor(GetColorFonts(notify.Type, notify.Icon)));
                holder.Image.SetImageDrawable(drawable);
                
                if (notify.Type == "share_post" || notify.Type == "shared_your_post")
                    holder.TextNotfy.Text = ActivityContext.GetText(Resource.String.Lbl_sharedYourPost);
                else
                    holder.TextNotfy.Text = notify.TypeText;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void AddIconFonts(NotificationsAdapterViewHolder holder, string type, string icon)
        {
            try
            {
                if (type == "following")
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotfy, IonIconsFonts.PersonAdd);
                    return;
                }

                if (type == "memory")
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotfy, IonIconsFonts.AndroidTime);
                    return;
                }

                if (type == "comment" || type == "comment_reply" || type == "also_replied")
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotfy, IonIconsFonts.IosChatboxes);
                    return;
                }

                if (type == "liked_post" || type == "liked_comment" || type == "liked_reply_comment" ||
                    type == "liked_page")
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotfy, IonIconsFonts.Thumbsup);
                    return;
                }

                if (type == "wondered_post" || type == "wondered_comment" || type == "wondered_reply_comment" ||
                    type == "exclamation-circle")
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotfy, IonIconsFonts.Information);
                    return;
                }

                if (type == "comment_mention" || type == "comment_reply_mention")
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotfy, IonIconsFonts.Pricetag);
                    return;
                }

                if (type == "post_mention")
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotfy, IonIconsFonts.At);
                    return;
                }

                if (type == "share_post" || type == "shared_your_post")
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotfy, IonIconsFonts.AndroidShareAlt);
                    return;
                }

                if (type == "profile_wall_post")
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotfy, IonIconsFonts.Image);
                    return;
                }

                if (type == "visited_profile")
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotfy, IonIconsFonts.Eye);
                    return;
                }

                if (type == "joined_group" || type == "accepted_invite" || type == "accepted_request" ||
                    type == "accepted_join_request")
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotfy, IonIconsFonts.Checkmark);
                    return;
                }

                if (type == "invited_page")
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotfy, IonIconsFonts.Flag);
                    return;
                }

                if (type == "added_you_to_group")
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotfy, IonIconsFonts.AndroidAdd);
                    return;
                }

                if (type == "requested_to_join_group")
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotfy, IonIconsFonts.IosTimerOutline);
                    return;
                }

                if (type == "thumbs-down")
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotfy, IonIconsFonts.Thumbsdown);
                    return;
                }

                if (type == "going_event")
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotfy, IonIconsFonts.Calendar);
                    return;
                }

                if (type == "viewed_story")
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotfy, IonIconsFonts.Aperture);
                    return;
                }

                if (type == "reaction")
                {
                    switch (icon)
                    {
                        case "like":
                            FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotfy, IonIconsFonts.Thumbsup);
                            break;
                        case "haha":
                            FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotfy, IonIconsFonts.Happy);
                            break;
                        case "love":
                            FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotfy, IonIconsFonts.Heart);
                            break;
                        case "wow":
                            FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotfy, IonIconsFonts.Information);
                            break;
                        case "sad":
                            FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotfy, IonIconsFonts.SadOutline);
                            break;
                        case "angry":
                            FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotfy, IonIconsFonts.SocialFreebsdDevil);
                            break;
                        default:
                            FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotfy, IonIconsFonts.AndroidNotifications);
                            break;
                    }
                }
                else
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotfy, IonIconsFonts.AndroidNotifications);
                } 
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception); 
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconNotfy, IonIconsFonts.AndroidNotifications);
            }
        }

        private string GetColorFonts(string type, string icon)
        {
            try
            {
                string iconColorFo;

                if (type == "following")
                {
                    iconColorFo = "#F50057";
                    return iconColorFo;
                }

                if (type == "memory")
                {
                    iconColorFo = "#00695C";
                    return iconColorFo;
                }

                if (type == "comment" || type == "comment_reply" || type == "also_replied")
                {
                    iconColorFo = AppSettings.MainColor;
                    return iconColorFo;
                }

                if (type == "liked_post" || type == "liked_comment" || type == "liked_reply_comment")
                {
                    iconColorFo = AppSettings.MainColor;
                    return iconColorFo;
                }

                if (type == "wondered_post" || type == "wondered_comment" || type == "wondered_reply_comment" ||
                    type == "exclamation-circle")
                {
                    iconColorFo = "#FFA500";
                    return iconColorFo;
                }

                if (type == "comment_mention" || type == "comment_reply_mention")
                {
                    iconColorFo = "#B20000";

                    return iconColorFo;
                }

                if (type == "post_mention")
                {
                    iconColorFo = "#B20000";
                    return iconColorFo;
                }

                if (type == "share_post")
                {
                    iconColorFo = "#2F2FFF";
                    return iconColorFo;
                }

                if (type == "profile_wall_post")
                {
                    iconColorFo = "#006064";
                    return iconColorFo;
                }

                if (type == "visited_profile")
                {
                    iconColorFo = "#328432";
                    return iconColorFo;
                }

                if (type == "liked_page")
                {
                    iconColorFo = "#2F2FFF";
                    return iconColorFo;
                }

                if (type == "joined_group" || type == "accepted_invite" || type == "accepted_request")
                {
                    iconColorFo = "#2F2FFF";
                    return iconColorFo;
                }

                if (type == "invited_page")
                {
                    iconColorFo = "#B20000";
                    return iconColorFo;
                }

                if (type == "accepted_join_request")
                {
                    iconColorFo = "#2F2FFF";
                    return iconColorFo;
                }

                if (type == "added_you_to_group")
                {
                    iconColorFo = "#311B92";
                    return iconColorFo;
                }

                if (type == "requested_to_join_group")
                {
                    iconColorFo = AppSettings.MainColor;
                    return iconColorFo;
                }

                if (type == "thumbs-down")
                {
                    iconColorFo = AppSettings.MainColor;
                    return iconColorFo;
                }

                if (type == "going_event")
                {
                    iconColorFo = "#33691E";
                    return iconColorFo;
                }
                
                if (type == "viewed_story")
                {
                    iconColorFo = "#D81B60";
                    return iconColorFo;
                }

                if (type == "reaction")
                {
                    if (icon == "like")
                    {
                        iconColorFo = AppSettings.MainColor;
                        return iconColorFo;
                    }
                    else if (icon == "haha")
                    {
                        iconColorFo = "#0277BD";
                        return iconColorFo; 
                    }
                    else if (icon == "love")
                    {
                        iconColorFo = "#d50000";
                        return iconColorFo; 
                    }
                    else if (icon == "wow")
                    {
                        iconColorFo = "#FBC02D";
                        return iconColorFo; 
                    }
                    else if (icon == "sad")
                    {
                        iconColorFo = "#455A64";
                        return iconColorFo; 
                    }
                    else if (icon == "angry")
                    {
                        iconColorFo = "#BF360C";
                        return iconColorFo; 
                    }
                    else
                    {
                        iconColorFo = "#424242";
                        return iconColorFo;
                    }
                }
                
                iconColorFo = "#424242";
                return iconColorFo;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return "#424242";
            }
        }
        public override void OnViewRecycled(Java.Lang.Object holder)
        {
            try
            {
                if (holder != null)
                {
                    if (holder is NotificationsAdapterViewHolder viewHolder)
                    {
                        Glide.With(ActivityContext).Clear(viewHolder.ImageUser);
                    }
                }
                base.OnViewRecycled(holder);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        public Notification GetItem(int position)
        {
            return NotificationsList[position];
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

        private void Click(NotificationsAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void LongClick(NotificationsAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }


        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = NotificationsList[p0];
                if (item == null)
                    return d;
                else
                {
                    if (!string.IsNullOrEmpty(item.Notifier.Avatar))
                        d.Add(item.Notifier.Avatar);

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

    public class NotificationsAdapterViewHolder : RecyclerView.ViewHolder
    {
        public NotificationsAdapterViewHolder(View itemView, Action<NotificationsAdapterClickEventArgs> clickListener,Action<NotificationsAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                LayoutMain = (LinearLayout) MainView.FindViewById(Resource.Id.main);

                //Get values
                ImageUser = (ImageView) MainView.FindViewById(Resource.Id.ImageUser);
                Image = MainView.FindViewById<ImageView>(Resource.Id.image_view);
                IconNotfy = (TextView) MainView.FindViewById(Resource.Id.IconNotifications);
                UserNameNotfy = (TextView) MainView.FindViewById(Resource.Id.NotificationsName);
                TextNotfy = (TextView) MainView.FindViewById(Resource.Id.NotificationsText);

                //Create an Event
                itemView.Click += (sender, e) => clickListener(new NotificationsAdapterClickEventArgs{View = itemView, Position = AdapterPosition});
                itemView.LongClick += (sender, e) => longClickListener(new NotificationsAdapterClickEventArgs{View = itemView, Position = AdapterPosition});
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #region Variables Basic

        public View MainView { get; }

        

        public LinearLayout LayoutMain;
        public ImageView ImageUser { get; set; }
        public ImageView Image { get; set; }
        public TextView IconNotfy { get; set; }
        public TextView UserNameNotfy { get; set; }
        public TextView TextNotfy { get; set; }

        #endregion
    }

    public class NotificationsAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}