using System;
using System.Collections.ObjectModel;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using Bumptech.Glide.Integration.RecyclerView;
using Bumptech.Glide.Util;
using Newtonsoft.Json;
using ObeeNetwork.Activities.Album.Adapters;
using ObeeNetwork.Helpers.Controller;
using ObeeNetwork.Helpers.Utils;
using ObeeNetworkClient.Classes.Album;
using ObeeNetworkClient.Classes.Posts;
using ObeeNetworkClient.Requests;
using Console = System.Console;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using Uri = Android.Net.Uri;

namespace ObeeNetwork.Activities.Album
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class AddImageToAlbumActivity : AppCompatActivity
    {
        #region Variables Basic

        private CollapsingToolbarLayout CollapsingToolbar;
        private TextView ToolbarTitle , AddImage;

        private EditText TxtAlbumName;
        private PhotosAdapter MAdapter;
        private RecyclerView MRecycler;
        private GridLayoutManager LayoutManager;

        private Button PublishButton;
        private PostDataObject ImageData;
        private ObservableCollection<Attachments> PathImage = new ObservableCollection<Attachments>();
        
        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                Methods.App.FullScreenApp(this);

                // Create your application here
                SetContentView(Resource.Layout.AddImageToAlbumLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();

                Get_DataImage();
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
            catch (Exception e)
            {
                Console.WriteLine(e);
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
                CollapsingToolbar = FindViewById<CollapsingToolbarLayout>(Resource.Id.collapsing_toolbar);
                CollapsingToolbar.Title = "";

                ToolbarTitle = FindViewById<TextView>(Resource.Id.toolbar_title);
                AddImage = FindViewById<TextView>(Resource.Id.addImage);
                TxtAlbumName = FindViewById<EditText>(Resource.Id.albumName);
                MRecycler = FindViewById<RecyclerView>(Resource.Id.recycle);
                PublishButton = FindViewById<Button>(Resource.Id.publishButton);
                
                Methods.SetColorEditText(TxtAlbumName, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                PathImage = new ObservableCollection<Attachments>();
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

        private void SetRecyclerViewAdapters()
        {
            try
            {
                MAdapter = new PhotosAdapter(this)
                {
                    PhotosList = new ObservableCollection<PhotoAlbumObject>()
                };
                LayoutManager = new GridLayoutManager(this, 2);
                LayoutManager.SetSpanSizeLookup(new MySpanSizeLookup(4, 1, 1)); //5, 1, 2
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<PhotoAlbumObject>(this, MAdapter, sizeProvider, 10);
                MRecycler.AddOnScrollListener(preLoader);
                MRecycler.SetAdapter(MAdapter); 
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
                    AddImage.Click += AddImageOnClick;
                    PublishButton.Click += PublishButtonOnClick;
                }
                else
                {
                    AddImage.Click -= AddImageOnClick;
                    PublishButton.Click -= PublishButtonOnClick;
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
                CollapsingToolbar = null;
                MAdapter = null;
                ToolbarTitle = null;
                AddImage = null;
                TxtAlbumName = null;
                MAdapter = null;
                MRecycler = null;
                LayoutManager = null;
                PublishButton = null;
                ImageData = null;
                PathImage = null; 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Events

        //Add image
        private void AddImageOnClick(object sender, EventArgs e)
        {
            try
            {
                // Check if we're running on Android 5.0 or higher 
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    //requestCode >> 500 => Image Gallery
                    new IntentController(this).OpenIntentImageGallery(GetText(Resource.String.Lbl_SelectPictures));
                }
                else
                {
                    if (CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted && CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted
                                                                                              && CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted)
                    {

                        //requestCode >> 500 => Image Gallery
                        new IntentController(this).OpenIntentImageGallery(GetText(Resource.String.Lbl_SelectPictures));
                    }
                    else
                    {
                        new PermissionsController(this).RequestPermission(108);
                    }
                }

            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Publish New image => send api 
        private async void PublishButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                    return;
                }

                if (PathImage?.Count == 0)
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_Please_select_Image), ToastLength.Short).Show();
                    return;
                }

                //Show a progress
                AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

                var (apiStatus, respond) = await RequestsAsync.Album.AddImageToAlbumAsync(ImageData.PostId, PathImage);
                if (apiStatus == 200)
                {
                    if (respond is CreateAlbumObject result)
                    {
                        //Add new item to list
                        if (result.Data?.PhotoAlbum?.Count > 0)
                        {
                            if (result.Data.PhotoAlbum.Count > 0)
                            {
                                AndHUD.Shared.ShowSuccess(this, "" , MaskType.Clear, TimeSpan.FromSeconds(2));

                                //AlbumItem >> PostDataObject  
                                Intent returnIntent = new Intent();
                                returnIntent.PutExtra("AlbumItem", JsonConvert.SerializeObject(result.Data));
                                SetResult(Result.Ok, returnIntent);
                                Finish();
                            } 
                        } 
                    }
                }
                else
                {
                    Methods.DisplayAndHUDErrorResult(this, respond);
                } 
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                AndHUD.Shared.Dismiss(this);
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

                if (requestCode == 500 && resultCode == Result.Ok) // Add image 
                {
                    if (data.ClipData != null)
                    {
                        var mClipData = data.ClipData;
                        for (var i = 0; i < mClipData.ItemCount; i++)
                        {
                            var item = mClipData.GetItemAt(i);
                            Uri uri = item.Uri;
                            var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, uri);
                            PickiTonCompleteListener(filepath);
                        }
                    }
                    else
                    {
                        Uri uri = data.Data;
                        var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, uri);
                        PickiTonCompleteListener(filepath);
                    }
                }
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

                if (requestCode == 108)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        //requestCode >> 500 => Image Gallery
                        new IntentController(this).OpenIntentImageGallery(GetText(Resource.String.Lbl_SelectPictures));
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

        #region Path

        private void PickiTonCompleteListener(string path)
        {
            try
            {
                //  Chick if it was successful
                var check = ObeeNetworkTools.CheckMimeTypesWithServer(path);
                if (!check)
                {
                    //this file not supported on the server , please select another file 
                    Toast.MakeText(this, GetString(Resource.String.Lbl_ErrorFileNotSupported), ToastLength.Short).Show();
                    return;
                }

                var type = Methods.AttachmentFiles.Check_FileExtension(path);
                if (type == "Image")
                {
                    MAdapter.PhotosList.Add(new PhotoAlbumObject()
                    {
                        Image = path
                    });
                    MAdapter.NotifyDataSetChanged();

                    PathImage.Add(new Attachments
                    {
                        Id = MAdapter.PhotosList.Count + 1,
                        TypeAttachment = "postPhotos[]",
                        FileSimple = path,
                        FileUrl = path
                    });
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_Failed_to_load), ToastLength.Short).Show();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        //Get Data 
        private void Get_DataImage()
        {
            try
            {
                ImageData = JsonConvert.DeserializeObject<PostDataObject>(Intent.GetStringExtra("AlbumObject"));
                if (ImageData != null)
                {
                    ToolbarTitle.Text = Methods.FunString.DecodeString(ImageData.AlbumName);

                    MAdapter.PhotosList = new ObservableCollection<PhotoAlbumObject>(ImageData.PhotoAlbum);
                    MAdapter.NotifyDataSetChanged(); 
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

    }
}