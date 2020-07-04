using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Auth.Api;
using Android.Gms.Auth.Api.SignIn;
using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using Org.Json;
using ObeeNetwork.Activities.Tabbes;
using ObeeNetwork.Activities.WalkTroutPage;
using ObeeNetwork.Helpers.Controller;
using ObeeNetwork.Helpers.Model;
using ObeeNetwork.Helpers.SocialLogins;
using ObeeNetwork.Helpers.Utils;
using ObeeNetwork.Library.OneSignal;
using ObeeNetwork.SQLite;
using ObeeNetworkClient;
using ObeeNetworkClient.Classes.Global;
using ObeeNetworkClient.Requests;
using Xamarin.Facebook;
using Xamarin.Facebook.Login;
using Xamarin.Facebook.Login.Widget;
using Console = System.Console;
using Exception = System.Exception;
using Object = Java.Lang.Object;

namespace ObeeNetwork.Activities.Default
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class LoginActivity : AppCompatActivity, IFacebookCallback, GraphRequest.IGraphJSONObjectCallback,GoogleApiClient.IConnectionCallbacks, GoogleApiClient.IOnConnectionFailedListener , IResultCallback
    {
        #region Variables Basic

        private TextView MTextViewForgotPwd, MTextViewCreateAccount, MTextViewSignUp;
        private Typeface RegularTxt;
        private EditText MEditTextEmail, MEditTextPassword;
        private Button MButtonViewSignIn;
        private ProgressBar ProgressBar;
        private LinearLayout LayoutCreateAccount;
        private LoginButton BtnFbLogin;
        private ICallbackManager MFbCallManager;
        private FbMyProfileTracker MprofileTracker;
         
        public static GoogleApiClient MGoogleApiClient;
        private SignInButton MGoogleSignIn;
        private string TimeZone = "";

        #endregion
     
        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                Methods.App.FullScreenApp(this);
                
                // Create your application here
                SetContentView(Resource.Layout.Login_Layout);

                Client a = new Client(AppSettings.TripleDesAppServiceProvider);
                Console.WriteLine(a);

                //Get Value And Set Toolbar
                InitComponent();
                InitSocialLogins();
                GetTimezone();

                if (string.IsNullOrEmpty(UserDetails.DeviceId))
                {
                    OneSignalNotification.RegisterNotificationDevice();
                }
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

        protected override void OnStop()
        {
            try
            {
                base.OnStop();
                if (MGoogleApiClient.IsConnected) MGoogleApiClient.Disconnect();
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
                MprofileTracker?.StopTracking();
                DestroyBasic();
                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
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

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                RegularTxt = Typeface.CreateFromAsset(Assets, "fonts/SF-UI-Display-Regular.ttf");
                //SemiboldTxt = Typeface.CreateFromAsset(Assets, "fonts/SF-UI-Display-Semibold.ttf");

                //declare layouts and editText
                MEditTextEmail = (EditText)FindViewById(Resource.Id.editTxtEmail);
                MEditTextPassword = (EditText)FindViewById(Resource.Id.editTxtPassword);

                MTextViewSignUp = (TextView)FindViewById(Resource.Id.tvSignUp); // Register
                MButtonViewSignIn = (Button)FindViewById(Resource.Id.SignInButton); // Login
                 
                MTextViewForgotPwd = (TextView)FindViewById(Resource.Id.tvForgotPwd); // Forget password 

                LayoutCreateAccount = (LinearLayout)FindViewById(Resource.Id.layout_create_account);  
                MTextViewCreateAccount = (TextView)FindViewById(Resource.Id.tvCreateAccount);

                ProgressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);
                ProgressBar.Visibility = ViewStates.Gone;
                MButtonViewSignIn.Visibility = ViewStates.Visible;
                 
                MTextViewCreateAccount.SetTypeface(RegularTxt, TypefaceStyle.Normal);
                MTextViewForgotPwd.SetTypeface(RegularTxt, TypefaceStyle.Normal);
                MTextViewSignUp.SetTypeface(RegularTxt, TypefaceStyle.Normal);
                MEditTextEmail.SetTypeface(RegularTxt, TypefaceStyle.Normal);
                MEditTextPassword.SetTypeface(RegularTxt, TypefaceStyle.Normal);

                if (!AppSettings.EnableRegisterSystem)
                    LayoutCreateAccount.Visibility = ViewStates.Gone;
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
                    MButtonViewSignIn.Click += BtnLoginOnClick;
                    MTextViewCreateAccount.Click += RegisterButton_Click;
                    MTextViewSignUp.Click += RegisterButton_Click;
                    MTextViewForgotPwd.Click += TxtForgetpassOnClick;
                }
                else
                {
                    MButtonViewSignIn.Click -= BtnLoginOnClick;
                    MTextViewCreateAccount.Click -= RegisterButton_Click;
                    MTextViewSignUp.Click -= RegisterButton_Click;
                    MTextViewForgotPwd.Click -= TxtForgetpassOnClick;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void InitSocialLogins()
        {
            try
            {
                //#Facebook
                if (AppSettings.ShowFacebookLogin)
                {
                    //FacebookSdk.SdkInitialize(this);

                    MprofileTracker = new FbMyProfileTracker();
                    MprofileTracker.MOnProfileChanged += MprofileTrackerOnMOnProfileChanged;
                    MprofileTracker.StartTracking();

                    BtnFbLogin = FindViewById<LoginButton>(Resource.Id.fblogin_button);
                    BtnFbLogin.Visibility = ViewStates.Visible;
                    BtnFbLogin.SetPermissions(new List<string>
                    {
                        "email",
                        "public_profile"
                    });

                    MFbCallManager = CallbackManagerFactory.Create();
                    BtnFbLogin.RegisterCallback(MFbCallManager, this);

                    //FB accessToken
                    var accessToken = AccessToken.CurrentAccessToken;
                    var isLoggedIn = accessToken != null && !accessToken.IsExpired;
                    if (isLoggedIn && Profile.CurrentProfile != null)
                    {
                        LoginManager.Instance.LogOut();
                    }

                    string hash = Methods.App.GetKeyHashesConfigured(this);
                    Console.WriteLine(hash);
                }
                else
                {
                    BtnFbLogin = FindViewById<LoginButton>(Resource.Id.fblogin_button);
                    BtnFbLogin.Visibility = ViewStates.Gone;
                }

                //#Google
                if (AppSettings.ShowGoogleLogin)
                { 
                    MGoogleSignIn = FindViewById<SignInButton>(Resource.Id.Googlelogin_button);
                    MGoogleSignIn.Click += GoogleSignInButtonOnClick;
                }
                else
                {
                    MGoogleSignIn = FindViewById<SignInButton>(Resource.Id.Googlelogin_button);
                    MGoogleSignIn.Visibility = ViewStates.Gone;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //Login With Facebook
        private void MprofileTrackerOnMOnProfileChanged(object sender, ProfileChangedEventArgs e)
        {
            try
            {
                if (e.MProfile != null)
                {
                   //var FbFirstName = e.MProfile.FirstName;
                   //var FbLastName = e.MProfile.LastName;
                   //var FbName = e.MProfile.Name;
                   //var FbProfileId = e.MProfile.Id;
                    
                    var request = GraphRequest.NewMeRequest(AccessToken.CurrentAccessToken, this);
                    var parameters = new Bundle();
                    parameters.PutString("fields", "id,name,age_range,email");
                    request.Parameters = parameters;
                    request.ExecuteAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        //Login With Google
        private void GoogleSignInButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                // Configure sign-in to request the user's ID, email address, and basic profile. ID and basic profile are included in DEFAULT_SIGN_IN.
                var gso = new GoogleSignInOptions.Builder(GoogleSignInOptions.DefaultSignIn)
                    .RequestIdToken(AppSettings.ClientId)
                    .RequestScopes(new Scope(Scopes.Profile))
                    .RequestScopes(new Scope(Scopes.PlusMe))
                    .RequestScopes(new Scope(Scopes.DriveAppfolder))
                    .RequestServerAuthCode(AppSettings.ClientId)
                    .RequestProfile().RequestEmail().Build();

                // Build a GoogleApiClient with access to the Google Sign-In API and the options specified by gso.
                MGoogleApiClient ??= new GoogleApiClient.Builder(this, this, this)
                    .EnableAutoManage(this, this)
                    .AddApi(Auth.GOOGLE_SIGN_IN_API, gso)
                    .Build();
                 
                MGoogleApiClient.Connect();

                var opr = Auth.GoogleSignInApi.SilentSignIn(MGoogleApiClient);
                if (opr.IsDone)
                {
                    // If the user's cached credentials are valid, the OptionalPendingResult will be "done"
                    // and the GoogleSignInResult will be available instantly.
                    Log.Debug("Login_Activity", "Got cached sign-in");
                    var result = opr.Get() as GoogleSignInResult;
                    HandleSignInResult(result);

                    //Auth.GoogleSignInApi.SignOut(mGoogleApiClient).SetResultCallback(this);
                }
                else
                {
                    // If the user has not previously signed in on this device or the sign-in has expired,
                    // this asynchronous branch will attempt to sign in the user silently.  Cross-device
                    // single sign-on will occur in this branch.
                    opr.SetResultCallback(new SignInResultCallback { Activity = this });
                }

                // Check if we're running on Android 5.0 or higher
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    if (!MGoogleApiClient.IsConnecting)
                        ResolveSignInError();
                    else if (MGoogleApiClient.IsConnected) MGoogleApiClient.Disconnect();
                }
                else
                {
                    if (CheckSelfPermission(Manifest.Permission.GetAccounts) == Permission.Granted &&
                        CheckSelfPermission(Manifest.Permission.UseCredentials) == Permission.Granted)
                    {
                        if (!MGoogleApiClient.IsConnecting)
                            ResolveSignInError();
                        else if (MGoogleApiClient.IsConnected) MGoogleApiClient.Disconnect();
                    }
                    else
                    {
                        new PermissionsController(this).RequestPermission(106);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void DestroyBasic()
        {
            try
            {
                MTextViewForgotPwd = null;
                MTextViewCreateAccount = null;
                MTextViewSignUp = null;
                RegularTxt = null;
                MEditTextEmail = null;
                MEditTextPassword = null;
                MButtonViewSignIn = null;
                LayoutCreateAccount = null;
                LayoutCreateAccount = null;
                ProgressBar = null;
                BtnFbLogin = null;
                MFbCallManager = null;
                MGoogleSignIn = null;
                TimeZone = null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        #endregion

        #region Events

        //Click Button Login
        private async void BtnLoginOnClick(object sender, EventArgs eventArgs)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security),
                        GetText(Resource.String.Lbl_CheckYourInternetConnection), GetText(Resource.String.Lbl_Ok));
                }
                else
                {
                    if (!string.IsNullOrEmpty(MEditTextEmail.Text.Replace(" ", "")) || !string.IsNullOrEmpty(MEditTextPassword.Text))
                    { 
                        ProgressBar.Visibility = ViewStates.Visible;
                        MButtonViewSignIn.Visibility = ViewStates.Gone;

                        if (string.IsNullOrEmpty(TimeZone))
                            GetTimezone();

                        var (apiStatus, respond) = await RequestsAsync.Global.Get_Auth(MEditTextEmail.Text.Replace(" ", ""), MEditTextPassword.Text, TimeZone, UserDetails.DeviceId);
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

                                ProgressBar.Visibility = ViewStates.Gone;
                                MButtonViewSignIn.Visibility = ViewStates.Visible;
                                Finish();
                            }
                            else if (respond is AuthMessageObject messageObject)
                            {
                                UserDetails.Username = MEditTextEmail.Text;
                                UserDetails.FullName = MEditTextEmail.Text;
                                UserDetails.Password = MEditTextPassword.Text;
                                UserDetails.UserId = messageObject.UserId;
                                UserDetails.Status = "Pending";
                                UserDetails.Email = MEditTextEmail.Text;

                                //Insert user data to database
                                var user = new DataTables.LoginTb
                                {
                                    UserId = UserDetails.UserId,
                                    AccessToken = "",
                                    Cookie = "",
                                    Username = MEditTextEmail.Text,
                                    Password = MEditTextPassword.Text,
                                    Status = "Pending",
                                    Lang = "",
                                    DeviceId = UserDetails.DeviceId,
                                };
                                ListUtils.DataUserLoginList.Add(user);

                                var dbDatabase = new SqLiteDatabase();
                                dbDatabase.InsertOrUpdateLogin_Credentials(user);
                                dbDatabase.Dispose();

                                Intent newIntent = new Intent(this, typeof(VerificationCodeActivity));
                                newIntent.PutExtra("TypeCode", "TwoFactor");
                                StartActivity(newIntent);
                            }
                        }
                        else if (apiStatus == 400)
                        {
                            if (respond is ErrorObject error)
                            {
                                var errorText = error.Error.ErrorText;
                                var errorId = error.Error.ErrorId;
                                if (errorId == "3")
                                    Methods.DialogPopup.InvokeAndShowDialog(this,
                                        GetText(Resource.String.Lbl_Security),
                                        GetText(Resource.String.Lbl_ErrorLogin_3), GetText(Resource.String.Lbl_Ok));
                                else if (errorId == "4")
                                    Methods.DialogPopup.InvokeAndShowDialog(this,
                                        GetText(Resource.String.Lbl_Security),
                                        GetText(Resource.String.Lbl_ErrorLogin_4), GetText(Resource.String.Lbl_Ok));
                                else if (errorId == "5")
                                    Methods.DialogPopup.InvokeAndShowDialog(this,
                                        GetText(Resource.String.Lbl_Security),
                                        GetText(Resource.String.Lbl_ErrorLogin_5), GetText(Resource.String.Lbl_Ok));
                                else
                                    Methods.DialogPopup.InvokeAndShowDialog(this,
                                        GetText(Resource.String.Lbl_Security), errorText,
                                        GetText(Resource.String.Lbl_Ok));
                            }

                            ProgressBar.Visibility = ViewStates.Gone;
                            MButtonViewSignIn.Visibility = ViewStates.Visible;
                        }
                        else if (apiStatus == 404)
                        { 
                            ProgressBar.Visibility = ViewStates.Gone;
                            MButtonViewSignIn.Visibility = ViewStates.Visible;
                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), respond.ToString(), GetText(Resource.String.Lbl_Ok));
                        }
                    }
                    else
                    {
                        ProgressBar.Visibility = ViewStates.Gone;
                        MButtonViewSignIn.Visibility = ViewStates.Visible;
                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security),
                            GetText(Resource.String.Lbl_Please_enter_your_data), GetText(Resource.String.Lbl_Ok));
                    }
                }
            }
            catch (Exception exception)
            {
                ProgressBar.Visibility = ViewStates.Gone;
                MButtonViewSignIn.Visibility = ViewStates.Visible;
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), exception.Message,GetText(Resource.String.Lbl_Ok));
                Console.WriteLine(exception);
            }
        }

        //Click Button Register
        private void RegisterButton_Click(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(this, typeof(RegisterActivity)));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Click Forget Password
        private void TxtForgetpassOnClick(object sender, EventArgs eventArgs)
        {
            try
            {
                StartActivity(typeof(ForgetPasswordActivity));
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

                Log.Debug("Login_Activity", "onActivityResult:" + requestCode + ":" + resultCode + ":" + data);
                if (requestCode == 0)
                {
                    var result = Auth.GoogleSignInApi.GetSignInResultFromIntent(data);
                    HandleSignInResult(result);
                }
                else
                {
                    // Logins Facebook
                    MFbCallManager.OnActivityResult(requestCode, (int) resultCode, data);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //Permissions
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions,  Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                if (requestCode == 106)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        if (!MGoogleApiClient.IsConnecting)
                            ResolveSignInError();
                        else if (MGoogleApiClient.IsConnected) MGoogleApiClient.Disconnect();
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long)
                            .Show();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Social Logins

        private string FbAccessToken,GAccessToken,GServerCode;

        #region Facebook

        public void OnCancel()
        {
            try
            {
                ProgressBar.Visibility = ViewStates.Gone;
                MButtonViewSignIn.Visibility = ViewStates.Visible;

                SetResult(Result.Canceled);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public void OnError(FacebookException error)
        {
            try
            {

                ProgressBar.Visibility = ViewStates.Gone;
                MButtonViewSignIn.Visibility = ViewStates.Visible;

                // Handle exception
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), error.Message, GetText(Resource.String.Lbl_Ok));

                SetResult(Result.Canceled);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public void OnSuccess(Object result)
        {
            try
            {
                //var loginResult = result as LoginResult;
                //var id = AccessToken.CurrentAccessToken.UserId;

                ProgressBar.Visibility = ViewStates.Visible;
                MButtonViewSignIn.Visibility = ViewStates.Gone;

                SetResult(Result.Ok);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public async void OnCompleted(JSONObject json, GraphResponse response)
        {
            try
            {
                //var data = json.ToString();
                //var result = JsonConvert.DeserializeObject<FacebookResult>(data);
                //var FbEmail = result.Email;

                var accessToken = AccessToken.CurrentAccessToken;
                if (accessToken != null)
                {
                    FbAccessToken = accessToken.Token;

                    var (apiStatus, respond) = await RequestsAsync.Global.Get_SocialLogin(FbAccessToken, "facebook", UserDetails.DeviceId);
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
                            ProgressBar.Visibility = ViewStates.Gone;
                            MButtonViewSignIn.Visibility = ViewStates.Visible; 
                        }
                        Finish();
                    }
                    else if (apiStatus == 400)
                    {
                        if (respond is ErrorObject error)
                        {
                            var errorText = error.Error.ErrorText;
                             
                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), errorText, GetText(Resource.String.Lbl_Ok));
                        }
                    }
                    else if (apiStatus == 404)
                    {
                        var error = respond.ToString();
                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), error, GetText(Resource.String.Lbl_Ok));
                    }

                    ProgressBar.Visibility = ViewStates.Gone;
                    MButtonViewSignIn.Visibility = ViewStates.Visible;
                } 
            }
            catch (Exception exception)
            {
                ProgressBar.Visibility = ViewStates.Gone;
                MButtonViewSignIn.Visibility = ViewStates.Visible;
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), exception.Message, GetText(Resource.String.Lbl_Ok));
                Console.WriteLine(exception);
            }
        }
         
        #endregion

        //======================================================

        #region Google
             
        public void HandleSignInResult(GoogleSignInResult result)
        {
            try
            { 
                Log.Debug("Login_Activity", "handleSignInResult:" + result.IsSuccess);
                if (result.IsSuccess)
                {
                    // Signed in successfully, show authenticated UI.
                    var acct = result.SignInAccount;
                    SetContentGoogle(acct);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void ResolveSignInError()
        {
            try
            {
                if (MGoogleApiClient.IsConnecting) return;

                var signInIntent = Auth.GoogleSignInApi.GetSignInIntent(MGoogleApiClient);
                StartActivityForResult(signInIntent, 0);
            }
            catch (IntentSender.SendIntentException io)
            {
                //The intent was cancelled before it was sent. Return to the default
                //state and attempt to connect to get an updated ConnectionResult
                Console.WriteLine(io);
                MGoogleApiClient.Connect();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void OnConnected(Bundle connectionHint)
        {
            try
            {
                var opr = Auth.GoogleSignInApi.SilentSignIn(MGoogleApiClient);
                if (opr.IsDone)
                {
                    // If the user's cached credentials are valid, the OptionalPendingResult will be "done"
                    // and the GoogleSignInResult will be available instantly.
                    Log.Debug("Login_Activity", "Got cached sign-in");
                    var result = opr.Get() as GoogleSignInResult;
                    HandleSignInResult(result);
                }
                else
                {
                    // If the user has not previously signed in on this device or the sign-in has expired,
                    // this asynchronous branch will attempt to sign in the user silently.  Cross-device
                    // single sign-on will occur in this branch.

                    opr.SetResultCallback(new SignInResultCallback {Activity = this});
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private async void SetContentGoogle(GoogleSignInAccount acct)
        {
            try
            {
                //Successful log in hooray!!
                if (acct != null)
                {
                    ProgressBar.Visibility = ViewStates.Visible;
                    MButtonViewSignIn.Visibility = ViewStates.Gone;

                    //var GAccountName = acct.Account.Name;
                    //var GAccountType = acct.Account.Type;
                    //var GDisplayName = acct.DisplayName;
                    //var GFirstName = acct.GivenName;
                    //var GLastName = acct.FamilyName;
                    //var GProfileId = acct.Id;
                    //var GEmail = acct.Email;
                    //var GImg = acct.PhotoUrl.Path;
                    GAccessToken = acct.IdToken;
                    GServerCode = acct.ServerAuthCode;
                    Console.WriteLine(GServerCode);

                    if (!string.IsNullOrEmpty(GAccessToken))
                    {
                        var (apiStatus, respond) = await RequestsAsync.Global.Get_SocialLogin(GAccessToken, "google", UserDetails.DeviceId);
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
                                ProgressBar.Visibility = ViewStates.Gone;
                                MButtonViewSignIn.Visibility = ViewStates.Visible; 
                            }
                            Finish();
                        }
                        else if (apiStatus == 400)
                        {
                            if (respond is ErrorObject error)
                            {
                                var errorText = error.Error.ErrorText;

                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), errorText, GetText(Resource.String.Lbl_Ok));
                            }
                        }
                        else if (apiStatus == 404)
                        {
                            var error = respond.ToString();
                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), error, GetText(Resource.String.Lbl_Ok));
                        }

                        ProgressBar.Visibility = ViewStates.Gone;
                        MButtonViewSignIn.Visibility = ViewStates.Visible;
                    }
                }
            }
            catch (Exception exception)
            {
                ProgressBar.Visibility = ViewStates.Gone;
                MButtonViewSignIn.Visibility = ViewStates.Visible;
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), exception.Message,GetText(Resource.String.Lbl_Ok));
                Console.WriteLine(exception);
            }
        }

        public void OnConnectionSuspended(int cause)
        {
            try
            {
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void OnConnectionFailed(ConnectionResult result)
        {
            try
            {
                // An unresolvable error has occurred and Google APIs (including Sign-In) will not
                // be available.
                Log.Debug("Login_Activity", "onConnectionFailed:" + result);

                //The user has already clicked 'sign-in' so we attempt to resolve all
                //errors until the user is signed in, or the cancel
                ResolveSignInError();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
         
        public void OnResult(Object result)
        {
            try
            {
                 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        //======================================================

        #endregion

        private async void GetTimezone()
        {
            try
            {
                if (Methods.CheckConnectivity())
                    TimeZone = await ApiRequest.GetTimeZoneAsync().ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void SetDataLogin(AuthObject auth)
        {
            try
            {
                Current.AccessToken = auth.AccessToken;

                UserDetails.Username = MEditTextEmail.Text;
                UserDetails.FullName = MEditTextEmail.Text;
                UserDetails.Password = MEditTextPassword.Text;
                UserDetails.AccessToken = auth.AccessToken;
                UserDetails.UserId = auth.UserId;
                UserDetails.Status = "Pending";
                UserDetails.Cookie = auth.AccessToken;
                UserDetails.Email = MEditTextEmail.Text;
                
                //Insert user data to database
                var user = new DataTables.LoginTb
                {
                    UserId = UserDetails.UserId,
                    AccessToken = UserDetails.AccessToken,
                    Cookie = UserDetails.Cookie,
                    Username = MEditTextEmail.Text,
                    Password = MEditTextPassword.Text,
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