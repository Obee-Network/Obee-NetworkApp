using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AFollestad.MaterialDialogs;
using Android.App;
using Android.Content;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Java.IO;
using Java.Lang;
using Java.Util;
using Newtonsoft.Json;
using ObeeNetwork.Activities.NativePost.Extra;
using ObeeNetwork.Activities.Tabbes;
using ObeeNetwork.Helpers.CacheLoaders;
using ObeeNetwork.Helpers.Controller;
using ObeeNetwork.Helpers.Fonts;
using ObeeNetwork.Helpers.Utils;
using ObeeNetworkClient;
using ObeeNetworkClient.Classes.Jobs;
using ObeeNetworkClient.Requests;
using Console = System.Console;
using Exception = System.Exception;

namespace ObeeNetwork.Activities.Jobs.Adapters
{
    public class JobsAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        public event EventHandler<JobsAdapterClickEventArgs> ItemClick;
        public event EventHandler<JobsAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext; 
        public ObservableCollection<JobInfoObject> JobList = new ObservableCollection<JobInfoObject>();
        private JobInfoObject DataInfoObject;
        private string DialogType;

        public JobsAdapter(Activity context)
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

        public override int ItemCount => JobList?.Count ?? 0;


        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_JobView
                var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Style_JobView, parent, false);
                var vh = new JobsAdapterViewHolder(itemView, Click, LongClick);
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
                if (viewHolder is JobsAdapterViewHolder holder)
                {
                    var item = JobList[position];
                    if (item != null)
                    { 
                        if (item.Image.Contains("http"))
                        {
                            var image = item.Image.Replace(Client.WebsiteUrl + "/", "");
                            if (!image.Contains("http"))
                                item.Image = Client.WebsiteUrl + "/" + image;
                            else
                                item.Image = image;

                            GlideImageLoader.LoadImage(ActivityContext, item.Image, holder.Image, ImageStyle.FitCenter, ImagePlaceholders.Drawable);
                        }
                        else
                        {
                            File file2 = new File(item.Image);
                            var photoUri = FileProvider.GetUriForFile(ActivityContext, ActivityContext.PackageName + ".fileprovider", file2);
                            Glide.With(ActivityContext).Load(photoUri).Apply(new RequestOptions()).Into(holder.Image);
                        }
                       
                        holder.Title.Text = Methods.FunString.DecodeString(item.Title);

                        var (currency, currencyIcon) = ObeeNetworkTools.GetCurrency(item.Currency);
                        var categoryName = CategoriesController.ListCategoriesJob.FirstOrDefault(categories => categories.CategoriesId == item.Category)?.CategoriesName;
                        Console.WriteLine(currency);
                        if (string.IsNullOrEmpty(categoryName))
                            categoryName = Application.Context.GetText(Resource.String.Lbl_Unknown);

                        holder.Salary.Text = currencyIcon + " " + item.Minimum + " - " + currencyIcon + " " + item.Maximum + " . " + categoryName;

                        holder.Description.Text = Methods.FunString.SubStringCutOf(Methods.FunString.DecodeString(item.Description), 100);

                        if (item.IsOwner)
                        {
                            holder.IconMore.Visibility = ViewStates.Visible;
                            holder.Button.Text = ActivityContext.GetString(Resource.String.Lbl_show_applies) + " (" + item.ApplyCount + ")";
                            holder.Button.Tag = "ShowApply";
                        }
                        else
                        {
                            holder.IconMore.Visibility = ViewStates.Gone;
                        }

                        //Set Button if its applied
                        if (item.Apply == "true")
                        {
                            holder.Button.Text = ActivityContext.GetString(Resource.String.Lbl_already_applied);
                            holder.Button.Enabled = false;
                        }
                         
                        if (!holder.Button.HasOnClickListeners)
                        {
                            holder.Button.Click += (sender, args) =>
                            {
                                try
                                { 
                                    switch (holder.Button.Tag.ToString())
                                    {
                                        // Open Apply Job Activity 
                                        case "ShowApply":
                                        {
                                            if (item.ApplyCount == "0")
                                            {
                                                Toast.MakeText(ActivityContext, ActivityContext.GetString(Resource.String.Lbl_ThereAreNoRequests), ToastLength.Short).Show();
                                                return;
                                            }
                                               
                                            var intent = new Intent(ActivityContext, typeof(ShowApplyJobActivity));
                                            intent.PutExtra("JobsObject", JsonConvert.SerializeObject(item));
                                            ActivityContext.StartActivity(intent);
                                            break;
                                        }
                                        case "Apply":
                                        {
                                            var intent = new Intent(ActivityContext, typeof(ApplyJobActivity));
                                            intent.PutExtra("JobsObject", JsonConvert.SerializeObject(item));
                                            ActivityContext.StartActivity(intent);
                                            break;
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                }
                            };
                            
                            holder.IconMore.Click += (sender, args) =>
                            {
                                try
                                {
                                    DialogType = "More";
                                    DataInfoObject = item;

                                    var arrayAdapter = new List<string>();
                                    var dialogList = new MaterialDialog.Builder(ActivityContext).Theme(AppSettings.SetTabDarkTheme ? Theme.Dark : Theme.Light);

                                    arrayAdapter.Add(ActivityContext.GetText(Resource.String.Lbl_Edit));
                                    arrayAdapter.Add(ActivityContext.GetText(Resource.String.Lbl_Delete));

                                    dialogList.Title(ActivityContext.GetText(Resource.String.Lbl_More));
                                    dialogList.Items(arrayAdapter);
                                    dialogList.NegativeText(ActivityContext.GetText(Resource.String.Lbl_Close)).OnNegative(this);
                                    dialogList.AlwaysCallSingleChoiceCallback();
                                    dialogList.ItemsCallback(this).Build().Show();
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                }
                            }; 
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
                    if (holder is JobsAdapterViewHolder viewHolder)
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
        public JobInfoObject GetItem(int position)
        {
            return JobList[position];
        }

        public override long GetItemId(int position)
        {
            try
            {
                return int.Parse(JobList[position].Id);
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
        private void InitToolbar()
        {
            try
            {
               var toolBar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolBar != null)
                {
                    toolBar.Title = GetText(Resource.String.Lbl_jobs);

                    toolBar.SetTitleTextColor(Color.White);
                    SetSupportActionBar(toolBar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);
                    
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void SetRecyclerViewAdapters()
        {
            try
            {
                MAdapter = new ShowApplyJobsAdapter(this)
                {
                    JobList = new ObservableCollection<JobDataObject>()
                };
                LayoutManager = new LinearLayoutManager(this);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<JobDataObject>(this, MAdapter, sizeProvider, 10);
                MRecycler.AddOnScrollListener(preLoader);
                MRecycler.SetAdapter(MAdapter);

                RecyclerViewOnScrollListener xamarinRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(LayoutManager);
                MainScrollEvent = xamarinRecyclerViewOnScrollListener;
                MainScrollEvent.LoadMoreEvent += MainScrollEventOnLoadMoreEvent;
                MRecycler.AddOnScrollListener(xamarinRecyclerViewOnScrollListener);
                MainScrollEvent.IsLoading = false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                // true +=  // false -=
                if (addEvent)
                {
                    MAdapter.ItemClick += MAdapterOnItemClick;
                    SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh; 
                }
                else
                {
                    MAdapter.ItemClick -= MAdapterOnItemClick;
                    SwipeRefreshLayout.Refresh -= SwipeRefreshLayoutOnRefresh; 
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        private void Click(JobsAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void LongClick(JobsAdapterClickEventArgs args)
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
                var item = JobList[p0];
                if (item == null)
                    return d;
                else
                {
                    if (string.IsNullOrEmpty(item.Image)) return d;
                    if (item.Image.Contains("http"))
                    {
                        var image = item.Image.Replace(Client.WebsiteUrl + "/", "");
                        if (!image.Contains("http"))
                            item.Image = Client.WebsiteUrl + "/" + image;
                        else
                            item.Image = image;

                        d.Add(item.Image);
                    }
                    else
                    {
                        d.Add(item.Image);
                    }

                    return d;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Collections.SingletonList(p0);
            }
        }

        #region MaterialDialog

        public void OnSelection(MaterialDialog p0, View p1, int itemId, ICharSequence itemString)
        {
            try
            {
                string text = itemString.ToString();
                if (text == ActivityContext.GetText(Resource.String.Lbl_Edit))
                {
                    //Open Edit Job
                    var intent = new Intent(ActivityContext, typeof(EditJobsActivity));
                    intent.PutExtra("JobsObject", JsonConvert.SerializeObject(DataInfoObject));
                    ActivityContext.StartActivityForResult(intent, 246);
                }
                else if (text == ActivityContext.GetText(Resource.String.Lbl_Delete))
                {
                    DialogType = "Delete";

                    var dialog = new MaterialDialog.Builder(ActivityContext).Theme(AppSettings.SetTabDarkTheme ? Theme.Dark : Theme.Light);
                    dialog.Title(Resource.String.Lbl_Warning);
                    dialog.Content(ActivityContext.GetText(Resource.String.Lbl_DeleteJobs));
                    dialog.PositiveText(ActivityContext.GetText(Resource.String.Lbl_Yes)).OnPositive(this);
                    dialog.NegativeText(ActivityContext.GetText(Resource.String.Lbl_No)).OnNegative(this);
                    dialog.AlwaysCallSingleChoiceCallback();
                    dialog.ItemsCallback(this).Build().Show();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void OnClick(MaterialDialog p0, DialogAction p1)
        {
            try
            {
                if (DialogType == "Delete")
                {
                    if (p1 == DialogAction.Positive)
                    {
                        // Send Api delete 

                        if (Methods.CheckConnectivity())
                        {
                            var adapterGlobal = WRecyclerView.GetInstance()?.NativeFeedAdapter;
                            var diff = adapterGlobal?.ListDiffer;
                            var dataGlobal = diff?.Where(a => a.PostData?.PostId == DataInfoObject?.PostId);
                            if (dataGlobal != null)
                            {
                                foreach (var postData in dataGlobal)
                                {
                                    WRecyclerView.GetInstance()?.RemoveByRowIndex(postData);
                                }
                            }

                            var recycler = TabbedMainActivity.GetInstance()?.NewsFeedTab?.MainRecyclerView;
                            var dataGlobal2 = recycler?.NativeFeedAdapter.ListDiffer?.Where(a => a.PostData?.PostId == DataInfoObject?.PostId);
                            if (dataGlobal2 != null)
                            {
                                foreach (var postData in dataGlobal2)
                                {
                                    recycler.RemoveByRowIndex(postData);
                                }
                            } 

                            var dataJob = JobList?.FirstOrDefault(a => a.Id == DataInfoObject.Id);
                            if (dataJob != null)
                            {
                                JobList.Remove(dataJob);
                                NotifyItemRemoved(JobsActivity.GetInstance().MAdapter.JobList.IndexOf(dataJob));
                            }

                            Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_postSuccessfullyDeleted), ToastLength.Short).Show();
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.Post_Actions(DataInfoObject.PostId, "delete") });
                        }
                        else
                        {
                            Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                        }
                    }
                    else if (p1 == DialogAction.Negative)
                    {
                        p0.Dismiss();
                    }
                }
                else
                {
                    if (p1 == DialogAction.Positive)
                    {

                    }
                    else if (p1 == DialogAction.Negative)
                    {
                        p0.Dismiss();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

    }

    public class JobsAdapterViewHolder : RecyclerView.ViewHolder
    {
        public JobsAdapterViewHolder(View itemView, Action<JobsAdapterClickEventArgs> clickListener, Action<JobsAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                Image = MainView.FindViewById<ImageView>(Resource.Id.JobCoverImage);
                Title = MainView.FindViewById<TextView>(Resource.Id.title);
                Salary = MainView.FindViewById<TextView>(Resource.Id.salary);
                Description = MainView.FindViewById<TextView>(Resource.Id.description);
                IconMore = MainView.FindViewById<TextView>(Resource.Id.iconMore);
                Button = MainView.FindViewById<Button>(Resource.Id.applyButton);
                Button.Tag = "Apply";

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconMore, IonIconsFonts.AndroidMoreVertical);
                 
                //Event  
                itemView.Click += (sender, e) => clickListener(new JobsAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new JobsAdapterClickEventArgs { View = itemView, Position = AdapterPosition });

                
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #region Variables Basic

        public View MainView { get; }

        public ImageView Image { get; private set; }
        public TextView Title { get; private set; }
        public TextView Salary { get; private set; }
        public TextView IconMore { get; private set; }
        public Button Button { get; private set; }
        public TextView Description { get; private set; }

        #endregion
    }

    public class JobsAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}