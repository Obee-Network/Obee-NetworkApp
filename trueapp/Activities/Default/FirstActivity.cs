using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using ObeeNetwork.Activities.Tabbes;
using ObeeNetwork.Activities.WalkTroutPage;
using ObeeNetwork.Helpers.Controller;
using ObeeNetwork.Helpers.Model;
using ObeeNetwork.Helpers.Utils;
using ObeeNetwork.Library.OneSignal;
using ObeeNetwork.SQLite;
using ObeeNetworkClient;
using Uri = Android.Net.Uri;

namespace ObeeNetwork.Activities.Default
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class FirstActivity : AppCompatActivity
    {
        private View IncludeLayout;
        private RelativeLayout LayoutBase;
        private Button LoginButton, RegisterButton, ContinueButton; 
        private VideoView VideoViewer;
        private DataTables.LoginTb LoginTb;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                Methods.App.FullScreenApp(this);
 
                SetContentView(Resource.Layout.First_Layout);

                LoginButton = FindViewById<Button>(Resource.Id.LoginButton);
                RegisterButton = FindViewById<Button>(Resource.Id.RegisterButton);
                ContinueButton = FindViewById<Button>(Resource.Id.ContinueButton); 
                IncludeLayout = FindViewById<View>(Resource.Id.IncludeLayout);
                VideoViewer = FindViewById<VideoView>(Resource.Id.videoView); 
                LayoutBase = FindViewById<RelativeLayout>(Resource.Id.Layout_Base);
                ContinueButton.Visibility = ViewStates.Invisible;


                switch (AppSettings.BackgroundScreenWelcomeType)
                {
                    //Set Theme
                    case "Image":
                        LayoutBase.SetBackgroundResource(Resource.Drawable.loginBackground);
                        IncludeLayout.Visibility = ViewStates.Gone;
                        break;
                    case "Video":
                    {
                        var uri = Uri.Parse("android.resource://" + PackageName + "/" + Resource.Raw.MainVideo);
                        VideoViewer.SetVideoURI(uri);
                        VideoViewer.Start();
                        break;
                    }
                    case "Gradient":
                        IncludeLayout.Visibility = ViewStates.Gone;
                        LayoutBase.SetBackgroundResource(Resource.Xml.login_background_shape);
                        break;
                }

                if (!AppSettings.EnableRegisterSystem)
                    RegisterButton.Visibility = ViewStates.Gone;

                // Check if we're running on Android 5.0 or higher
                if ((int) Build.VERSION.SdkInt < 23)
                {
                    LoadConfigSettings();
                    CheckCrossAppAuthentication();
                }
                else
                {
                    if (CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted && CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted)
                    {
                        LoadConfigSettings();
                        CheckCrossAppAuthentication();
                    }
                    else
                    {
                        RequestPermissions(new[]
                        {
                            Manifest.Permission.ReadExternalStorage,
                            Manifest.Permission.WriteExternalStorage
                        }, 101);
                    }
                }

                if (string.IsNullOrEmpty(UserDetails.DeviceId))
                {
                    OneSignalNotification.RegisterNotificationDevice();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        protected override void OnResume()
        {
            try
            {
                base.OnResume();

                if (AppSettings.BackgroundScreenWelcomeType == "Video")
                {
                    if (!VideoViewer.IsPlaying)
                        VideoViewer.Start();

                    VideoViewer.Completion += VideoViewer_Completion;
                }


                RegisterButton.Click += RegisterButton_Click;
                LoginButton.Click += LoginButton_Click;
                ContinueButton.Click += ContinueButtonOnClick;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        protected override void OnPause()
        {
            try
            {
                base.OnPause();

                //Close Event

                RegisterButton.Click -= RegisterButton_Click;
                LoginButton.Click -= LoginButton_Click;
                ContinueButton.Click -= ContinueButtonOnClick;

                if (AppSettings.BackgroundScreenWelcomeType == "Video")
                    VideoViewer.Completion -= VideoViewer_Completion;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        protected override void OnStop()
        {
            try
            {
                base.OnStop();

                if (AppSettings.BackgroundScreenWelcomeType == "Video")
                    VideoViewer.StopPlayback();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
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

        private void DestroyBasic()
        {
            try
            { 
                IncludeLayout = null;
                LayoutBase = null;
                LoginButton = null; RegisterButton = null; ContinueButton = null;
                VideoViewer = null;
                LoginTb = null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        //protected override void AttachBaseContext(Context @base)
        //{
        //    try
        //    {
        //        base.AttachBaseContext(@base);
        //        if (AppSettings.Lang != "")
        //            LangController.SetApplicationLang(@base, AppSettings.Lang);
        //        else
        //        {
        //            UserDetails.LangName = Resources.Configuration.Locale.DisplayLanguage.ToLower();
        //            LangController.SetAppLanguage(@base);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e);
        //    }
        //}

        //Permissions
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                if (requestCode == 101)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        LoadConfigSettings();
                        CheckCrossAppAuthentication();
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long).Show();
                        Finish();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
         
        private void RegisterButton_Click(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(Application.Context, typeof(RegisterActivity)));  
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void LoginButton_Click(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(Application.Context, typeof(LoginActivity))); 
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
         
        private void ContinueButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                CrossAppAuthentication();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
         
        private void VideoViewer_Completion(object sender, EventArgs e)
        {
            try
            {
                VideoViewer?.Start();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
         
        public override void OnTrimMemory(TrimMemory level)
        {
            try
            { 
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                base.OnTrimMemory(level);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
         
        private void LoadConfigSettings()
        {
            try
            {
                var dbDatabase = new SqLiteDatabase();
                var settingsData = dbDatabase.GetSettings();
                if (settingsData != null)
                    ListUtils.SettingsSiteList = settingsData;
                  
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.GetSettings_Api(this) });

                dbDatabase.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
         
        private void CheckCrossAppAuthentication()
        {
            try
            {
                LoginTb = JsonConvert.DeserializeObject<DataTables.LoginTb>(Methods.ReadNoteOnSD());
                if (LoginTb != null && !string.IsNullOrEmpty(LoginTb.AccessToken) && !string.IsNullOrEmpty(LoginTb.Username))
                {
                    ContinueButton.Text = GetString(Resource.String.Lbl_ContinueAs) + " " + LoginTb.Username;
                    ContinueButton.Visibility = ViewStates.Visible;
                }
                else
                    ContinueButton.Visibility = ViewStates.Invisible;
            }
            catch (Exception e)
            {
                Console.WriteLine(e); 
            }
        }
         
        private void CrossAppAuthentication()
        {
            try
            {
                if (LoginTb != null && !string.IsNullOrEmpty(LoginTb.AccessToken) && !string.IsNullOrEmpty(LoginTb.Username))
                {
                    Current.AccessToken = LoginTb.AccessToken;

                    UserDetails.Username = LoginTb.Username;
                    UserDetails.FullName = LoginTb.Username;
                    UserDetails.Password = LoginTb.Password;
                    UserDetails.AccessToken = LoginTb.AccessToken;
                    UserDetails.UserId = LoginTb.UserId;
                    UserDetails.Status = LoginTb.Status;
                    UserDetails.Cookie = LoginTb.AccessToken;
                    UserDetails.Email = LoginTb.Email;

                    //Insert user data to database
                    var user = new DataTables.LoginTb
                    {
                        UserId = UserDetails.UserId,
                        AccessToken = UserDetails.AccessToken,
                        Cookie = UserDetails.Cookie,
                        Username = UserDetails.Username,
                        Password = UserDetails.Password,
                        Status = UserDetails.Status,
                        DeviceId = UserDetails.DeviceId,
                        Email = UserDetails.Email,
                    };
                    ListUtils.DataUserLoginList.Clear();
                    ListUtils.DataUserLoginList.Add(user);

                    var dbDatabase = new SqLiteDatabase();
                    dbDatabase.InsertOrUpdateLogin_Credentials(user);
                    dbDatabase.Dispose();

                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.Get_MyProfileData_Api(this) });

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

                    Finish();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}