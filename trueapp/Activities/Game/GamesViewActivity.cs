using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Ads;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using Newtonsoft.Json;
using ObeeNetwork.Helpers.Controller;
using ObeeNetwork.Helpers.Utils;
using ObeeNetworkClient.Classes.Games;
using ObeeNetworkClient.Requests;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace ObeeNetwork.Activities.Games
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class GamesViewActivity : AppCompatActivity
    {
        #region Variables Basic

        private static SwipeRefreshLayout SwipeRefreshLayout;
        private static WebView HybridView;
        private AdView MAdView;
        private GamesDataObject DataObject;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                 
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);

                View mContentView = Window.DecorView;
                var uiOptions = (int)mContentView.SystemUiVisibility;
                var newUiOptions = uiOptions;

                newUiOptions |= (int)SystemUiFlags.Fullscreen;
                newUiOptions |= (int)SystemUiFlags.HideNavigation;
                mContentView.SystemUiVisibility = (StatusBarVisibility)newUiOptions;

                Window.AddFlags(WindowManagerFlags.Fullscreen);
                 
                //ScreenOrientation.Portrait >>  Make to run your application only in portrait mode
                //ScreenOrientation.Landscape >> Make to run your application only in LANDSCAPE mode 
                RequestedOrientation = ScreenOrientation.Landscape;

                // Create your application here
                SetContentView(Resource.Layout.LocalWebViewLayout);
                 
                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetWebView();
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

                MAdView?.Resume();
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

                MAdView?.Pause();
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
                HybridView = FindViewById<WebView>(Resource.Id.LocalWebView);
                
                SwipeRefreshLayout = (SwipeRefreshLayout)FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = true;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));


                MAdView = FindViewById<AdView>(Resource.Id.adView);
                MAdView.Visibility = ViewStates.Gone;
                //AdsGoogle.InitAdView(MAdView, null);
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
                    toolbar.Visibility = ViewStates.Gone;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void SetWebView()
        {
            try
            {
                //Set WebView and Load url to be rendered on WebView
                if (!Methods.CheckConnectivity())
                {
                    SwipeRefreshLayout.Refreshing = false; 
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long).Show();
                }
                else
                {
                    DataObject = JsonConvert.DeserializeObject<GamesDataObject>(Intent.GetStringExtra("ItemObject"));
                    if (DataObject != null)
                    {
                        var url = "https://www.miniclip.com/games/" + DataObject.GameLink + "/en/webgame.php";
                        var frame = "<iframe src='" + url + "' id='iframe-game' name='iframe-game' class='loader-game-frame' width='100%' height='100%' border='0' frameborder='0' scrolling='no' allow='autoplay; fullscreen' allowfullscreen='true' style='border: none; height: -webkit-fill-available; min-height: 600px;'></iframe>";

                        string style = AppSettings.SetTabDarkTheme ? "<style type='text/css'>body{color: #fff; background-color: #444;}</style>" : "<style type='text/css'>body{color: #444; background-color: #fff;}</style>";

                        string data = "<!DOCTYPE html>";
                        data += "<head><title></title>" + style +
                                "<script type='text/javascript' src ='https://static.miniclipcdn.com/js/game-embed.js'></script>" +
                                "<script type='text/javascript' src ='https://static.miniclipcdn.com/js/mc.js'></script>" +
                                "<script type='text/javascript' src ='https://static.miniclipcdn.com/js/currency.js'></script>" +
                                "<script type='text/javascript' src ='https://static.miniclipcdn.com/js/currency/miniclip.js'></script>" +
                                "</head>";
                        data += "<body>" + frame + "</body>";
                        data += "</html>";
                         
                        WebSettings webSettings = HybridView.Settings;
                        webSettings.JavaScriptEnabled = true;

                        HybridView.HorizontalScrollBarEnabled = false;
                        HybridView.VerticalScrollBarEnabled = false;
                        HybridView.ScrollbarFadingEnabled = false;
                        HybridView.SetScrollContainer(false);  

                        webSettings.SetSupportZoom(false);
                        webSettings.BuiltInZoomControls = false;
                        webSettings.DisplayZoomControls = false;

                        HybridView.SetWebViewClient(new MyWebViewClient(this));
                        HybridView.SetWebChromeClient(new WebChromeClient());
                        HybridView.SetInitialScale(1);
                        webSettings.AllowFileAccess = true;
                        webSettings.SetPluginState(WebSettings.PluginState.On);
                        webSettings.SetPluginState(WebSettings.PluginState.OnDemand);
                        webSettings.LoadWithOverviewMode = true;
                        webSettings.UseWideViewPort = true;
                        webSettings.DomStorageEnabled = true;
                        webSettings.LoadsImagesAutomatically = true;
                        webSettings.JavaScriptCanOpenWindowsAutomatically = true;
                        webSettings.SetLayoutAlgorithm(WebSettings.LayoutAlgorithm.SingleColumn);
                        HybridView.ScrollBarStyle = ScrollbarStyles.InsideOverlay;
                         
                        string desktopUserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/37.0.2049.0 Safari/537.36";
                        //string mobileUserAgent = "Mozilla/5.0 (Linux; U; Android 4.4; en-us; Nexus 4 Build/JOP24G) AppleWebKit/534.30 (KHTML, like Gecko) Version/4.0 Mobile Safari/534.30";

                        //Choose Mobile/Desktop client. 
                        webSettings.UserAgentString = desktopUserAgent;
                         
                        //Load url to be rendered on WebView
                        HybridView.LoadDataWithBaseURL(null, data, "text/html", "UTF-8", null);

                        if (DataObject.Active != "1")
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Games.AddToMyGames(DataObject.Id)  }); 
                    } 
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
                    //HybridView.Touch += HybridViewOnTouch;
                    SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
                }
                else
                {
                    //HybridView.Touch -= HybridViewOnTouch;
                    SwipeRefreshLayout.Refresh -= SwipeRefreshLayoutOnRefresh;
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
                MAdView?.Destroy();

                HybridView = null;
                SwipeRefreshLayout = null;
                DataObject = null;
                MAdView = null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        #endregion

        #region Events

        //Event Refresh Data Page
        private void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
        {
            try
            {
                HybridView.Reload();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
          
        //private void HybridViewOnTouch(object sender, View.TouchEventArgs e)
        //{
        //    try
        //    {
        //        if (e.Event.Action != MotionEventActions.Move) return;
        //    }
        //    catch (Exception exception)
        //    {
        //        Console.WriteLine(exception);
        //    }
        //}


        #endregion

        private class MyWebViewClient : WebViewClient, IValueCallback
        {
            private readonly Activity MActivity;
            public MyWebViewClient(Activity mActivity)
            {
                MActivity = mActivity;
            }

            public override bool ShouldOverrideUrlLoading(WebView view, IWebResourceRequest request)
            {
                view.LoadUrl(request.Url.ToString());
                return true;
            }

            public override void OnPageStarted(WebView view, string url, Bitmap favicon)
            {
                try
                {
                    base.OnPageStarted(view, url, favicon);
                    SwipeRefreshLayout.Refreshing = true;
                    SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            public override void OnPageFinished(WebView view, string url)
            {
                try
                {
                    base.OnPageFinished(view, url);
                    SwipeRefreshLayout.Refreshing = false;
                    SwipeRefreshLayout.Enabled = false;

                    //const string js = "javascript:" +
                    //                  "$('.header-container').hide();" +
                    //                  "$('.footer-wrapper').hide();" +
                    //                  "$('.content-container').css('margin-top', '0');" +
                    //                  "$('.wo_about_wrapper_parent').css('top', '0');";

                    //if (Build.VERSION.SdkInt >= (BuildVersionCodes)19)
                    //{
                    //    view.EvaluateJavascript(js, this);
                    //}
                    //else
                    //{
                    //    view.LoadUrl(js);
                    //}
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            public override void OnReceivedError(WebView view, IWebResourceRequest request, WebResourceError error)
            {
                try
                {
                    base.OnReceivedError(view, request, error);
                    SwipeRefreshLayout.Refreshing = false;
                    SwipeRefreshLayout.Enabled = false;
                    //const string js = "javascript:" +
                    //                  "$('.header-container').hide();" +
                    //                  "$('.footer-wrapper').hide();" +
                    //                  "$('.content-container').css('margin-top', '0');" +
                    //                  "$('.wo_about_wrapper_parent').css('top', '0');";

                    //if (Build.VERSION.SdkInt >= (BuildVersionCodes)19)
                    //{
                    //    view.EvaluateJavascript(js, this);
                    //}
                    //else
                    //{
                    //    view.LoadUrl(js);
                    //}
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            public void OnReceiveValue(Java.Lang.Object value)
            {
                try
                {

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
    }
}