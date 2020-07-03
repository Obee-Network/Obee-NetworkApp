using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AFollestad.MaterialDialogs;
using Android.App;
using Android.Content;
using Android.Media;
using Android.Support.V4.Content;
using Android.Text;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using Java.Lang;
using Newtonsoft.Json;
using ObeeNetwork.Activities.Comment.Adapters;
using ObeeNetwork.Activities.NativePost.Post;
using ObeeNetwork.Helpers.Controller;
using ObeeNetwork.Helpers.Model;
using ObeeNetwork.Helpers.Utils;
using ObeeNetworkClient;
using ObeeNetworkClient.Requests;
using Exception = System.Exception;

namespace ObeeNetwork.Activities.Comment
{
    public class CommentClickListener : Java.Lang.Object , MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback, MaterialDialog.IInputCallback
    {
        private readonly Activity MainContext;
        private CommentObjectExtra CommentObject;
        private string TypeDialog;
        private readonly string TypeClass;

        public CommentClickListener(Activity context, string typeClass)
        {
            MainContext = context;
            TypeClass = typeClass;
        }

        public void ProfilePostClick(ProfileClickEventArgs e)
        {
            try
            {
                ObeeNetworkTools.OpenProfile(MainContext, e.CommentClass.UserId, e.CommentClass.Publisher);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public void MoreCommentReplyPostClick(CommentReplyClickEventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    TypeDialog = "MoreComment";
                    CommentObject = e.CommentObject;

                    var arrayAdapter = new List<string>();
                    var dialogList = new MaterialDialog.Builder(MainContext).Theme(AppSettings.SetTabDarkTheme ? Theme.Dark : Theme.Light);

                    arrayAdapter.Add(MainContext.GetString(Resource.String.Lbl_CopeText));
                    arrayAdapter.Add(MainContext.GetString(Resource.String.Lbl_Report));

                    if (CommentObject?.Owner != null && (bool)CommentObject?.Owner || CommentObject?.Publisher?.UserId == UserDetails.UserId)
                    {
                        arrayAdapter.Add(MainContext.GetString(Resource.String.Lbl_Edit));
                        arrayAdapter.Add(MainContext.GetString(Resource.String.Lbl_Delete));
                    }

                    dialogList.Title(MainContext.GetString(Resource.String.Lbl_More));
                    dialogList.Items(arrayAdapter);
                    dialogList.PositiveText(MainContext.GetText(Resource.String.Lbl_Close)).OnNegative(this);
                    dialogList.AlwaysCallSingleChoiceCallback();
                    dialogList.ItemsCallback(this).Build().Show();
                }
                else
                {
                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Event Menu >> Delete Comment
        private void DeleteCommentEvent(CommentObjectExtra item)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    TypeDialog = "DeleteComment";
                    CommentObject = item;

                    var dialog = new MaterialDialog.Builder(MainContext).Theme(AppSettings.SetTabDarkTheme ? Theme.Dark : Theme.Light);
                    dialog.Title(MainContext.GetText(Resource.String.Lbl_DeleteComment));
                    dialog.Content(MainContext.GetText(Resource.String.Lbl_AreYouSureDeleteComment));
                    dialog.PositiveText(MainContext.GetText(Resource.String.Lbl_Yes)).OnPositive(this);
                    dialog.NegativeText(MainContext.GetText(Resource.String.Lbl_No)).OnNegative(this);
                    dialog.AlwaysCallSingleChoiceCallback();
                    dialog.ItemsCallback(this).Build().Show();
                }
                else
                {
                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //Event Menu >> Edit Comment
        private void EditCommentEvent(CommentObjectExtra item)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    TypeDialog = "EditComment";
                    CommentObject = item;

                    var dialog = new MaterialDialog.Builder(MainContext).Theme(AppSettings.SetTabDarkTheme ? Theme.Dark : Theme.Light);

                    dialog.Title(Resource.String.Lbl_Edit);
                    dialog.Input(MainContext.GetString(Resource.String.Lbl_Write_comment), Methods.FunString.DecodeString(item.Text), this);
                    
                    dialog.InputType(InputTypes.TextFlagImeMultiLine);
                    dialog.PositiveText(MainContext.GetText(Resource.String.Lbl_Update)).OnPositive(this);
                    dialog.NegativeText(MainContext.GetText(Resource.String.Lbl_Cancel)).OnNegative(this);
                    dialog.Build().Show();
                    dialog.AlwaysCallSingleChoiceCallback();
                }
                else
                {
                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
          
        public void CommentReplyPostClick(CommentReplyClickEventArgs e)
        {
            try
            {
                var intent = new Intent(MainContext, typeof(ReplyCommentActivity));
                intent.PutExtra("CommentId", e.CommentObject.Id);
                intent.PutExtra("CommentObject", JsonConvert.SerializeObject(e.CommentObject));
                MainContext.StartActivity(intent);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
         
        public void OpenImageLightBox(CommentObjectExtra item)
        {
            try
            {
                if (item == null)
                    return;

                var imageUrl = !string.IsNullOrEmpty(item.CFile) && (item.CFile.Contains("file://") || item.CFile.Contains("content://") || item.CFile.Contains("storage")) ? item.CFile : Client.WebsiteUrl + "/" + item.CFile;
                 
                MainContext.RunOnUiThread(() =>
                {  
                    var fileName = imageUrl.Split('/').Last();

                    var getImage = Methods.MultiMedia.GetMediaFrom_Gallery(Methods.Path.FolderDcimImage, fileName);
                    if (getImage != "File Dont Exists")
                    {
                        Java.IO.File file2 = new Java.IO.File(getImage);
                        var photoUri = FileProvider.GetUriForFile(MainContext, MainContext.PackageName + ".fileprovider", file2);

                        Intent intent = new Intent(Intent.ActionPick);
                        intent.SetAction(Intent.ActionView);
                        intent.AddFlags(ActivityFlags.GrantReadUriPermission);
                        intent.SetDataAndType(photoUri, "image/*");
                        MainContext.StartActivity(intent);
                    }
                    else
                    {
                        string filename = imageUrl.Split('/').Last();
                        string filePath = Path.Combine(Methods.Path.FolderDcimImage);
                        string mediaFile = filePath + "/" + filename;

                        if (!Directory.Exists(filePath))
                            Directory.CreateDirectory(filePath);

                        if (!File.Exists(mediaFile))
                        {
                            WebClient webClient = new WebClient();
                            AndHUD.Shared.Show(MainContext, MainContext.GetText(Resource.String.Lbl_Loading));

                            webClient.DownloadDataAsync(new Uri(imageUrl));
                            webClient.DownloadProgressChanged += (sender, args) =>
                            {
                                //var progress = args.ProgressPercentage;
                                // holder.loadingProgressview.Progress = progress;
                                //Show a progress
                                AndHUD.Shared.Show(MainContext, MainContext.GetText(Resource.String.Lbl_Loading));
                            };
                            webClient.DownloadDataCompleted += (s, e) =>
                            {
                                try
                                {
                                    File.WriteAllBytes(mediaFile, e.Result);

                                    getImage = Methods.MultiMedia.GetMediaFrom_Gallery(Methods.Path.FolderDcimImage, fileName);
                                    if (getImage != "File Dont Exists")
                                    {
                                        Java.IO.File file2 = new Java.IO.File(getImage);

                                        Android.Net.Uri photoUri = FileProvider.GetUriForFile(MainContext, MainContext.PackageName + ".fileprovider", file2);

                                        Intent intent = new Intent(Intent.ActionPick);
                                        intent.SetAction(Intent.ActionView);
                                        intent.AddFlags(ActivityFlags.GrantReadUriPermission);
                                        intent.SetDataAndType(photoUri, "image/*");
                                        MainContext.StartActivity(intent);
                                    }
                                    else
                                    {
                                        Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long).Show();
                                    }
                                }
                                catch (Exception exception)
                                {
                                    Console.WriteLine(exception);
                                }

                                //var mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
                                //mediaScanIntent.SetData(Uri.FromFile(new File(mediaFile)));
                                //Application.Context.SendBroadcast(mediaScanIntent);

                                // Tell the media scanner about the new file so that it is
                                // immediately available to the user.
                                MediaScannerConnection.ScanFile(Application.Context, new[] { mediaFile }, null, null);
                                 
                                AndHUD.Shared.Dismiss(MainContext);
                            };
                        }
                    }
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
         
        #region MaterialDialog

        public void OnSelection(MaterialDialog p0, View p1, int itemId, ICharSequence itemString)
        {
            try
            {
                string text = itemString.ToString();
                if (text == MainContext.GetString(Resource.String.Lbl_CopeText))
                {
                    Methods.CopyToClipboard(MainContext,Methods.FunString.DecodeString(CommentObject.Text));
                }
                else if (text == MainContext.GetString(Resource.String.Lbl_Report))
                {
                    if (!Methods.CheckConnectivity())
                        Toast.MakeText(MainContext, MainContext.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                    else
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Comment.ReportCommentAsync(CommentObject.Id) });

                    Toast.MakeText(MainContext, MainContext.GetString(Resource.String.Lbl_YourReportPost), ToastLength.Short).Show();
                }
                else if (text == MainContext.GetString(Resource.String.Lbl_Edit))
                {
                    EditCommentEvent(CommentObject);
                }
                else if (text == MainContext.GetString(Resource.String.Lbl_Delete))
                {
                    DeleteCommentEvent(CommentObject);
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
                if (p1 == DialogAction.Positive)
                {
                    if (TypeDialog == "DeleteComment")
                    {
                        MainContext.RunOnUiThread(() =>
                        {
                            try
                            {
                                if (TypeClass == "Comment")
                                {
                                    //TypeClass
                                    var adapterGlobal = CommentActivity.GetInstance()?.CommentsAdapter;
                                    var dataGlobal = adapterGlobal?.CommentList?.FirstOrDefault(a => a.Id == CommentObject?.Id);
                                    if (dataGlobal != null)
                                    {

                                        var index = adapterGlobal.CommentList.IndexOf(dataGlobal);
                                        if (index > -1)
                                        {
                                            adapterGlobal.CommentList.RemoveAt(index);
                                            adapterGlobal.NotifyItemRemoved(index);
                                        }
                                    }

                                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Comment.DeleteCommentAsync(CommentObject.Id) });
                                }
                                else if (TypeClass == "Reply")
                                {
                                    //TypeClass
                                    var adapterGlobal = ReplyCommentActivity.GetInstance()?.MAdapter;
                                    var dataGlobal = adapterGlobal?.ReplyCommentList?.FirstOrDefault(a => a.Id == CommentObject?.Id);
                                    if (dataGlobal != null)
                                    {

                                        var index = adapterGlobal.ReplyCommentList.IndexOf(dataGlobal);
                                        if (index > -1)
                                        {
                                            adapterGlobal.ReplyCommentList.RemoveAt(index);
                                            adapterGlobal.NotifyItemRemoved(index);
                                        }
                                    }

                                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Comment.DeleteCommentAsync(CommentObject.Id, "delete_reply") });
                                }
                                 
                                Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CommentSuccessfullyDeleted), ToastLength.Short).Show();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }
                        });
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

        public void OnInput(MaterialDialog p0, ICharSequence p1)
        {
            try
            {
                if (p1.Length() > 0)
                {
                    var strName = p1.ToString();

                    if (!Methods.CheckConnectivity())
                    {
                        Toast.MakeText(MainContext, MainContext.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                    }
                    else
                    {
                        if (TypeClass == "Comment")
                        {
                            //TypeClass
                            var adapterGlobal = CommentActivity.GetInstance()?.CommentsAdapter;
                            var dataGlobal = adapterGlobal?.CommentList?.FirstOrDefault(a => a.Id == CommentObject?.Id);
                            if (dataGlobal != null)
                            {
                                dataGlobal.Text = strName;
                                var index = adapterGlobal.CommentList.IndexOf(dataGlobal);
                                if (index > -1)
                                {
                                    adapterGlobal.NotifyItemChanged(index);
                                }
                            }
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Comment.EditCommentAsync(CommentObject.Id, strName) });
                        }
                        else if (TypeClass == "Reply")
                        {
                            //TypeClass
                            var adapterGlobal = ReplyCommentActivity.GetInstance()?.MAdapter;
                            var dataGlobal = adapterGlobal?.ReplyCommentList?.FirstOrDefault(a => a.Id == CommentObject?.Id);
                            if (dataGlobal != null)
                            {
                                dataGlobal.Text = strName;
                                var index = adapterGlobal.ReplyCommentList.IndexOf(dataGlobal);
                                if (index > -1)
                                {
                                    adapterGlobal.NotifyItemChanged(index);
                                }
                            }

                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Comment.EditCommentAsync(CommentObject.Id, strName, "edit_reply") });
                        } 
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion MaterialDialog 
    }
}