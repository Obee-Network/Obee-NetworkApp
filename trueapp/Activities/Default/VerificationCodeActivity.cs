using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using ObeeNetwork.Activities.Tabbes;
using ObeeNetwork.Activities.WalkTroutPage;
using ObeeNetwork.Helpers.Controller;
using ObeeNetwork.Helpers.Model;
using ObeeNetwork.Helpers.Utils;
using ObeeNetwork.SQLite;
using ObeeNetworkClient;
using ObeeNetworkClient.Classes.Global;
using ObeeNetworkClient.Requests;
using Exception = System.Exception;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace ObeeNetwork.Activities.Default
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class VerificationCodeActivity : AppCompatActivity
    {
        #region Variables Basic

        private EditText TxtNumber1;
        private Button BtnVerify;
        private string TypeCode;

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
                SetContentView(Resource.Layout.VerificationCodeLayout);

                TypeCode = Intent.GetStringExtra("TypeCode") ?? "";

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
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
                TxtNumber1 = FindViewById<EditText>(Resource.Id.TextNumber1);
                BtnVerify = FindViewById<Button>(Resource.Id.verifyButton);

                Methods.SetColorEditText(TxtNumber1, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
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
                    toolbar.Title = " ";
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
                    BtnVerify.Click += BtnVerifyOnClick;
                }
                else
                {
                    BtnVerify.Click -= BtnVerifyOnClick;
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
                TxtNumber1 = null;
                BtnVerify = null;
                TypeCode = null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        #endregion

        #region Events

        private async void BtnVerifyOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(TxtNumber1.Text) && !string.IsNullOrWhiteSpace(TxtNumber1.Text))
                {
                    if (Methods.CheckConnectivity())
                    {
                        //Show a progress
                        AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

                        if (TypeCode == "TwoFactor")
                        {
                            (int apiStatus, var respond) = await RequestsAsync.Global.TwoFactorAsync(UserDetails.UserId, TxtNumber1.Text, UserDetails.DeviceId);
                            if (apiStatus == 200)
                            {
                                if (respond is AuthObject auth)
                                {
                                    SetDataLogin(auth);

                                    if (AppSettings.ShowWalkTroutPage)
                                    {
                                        Intent newIntent = new Intent(this, typeof(AppIntroWalkTroutPage));
                                        newIntent.PutExtra("class", "login");
                                        StartActivity(newIntent);
                                    }
                                    else
                                    {
                                        StartActivity(new Intent(this, typeof(TabbedMainActivity)));
                                    }

                                    AndHUD.Shared.Dismiss(this);
                                    FinishAffinity();
                                }
                            }
                            else
                            {
                                if (respond is ErrorObject errorMessage)
                                {
                                    var errorId = errorMessage.Error.ErrorId;
                                    if (errorId == "3")
                                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_CodeNotCorrect), GetText(Resource.String.Lbl_Ok));
                                }
                                Methods.DisplayReportResult(this, respond);
                            }
                        }
                        else if (TypeCode == "AccountSms")
                        { 
                            (int apiStatus, var respond) = await RequestsAsync.Global.ActiveAccountSmsAsync(UserDetails.UserId, TxtNumber1.Text, UserDetails.DeviceId);
                            if (apiStatus == 200)
                            {
                                if (respond is AuthObject auth)
                                {
                                    SetDataLogin(auth);

                                    if (AppSettings.ShowWalkTroutPage)
                                    {
                                        Intent newIntent = new Intent(this, typeof(AppIntroWalkTroutPage));
                                        newIntent.PutExtra("class", "login");
                                        StartActivity(newIntent);
                                    }
                                    else
                                    {
                                        StartActivity(new Intent(this, typeof(TabbedMainActivity)));
                                    }

                                    AndHUD.Shared.Dismiss(this);
                                    FinishAffinity();
                                }
                            }
                            else
                            {
                                if (respond is ErrorObject errorMessage)
                                {
                                    var errorId = errorMessage.Error.ErrorId;
                                    if (errorId == "3")
                                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_CodeNotCorrect), GetText(Resource.String.Lbl_Ok));
                                }
                                Methods.DisplayReportResult(this, respond);
                            } 
                        }
                         
                        AndHUD.Shared.Dismiss(this);
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                    }
                }
                else
                {
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Please_enter_your_data), GetText(Resource.String.Lbl_Ok));
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                AndHUD.Shared.Dismiss(this);
            }
        }

        #endregion

        private void SetDataLogin(AuthObject auth)
        {
            try
            {
                Current.AccessToken = auth.AccessToken;

                UserDetails.AccessToken = auth.AccessToken;
                UserDetails.UserId = auth.UserId;
                UserDetails.Status = "Pending";
                UserDetails.Cookie = auth.AccessToken;

                //Insert user data to database
                var user = new DataTables.LoginTb
                {
                    UserId = UserDetails.UserId,
                    AccessToken = UserDetails.AccessToken,
                    Cookie = UserDetails.Cookie,
                    Username = UserDetails.Username,
                    Password = UserDetails.Password,
                    Status = "Pending",
                    Lang = "",
                    DeviceId = UserDetails.DeviceId,
                    Email = UserDetails.Email,
                };
                ListUtils.DataUserLoginList.Clear();
                ListUtils.DataUserLoginList.Add(user);

                var dbDatabase = new SqLiteDatabase();
                dbDatabase.InsertOrUpdateLogin_Credentials(user);
                dbDatabase.Dispose();

                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.Get_MyProfileData_Api(this) });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}