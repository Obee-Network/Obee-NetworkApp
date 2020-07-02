using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Widget;
using Java.Lang;
using Newtonsoft.Json;
using ObeeNetwork.Activities.AddPost.Adapters;
using ObeeNetwork.Activities.NativePost.Extra;
using ObeeNetwork.Activities.NativePost.Post;
using ObeeNetwork.Activities.Tabbes;
using ObeeNetwork.Helpers.Controller;
using ObeeNetwork.Helpers.Model;
using ObeeNetwork.Helpers.Utils;
using ObeeNetworkClient.Classes.Global;
using ObeeNetworkClient.Classes.Posts;
using ObeeNetworkClient.Classes.Story;
using ObeeNetworkClient.Requests;
using Exception = System.Exception;

namespace ObeeNetwork.Activities.AddPost.Service
{
    public class FileUpload
    {
        public string IdPost { set; get; }
        public string PagePost { set; get; }
        public string Content { set; get; }
        public string PostPrivacy { set; get; }
        public string PostFeelingType { set; get; }
        public string PostFeelingText { set; get; }
        public string PlaceText { set; get; }
        public ObservableCollection<Attachments> AttachmentList { set; get; }
        public ObservableCollection<PollAnswers> AnswersList { set; get; }
        public string IdColor { set; get; }
        public string AlbumName { set; get; }

        //Story
        public string StoryTitle { set; get; }
        public string StoryDescription { set; get; }
        public string StoryFilePath { set; get; }
        public string StoryFileType { set; get; }
        public string StoryThumbnail { set; get; }
    }

    [Service(Exported = false)]
    public class PostService : IntentService
    {
        #region Variables Basic

        public static string ActionPost;
        public static string ActionStory;
        private static PostService Service;
        private AddPostActivity GlobalContextPost;
        private TabbedMainActivity GlobalContextTabbed;
        private FileUpload DataPost;

        #endregion

        #region General

        public static PostService GetPostService()
        {
            return Service;
        }

        public PostService() : base("PlayerService")
        {

        }

        protected override void OnHandleIntent(Intent intent)
        {

        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override void OnCreate()
        {
            try
            {
                base.OnCreate();
                Service = this;

                GlobalContextPost = AddPostActivity.GetInstance();
                GlobalContextTabbed = TabbedMainActivity.GetInstance();
                MNotificationManager = (NotificationManager)GetSystemService(NotificationService);

                Create_Progress_Notification(); 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {  
            try
            {
                base.OnStartCommand(intent, flags, startId);

                string action = intent.Action;
                var data = intent.GetStringExtra("DataPost");
                if (!string.IsNullOrEmpty(data))
                {
                    if (action == ActionPost)
                    {
                        DataPost = JsonConvert.DeserializeObject<FileUpload>(data);
                        if (DataPost != null)
                        {
                            AddPost();
                        }
                    }
                    else if (action == ActionStory)
                    {
                        DataPost = JsonConvert.DeserializeObject<FileUpload>(data);
                        if (DataPost != null)
                        {
                            AddStory();
                            UpdateNotification("Story");

                        }
                    }  
                }
                 
                return StartCommandResult.Sticky;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return StartCommandResult.NotSticky;
            }
        }

        private async void AddPost()
        {
            try
            {
                var (apiStatus, respond) = await ApiRequest.AddNewPost_Async(DataPost.IdPost, DataPost. PagePost, DataPost.Content, DataPost.PostPrivacy, DataPost. PostFeelingType, DataPost.PostFeelingText, DataPost.PlaceText, DataPost.AttachmentList, DataPost.AnswersList, DataPost.IdColor, DataPost.AlbumName);
                if (apiStatus == 200)
                {
                    if (respond is AddPostObject postObject)
                    {
                        Toast.MakeText(GlobalContextPost, GlobalContextPost.GetText(Resource.String.Lbl_Post_Added), ToastLength.Short).Show();

                        // put the String to pass back into an Intent and close this activity
                        var resultIntent = new Intent();
                        if (postObject.PostData != null)
                        {
                            var countList = GlobalContextTabbed.NewsFeedTab.PostFeedAdapter.ItemCount;

                            var combine = new FeedCombiner(postObject.PostData, GlobalContextTabbed.NewsFeedTab.PostFeedAdapter.ListDiffer, this);
                            combine.CombineDefaultPostSections("Top");
                             
                            int countIndex = 1;
                            var model1 = GlobalContextTabbed.NewsFeedTab.PostFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.Story);
                            var model2 = GlobalContextTabbed.NewsFeedTab.PostFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.AddPostBox);
                            var model3 = GlobalContextTabbed.NewsFeedTab.PostFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.AlertBox);
                            var model4 = GlobalContextTabbed.NewsFeedTab.PostFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.SearchForPosts);

                            if (model4 != null)
                                countIndex += GlobalContextTabbed.NewsFeedTab.PostFeedAdapter.ListDiffer.IndexOf(model4) + 1;
                            else if (model3 != null)
                                countIndex += GlobalContextTabbed.NewsFeedTab.PostFeedAdapter.ListDiffer.IndexOf(model3) + 1;
                            else if (model2 != null)
                                countIndex += GlobalContextTabbed.NewsFeedTab.PostFeedAdapter.ListDiffer.IndexOf(model2) + 1;
                            else if (model1 != null)
                                countIndex += GlobalContextTabbed.NewsFeedTab.PostFeedAdapter.ListDiffer.IndexOf(model1) + 1;
                            else
                                countIndex = 0;
                             
                            GlobalContextTabbed.NewsFeedTab.PostFeedAdapter.NotifyItemRangeInserted(countIndex, GlobalContextTabbed.NewsFeedTab.PostFeedAdapter.ListDiffer.Count - countList);

                            resultIntent.PutExtra("itemObject", JsonConvert.SerializeObject(postObject.PostData));
                        }
                        GlobalContextPost.SetResult(Result.Ok, resultIntent);

                        if (UserDetails.SoundControl)
                            Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("PopNotificationPost.mp3");
                    }
                }
                else
                {
                    Methods.DisplayReportResult(GlobalContextPost, respond);
                }
                RemoveNotification();  
            }
            catch (Exception e)
            {
                RemoveNotification(); 
                Console.WriteLine(e);
            }
        }

        private async void AddStory()
        {
            try
            {
                var postFeedAdapter = GlobalContextTabbed.NewsFeedTab.PostFeedAdapter;
                var checkSection = postFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.Story);
                if (checkSection != null)
                {
                    var modelStory = GlobalContextTabbed.NewsFeedTab.PostFeedAdapter.HolderStory.StoryAdapter;

                    string time = Methods.Time.TimeAgo(DateTime.Now);
                    var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    string time2 = unixTimestamp.ToString();

                    var userData = ListUtils.MyProfileList.FirstOrDefault();

                    //just pass file_path and type video or image
                    var (apiStatus, respond) = await RequestsAsync.Story.Create_Story(DataPost.StoryTitle, DataPost.StoryDescription, DataPost.StoryFilePath, DataPost.StoryFileType);
                    if (apiStatus == 200)
                    {
                        if (respond is CreateStoryObject result)
                        {
                            Toast.MakeText(GlobalContextTabbed, GlobalContextTabbed.GetText(Resource.String.Lbl_Story_Added), ToastLength.Short).Show();
                             
                            var check = modelStory.StoryList?.FirstOrDefault(a => a.UserId == UserDetails.UserId);
                            if (check != null)
                            {
                                if (DataPost.StoryFileType == "image")
                                {
                                    var item = new GetUserStoriesObject.StoryObject.Story()
                                    {
                                        UserId = UserDetails.UserId,
                                        Id = result.StoryId,
                                        Description = DataPost.StoryDescription,
                                        Title = DataPost.StoryTitle,
                                        TimeText = time,
                                        IsOwner = true,
                                        Expire = "",
                                        Posted = time2,
                                        Thumbnail = DataPost.StoryFilePath,
                                        UserData = userData,
                                        Images = new List<GetUserStoriesObject.StoryObject.Image>(),
                                        Videos = new List<GetUserStoriesObject.StoryObject.Video>()
                                    };

                                    if (check.DurationsList == null)
                                        check.DurationsList = new List<long>() { AppSettings.StoryDuration };
                                    else
                                        check.DurationsList.Add(AppSettings.StoryDuration);

                                    check.Stories.Add(item);
                                }
                                else
                                {
                                    var item = new GetUserStoriesObject.StoryObject.Story()
                                    {
                                        UserId = UserDetails.UserId,
                                        Id = result.StoryId,
                                        Description = DataPost.StoryDescription,
                                        Title = DataPost.StoryTitle,
                                        TimeText = time,
                                        IsOwner = true,
                                        Expire = "",
                                        Posted = time2,
                                        Thumbnail = DataPost.StoryThumbnail,
                                        UserData = userData,
                                        Images = new List<GetUserStoriesObject.StoryObject.Image>(),
                                        Videos = new List<GetUserStoriesObject.StoryObject.Video>()
                                        {
                                            new GetUserStoriesObject.StoryObject.Video()
                                            {
                                                StoryId = result.StoryId,
                                                Filename = DataPost.StoryFilePath,
                                                Id = time2,
                                                Expire = time2,
                                                Type = "video",
                                            }
                                        }
                                    };

                                    var duration = ObeeNetworkTools.GetDuration(DataPost.StoryFilePath);

                                    if (check.DurationsList == null)
                                        check.DurationsList = new List<long>() { Long.ParseLong(duration) };
                                    else
                                        check.DurationsList.Add(Long.ParseLong(duration));

                                    check.Stories.Add(item);
                                }
                            }
                            else
                            {
                                if (DataPost.StoryFileType == "image")
                                {
                                    var item = new GetUserStoriesObject.StoryObject
                                    {
                                        Type = "image",
                                        Stories = new List<GetUserStoriesObject.StoryObject.Story>
                                        {
                                            new GetUserStoriesObject.StoryObject.Story()
                                            {
                                                UserId = UserDetails.UserId,
                                                Id = result.StoryId,
                                                Description = DataPost.StoryDescription,
                                                Title = DataPost.StoryTitle,
                                                TimeText = time,
                                                IsOwner = true,
                                                Expire = "",
                                                Posted = time2,
                                                Thumbnail = DataPost.StoryFilePath,
                                                UserData = userData,
                                                Images = new List<GetUserStoriesObject.StoryObject.Image>(),
                                                Videos = new List<GetUserStoriesObject.StoryObject.Video>(),
                                            }

                                        },
                                        UserId = userData?.UserId,
                                        Username = userData?.Username,
                                        Email = userData?.Email,
                                        FirstName = userData?.FirstName,
                                        LastName = userData?.LastName,
                                        Avatar = userData?.Avatar,
                                        Cover = userData?.Cover,
                                        BackgroundImage = userData?.BackgroundImage,
                                        RelationshipId = userData?.RelationshipId,
                                        Address = userData?.Address,
                                        Working = userData?.Working,
                                        Gender = userData?.Gender,
                                        Facebook = userData?.Facebook,
                                        Google = userData?.Google,
                                        Twitter = userData?.Twitter,
                                        Linkedin = userData?.Linkedin,
                                        Website = userData?.Website,
                                        Instagram = userData?.Instagram,
                                        WebDeviceId = userData?.WebDeviceId,
                                        Language = userData?.Language,
                                        IpAddress = userData?.IpAddress,
                                        PhoneNumber = userData?.PhoneNumber,
                                        Timezone = userData?.Timezone,
                                        Lat = userData?.Lat,
                                        Lng = userData?.Lng,
                                        About = userData?.About,
                                        Birthday = userData?.Birthday,
                                        Registered = userData?.Registered,
                                        Lastseen = userData?.Lastseen,
                                        LastLocationUpdate = userData?.LastLocationUpdate,
                                        Balance = userData?.Balance,
                                        Verified = userData?.Verified,
                                        Status = userData?.Status,
                                        Active = userData?.Active,
                                        Admin = userData?.Admin,
                                        IsPro = userData?.IsPro,
                                        ProType = userData?.ProType,
                                        School = userData?.School,
                                        Name = userData?.Name,
                                        AndroidMDeviceId = userData?.AndroidMDeviceId,
                                        ECommented = userData?.ECommented,
                                        AndroidNDeviceId = userData?.AndroidMDeviceId,
                                        AvatarFull = userData?.AvatarFull,
                                        BirthPrivacy = userData?.BirthPrivacy,
                                        CanFollow = userData?.CanFollow,
                                        ConfirmFollowers = userData?.ConfirmFollowers,
                                        CountryId = userData?.CountryId,
                                        EAccepted = userData?.EAccepted,
                                        EFollowed = userData?.EFollowed,
                                        EJoinedGroup = userData?.EJoinedGroup,
                                        ELastNotif = userData?.ELastNotif,
                                        ELiked = userData?.ELiked,
                                        ELikedPage = userData?.ELikedPage,
                                        EMentioned = userData?.EMentioned,
                                        EProfileWallPost = userData?.EProfileWallPost,
                                        ESentmeMsg = userData?.ESentmeMsg,
                                        EShared = userData?.EShared,
                                        EVisited = userData?.EVisited,
                                        EWondered = userData?.EWondered,
                                        EmailNotification = userData?.EmailNotification,
                                        FollowPrivacy = userData?.FollowPrivacy,
                                        FriendPrivacy = userData?.FriendPrivacy,
                                        GenderText = userData?.GenderText,
                                        InfoFile = userData?.InfoFile,
                                        IosMDeviceId = userData?.IosMDeviceId,
                                        IosNDeviceId = userData?.IosNDeviceId,
                                        IsFollowing = userData?.IsFollowing,
                                        IsFollowingMe = userData?.IsFollowingMe,
                                        LastAvatarMod = userData?.LastAvatarMod,
                                        LastCoverMod = userData?.LastCoverMod,
                                        LastDataUpdate = userData?.LastDataUpdate,
                                        LastFollowId = userData?.LastFollowId,
                                        LastLoginData = userData?.LastLoginData,
                                        LastseenStatus = userData?.LastseenStatus,
                                        LastseenTimeText = userData?.LastseenTimeText,
                                        LastseenUnixTime = userData?.LastseenUnixTime,
                                        MessagePrivacy = userData?.MessagePrivacy,
                                        NewEmail = userData?.NewEmail,
                                        NewPhone = userData?.NewPhone,
                                        NotificationSettings = userData?.NotificationSettings,
                                        NotificationsSound = userData?.NotificationsSound,
                                        OrderPostsBy = userData?.OrderPostsBy,
                                        PaypalEmail = userData?.PaypalEmail,
                                        PostPrivacy = userData?.PostPrivacy,
                                        Referrer = userData?.Referrer,
                                        ShareMyData = userData?.ShareMyData,
                                        ShareMyLocation = userData?.ShareMyLocation,
                                        ShowActivitiesPrivacy = userData?.ShowActivitiesPrivacy,
                                        TwoFactor = userData?.TwoFactor,
                                        TwoFactorVerified = userData?.TwoFactorVerified,
                                        Url = userData?.Url,
                                        VisitPrivacy = userData?.VisitPrivacy,
                                        Vk = userData?.Vk,
                                        Wallet = userData?.Wallet,
                                        WorkingLink = userData?.WorkingLink,
                                        Youtube = userData?.Youtube,
                                        City = userData?.City,
                                        Points = userData?.Points,
                                        DailyPoints = userData?.DailyPoints,
                                        PointDayExpire = userData?.PointDayExpire,
                                        State = userData?.State,
                                        Zip = userData?.Zip,
                                        Details = new DetailsUnion()
                                        {
                                            DetailsClass = new Details(),
                                        },
                                    };

                                    if (item.DurationsList == null)
                                        item.DurationsList = new List<long>() { AppSettings.StoryDuration };
                                    else
                                        item.DurationsList.Add(AppSettings.StoryDuration);

                                    modelStory.StoryList?.Add(item);
                                }
                                else
                                {
                                    var item = new GetUserStoriesObject.StoryObject()
                                    {
                                        Type = "video",
                                        Stories = new List<GetUserStoriesObject.StoryObject.Story>()
                                        {
                                            new GetUserStoriesObject.StoryObject.Story()
                                            {
                                                UserId = UserDetails.UserId,
                                                Id = result.StoryId,
                                                Description = DataPost.StoryDescription,
                                                Title = DataPost.StoryTitle,
                                                TimeText = time,
                                                IsOwner = true,
                                                Expire = "",
                                                Posted = time2,
                                                Thumbnail = DataPost.StoryThumbnail,
                                                UserData = userData,
                                                Images = new List<GetUserStoriesObject.StoryObject.Image>(),
                                                Videos = new List<GetUserStoriesObject.StoryObject.Video>()
                                                {
                                                    new GetUserStoriesObject.StoryObject.Video()
                                                    {
                                                        StoryId = result.StoryId,
                                                        Filename = DataPost.StoryFilePath,
                                                        Id = time2,
                                                        Expire = time2,
                                                        Type = "video",
                                                    }
                                                }
                                            },
                                        },
                                        UserId = userData?.UserId,
                                        Username = userData?.Username,
                                        Email = userData?.Email,
                                        FirstName = userData?.FirstName,
                                        LastName = userData?.LastName,
                                        Avatar = userData?.Avatar,
                                        Cover = userData?.Cover,
                                        BackgroundImage = userData?.BackgroundImage,
                                        RelationshipId = userData?.RelationshipId,
                                        Address = userData?.Address,
                                        Working = userData?.Working,
                                        Gender = userData?.Gender,
                                        Facebook = userData?.Facebook,
                                        Google = userData?.Google,
                                        Twitter = userData?.Twitter,
                                        Linkedin = userData?.Linkedin,
                                        Website = userData?.Website,
                                        Instagram = userData?.Instagram,
                                        WebDeviceId = userData?.WebDeviceId,
                                        Language = userData?.Language,
                                        IpAddress = userData?.IpAddress,
                                        PhoneNumber = userData?.PhoneNumber,
                                        Timezone = userData?.Timezone,
                                        Lat = userData?.Lat,
                                        Lng = userData?.Lng,
                                        About = userData?.About,
                                        Birthday = userData?.Birthday,
                                        Registered = userData?.Registered,
                                        Lastseen = userData?.Lastseen,
                                        LastLocationUpdate = userData?.LastLocationUpdate,
                                        Balance = userData?.Balance,
                                        Verified = userData?.Verified,
                                        Status = userData?.Status,
                                        Active = userData?.Active,
                                        Admin = userData?.Admin,
                                        IsPro = userData?.IsPro,
                                        ProType = userData?.ProType,
                                        School = userData?.School,
                                        Name = userData?.Name,
                                        AndroidMDeviceId = userData?.AndroidMDeviceId,
                                        ECommented = userData?.ECommented,
                                        AndroidNDeviceId = userData?.AndroidMDeviceId,
                                        AvatarFull = userData?.AvatarFull,
                                        BirthPrivacy = userData?.BirthPrivacy,
                                        CanFollow = userData?.CanFollow,
                                        ConfirmFollowers = userData?.ConfirmFollowers,
                                        CountryId = userData?.CountryId,
                                        EAccepted = userData?.EAccepted,
                                        EFollowed = userData?.EFollowed,
                                        EJoinedGroup = userData?.EJoinedGroup,
                                        ELastNotif = userData?.ELastNotif,
                                        ELiked = userData?.ELiked,
                                        ELikedPage = userData?.ELikedPage,
                                        EMentioned = userData?.EMentioned,
                                        EProfileWallPost = userData?.EProfileWallPost,
                                        ESentmeMsg = userData?.ESentmeMsg,
                                        EShared = userData?.EShared,
                                        EVisited = userData?.EVisited,
                                        EWondered = userData?.EWondered,
                                        EmailNotification = userData?.EmailNotification,
                                        FollowPrivacy = userData?.FollowPrivacy,
                                        FriendPrivacy = userData?.FriendPrivacy,
                                        GenderText = userData?.GenderText,
                                        InfoFile = userData?.InfoFile,
                                        IosMDeviceId = userData?.IosMDeviceId,
                                        IosNDeviceId = userData?.IosNDeviceId,
                                        IsFollowing = userData?.IsFollowing,
                                        IsFollowingMe = userData?.IsFollowingMe,
                                        LastAvatarMod = userData?.LastAvatarMod,
                                        LastCoverMod = userData?.LastCoverMod,
                                        LastDataUpdate = userData?.LastDataUpdate,
                                        LastFollowId = userData?.LastFollowId,
                                        LastLoginData = userData?.LastLoginData,
                                        LastseenStatus = userData?.LastseenStatus,
                                        LastseenTimeText = userData?.LastseenTimeText,
                                        LastseenUnixTime = userData?.LastseenUnixTime,
                                        MessagePrivacy = userData?.MessagePrivacy,
                                        NewEmail = userData?.NewEmail,
                                        NewPhone = userData?.NewPhone,
                                        NotificationSettings = userData?.NotificationSettings,
                                        NotificationsSound = userData?.NotificationsSound,
                                        OrderPostsBy = userData?.OrderPostsBy,
                                        PaypalEmail = userData?.PaypalEmail,
                                        PostPrivacy = userData?.PostPrivacy,
                                        Referrer = userData?.Referrer,
                                        ShareMyData = userData?.ShareMyData,
                                        ShareMyLocation = userData?.ShareMyLocation,
                                        ShowActivitiesPrivacy = userData?.ShowActivitiesPrivacy,
                                        TwoFactor = userData?.TwoFactor,
                                        TwoFactorVerified = userData?.TwoFactorVerified,
                                        Url = userData?.Url,
                                        VisitPrivacy = userData?.VisitPrivacy,
                                        Vk = userData?.Vk,
                                        Wallet = userData?.Wallet,
                                        WorkingLink = userData?.WorkingLink,
                                        Youtube = userData?.Youtube,
                                        City = userData?.City,
                                        Points = userData?.Points,
                                        DailyPoints = userData?.DailyPoints,
                                        State = userData?.State,
                                        Zip = userData?.Zip,
                                        Details = new DetailsUnion()
                                        {
                                            DetailsClass = new Details(),
                                        },
                                    };

                                    var duration = ObeeNetworkTools.GetDuration(DataPost.StoryFilePath);

                                    if (item.DurationsList == null)
                                        item.DurationsList = new List<long>() { Long.ParseLong(duration) };
                                    else
                                        item.DurationsList.Add(Long.ParseLong(duration));

                                    modelStory.StoryList?.Add(item);
                                }
                            }

                            modelStory.NotifyDataSetChanged();

                            if (UserDetails.SoundControl)
                                Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("PopNotificationPost.mp3");
                        }
                    }
                    else Methods.DisplayReportResult(GlobalContextTabbed, respond);
                }

                RemoveNotification();
            }
            catch (Exception e)
            {
                RemoveNotification(); 
                Console.WriteLine(e);
            } 
        }


        #region Notification

        private readonly string NotificationChannelId = "ObeeNetwork_ch_1";

        private NotificationManager MNotificationManager;
        private NotificationCompat.Builder NotificationBuilder;
        private RemoteViews NotificationView;
        private void Create_Progress_Notification()
        {
            try
            {
                MNotificationManager = (NotificationManager)GetSystemService(NotificationService);

                NotificationView = new RemoteViews(PackageName, Resource.Layout.ViewProgressNotification);
                 
                Intent resultIntent = new Intent();
                PendingIntent resultPendingIntent = PendingIntent.GetActivity(this, 0, resultIntent, PendingIntentFlags.UpdateCurrent);
                NotificationBuilder = new NotificationCompat.Builder(this, NotificationChannelId);
                NotificationBuilder.SetSmallIcon(Resource.Mipmap.icon);
                NotificationBuilder.SetColor(ContextCompat.GetColor(this, Resource.Color.accent));
                NotificationBuilder.SetCustomContentView(NotificationView)
                    .SetOngoing(true)
                    .SetContentIntent(resultPendingIntent)
                    .SetDefaults(NotificationCompat.DefaultAll)
                    .SetPriority((int)NotificationPriority.High);

                NotificationBuilder.SetVibrate(new[] { 0L });
                NotificationBuilder.SetVisibility(NotificationCompat.VisibilityPublic);

                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    var importance = NotificationImportance.High;
                    NotificationChannel notificationChannel = new NotificationChannel(NotificationChannelId, AppSettings.ApplicationName, importance);
                    notificationChannel.EnableLights(false);
                    notificationChannel.EnableVibration(false);
                    NotificationBuilder.SetChannelId(NotificationChannelId);

                    MNotificationManager?.CreateNotificationChannel(notificationChannel);
                }

                MNotificationManager?.Notify(2020, NotificationBuilder.Build());
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void RemoveNotification()
        {
            try
            {
                MNotificationManager.CancelAll();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void UpdateNotification(string type)
        {
            try
            {
                switch (type)
                {
                    case "Post":
                        NotificationView.SetTextViewText(Resource.Id.title, GetString(Resource.String.Lbl_UploadingPost));
                        break;
                    case "Story":
                        NotificationView.SetTextViewText(Resource.Id.title, GetString(Resource.String.Lbl_UploadingStory));
                        break;
                }

                MNotificationManager?.Notify(2020, NotificationBuilder.Build());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
          
        #endregion 
    }
}