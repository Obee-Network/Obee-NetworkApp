using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AFollestad.MaterialDialogs;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.App;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Newtonsoft.Json;
using ObeeNetwork.Activities.NativePost.Extra;
using ObeeNetwork.Activities.Tabbes;
using ObeeNetwork.Helpers.CacheLoaders;
using ObeeNetwork.Helpers.Controller;
using ObeeNetwork.Helpers.Fonts;
using ObeeNetwork.Helpers.Utils;
using ObeeNetwork.Library.Anjo.SuperTextLibrary;
using ObeeNetworkClient.Classes.Jobs;
using ObeeNetworkClient.Requests;
using ClipboardManager = Android.Content.ClipboardManager;
using Exception = System.Exception;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using String = Java.Lang.String;
using Android.Support.V7.Widget;
using ObeeNetworkClient;

namespace ObeeNetwork.Activities.Jobs
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class JobsViewActivity : AppCompatActivity, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        #region Variables Basic

        private ImageView JobCoverImage, IconBack;
        private ImageView JobAvatar;
        private TextView TxtMore, JobTitle, PageName, MaximumNumber, MinimumNumber;
        private AppCompatTextView JobInfo;
        private Button JobButton; 
        private SuperTextView Description;
        private JobInfoObject DataInfoObject;
        private string DialogType;
        private StReadMoreOption ReadMoreOption;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);

                Methods.App.FullScreenApp(this);

                // Create your application here
                SetContentView(Resource.Layout.JobsViewLayout);
              
                var dataObject = Intent.GetStringExtra("JobsObject");
                if (!string.IsNullOrEmpty(dataObject))
                    DataInfoObject = JsonConvert.DeserializeObject<JobInfoObject>(dataObject);
                 
                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();

                BindJobPost();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        protected override void OnResume()
        {
            try
            {
                base.OnResume();
                AddOrRemoveEvent(true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        protected override void OnPause()
        {
            try
            {
                base.OnPause();
                AddOrRemoveEvent(false);
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
        #endregion

        #region Menu

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                IconBack = FindViewById<ImageView>(Resource.Id.iv_back);

                JobCoverImage = FindViewById<ImageView>(Resource.Id.JobCoverImage);
                JobAvatar = FindViewById<ImageView>(Resource.Id.JobAvatar);
                JobTitle = FindViewById<TextView>(Resource.Id.Jobtitle);
                PageName = FindViewById<TextView>(Resource.Id.pageName);
                JobInfo = FindViewById<AppCompatTextView>(Resource.Id.JobInfo);
                JobButton = FindViewById<Button>(Resource.Id.JobButton);
                JobButton.Tag = "Apply";

                //MinimumTextView = FindViewById<TextView>(Resource.Id.minimum);
                //MaximumTextView = FindViewById<TextView>(Resource.Id.maximum);
                MaximumNumber = FindViewById<TextView>(Resource.Id.maximumNumber);
                MinimumNumber = FindViewById<TextView>(Resource.Id.minimumNumber);
                Description = FindViewById<SuperTextView>(Resource.Id.description);

                var font = Typeface.CreateFromAsset(Resources.Assets, "ionicons.ttf");
                JobInfo.SetTypeface(font, TypefaceStyle.Normal);

                TxtMore = FindViewById<TextView>(Resource.Id.toolbar_title);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, TxtMore, IonIconsFonts.AndroidMoreVertical);
                TxtMore.SetTextSize(ComplexUnitType.Sp, 20f);
                TxtMore.Visibility = ViewStates.Gone;

                ReadMoreOption = new StReadMoreOption.Builder()
                    .TextLength(400, StReadMoreOption.TypeCharacter)
                    .MoreLabel(GetText(Resource.String.Lbl_ReadMore))
                    .LessLabel(GetText(Resource.String.Lbl_ReadLess))
                    .MoreLabelColor(Color.ParseColor(AppSettings.MainColor))
                    .LessLabelColor(Color.ParseColor(AppSettings.MainColor))
                    .LabelUnderLine(true)
                    .Build();

                if (AppSettings.FlowDirectionRightToLeft)
                    IconBack.SetImageResource(Resource.Drawable.ic_action_ic_back_rtl);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void InitToolbar()
        {
            try
            {
                var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolbar != null)
                {
                    toolbar.Title = Methods.FunString.DecodeString(DataInfoObject.Title);
                    toolbar.SetTitleTextColor(Color.White);
                    SetSupportActionBar(toolbar);
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
         
        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                // true +=  // false -=
                if (addEvent)
                {
                    TxtMore.Click += TxtMoreOnClick;
                    JobButton.Click += JobButtonOnClick;
                    IconBack.Click += IconBackOnClick;
                    Description.LongClick += DescriptionOnLongClick;
                }
                else
                {
                    TxtMore.Click -= TxtMoreOnClick;
                    JobButton.Click -= JobButtonOnClick;
                    IconBack.Click -= IconBackOnClick;
                    Description.LongClick -= DescriptionOnLongClick;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        private void DestroyBasic()
        {
            try
            {
                IconBack = null;
                JobCoverImage = null;
                JobAvatar= null;
                JobTitle = null;
                PageName = null;
                JobInfo = null;
                JobButton = null;
                MaximumNumber = null;
                MinimumNumber = null;
                Description = null;
                TxtMore = null;
                DialogType = null;
                ReadMoreOption = null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        #endregion

        #region Events

        private void DescriptionOnLongClick(object sender, View.LongClickEventArgs e)
        {
            try
            {
                if (Methods.FunString.StringNullRemover(DataInfoObject.Description) != "Empty")
                {
                    var clipboardManager = (ClipboardManager)GetSystemService(ClipboardService);

                    var clipData = ClipData.NewPlainText("text", Methods.FunString.DecodeString(DataInfoObject.Description));
                    clipboardManager.PrimaryClip = clipData;

                    Toast.MakeText(this, GetText(Resource.String.Lbl_Copied), ToastLength.Short).Show();
                } 
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void IconBackOnClick(object sender, EventArgs e)
        {
            try
            {
                Finish();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
          
        private void TxtMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                DialogType = "More";
                  
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                arrayAdapter.Add(GetText(Resource.String.Lbl_Edit));
                arrayAdapter.Add(GetText(Resource.String.Lbl_Delete)); 

                dialogList.Title(GetText(Resource.String.Lbl_More));
                dialogList.Items(arrayAdapter);
                dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(this);
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show(); 
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void JobButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (DataInfoObject == null)
                    return;
                 
                switch (JobButton.Tag.ToString())
                {
                    // Open Apply Job Activity 
                    case "ShowApply":
                    {
                        if (DataInfoObject.ApplyCount == "0")
                        {
                            Toast.MakeText(this, GetString(Resource.String.Lbl_ThereAreNoRequests), ToastLength.Short).Show();
                            return;
                        }
                         
                        var intent = new Intent(this, typeof(ShowApplyJobActivity)); 
                        intent.PutExtra("JobsObject", JsonConvert.SerializeObject(DataInfoObject));
                        StartActivity(intent);
                        break;
                    }
                    case "Apply":
                    {
                        var intent = new Intent(this, typeof(ApplyJobActivity));
                        intent.PutExtra("JobsObject", JsonConvert.SerializeObject(DataInfoObject));
                        StartActivityForResult(intent,367);
                        break;
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        #region MaterialDialog

        public void OnSelection(MaterialDialog p0, View p1, int itemId, ICharSequence itemString)
        {
            try
            {
                string text = itemString.ToString();
                if (text == GetText(Resource.String.Lbl_Edit))
                {
                    //Open Edit Job
                    var intent = new Intent(this, typeof(EditJobsActivity));
                    intent.PutExtra("JobsObject", JsonConvert.SerializeObject(DataInfoObject));
                    StartActivityForResult(intent,246); 
                }
                else if (text == GetText(Resource.String.Lbl_Delete))
                {
                    DialogType = "Delete";

                    var dialog = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light); 
                    dialog.Title(Resource.String.Lbl_Warning);
                    dialog.Content(GetText(Resource.String.Lbl_DeleteJobs));
                    dialog.PositiveText(GetText(Resource.String.Lbl_Yes)).OnPositive(this);
                    dialog.NegativeText(GetText(Resource.String.Lbl_No)).OnNegative(this);
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
                              
                            var dataJob = JobsActivity.GetInstance()?.MAdapter?.JobList?.FirstOrDefault(a => a.Id == DataInfoObject.Id);
                            if (dataJob != null)
                            {
                                JobsActivity.GetInstance()?.MAdapter?.JobList.Remove(dataJob);
                                JobsActivity.GetInstance().MAdapter.NotifyItemRemoved(JobsActivity.GetInstance().MAdapter.JobList.IndexOf(dataJob));
                            }

                            Toast.MakeText(this, GetText(Resource.String.Lbl_postSuccessfullyDeleted), ToastLength.Short).Show();
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.Post_Actions(DataInfoObject.PostId, "delete") });
                        }
                        else
                        {
                            Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
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
         
        #region  Result

        //Result
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data); 
                if (requestCode == 367 && resultCode == Result.Ok)
                {
                    //Already applied 
                    DataInfoObject.Apply = "true";
                    JobButton.Text = GetString(Resource.String.Lbl_already_applied);
                    JobButton.Enabled = false;
                }
                else if (requestCode == 246 && resultCode == Result.Ok)
                {
                    var jobsItem = data.GetStringExtra("JobsItem") ?? "";
                    if (string.IsNullOrEmpty(jobsItem)) return;
                    var dataObject = JsonConvert.DeserializeObject<JobInfoObject>(jobsItem);
                    if (dataObject != null)
                    {
                        DataInfoObject.Title =  dataObject.Title;
                        DataInfoObject.Location =  dataObject.Location;
                        DataInfoObject.Minimum =  dataObject.Minimum;
                        DataInfoObject.Maximum =  dataObject.Maximum;
                        DataInfoObject.SalaryDate =  dataObject.SalaryDate;
                        DataInfoObject.JobType =  dataObject.JobType;
                        DataInfoObject.Description =  dataObject.Description;
                        DataInfoObject.Category = dataObject.Category;

                        BindJobPost();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
         
        #endregion

        private void BindJobPost()
        {
            try
            {
                if (DataInfoObject != null)
                {
                    DataInfoObject = ObeeNetworkTools.ListFilterJobs(DataInfoObject);
                     
                    GlideImageLoader.LoadImage(this, DataInfoObject.Page.Avatar, JobAvatar, ImageStyle.RoundedCrop, ImagePlaceholders.Drawable);

                    var image = DataInfoObject.Image.Replace(Client.WebsiteUrl + "/", "");
                    if (!image.Contains("http"))
                        DataInfoObject.Image = Client.WebsiteUrl + "/" + image;
                    else
                        DataInfoObject.Image = image;

                    GlideImageLoader.LoadImage(this, DataInfoObject.Image, JobCoverImage, ImageStyle.FitCenter, ImagePlaceholders.Drawable);

                    if (DataInfoObject.IsOwner)
                    {
                        TxtMore.Visibility = ViewStates.Visible;
                        JobButton.Text = GetString(Resource.String.Lbl_show_applies) + " (" + DataInfoObject.ApplyCount + ")";
                        JobButton.Tag = "ShowApply";
                    }

                    //Set Button if its applied
                    if (DataInfoObject.Apply == "true")
                    {
                        JobButton.Text = GetString(Resource.String.Lbl_already_applied);
                        JobButton.Enabled = false;
                    }
                     
                    JobTitle.Text = Methods.FunString.DecodeString(DataInfoObject.Title);

                    if (DataInfoObject.Page != null)
                    {
                        PageName.Text = "@" + Methods.FunString.DecodeString(DataInfoObject.Page.PageName);
                        if (DataInfoObject.Page.IsPageOnwer != null && DataInfoObject.Page.IsPageOnwer.Value)
                        {
                            JobButton.Text = GetString(Resource.String.Lbl_show_applies) + " (" + DataInfoObject.ApplyCount + ")";
                        }
                    }

                    //Set Button if its applied
                    if (DataInfoObject.Apply == "true")
                    {
                        JobButton.Text = GetString(Resource.String.Lbl_already_applied);
                        JobButton.Enabled = false;
                    }

                    //Set Description
                    var description = Methods.FunString.DecodeString(DataInfoObject.Description);
                    Description.Text = description; 
                    ReadMoreOption.AddReadMoreTo(Description, new String(description));

                    //Set Salary Date
                    string salaryDate = DataInfoObject.SalaryDate switch
                    {
                        "per_hour" => GetString(Resource.String.Lbl_per_hour),
                        "per_day" => GetString(Resource.String.Lbl_per_day),
                        "per_week" => GetString(Resource.String.Lbl_per_week),
                        "per_month" => GetString(Resource.String.Lbl_per_month),
                        "per_year" => GetString(Resource.String.Lbl_per_year),
                        _ => GetString(Resource.String.Lbl_Unknown)
                    };

                    MinimumNumber.Text = DataInfoObject.Minimum + " " + salaryDate;
                    MaximumNumber.Text = DataInfoObject.Maximum + " " + salaryDate;

                    //Set job Time
                    var jobInfo = IonIconsFonts.AndroidPin + " " + DataInfoObject.Location + "  " + " ";
                    jobInfo += IonIconsFonts.AndroidTime + " " + Methods.Time.TimeAgo(int.Parse(DataInfoObject.Time)) + " " + " ";

                    //Set job type
                    if (DataInfoObject.JobType == "full_time")
                        jobInfo += IonIconsFonts.IosBriefcase + " " + GetString(Resource.String.Lbl_full_time);
                    else if (DataInfoObject.JobType == "part_time")
                        jobInfo += IonIconsFonts.IosBriefcase + " " + GetString(Resource.String.Lbl_part_time);
                    else if (DataInfoObject.JobType == "internship")
                        jobInfo += IonIconsFonts.IosBriefcase + " " + GetString(Resource.String.Lbl_internship);
                    else if (DataInfoObject.JobType == "volunteer")
                        jobInfo += IonIconsFonts.IosBriefcase + " " + GetString(Resource.String.Lbl_volunteer);
                    else if (DataInfoObject.JobType == "contract")
                        jobInfo += IonIconsFonts.IosBriefcase + " " + GetString(Resource.String.Lbl_contract);
                    else
                        jobInfo += IonIconsFonts.IosBriefcase + " " + GetString(Resource.String.Lbl_Unknown);

                    var categoryName = CategoriesController.ListCategoriesJob.FirstOrDefault(categories => categories.CategoriesId == DataInfoObject.Category)?.CategoriesName;
                    jobInfo += " " + " " + IonIconsFonts.Pricetag + " " + categoryName;

                    var woTextDecorator = new WoTextDecorator
                    {
                        DecoratedContent = new SpannableString(jobInfo),
                        Content = jobInfo
                    };
                    woTextDecorator.SetTextColor(IonIconsFonts.AndroidPin, "#ff5722");
                    woTextDecorator.SetTextColor(IonIconsFonts.AndroidTime, "#4caf50");
                    woTextDecorator.SetTextColor(IonIconsFonts.IosBriefcase, "#2196f3");
                    woTextDecorator.SetTextColor(IonIconsFonts.Pricetag, "#795548");
                    woTextDecorator.Build(JobInfo, woTextDecorator.DecoratedContent);
                } 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

    }
}