using System;
using System.Collections.ObjectModel;
using System.Linq;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using Com.Theartofdev.Edmodo.Cropper;
using Newtonsoft.Json;
using ObeeNetwork.Activities.AddPost.Adapters;
using ObeeNetwork.Helpers.Controller;
using ObeeNetwork.Helpers.Fonts;
using ObeeNetwork.Helpers.Utils;
using ObeeNetworkClient.Classes.Album;
using ObeeNetworkClient.Classes.Global;
using ObeeNetworkClient.Classes.Posts;
using ObeeNetworkClient.Requests;
using Console = System.Console;
using Exception = System.Exception;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using Uri = Android.Net.Uri;

namespace ObeeNetwork.Activities.Album
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class CreateAlbumActivity : AppCompatActivity
    {
        #region Variables Basic

        private AttachmentsAdapter MAdapter;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private EditText TxtNameAlbum;
        private TextView TxtAdd, IconName;
        

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
                SetContentView(Resource.Layout.CreateAlbumLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters(); 
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
                MRecycler = (RecyclerView)FindViewById(Resource.Id.recycler);
                TxtNameAlbum = (EditText)FindViewById(Resource.Id.NameEditText);

                IconName = FindViewById<TextView>(Resource.Id.IconName);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconName, FontAwesomeIcon.User);
                 
                TxtAdd = FindViewById<TextView>(Resource.Id.toolbar_title);
                TxtAdd.Text = GetText(Resource.String.Lbl_Create);

                Methods.SetColorEditText(TxtNameAlbum, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                
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
                    toolbar.Title = GetString(Resource.String.Lbl_CreateAlbum);
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
                MAdapter = new AttachmentsAdapter(this) {AttachmentList = new ObservableCollection<Attachments>()};
                LayoutManager = new LinearLayoutManager(this, LinearLayoutManager.Horizontal, false);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.SetAdapter(MAdapter);

                MRecycler.Visibility = ViewStates.Visible;

                // Add first image Default 
                var attach = new Attachments
                {
                    Id = MAdapter.AttachmentList.Count + 1,
                    TypeAttachment = "Default",
                    FileSimple = "addImage",
                    FileUrl = "addImage"
                };

                MAdapter.Add(attach);
                MAdapter.NotifyDataSetChanged();
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
                    MAdapter.DeleteItemClick += MAdapterOnDeleteItemClick;
                    MAdapter.ItemClick += MAdapterOnItemClick;
                    TxtAdd.Click += TxtAddOnClick;
                }
                else
                {
                    MAdapter.DeleteItemClick -= MAdapterOnDeleteItemClick;
                    MAdapter.ItemClick -= MAdapterOnItemClick;
                    TxtAdd.Click -= TxtAddOnClick;
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
                MAdapter = null;
                MRecycler = null;
                TxtNameAlbum = null;
                TxtAdd = null;
                IconName = null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Events

        private void MAdapterOnDeleteItemClick(object sender, AttachmentsAdapterClickEventArgs e)
        {
            try
            {
                var position = e.Position;
                if (position >= 0)
                {
                    var item = MAdapter.GetItem(position);
                    if (item != null)
                    {
                        MAdapter.Remove(item);
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private async void TxtAddOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                }
                else
                {
                    if (string.IsNullOrEmpty(TxtNameAlbum.Text.Replace(" ","")))
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Please_enter_name), ToastLength.Short).Show();
                        return;
                    }

                    if (MAdapter.AttachmentList.Count <= 1)
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Please_select_Image), ToastLength.Short).Show();
                    }
                    else
                    {
                        //Show a progress
                        AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading) + "...");
                         
                        var list = MAdapter.AttachmentList.Where(a => a.TypeAttachment != "Default").ToList(); 
                        var (apiStatus, respond) = await RequestsAsync.Album.CreateAlbumAsync(TxtNameAlbum.Text.Replace(" ", ""), new ObservableCollection<Attachments>(list));
                        if (apiStatus == 200)
                        {
                            if (respond is CreateAlbumObject result)
                            {
                                if (result.Data.PhotoAlbum.Count > 0)
                                {
                                    AndHUD.Shared.ShowSuccess(this);

                                    //AlbumItem >> PostDataObject  
                                    Intent returnIntent = new Intent();
                                    returnIntent.PutExtra("AlbumItem", JsonConvert.SerializeObject(result.Data));
                                    SetResult(Result.Ok, returnIntent);
                                    Finish(); 
                                }
                            } 
                        }
                        else  
                        {
                            if (respond is ErrorObject error)
                            {
                                var errorText = error.Error.ErrorText;
                                //Show a Error 
                                AndHUD.Shared.ShowError(this, errorText, MaskType.Clear, TimeSpan.FromSeconds(2));
                            }

                            Methods.DisplayReportResult(this, respond);
                        } 

                        AndHUD.Shared.Dismiss(this);
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                AndHUD.Shared.Dismiss(this);
            }
        }
         
        private void MAdapterOnItemClick(object sender, AttachmentsAdapterClickEventArgs adapterClickEvents)
        {
            try
            {
                var position = adapterClickEvents.Position;
                if (position >= 0)
                {
                    var item = MAdapter.GetItem(position);
                    if (item == null) return;
                    if (item.TypeAttachment != "Default") return;
                    // Check if we're running on Android 5.0 or higher 
                    if ((int)Build.VERSION.SdkInt < 23)
                    {
                        //if (AppSettings.ImageCropping)
                        //    OpenDialogGallery(); //requestCode >> 500 => Image Gallery
                        //else
                            new IntentController(this).OpenIntentImageGallery(GetText(Resource.String.Lbl_SelectPictures)); //requestCode >> 500 => Image Gallery
                    }
                    else
                    {
                        if (CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted && CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted
                                                                                                  && CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted)
                        {
                            //if (AppSettings.ImageCropping)
                            //    OpenDialogGallery(); //requestCode >> 500 => Image Gallery
                            //else
                                new IntentController(this).OpenIntentImageGallery(GetText(Resource.String.Lbl_SelectPictures)); //requestCode >> 500 => Image Gallery
                        }
                        else
                        {
                            new PermissionsController(this).RequestPermission(108);
                        }
                    }
                } 
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
                if (requestCode == 500 && resultCode == Result.Ok) // Add image 
                {
                    if (data.ClipData != null)
                    {
                        var mClipData = data.ClipData;
                        for (var i = 0; i < mClipData.ItemCount; i++)
                        {
                            //var item = mClipData.GetItemAt(i);
                            Uri uri = data.Data;
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
                else if (requestCode == CropImage.CropImageActivityRequestCode) // Add image 
                {
                    var result = CropImage.GetActivityResult(data);

                    if (resultCode == Result.Ok)
                    {
                        if (result.IsSuccessful)
                        {
                            var resultUri = result.Uri;

                            if (!string.IsNullOrEmpty(resultUri.Path))
                            {
                                var attach = new Attachments
                                {
                                    Id = MAdapter.AttachmentList.Count + 1,
                                    TypeAttachment = "postPhotos[]",
                                    FileSimple = resultUri.Path,
                                    FileUrl = resultUri.Path
                                };

                                MAdapter.Add(attach);
                            }
                            else
                            {
                                Toast.MakeText(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long).Show();
                            }
                        }
                        else
                        {
                            Toast.MakeText(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long).Show();
                        }
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
                        //if (AppSettings.ImageCropping)
                        //    OpenDialogGallery(); //requestCode >> 500 => Image Gallery
                        //else //requestCode >> 500 => Image Gallery
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

        //private void OpenDialogGallery()
        //{
        //    try
        //    {
        //        // Check if we're running on Android 5.0 or higher
        //        if ((int)Build.VERSION.SdkInt < 23)
        //        {
        //            Methods.Path.Chack_MyFolder();

        //            //Open Image 
        //            var myUri = Uri.FromFile(new File(Methods.Path.FolderDiskImage, Methods.GetTimestamp(DateTime.Now) + ".jpeg"));
        //            CropImage.Builder()
        //                .SetInitialCropWindowPaddingRatio(0)
        //                .SetAutoZoomEnabled(true)
        //                .SetMaxZoom(4)
        //                .SetGuidelines(CropImageView.Guidelines.On)
        //                .SetCropMenuCropButtonTitle(GetText(Resource.String.Lbl_Crop))
        //                .SetOutputUri(myUri).Start(this);
        //        }
        //        else
        //        {
        //            if (!CropImage.IsExplicitCameraPermissionRequired(this) && CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted &&
        //                CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted && CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted)
        //            {
        //                Methods.Path.Chack_MyFolder();

        //                //Open Image 
        //                var myUri = Uri.FromFile(new File(Methods.Path.FolderDiskImage, Methods.GetTimestamp(DateTime.Now) + ".jpeg"));
        //                CropImage.Builder()
        //                    .SetInitialCropWindowPaddingRatio(0)
        //                    .SetAutoZoomEnabled(true)
        //                    .SetMaxZoom(4)
        //                    .SetGuidelines(CropImageView.Guidelines.On)
        //                    .SetCropMenuCropButtonTitle(GetText(Resource.String.Lbl_Crop))
        //                    .SetOutputUri(myUri).Start(this);
        //            }
        //            else
        //            {
        //                new PermissionsController(this).RequestPermission(108);
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e);
        //    }
        //}

        #region PickiT >> Gert path file

        private void PickiTonCompleteListener(string path)
        {
            //Dismiss dialog and return the path
            try
            {
                //  Check if it was a Drive/local/unknown provider file and display a Toast
                //if (wasDriveFile) => "Drive file was selected" 
                //else if (wasUnknownProvider)  => "File was selected from unknown provider" 
                //else => "Local file was selected"

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
                    var attach = new Attachments
                    {
                        Id = MAdapter.AttachmentList.Count + 1,
                        TypeAttachment = "postPhotos[]",
                        FileSimple = path,
                        FileUrl = path
                    };

                    MAdapter.Add(attach);
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
    }
}