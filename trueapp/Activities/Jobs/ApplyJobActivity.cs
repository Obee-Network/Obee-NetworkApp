using System;
using System.Collections.Generic;
using System.Linq;
using AFollestad.MaterialDialogs;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using Java.Lang;
using Newtonsoft.Json;
using ObeeNetwork.Helpers.Controller;
using ObeeNetwork.Helpers.Fonts;
using ObeeNetwork.Helpers.Utils;
using ObeeNetworkClient.Classes.Jobs;
using ObeeNetworkClient.Requests;
using Console = System.Console;
using Exception = System.Exception;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace ObeeNetwork.Activities.Jobs
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class ApplyJobActivity : AppCompatActivity, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        #region Variables Basic

        private TextView TxtSave;
        private TextView IconName, IconLocation, IconPhone, IconEmail, IconWork, IconPosition, IconDescription, IconDate;
        private EditText TxtName, TxtLocation, TxtPhone, TxtEmail, TxtWork, TxtPosition, TxtDescription, TxtFromDate, TxtToDate;
        private CheckBox ChkCurrentlyWork;
        private ViewStub QuestionOneLayout, QuestionTwoLayout, QuestionThreeLayout;
        private View InflatedQuestionOne, InflatedQuestionTwo, InflatedQuestionThree;
        private TextView TxtQuestion;
        private EditText EdtQuestion;
        private RadioButton RdoYes, RdoNo; 
        private string DialogType, CurrentlyWork;
        private JobInfoObject DataInfoObject;
        private readonly string[] ExperienceDate = Application.Context.Resources.GetStringArray(Resource.Array.experience_date);
        private readonly string[] JobCategories = Application.Context.Resources.GetStringArray(Resource.Array.job_categories);
        private string QuestionOneAnswer, QuestionTwoAnswer, QuestionThreeAnswer;
         
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
                SetContentView(Resource.Layout.ApplyJoblayout);
              
                var dataObject = Intent.GetStringExtra("JobsObject");
                if (!string.IsNullOrEmpty(dataObject))
                    DataInfoObject = JsonConvert.DeserializeObject<JobInfoObject>(dataObject);
                  
                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                LoadMyDate();
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
                TxtSave = FindViewById<TextView>(Resource.Id.toolbar_title);

                IconName = FindViewById<TextView>(Resource.Id.IconName);
                TxtName = FindViewById<EditText>(Resource.Id.NameEditText);
                IconLocation = FindViewById<TextView>(Resource.Id.IconLocation);
                TxtLocation = FindViewById<EditText>(Resource.Id.LocationEditText);
                IconPhone = FindViewById<TextView>(Resource.Id.IconPhone);
                TxtPhone = FindViewById<EditText>(Resource.Id.PhoneEditText);
                IconEmail = FindViewById<TextView>(Resource.Id.IconEmail);
                TxtEmail = FindViewById<EditText>(Resource.Id.EmailEditText);

                IconPosition = FindViewById<TextView>(Resource.Id.IconPosition);
                TxtPosition = FindViewById<EditText>(Resource.Id.PositionEditText);

                IconWork = FindViewById<TextView>(Resource.Id.IconWorkStatus);
                TxtWork = FindViewById<EditText>(Resource.Id.WorkStatusEditText);

                IconDescription = FindViewById<TextView>(Resource.Id.IconDescription);
                TxtDescription = FindViewById<EditText>(Resource.Id.DescriptionEditText);

                IconDate = FindViewById<TextView>(Resource.Id.IconDate);
                TxtFromDate = FindViewById<EditText>(Resource.Id.FromDateEditText);
                TxtToDate = FindViewById<EditText>(Resource.Id.ToDateEditText);
                 
                ChkCurrentlyWork = FindViewById<CheckBox>(Resource.Id.iCurrentlyWorkCheckBox);
            
                QuestionOneLayout = FindViewById<ViewStub>(Resource.Id.viewStubQuestionOne);
                QuestionTwoLayout = FindViewById<ViewStub>(Resource.Id.viewStubQuestionTwo);
                QuestionThreeLayout = FindViewById<ViewStub>(Resource.Id.viewStubQuestionThree);

                //free_text_question,yes_no_question,multiple_choice_question
                SetQuestion();
                 
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconName, FontAwesomeIcon.User);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconLocation, FontAwesomeIcon.MapMarkedAlt);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconPhone, FontAwesomeIcon.Phone);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconWork, FontAwesomeIcon.Briefcase);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconEmail, FontAwesomeIcon.PaperPlane);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconPosition, FontAwesomeIcon.MapPin);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconDescription, FontAwesomeIcon.Paragraph);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconDate, FontAwesomeIcon.Calendar);

                Methods.SetColorEditText(TxtName, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtLocation, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtPhone, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtEmail, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtPosition, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtWork, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtDescription, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtFromDate, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtToDate, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                Methods.SetFocusable(TxtFromDate); 
                Methods.SetFocusable(TxtToDate); 
                Methods.SetFocusable(TxtPosition); 
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
                    TxtSave.Click += TxtSaveOnClick;
                    TxtLocation.FocusChange += TxtLocationOnClick;
                    TxtFromDate.Touch += TxtFromDateOnTouch;
                    TxtToDate.Touch += TxtToDateOnTouch;
                    TxtPosition.Touch += TxtPositionOnClick;
                    ChkCurrentlyWork.CheckedChange += ChkCurrentlyWorkOnCheckedChange;
                }
                else
                {
                    TxtSave.Click -= TxtSaveOnClick;
                    TxtLocation.FocusChange -= TxtLocationOnClick;
                    TxtFromDate.Touch -= TxtFromDateOnTouch;
                    TxtToDate.Touch -= TxtToDateOnTouch;
                    TxtPosition.Touch -= TxtPositionOnClick;
                    ChkCurrentlyWork.CheckedChange -= ChkCurrentlyWorkOnCheckedChange;
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
                TxtSave = null;
                IconName = null;
                TxtName = null;
                IconLocation = null;
                TxtLocation = null;
                IconPhone = null;
                TxtPhone = null;
                IconEmail = null;
                TxtEmail = null;
                IconPosition = null;
                TxtPosition = null;
                IconWork = null;
                TxtWork = null;
                IconDescription = null;
                TxtDescription = null;
                IconDate = null;
                TxtFromDate = null;
                TxtToDate = null;
                ChkCurrentlyWork = null;
                DataInfoObject = null;
                QuestionOneLayout = null;
                DialogType = null;
                TxtQuestion = null;
                CurrentlyWork = null;
                EdtQuestion = null;
                RdoYes = null;
                RdoNo = null;
                InflatedQuestionOne = null;
                InflatedQuestionTwo = null; 
                InflatedQuestionThree = null;
                QuestionTwoLayout = null;
                QuestionThreeLayout = null;
                QuestionOneAnswer = null; 
                QuestionTwoAnswer = null; 
                QuestionThreeAnswer = null;

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
      
        #endregion

        #region Events

        private async void TxtSaveOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {

                    if (string.IsNullOrEmpty(TxtName.Text) || string.IsNullOrEmpty(TxtPhone.Text) || string.IsNullOrEmpty(TxtLocation.Text)
                        || string.IsNullOrEmpty(TxtWork.Text) || string.IsNullOrEmpty(TxtDescription.Text)|| string.IsNullOrEmpty(TxtFromDate.Text))
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Please_enter_your_data), ToastLength.Short).Show();
                        return;
                    }

                    if (CurrentlyWork == "off" && string.IsNullOrEmpty(TxtToDate.Text))
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Please_enter_your_data), ToastLength.Short).Show();
                        return;
                    }

                    var check = Methods.FunString.IsEmailValid(TxtEmail.Text.Replace(" ", ""));
                    if (!check)
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_IsEmailValid), ToastLength.Short).Show();
                        return;
                    }
                     
                    //Show a progress
                    AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));
                     
                    var dictionary = new Dictionary<string, string>
                    {
                        {"job_id", DataInfoObject.Id},
                        {"user_name", TxtName.Text},
                        {"phone_number", TxtPhone.Text},
                        {"location", TxtLocation.Text},
                        {"email", TxtEmail.Text},
                        {"where_did_you_work", TxtWork.Text},
                        {"Position", TxtPosition.Text},
                        {"experience_description", TxtDescription.Text},
                        {"experience_start_date", TxtFromDate.Text},
                        {"experience_end_date", TxtToDate.Text},
                        {"i_currently_work", CurrentlyWork},
                        {"question_one_answer", QuestionOneAnswer},
                        {"question_two_answer", QuestionTwoAnswer},
                        {"question_three_answer", QuestionThreeAnswer},
                    };
                    
                    var (apiStatus, respond) = await RequestsAsync.Jobs.ApplyJob(dictionary);
                    if (apiStatus == 200)
                    {
                        if (respond is MessageJobObject result)
                        {
                            Console.WriteLine(result.MessageData);
                            Toast.MakeText(this, "You have successfully applied to this job", ToastLength.Short).Show();
                            AndHUD.Shared.Dismiss(this);

                           var data =  JobsActivity.GetInstance()?.MAdapter?.JobList?.FirstOrDefault(a => a.Id == DataInfoObject.Id);
                           if (data != null)
                           {
                               data.Apply = "true"; 
                               JobsActivity.GetInstance().MAdapter.NotifyItemChanged(JobsActivity.GetInstance().MAdapter.JobList.IndexOf(data));
                           }

                           SetResult(Result.Ok);
                           Finish();
                        }
                    }
                    else Methods.DisplayReportResult(this, respond);

                    AndHUD.Shared.Dismiss(this);
                }
                else
                {
                    Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                AndHUD.Shared.Dismiss(this);
            }
        }
         
        private void TxtPositionOnClick(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e.Event.Action != MotionEventActions.Down) return;

                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                DialogType = "Position";
                var arrayAdapter = JobCategories.ToList();

                dialogList.Title(GetText(Resource.String.Lbl_Position));
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
         
        private void TxtLocationOnClick(object sender, View.FocusChangeEventArgs e)
        {
            try
            {
                // Check if we're running on Android 5.0 or higher
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    //Open intent Location when the request code of result is 502
                    new IntentController(this).OpenIntentLocation();
                }
                else
                {
                    if (CheckSelfPermission(Manifest.Permission.AccessFineLocation) == Permission.Granted && CheckSelfPermission(Manifest.Permission.AccessCoarseLocation) == Permission.Granted)
                    {
                        //Open intent Location when the request code of result is 502
                        new IntentController(this).OpenIntentLocation();
                    }
                    else
                    {
                        new PermissionsController(this).RequestPermission(105);
                    }
                } 
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
         
        private void ChkCurrentlyWorkOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                TxtToDate.Visibility = e.IsChecked ? ViewStates.Invisible : ViewStates.Visible;
                CurrentlyWork = e.IsChecked ? "on" : "off";
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void TxtToDateOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e.Event.Action != MotionEventActions.Down) return;

                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                DialogType = "ToDate";
                var arrayAdapter = ExperienceDate.ToList();

                dialogList.Title(GetText(Resource.String.Lbl_ToDate));
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

        private void TxtFromDateOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e.Event.Action != MotionEventActions.Down) return;

                DialogType = "FromDate";
                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                var arrayAdapter = ExperienceDate.ToList();

                dialogList.Title(GetText(Resource.String.Lbl_FromDate));
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
          
        #endregion

        #region Permissions && Result

        //Result
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);

                if (requestCode == 502 && resultCode == Result.Ok) 
                    GetPlaceFromPicker(data);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //Permissions
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                if (requestCode == 105)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        //Open intent Camera when the request code of result is 503
                        new IntentController(this).OpenIntentLocation();
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long).Show();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion
         
        #region MaterialDialog

        public void OnClick(MaterialDialog p0, DialogAction p1)
        {
            try
            {
                if (p1 == DialogAction.Positive)
                {
                    
                }
                else if (p1 == DialogAction.Negative)
                {
                    p0.Dismiss();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            } 
        }

        public void OnSelection(MaterialDialog p0, View p1, int itemId, ICharSequence itemString)
        {
            try
            {
                switch (DialogType)
                {
                    case "Position":
                        TxtPosition.Text = itemString.ToString();
                        break;
                    case "FromDate":
                        TxtFromDate.Text = itemString.ToString();
                        break;
                    case "ToDate":
                        TxtToDate.Text = itemString.ToString();
                        break;
                    case "QuestionOne":
                        EdtQuestion.Text = itemString.ToString();
                        QuestionOneAnswer = itemId.ToString();
                        break;
                    case "QuestionTwo":
                        EdtQuestion.Text = itemString.ToString();
                        QuestionTwoAnswer = itemId.ToString();
                        break;
                    case "QuestionThree":
                        EdtQuestion.Text = itemString.ToString();
                        QuestionThreeAnswer = itemId.ToString();
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Question

        private void SetQuestion()
        {
            try
            {
                #region Question One

                switch (DataInfoObject.QuestionOneType)
                {
                    case "free_text_question":
                    {
                        QuestionOneLayout.LayoutResource = Resource.Layout.ViewSub_Question_EditText;

                        if (InflatedQuestionOne == null)
                            InflatedQuestionOne = QuestionOneLayout.Inflate();

                        QuestionOneLayout.Visibility = ViewStates.Visible;

                        TxtQuestion = InflatedQuestionOne.FindViewById<TextView>(Resource.Id.QuestionTextView);
                        TxtQuestion.Text = Methods.FunString.DecodeString(DataInfoObject.QuestionOne);

                        EdtQuestion = InflatedQuestionOne.FindViewById<EditText>(Resource.Id.QuestionEditText);
                        Methods.SetColorEditText(EdtQuestion, AppSettings.SetTabDarkTheme ? Color.White : Color.Black); 
                          
                        EdtQuestion.TextChanged += (sender, args) =>
                        {
                            try
                            {
                                QuestionOneAnswer = args.Text.ToString();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }
                        };
                        break;
                    }
                    case "yes_no_question":
                    {
                        QuestionOneLayout.LayoutResource = Resource.Layout.ViewSub_Question_CheckBox;

                        if (InflatedQuestionOne == null)
                            InflatedQuestionOne = QuestionOneLayout.Inflate();

                        QuestionOneLayout.Visibility = ViewStates.Visible;

                        TxtQuestion = InflatedQuestionOne.FindViewById<TextView>(Resource.Id.QuestionTextView);
                        TxtQuestion.Text = Methods.FunString.DecodeString(DataInfoObject.QuestionOne);
                             
                        RdoYes = InflatedQuestionOne.FindViewById<RadioButton>(Resource.Id.radioYes);
                        RdoNo = InflatedQuestionOne.FindViewById<RadioButton>(Resource.Id.radioNo);
                        RdoYes.CheckedChange += (sender, args) =>
                        {
                            try
                            {
                                var isChecked = RdoYes.Checked;
                                if (!isChecked) return;
                                RdoNo.Checked = false;
                                QuestionOneAnswer = "yes";
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }
                        };
                        RdoNo.CheckedChange += (sender, args) =>
                        {
                            try
                            {
                                var isChecked = RdoNo.Checked;
                                if (!isChecked) return;
                                RdoNo.Checked = false;
                                QuestionOneAnswer = "no";
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }
                        }; 
                        break;
                    }
                    case "multiple_choice_question":
                    {
                        QuestionOneLayout.LayoutResource = Resource.Layout.ViewSub_Question_List;

                        if (InflatedQuestionOne == null)
                            InflatedQuestionOne = QuestionOneLayout.Inflate();

                        QuestionOneLayout.Visibility = ViewStates.Visible;

                        TxtQuestion = InflatedQuestionOne.FindViewById<TextView>(Resource.Id.QuestionTextView);  
                        TxtQuestion.Text = Methods.FunString.DecodeString(DataInfoObject.QuestionOne);
                             
                        EdtQuestion = InflatedQuestionOne.FindViewById<EditText>(Resource.Id.QuestionEditText);
                        Methods.SetColorEditText(EdtQuestion, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                        Methods.SetFocusable(EdtQuestion);
                        EdtQuestion.Touch += (sender, args) =>
                        {
                            if (args.Event.Action != MotionEventActions.Down) return;

                            DialogType = "QuestionOne";
                            var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                            var arrayAdapter = new List<string>();
                            if (DataInfoObject.QuestionOneAnswers?.Count > 0)
                                arrayAdapter = DataInfoObject.QuestionOneAnswers;
                             
                            dialogList.Title(Methods.FunString.DecodeString(DataInfoObject.QuestionOne)); 
                            dialogList.Items(arrayAdapter);
                            dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(this);
                            dialogList.AlwaysCallSingleChoiceCallback();
                            dialogList.ItemsCallback(this).Build().Show();
                        };
                        break;
                    }
                    default:
                        QuestionOneLayout.Visibility = ViewStates.Gone;
                        break;
                }

                #endregion

                #region Question Two

                switch (DataInfoObject.QuestionTwoType)
                {
                    case "free_text_question":
                    {
                        QuestionTwoLayout.LayoutResource = Resource.Layout.ViewSub_Question_EditText;

                        if (InflatedQuestionTwo == null)
                            InflatedQuestionTwo = QuestionTwoLayout.Inflate();

                        QuestionTwoLayout.Visibility = ViewStates.Visible;

                        TxtQuestion = InflatedQuestionTwo.FindViewById<TextView>(Resource.Id.QuestionTextView);
                        TxtQuestion.Text = Methods.FunString.DecodeString(DataInfoObject.QuestionTwo);
                             
                        EdtQuestion = InflatedQuestionTwo.FindViewById<EditText>(Resource.Id.QuestionEditText);
                        Methods.SetColorEditText(EdtQuestion, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                         
                        EdtQuestion.TextChanged += (sender, args) =>
                        {
                            try
                            {
                                QuestionTwoAnswer = args.Text.ToString();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }
                        };
                        break;
                    }
                    case "yes_no_question":
                    {
                        QuestionTwoLayout.LayoutResource = Resource.Layout.ViewSub_Question_CheckBox;

                        if (InflatedQuestionTwo == null)
                            InflatedQuestionTwo = QuestionTwoLayout.Inflate();

                        QuestionTwoLayout.Visibility = ViewStates.Visible;

                        TxtQuestion = InflatedQuestionTwo.FindViewById<TextView>(Resource.Id.QuestionTextView); 
                        TxtQuestion.Text = Methods.FunString.DecodeString(DataInfoObject.QuestionTwo);
                             
                        RdoYes = InflatedQuestionTwo.FindViewById<RadioButton>(Resource.Id.radioYes);
                        RdoNo = InflatedQuestionTwo.FindViewById<RadioButton>(Resource.Id.radioNo);
                        RdoYes.CheckedChange += (sender, args) =>
                        {
                            try
                            {
                                var isChecked = RdoYes.Checked;
                                if (!isChecked) return;
                                RdoNo.Checked = false;
                                QuestionTwoAnswer = "yes";
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }
                        };
                        RdoNo.CheckedChange += (sender, args) =>
                        {
                            try
                            {
                                var isChecked = RdoNo.Checked;
                                if (!isChecked) return;
                                RdoNo.Checked = false;
                                QuestionTwoAnswer = "no";
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }
                        };
                        break;
                    }
                    case "multiple_choice_question":
                    {
                        QuestionTwoLayout.LayoutResource = Resource.Layout.ViewSub_Question_List;

                        if (InflatedQuestionTwo == null)
                            InflatedQuestionTwo = QuestionTwoLayout.Inflate();

                        QuestionTwoLayout.Visibility = ViewStates.Visible;

                        TxtQuestion = InflatedQuestionTwo.FindViewById<TextView>(Resource.Id.QuestionTextView);
                        TxtQuestion.Text = Methods.FunString.DecodeString(DataInfoObject.QuestionTwo);
                             
                        EdtQuestion = InflatedQuestionTwo.FindViewById<EditText>(Resource.Id.QuestionEditText);
                        Methods.SetColorEditText(EdtQuestion, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                        Methods.SetFocusable(EdtQuestion);
                        EdtQuestion.Touch += (sender, args) =>
                        {
                            if (args.Event.Action != MotionEventActions.Down) return;

                            DialogType = "QuestionTwo";
                            var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                            var arrayAdapter = new List<string>();
                            if (DataInfoObject.QuestionTwoAnswers?.Count > 0)
                                arrayAdapter = DataInfoObject.QuestionTwoAnswers;

                            dialogList.Title(Methods.FunString.DecodeString(DataInfoObject.QuestionTwo));
                            dialogList.Items(arrayAdapter);
                            dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(this);
                            dialogList.AlwaysCallSingleChoiceCallback();
                            dialogList.ItemsCallback(this).Build().Show();
                        };
                        break;
                    }
                    default:
                        QuestionTwoLayout.Visibility = ViewStates.Gone;
                        break;
                }

                #endregion

                #region Question Three

                switch (DataInfoObject.QuestionThreeType)
                {
                    case "free_text_question":
                    {
                        QuestionThreeLayout.LayoutResource = Resource.Layout.ViewSub_Question_EditText;

                        if (InflatedQuestionThree == null)
                            InflatedQuestionThree = QuestionThreeLayout.Inflate();

                        QuestionThreeLayout.Visibility = ViewStates.Visible;

                        TxtQuestion = InflatedQuestionThree.FindViewById<TextView>(Resource.Id.QuestionTextView);
                        TxtQuestion.Text = Methods.FunString.DecodeString(DataInfoObject.QuestionThree);
                             
                        EdtQuestion = InflatedQuestionThree.FindViewById<EditText>(Resource.Id.QuestionEditText);
                        Methods.SetColorEditText(EdtQuestion, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                          
                        EdtQuestion.TextChanged += (sender, args) =>
                        {
                            try
                            {
                                QuestionThreeAnswer = args.Text.ToString();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }
                        };
                        break;
                    }
                    case "yes_no_question":
                    {
                        QuestionThreeLayout.LayoutResource = Resource.Layout.ViewSub_Question_CheckBox;

                        if (InflatedQuestionThree == null)
                            InflatedQuestionThree = QuestionThreeLayout.Inflate();

                        QuestionThreeLayout.Visibility = ViewStates.Visible;

                        TxtQuestion = InflatedQuestionThree.FindViewById<TextView>(Resource.Id.QuestionTextView);
                        TxtQuestion.Text = Methods.FunString.DecodeString(DataInfoObject.QuestionThree);
                        
                        RdoYes = InflatedQuestionThree.FindViewById<RadioButton>(Resource.Id.radioYes);
                        RdoNo = InflatedQuestionThree.FindViewById<RadioButton>(Resource.Id.radioNo);
                        RdoYes.CheckedChange += (sender, args) =>
                        {
                            try
                            {
                                var isChecked = RdoYes.Checked;
                                if (!isChecked) return;
                                RdoNo.Checked = false;
                                QuestionThreeAnswer = "yes";
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }
                        };
                        RdoNo.CheckedChange += (sender, args) =>
                        {
                            try
                            {
                                var isChecked = RdoNo.Checked;
                                if (!isChecked) return;
                                RdoNo.Checked = false;
                                QuestionThreeAnswer = "no";
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }
                        };
                            break;
                    }
                    case "multiple_choice_question":
                    {
                        QuestionThreeLayout.LayoutResource = Resource.Layout.ViewSub_Question_List;

                        if (InflatedQuestionThree == null)
                            InflatedQuestionThree = QuestionThreeLayout.Inflate();

                        QuestionThreeLayout.Visibility = ViewStates.Visible;

                        TxtQuestion = InflatedQuestionThree.FindViewById<TextView>(Resource.Id.QuestionTextView);
                        TxtQuestion.Text = Methods.FunString.DecodeString(DataInfoObject.QuestionThree);
                        
                        EdtQuestion = InflatedQuestionThree.FindViewById<EditText>(Resource.Id.QuestionEditText);
                        Methods.SetColorEditText(EdtQuestion, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
 
                        Methods.SetFocusable(EdtQuestion);
                        EdtQuestion.Touch += (sender, args) =>
                        {
                            if (args.Event.Action != MotionEventActions.Down) return;

                            DialogType = "QuestionThree";
                            var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                            var arrayAdapter = new List<string>();
                            if (DataInfoObject.QuestionThreeAnswers?.Count > 0)
                                arrayAdapter = DataInfoObject.QuestionThreeAnswers;

                            dialogList.Title(Methods.FunString.DecodeString(DataInfoObject.QuestionThree));
                            dialogList.Items(arrayAdapter);
                            dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(this);
                            dialogList.AlwaysCallSingleChoiceCallback();
                            dialogList.ItemsCallback(this).Build().Show();
                        };
                        break;
                    }
                    default:
                        QuestionThreeLayout.Visibility = ViewStates.Gone;
                        break;
                }

                #endregion
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
          
        #endregion

        private void GetPlaceFromPicker(Intent data)
        {
            try
            {
                var placeAddress = data.GetStringExtra("Address") ?? "";
                //var placeLatLng = data.GetStringExtra("latLng") ?? "";
                if (!string.IsNullOrEmpty(placeAddress))
                    TxtLocation.Text = placeAddress;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void LoadMyDate()
        {
            try
            {
                var dataUser = ListUtils.MyProfileList.FirstOrDefault();
                if (dataUser == null) return;
                TxtName.Text = ObeeNetworkTools.GetNameFinal(dataUser); 
                TxtPhone.Text = dataUser.PhoneNumber;
                TxtEmail.Text = dataUser.Email;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        } 
    }
}