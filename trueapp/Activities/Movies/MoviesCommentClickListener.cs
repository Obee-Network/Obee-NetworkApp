using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AFollestad.MaterialDialogs;
using Android.App;
using Android.Views;
using Android.Widget;
using Java.Lang;
using ObeeNetwork.Activities.Movies.Adapters;
using ObeeNetwork.Activities.Videos;
using ObeeNetwork.Helpers.Controller;
using ObeeNetwork.Helpers.Model;
using ObeeNetwork.Helpers.Utils;
using ObeeNetworkClient.Classes.Movies;
using ObeeNetworkClient.Requests;
using Exception = System.Exception;

namespace ObeeNetwork.Activities.Movies
{
    public class MoviesCommentClickListener : Java.Lang.Object, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        private readonly Activity MainContext;
        private CommentsMoviesObject CommentObject;
        private string TypeDialog;
        private readonly string TypeClass;

        public MoviesCommentClickListener(Activity context, string typeClass)
        {
            MainContext = context;
            TypeClass = typeClass;
        }

        public void MoreCommentReplyPostClick(CommentReplyMoviesClickEventArgs e)
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

                    if (CommentObject?.IsOwner != null && (bool)CommentObject?.IsOwner || CommentObject?.UserData?.UserId == UserDetails.UserId)
                        arrayAdapter.Add(MainContext.GetString(Resource.String.Lbl_Delete));

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
        private void DeleteCommentEvent(CommentsMoviesObject item)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                    return;
                }

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
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void ProfileClick(CommentReplyMoviesClickEventArgs e)
        {
            try
            {
                ObeeNetworkTools.OpenProfile(MainContext, e.CommentObject.UserData.UserId, e.CommentObject.UserData);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
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
                    Methods.CopyToClipboard(MainContext, Methods.FunString.DecodeString(CommentObject.Text));
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
                if (TypeDialog == "DeleteComment")
                {
                    if (p1 == DialogAction.Positive)
                    {
                        MainContext.RunOnUiThread(() =>
                        {
                            try
                            {
                                if (TypeClass == "Comment")
                                {
                                    //TypeClass
                                    var adapterGlobal = VideoViewerActivity.GetInstance()?.TabVideosComment?.MAdapter;
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

                                    if (!Methods.CheckConnectivity())
                                        Toast.MakeText(MainContext, MainContext.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                                    else
                                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Movies.DeleteCommentAsync(CommentObject.MovieId, CommentObject.Id) });
                                }
                                else if (TypeClass == "Reply")
                                {
                                    //TypeClass
                                    var adapterGlobal = VideoViewerActivity.GetInstance()?.ReplyFragment?.MAdapterMovies;
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

                                    if (!Methods.CheckConnectivity())
                                        Toast.MakeText(MainContext, MainContext.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                                    else
                                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Movies.DeleteCommentAsync(CommentObject.MovieId, CommentObject.Id, "reply_delete") });
                                }

                                Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CommentSuccessfullyDeleted), ToastLength.Short).Show();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }
                        });
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
}