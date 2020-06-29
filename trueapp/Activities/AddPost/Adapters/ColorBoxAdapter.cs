using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Android.App;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using Bumptech.Glide;
using ObeeNetwork.Helpers.CacheLoaders;
using ObeeNetwork.Helpers.Utils;
using ObeeNetworkClient.Classes.Global;

namespace ObeeNetwork.Activities.AddPost.Adapters
{ 
    public class ColorBoxAdapter : RecyclerView.Adapter
    {
        public event EventHandler<ColorBoxAdapterClickEventArgs> ItemClick;

        private readonly Activity ActivityContext;
        public readonly ObservableCollection<PostColorsObject> ColorsList = new ObservableCollection<PostColorsObject>();

        public ColorBoxAdapter(Activity context, RecyclerView recycler)
        {
            try
            {
                ActivityContext = context;
                var mainRecyclerView = recycler;
                mainRecyclerView.SetLayoutManager(new LinearLayoutManager(ActivityContext, LinearLayoutManager.Horizontal, false));

                LayoutAnimationController controller = AnimationUtils.LoadLayoutAnimation(context, Resource.Animation.layout_animation_slide_right);
                mainRecyclerView.LayoutAnimation = controller;

                if (ListUtils.SettingsSiteList?.PostColors != null && ListUtils.SettingsSiteList?.PostColors?.PostColorsList != null && !ListUtils.SettingsSiteList.PostColors.Value.PostColorsList.ContainsKey("white"))
                    ListUtils.SettingsSiteList?.PostColors?.PostColorsList.Add("white", new PostColorsObject { Color1 = "#ffffff", Color2 = "#efefef", TextColor = "#444444" });

                if (ListUtils.SettingsSiteList?.PostColors != null && ListUtils.SettingsSiteList?.PostColors?.PostColorsList != null)
                    ColorsList = new ObservableCollection<PostColorsObject>(ListUtils.SettingsSiteList?.PostColors?.PostColorsList.Values);

                mainRecyclerView.SetAdapter(this);
                NotifyDataSetChanged();
                mainRecyclerView.ScheduleLayoutAnimation();

                if (ColorsList.Count == 0)
                    mainRecyclerView.Visibility = ViewStates.Invisible;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        public override int ItemCount => ColorsList?.Count ?? 0;



        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_Gif_View
                var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Style_ColorBox, parent, false);
                var vh = new ColorBoxAdapterViewHolder(itemView, Click);
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
                var item = ColorsList[position];
                if (viewHolder is ColorBoxAdapterViewHolder holder)
                {
                    // var getColorObject = ListUtils.SettingsSiteList?.PostColors.FirstOrDefault(a => a.Key == item.ColorId);
                    if (!string.IsNullOrEmpty(item.Image))
                    {
                        GlideImageLoader.LoadImage(ActivityContext, item.Image, holder.ColoredImage, ImageStyle.RoundedCrop, ImagePlaceholders.Color);
                    }
                    else
                    {
                        var colorsList = new List<int>();

                        if (!string.IsNullOrEmpty(item.Color1))
                            colorsList.Add(Color.ParseColor(item.Color1));

                        if (!string.IsNullOrEmpty(item.Color2))
                            colorsList.Add(Color.ParseColor(item.Color2));

                        GradientDrawable gd = new GradientDrawable(GradientDrawable.Orientation.TopBottom, colorsList.ToArray());
                        gd.SetCornerRadius(0f);
                        holder.ColoredImage.Background = (gd);
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
                    if (holder is ColorBoxAdapterViewHolder viewHolder)
                        Glide.With(ActivityContext).Clear(viewHolder.ColoredImage);
                }
                base.OnViewRecycled(holder);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public PostColorsObject GetItem(int position)
        {
            return ColorsList[position];
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

        private void Click(ColorBoxAdapterClickEventArgs args)
        {
            try
            {
                ItemClick?.Invoke(this, args);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }

    }

    public class ColorBoxAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; set; }
        public ImageView ColoredImage { get; set; }
        #endregion

        public ColorBoxAdapterViewHolder(View itemView, Action<ColorBoxAdapterClickEventArgs> clickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                ColoredImage = (ImageView)MainView.FindViewById(Resource.Id.Image);
                ItemView.Click += (sender, e) => clickListener(new ColorBoxAdapterClickEventArgs { View = itemView, Position = AdapterPosition });

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


    }

    public class ColorBoxAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
        public string Text { get; set; }
        public EditText Input { get; set; }
    }
}