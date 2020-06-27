using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using AutoMapper;
using Newtonsoft.Json;
using SQLite;
using ObeeNetwork.Helpers.Controller;
using ObeeNetwork.Helpers.Model;
using ObeeNetwork.Helpers.Utils;
using ObeeNetwork.Library.OneSignal;
using ObeeNetworkClient;
using ObeeNetworkClient.Classes.Global;
using ObeeNetworkClient.Classes.Movies;
using Exception = System.Exception;

namespace ObeeNetwork.SQLite
{
    public class SqLiteDatabase : IDisposable
    {
        //############# DON'T MODIFY HERE #############
        private static readonly string Folder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        public static readonly string PathCombine = Path.Combine(Folder, "ObeeNetworkSocial.db");
        private SQLiteConnection Connection;

        //Open Connection in Database
        //*********************************************************

        #region Connection

        private SQLiteConnection OpenConnection()
        {
            try
            {
                Connection = new SQLiteConnection(PathCombine);
                return Connection;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public void CheckTablesStatus()
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return;

                    Connection.CreateTable<DataTables.LoginTb>();
                    Connection.CreateTable<DataTables.MyContactsTb>();
                    Connection.CreateTable<DataTables.MyFollowersTb>();
                    Connection.CreateTable<DataTables.MyProfileTb>();
                    Connection.CreateTable<DataTables.SearchFilterTb>();
                    Connection.CreateTable<DataTables.NearByFilterTb>();
                    Connection.CreateTable<DataTables.WatchOfflineVideosTb>();
                    Connection.CreateTable<DataTables.SettingsTb>();
                    Connection.CreateTable<DataTables.GiftsTb>();

                    Connection.Dispose();
                    Connection.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //Close Connection in Database
        public void Dispose()
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return;
                    Connection.Dispose();
                    Connection.Close();
                    GC.SuppressFinalize(this);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void ClearAll()
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return;
                    Connection.DeleteAll<DataTables.LoginTb>();
                    Connection.DeleteAll<DataTables.MyContactsTb>();
                    Connection.DeleteAll<DataTables.MyFollowersTb>();
                    Connection.DeleteAll<DataTables.MyProfileTb>();
                    Connection.DeleteAll<DataTables.SearchFilterTb>();
                    Connection.DeleteAll<DataTables.NearByFilterTb>();
                    Connection.DeleteAll<DataTables.WatchOfflineVideosTb>();
                    Connection.DeleteAll<DataTables.SettingsTb>();
                    Connection.DeleteAll<DataTables.GiftsTb>();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Delete table 
        public void DropAll()
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return;
                    Connection.DropTable<DataTables.LoginTb>();
                    Connection.DropTable<DataTables.MyContactsTb>();
                    Connection.DropTable<DataTables.MyFollowersTb>();
                    Connection.DropTable<DataTables.MyProfileTb>();
                    Connection.DropTable<DataTables.SearchFilterTb>();
                    Connection.DropTable<DataTables.NearByFilterTb>();
                    Connection.DropTable<DataTables.WatchOfflineVideosTb>();
                    Connection.DropTable<DataTables.SettingsTb>();
                    Connection.DropTable<DataTables.GiftsTb>();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        //########################## End SQLite_Entity ##########################

        //Start SQL_Commander >>  General 
        //*********************************************************

        #region General

        public void InsertRow(object row)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return;
                    Connection.Insert(row);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public void UpdateRow(object row)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return;
                    Connection.Update(row);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public void DeleteRow(object row)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return;
                    Connection.Delete(row);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public void InsertListOfRows(List<object> row)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return;
                    Connection.InsertAll(row);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        //Start SQL_Commander >>  Custom 
        //*********************************************************

        #region Login

        //Insert Or Update data Login
        public void InsertOrUpdateLogin_Credentials(DataTables.LoginTb db)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return;
                    var dataUser = Connection.Table<DataTables.LoginTb>().FirstOrDefault();
                    if (dataUser != null)
                    {
                        dataUser.UserId = UserDetails.UserId;
                        dataUser.AccessToken = UserDetails.AccessToken;
                        dataUser.Cookie = UserDetails.Cookie;
                        dataUser.Username = UserDetails.Username;
                        dataUser.Password = UserDetails.Password;
                        dataUser.Status = UserDetails.Status;
                        dataUser.Lang = AppSettings.Lang;
                        dataUser.DeviceId = UserDetails.DeviceId;
                        dataUser.Email = UserDetails.Email;

                        Connection.Update(dataUser);
                    }
                    else
                    {
                        Connection.Insert(db);
                    }

                    Methods.GenerateNoteOnSD(JsonConvert.SerializeObject(db));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //Get data Login
        public DataTables.LoginTb Get_data_Login_Credentials()
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return null;
                    var dataUser = Connection.Table<DataTables.LoginTb>().FirstOrDefault();
                    if (dataUser != null)
                    {
                        UserDetails.Username = dataUser.Username;
                        UserDetails.FullName = dataUser.Username;
                        UserDetails.Password = dataUser.Password;
                        UserDetails.AccessToken = dataUser.AccessToken;
                        UserDetails.UserId = dataUser.UserId;
                        UserDetails.Status = dataUser.Status;
                        UserDetails.Cookie = dataUser.Cookie;
                        UserDetails.Email = dataUser.Email;
                        UserDetails.PlayTubeUrl = dataUser.PlayTubeUrl;
                        AppSettings.Lang = dataUser.Lang;
                        UserDetails.DeviceId = dataUser.DeviceId;

                        Current.AccessToken = dataUser.AccessToken;
                        ListUtils.DataUserLoginList.Add(dataUser);

                        return dataUser;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return null;
            }
        }

        #endregion

        #region Settings

        public void InsertOrUpdateSettings(GetSiteSettingsObject.ConfigObject settingsData)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return;
                    if (settingsData != null)
                    {
                        var select = Connection.Table<DataTables.SettingsTb>().FirstOrDefault();
                        if (select == null)
                        {
                            var db = Mapper.Map<DataTables.SettingsTb>(settingsData);

                            db.CurrencyArray = JsonConvert.SerializeObject(settingsData.CurrencyArray.CurrencyList);
                            db.CurrencySymbolArray = JsonConvert.SerializeObject(settingsData.CurrencySymbolArray.CurrencyList);
                            db.PageCategories = JsonConvert.SerializeObject(settingsData.PageCategories);
                            db.GroupCategories = JsonConvert.SerializeObject(settingsData.GroupCategories);
                            db.BlogCategories = JsonConvert.SerializeObject(settingsData.BlogCategories);
                            db.ProductsCategories = JsonConvert.SerializeObject(settingsData.ProductsCategories);
                            db.JobCategories = JsonConvert.SerializeObject(settingsData.JobCategories);
                            db.Genders = JsonConvert.SerializeObject(settingsData.Genders);
                            db.Family = JsonConvert.SerializeObject(settingsData.Family);
                            db.MovieCategory = JsonConvert.SerializeObject(settingsData.MovieCategory);
                            if (settingsData.PostColors != null)
                                db.PostColors = JsonConvert.SerializeObject(settingsData.PostColors.Value.PostColorsList);
                            db.Fields = JsonConvert.SerializeObject(settingsData.Fields);
                            db.PostReactionsTypes = JsonConvert.SerializeObject(settingsData.PostReactionsTypes);
                            db.PageSubCategories = JsonConvert.SerializeObject(settingsData.PageSubCategories?.SubCategoriesList);
                            db.GroupSubCategories = JsonConvert.SerializeObject(settingsData.GroupSubCategories?.SubCategoriesList);
                            db.ProductsSubCategories = JsonConvert.SerializeObject(settingsData.ProductsSubCategories?.SubCategoriesList);
                            db.PageCustomFields = JsonConvert.SerializeObject(settingsData.PageCustomFields);
                            db.GroupCustomFields = JsonConvert.SerializeObject(settingsData.GroupCustomFields);
                            db.ProductCustomFields = JsonConvert.SerializeObject(settingsData.ProductCustomFields);

                            Connection.Insert(db);
                        }
                        else
                        {
                            select = Mapper.Map<DataTables.SettingsTb>(settingsData);

                            select.CurrencyArray = JsonConvert.SerializeObject(settingsData.CurrencyArray.CurrencyList);
                            select.CurrencySymbolArray = JsonConvert.SerializeObject(settingsData.CurrencySymbolArray.CurrencyList);
                            select.PageCategories = JsonConvert.SerializeObject(settingsData.PageCategories);
                            select.GroupCategories = JsonConvert.SerializeObject(settingsData.GroupCategories);
                            select.BlogCategories = JsonConvert.SerializeObject(settingsData.BlogCategories);
                            select.ProductsCategories = JsonConvert.SerializeObject(settingsData.ProductsCategories);
                            select.JobCategories = JsonConvert.SerializeObject(settingsData.JobCategories);
                            select.Genders = JsonConvert.SerializeObject(settingsData.Genders);
                            select.Family = JsonConvert.SerializeObject(settingsData.Family);
                            select.MovieCategory = JsonConvert.SerializeObject(settingsData.MovieCategory);
                            if (settingsData.PostColors != null)
                                select.PostColors = JsonConvert.SerializeObject(settingsData.PostColors.Value.PostColorsList);
                            select.Fields = JsonConvert.SerializeObject(settingsData.Fields);
                            select.PostReactionsTypes = JsonConvert.SerializeObject(settingsData.PostReactionsTypes);
                            select.PageSubCategories = JsonConvert.SerializeObject(settingsData.PageSubCategories?.SubCategoriesList);
                            select.GroupSubCategories = JsonConvert.SerializeObject(settingsData.GroupSubCategories?.SubCategoriesList);
                            select.ProductsSubCategories = JsonConvert.SerializeObject(settingsData.ProductsSubCategories?.SubCategoriesList);
                            select.PageCustomFields = JsonConvert.SerializeObject(settingsData.PageCustomFields);
                            select.GroupCustomFields = JsonConvert.SerializeObject(settingsData.GroupCustomFields);
                            select.ProductCustomFields = JsonConvert.SerializeObject(settingsData.ProductCustomFields);

                            Connection.Update(select);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Get Settings
        public GetSiteSettingsObject.ConfigObject GetSettings()
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return null;
                    var select = Connection.Table<DataTables.SettingsTb>().FirstOrDefault();
                    if (select != null)
                    {
                        var db = Mapper.Map<GetSiteSettingsObject.ConfigObject>(select);
                        if (db != null)
                        {
                            GetSiteSettingsObject.ConfigObject asd = db;
                            asd.CurrencyArray = new GetSiteSettingsObject.CurrencyArray();
                            asd.CurrencySymbolArray = new GetSiteSettingsObject.CurrencySymbol();
                            asd.PageCategories = new Dictionary<string, string>();
                            asd.GroupCategories = new Dictionary<string, string>();
                            asd.BlogCategories = new Dictionary<string, string>();
                            asd.ProductsCategories = new Dictionary<string, string>();
                            asd.JobCategories = new Dictionary<string, string>();
                            asd.Genders = new Dictionary<string, string>();
                            asd.Family = new Dictionary<string, string>();
                            asd.MovieCategory = new Dictionary<string, string>();
                            asd.PostColors = new Dictionary<string, PostColorsObject>();
                            asd.Fields = new List<Field>();
                            asd.PostReactionsTypes = new Dictionary<string, PostReactionsType>();
                            asd.PageSubCategories = new GetSiteSettingsObject.SubCategoriesUnion()
                            {
                                SubCategoriesList = new Dictionary<string, List<SubCategories>>()
                            };
                            asd.GroupSubCategories = new GetSiteSettingsObject.SubCategoriesUnion()
                            {
                                SubCategoriesList = new Dictionary<string, List<SubCategories>>()
                            };
                            asd.ProductsSubCategories = new GetSiteSettingsObject.SubCategoriesUnion()
                            {
                                SubCategoriesList = new Dictionary<string, List<SubCategories>>()
                            };
                            asd.PageCustomFields = new List<CustomField>();
                            asd.GroupCustomFields = new List<CustomField>();
                            asd.ProductCustomFields = new List<CustomField>();

                            if (!string.IsNullOrEmpty(select.CurrencyArray))
                                asd.CurrencyArray = new GetSiteSettingsObject.CurrencyArray()
                                {
                                    CurrencyList = JsonConvert.DeserializeObject<List<string>>(select.CurrencyArray)
                                };

                            if (!string.IsNullOrEmpty(select.CurrencySymbolArray))
                                asd.CurrencySymbolArray = new GetSiteSettingsObject.CurrencySymbol()
                                {
                                    CurrencyList =JsonConvert.DeserializeObject<CurrencySymbolArray>(select.CurrencySymbolArray),
                                };

                            if (!string.IsNullOrEmpty(select.PageCategories))
                                asd.PageCategories = JsonConvert.DeserializeObject<Dictionary<string, string>>(select.PageCategories);

                            if (!string.IsNullOrEmpty(select.GroupCategories))
                                asd.GroupCategories = JsonConvert.DeserializeObject<Dictionary<string, string>>(select.GroupCategories);

                            if (!string.IsNullOrEmpty(select.BlogCategories))
                                asd.BlogCategories = JsonConvert.DeserializeObject<Dictionary<string, string>>(select.BlogCategories);

                            if (!string.IsNullOrEmpty(select.ProductsCategories))
                                asd.ProductsCategories = JsonConvert.DeserializeObject<Dictionary<string, string>>(select.ProductsCategories);
                          
                            if (!string.IsNullOrEmpty(select.JobCategories))
                                asd.JobCategories = JsonConvert.DeserializeObject<Dictionary<string, string>>(select.JobCategories);

                            if (!string.IsNullOrEmpty(select.Genders))
                                asd.Genders = JsonConvert.DeserializeObject<Dictionary<string, string>>(select.Genders);

                            if (!string.IsNullOrEmpty(select.Family))
                                asd.Family = JsonConvert.DeserializeObject<Dictionary<string, string>>(select.Family);
                            
                            if (!string.IsNullOrEmpty(select.MovieCategory))
                                asd.MovieCategory = JsonConvert.DeserializeObject<Dictionary<string, string>>(select.MovieCategory);

                            if (!string.IsNullOrEmpty(select.PostColors))
                                asd.PostColors = new GetSiteSettingsObject.PostColorUnion { PostColorsList = JsonConvert.DeserializeObject<Dictionary<string, PostColorsObject>>(select.PostColors) };

                            if (!string.IsNullOrEmpty(select.PostReactionsTypes))
                                asd.PostReactionsTypes = JsonConvert.DeserializeObject<Dictionary<string, PostReactionsType>>(select.PostReactionsTypes);

                            if (!string.IsNullOrEmpty(select.Fields))
                                asd.Fields = JsonConvert.DeserializeObject<List<Field>>(select.Fields);
                          
                            if (!string.IsNullOrEmpty(select.PageSubCategories))
                                asd.PageSubCategories = new GetSiteSettingsObject.SubCategoriesUnion()
                                {
                                    SubCategoriesList = JsonConvert.DeserializeObject<Dictionary<string, List<SubCategories>>>(select.PageSubCategories)
                                };

                            if (!string.IsNullOrEmpty(select.GroupSubCategories))
                                asd.GroupSubCategories = new GetSiteSettingsObject.SubCategoriesUnion()
                                {
                                    SubCategoriesList = JsonConvert.DeserializeObject<Dictionary<string, List<SubCategories>>>(select.GroupSubCategories)
                                };
                             
                            if (!string.IsNullOrEmpty(select.ProductsSubCategories))
                                asd.ProductsSubCategories = new GetSiteSettingsObject.SubCategoriesUnion()
                                {
                                    SubCategoriesList = JsonConvert.DeserializeObject<Dictionary<string, List<SubCategories>>>(select.ProductsSubCategories)
                                };
                             
                            if (!string.IsNullOrEmpty(select.PageCustomFields))
                                asd.PageCustomFields = JsonConvert.DeserializeObject<List<CustomField>>(select.PageCustomFields);

                            if (!string.IsNullOrEmpty(select.GroupCustomFields))
                                asd.GroupCustomFields = JsonConvert.DeserializeObject<List<CustomField>>(select.GroupCustomFields);

                            if (!string.IsNullOrEmpty(select.ProductCustomFields))
                                asd.ProductCustomFields = JsonConvert.DeserializeObject<List<CustomField>>(select.ProductCustomFields);
                             
                            UserDetails.PlayTubeUrl = asd.PlaytubeUrl; 
                            AppSettings.OneSignalAppId = asd.AndroidNPushId;
                            OneSignalNotification.RegisterNotificationDevice();
                             
                            //Page Categories
                            var listPage = asd.PageCategories.Select(cat => new Classes.Categories
                            {
                                CategoriesId = cat.Key,
                                CategoriesName = Methods.FunString.DecodeString(cat.Value),
                                CategoriesColor = "#ffffff",
                                SubList = new List<SubCategories>()
                            }).ToList();

                            CategoriesController.ListCategoriesPage.Clear();
                            CategoriesController.ListCategoriesPage = new ObservableCollection<Classes.Categories>(listPage);

                            if (asd.PageSubCategories?.SubCategoriesList?.Count > 0)
                            {
                                //Sub Categories Page
                                foreach (var sub in asd.PageSubCategories?.SubCategoriesList)
                                {
                                    var subCategories = asd.PageSubCategories?.SubCategoriesList?.FirstOrDefault(a => a.Key == sub.Key).Value;
                                    if (subCategories?.Count > 0)
                                    {
                                        var cat = CategoriesController.ListCategoriesPage.FirstOrDefault(a => a.CategoriesId == sub.Key);
                                        if (cat != null)
                                        {
                                            foreach (var pairs in subCategories)
                                            {
                                                cat.SubList.Add(pairs);
                                            }
                                        }
                                    }
                                }
                            }

                            //Group Categories
                            var listGroup = asd.GroupCategories.Select(cat => new Classes.Categories
                            {
                                CategoriesId = cat.Key,
                                CategoriesName = Methods.FunString.DecodeString(cat.Value),
                                CategoriesColor = "#ffffff",
                                SubList = new List<SubCategories>()
                            }).ToList();

                            CategoriesController.ListCategoriesGroup.Clear();
                            CategoriesController.ListCategoriesGroup = new ObservableCollection<Classes.Categories>(listGroup);

                            if (asd.GroupSubCategories?.SubCategoriesList?.Count > 0)
                            {
                                //Sub Categories Group
                                foreach (var sub in asd.GroupSubCategories?.SubCategoriesList)
                                {
                                    var subCategories = asd.GroupSubCategories?.SubCategoriesList?.FirstOrDefault(a => a.Key == sub.Key).Value;
                                    if (subCategories?.Count > 0)
                                    {
                                        var cat = CategoriesController.ListCategoriesGroup.FirstOrDefault(a => a.CategoriesId == sub.Key);
                                        if (cat != null)
                                        {
                                            foreach (var pairs in subCategories)
                                            {
                                                cat.SubList.Add(pairs);
                                            }
                                        }
                                    }
                                }
                            }

                            //Blog Categories
                            var listBlog = asd.BlogCategories.Select(cat => new Classes.Categories
                            {
                                CategoriesId = cat.Key,
                                CategoriesName = Methods.FunString.DecodeString(cat.Value),
                                CategoriesColor = "#ffffff",
                                SubList = new List<SubCategories>()
                            }).ToList();

                            CategoriesController.ListCategoriesBlog.Clear();
                            CategoriesController.ListCategoriesBlog = new ObservableCollection<Classes.Categories>(listBlog);

                            //Products Categories
                            var listProducts = asd.ProductsCategories.Select(cat => new Classes.Categories
                            {
                                CategoriesId = cat.Key,
                                CategoriesName = Methods.FunString.DecodeString(cat.Value),
                                CategoriesColor = "#ffffff",
                                SubList = new List<SubCategories>()
                            }).ToList();

                            CategoriesController.ListCategoriesProducts.Clear();
                            CategoriesController.ListCategoriesProducts = new ObservableCollection<Classes.Categories>(listProducts);

                            if (asd.ProductsSubCategories?.SubCategoriesList?.Count > 0)
                            {
                                //Sub Categories Products
                                foreach (var sub in asd.ProductsSubCategories?.SubCategoriesList)
                                {
                                    var subCategories = asd.ProductsSubCategories?.SubCategoriesList?.FirstOrDefault(a => a.Key == sub.Key).Value;
                                    if (subCategories?.Count > 0)
                                    {
                                        var cat = CategoriesController.ListCategoriesProducts.FirstOrDefault(a => a.CategoriesId == sub.Key);
                                        if (cat != null)
                                        {
                                            foreach (var pairs in subCategories)
                                            {
                                                cat.SubList.Add(pairs);
                                            }
                                        }
                                    }
                                }
                            }

                            //Job Categories
                            var listJob = asd.JobCategories.Select(cat => new Classes.Categories
                            {
                                CategoriesId = cat.Key,
                                CategoriesName = Methods.FunString.DecodeString(cat.Value),
                                CategoriesColor = "#ffffff",
                                SubList = new List<SubCategories>()
                            }).ToList();

                            CategoriesController.ListCategoriesJob.Clear();
                            CategoriesController.ListCategoriesJob = new ObservableCollection<Classes.Categories>(listJob);

                            //Family
                            var listFamily = asd.Family.Select(cat => new Classes.Family
                            {
                                FamilyId = cat.Key,
                                FamilyName = Methods.FunString.DecodeString(cat.Value),
                            }).ToList();

                            ListUtils.FamilyList.Clear();
                            ListUtils.FamilyList = new ObservableCollection<Classes.Family>(listFamily);

                            //Movie Category
                            var listMovie = asd.MovieCategory.Select(cat => new Classes.Categories
                            {
                                CategoriesId = cat.Key,
                                CategoriesName = Methods.FunString.DecodeString(cat.Value),
                                CategoriesColor = "#ffffff",
                                SubList = new List<SubCategories>()
                            }).ToList();

                            CategoriesController.ListCategoriesMovies.Clear();
                            CategoriesController.ListCategoriesMovies = new ObservableCollection<Classes.Categories>(listMovie);
                              
                            return asd;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return null;
            }
        }

        #endregion

        #region My Contacts >> Following

        //Insert data To My Contact Table
        public void Insert_Or_Replace_MyContactTable(ObservableCollection<UserDataObject> usersContactList)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return;
                    var result = Connection.Table<DataTables.MyContactsTb>().ToList();
                    List<DataTables.MyContactsTb> list = new List<DataTables.MyContactsTb>();
                    foreach (var info in usersContactList)
                    {
                        var db = Mapper.Map<DataTables.MyContactsTb>(info);
                        if (info.Details.DetailsClass != null)
                            db.Details = JsonConvert.SerializeObject(info.Details.DetailsClass);
                        list.Add(db);
                         
                        var update = result.FirstOrDefault(a => a.UserId == info.UserId);
                        if (update != null)
                        {
                            update = Mapper.Map<DataTables.MyContactsTb>(info);
                            if (info.Details.DetailsClass != null)
                                update.Details = JsonConvert.SerializeObject(info.Details.DetailsClass);
                            
                            Connection.Update(update);
                        }
                    }

                    if (list.Count <= 0) return;

                    Connection.BeginTransaction();
                    //Bring new  
                    var newItemList = list.Where(c => !result.Select(fc => fc.UserId).Contains(c.UserId)).ToList();
                    if (newItemList.Count > 0)
                        Connection.InsertAll(newItemList);

                    result = Connection.Table<DataTables.MyContactsTb>().ToList();
                    var deleteItemList = result.Where(c => !list.Select(fc => fc.UserId).Contains(c.UserId)).ToList();
                    if (deleteItemList.Count > 0)
                        foreach (var delete in deleteItemList)
                            Connection.Delete(delete);

                    Connection.Commit();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        // Get data To My Contact Table
        public ObservableCollection<UserDataObject> Get_MyContact(/*int id = 0, int nSize = 20*/)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return new ObservableCollection<UserDataObject>();
                    // var query = Connection.Table<DataTables.MyContactsTb>().Where(w => w.AutoIdMyFollowing >= id).OrderBy(q => q.AutoIdMyFollowing).Take(nSize).ToList();

                    var select = Connection.Table<DataTables.MyContactsTb>().ToList();
                    if (select.Count > 0)
                    {
                        var list = new ObservableCollection<UserDataObject>();

                        foreach (var item in select)
                        {
                            UserDataObject infoObject = new UserDataObject()
                            {
                                UserId = item.UserId,
                                Username = item.Username,
                                Email = item.Email,
                                FirstName = item.FirstName,
                                LastName = item.LastName,
                                Avatar = item.Avatar,
                                Cover = item.Cover,
                                BackgroundImage = item.BackgroundImage,
                                RelationshipId = item.RelationshipId,
                                Address = item.Address,
                                Working = item.Working,
                                Gender = item.Gender,
                                Facebook = item.Facebook,
                                Google = item.Google,
                                Twitter = item.Twitter,
                                Linkedin = item.Linkedin,
                                Website = item.Website,
                                Instagram = item.Instagram,
                                WebDeviceId = item.WebDeviceId,
                                Language = item.Language,
                                IpAddress = item.IpAddress,
                                PhoneNumber = item.PhoneNumber,
                                Timezone = item.Timezone,
                                Lat = item.Lat,
                                Lng = item.Lng,
                                About = item.About,
                                Birthday = item.Birthday,
                                Registered = item.Registered,
                                Lastseen = item.Lastseen,
                                LastLocationUpdate = item.LastLocationUpdate,
                                Balance = item.Balance,
                                Verified = item.Verified,
                                Status = item.Status,
                                Active = item.Active,
                                Admin = item.Admin,
                                IsPro = item.IsPro,
                                ProType = item.ProType,
                                School = item.School,
                                Name = item.Name,
                                AndroidMDeviceId = item.AndroidMDeviceId,
                                ECommented = item.ECommented,
                                AndroidNDeviceId = item.AndroidMDeviceId,
                                AvatarFull = item.AvatarFull,
                                BirthPrivacy = item.BirthPrivacy,
                                CanFollow = item.CanFollow,
                                ConfirmFollowers = item.ConfirmFollowers,
                                CountryId = item.CountryId,
                                EAccepted = item.EAccepted,
                                EFollowed = item.EFollowed,
                                EJoinedGroup = item.EJoinedGroup,
                                ELastNotif = item.ELastNotif,
                                ELiked = item.ELiked,
                                ELikedPage = item.ELikedPage,
                                EMentioned = item.EMentioned,
                                EProfileWallPost = item.EProfileWallPost,
                                ESentmeMsg = item.ESentmeMsg,
                                EShared = item.EShared,
                                EVisited = item.EVisited,
                                EWondered = item.EWondered,
                                EmailNotification = item.EmailNotification,
                                FollowPrivacy = item.FollowPrivacy,
                                FriendPrivacy = item.FriendPrivacy,
                                GenderText = item.GenderText,
                                InfoFile = item.InfoFile,
                                IosMDeviceId = item.IosMDeviceId,
                                IosNDeviceId = item.IosNDeviceId,
                                IsBlocked = item.IsBlocked,
                                IsFollowing = item.IsFollowing,
                                IsFollowingMe = item.IsFollowingMe,
                                LastAvatarMod = item.LastAvatarMod,
                                LastCoverMod = item.LastCoverMod,
                                LastDataUpdate = item.LastDataUpdate,
                                LastFollowId = item.LastFollowId,
                                LastLoginData = item.LastLoginData,
                                LastseenStatus = item.LastseenStatus,
                                LastseenTimeText = item.LastseenTimeText,
                                LastseenUnixTime = item.LastseenUnixTime,
                                MessagePrivacy = item.MessagePrivacy,
                                NewEmail = item.NewEmail,
                                NewPhone = item.NewPhone,
                                NotificationSettings = item.NotificationSettings,
                                NotificationsSound = item.NotificationsSound,
                                OrderPostsBy = item.OrderPostsBy,
                                PaypalEmail = item.PaypalEmail,
                                PostPrivacy = item.PostPrivacy,
                                Referrer = item.Referrer,
                                ShareMyData = item.ShareMyData,
                                ShareMyLocation = item.ShareMyLocation,
                                ShowActivitiesPrivacy = item.ShowActivitiesPrivacy,
                                TwoFactor = item.TwoFactor,
                                TwoFactorVerified = item.TwoFactorVerified,
                                Url = item.Url,
                                VisitPrivacy = item.VisitPrivacy,
                                Vk = item.Vk,
                                Wallet = item.Wallet,
                                WorkingLink = item.WorkingLink,
                                Youtube = item.Youtube,
                                City = item.City,
                                State = item.State,
                                Zip = item.Zip,
                                Points = item.Points,
                                DailyPoints = item.DailyPoints,
                                PointDayExpire = item.PointDayExpire,
                                CashfreeSignature = item.CashfreeSignature,
                                IsAdmin = item.IsAdmin,
                                MemberId = item.MemberId,
                                ChatColor = item.ChatColor,
                                PaystackRef = item.PaystackRef,
                                RefUserId = item.RefUserId,
                                SchoolCompleted = item.SchoolCompleted,
                                Type = item.Type,
                                UserPlatform = item.UserPlatform,
                                WeatherUnit = item.WeatherUnit,
                                Details = new DetailsUnion(),
                                Selected = false,
                            };

                            if (!string.IsNullOrEmpty(item.Details))
                                infoObject.Details = new DetailsUnion
                                {
                                    DetailsClass = JsonConvert.DeserializeObject<Details>(item.Details)
                                };
                             
                            list.Add(infoObject);
                        }

                        return list;
                    }
                    else
                    {
                        return new ObservableCollection<UserDataObject>();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new ObservableCollection<UserDataObject>();
            }
        }

        public void Delete_UsersContact(string userId)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return;
                    var user = Connection.Table<DataTables.MyContactsTb>().FirstOrDefault(c => c.UserId == userId);
                    if (user != null)
                    {
                        Connection.Delete(user);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region My Contacts >> Following

        //Insert data To my Followers Table
        public void Insert_Or_Replace_MyFollowersTable(ObservableCollection<UserDataObject> myFollowersList)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return;
                    var result = Connection.Table<DataTables.MyFollowersTb>().ToList();
                    List<DataTables.MyFollowersTb> list = new List<DataTables.MyFollowersTb>();
                    foreach (var info in myFollowersList)
                    {
                        var db = Mapper.Map<DataTables.MyFollowersTb>(info);
                        if (info.Details.DetailsClass != null)
                            db.Details = JsonConvert.SerializeObject(info.Details.DetailsClass);
                        list.Add(db);

                        var update = result.FirstOrDefault(a => a.UserId == info.UserId);
                        if (update != null)
                        {
                            update = Mapper.Map<DataTables.MyFollowersTb>(info);
                            if (info.Details.DetailsClass != null)
                                update.Details = JsonConvert.SerializeObject(info.Details.DetailsClass);

                            Connection.Update(update);
                        }
                    }

                    if (list.Count <= 0) return;

                    Connection.BeginTransaction();

                    //Bring new  
                    var newItemList = list.Where(c => !result.Select(fc => fc.UserId).Contains(c.UserId)).ToList();
                    if (newItemList.Count > 0)
                        Connection.InsertAll(newItemList);
                     
                    result = Connection.Table<DataTables.MyFollowersTb>().ToList();
                    var deleteItemList = result.Where(c => !list.Select(fc => fc.UserId).Contains(c.UserId)).ToList();
                    if (deleteItemList.Count > 0)
                        foreach (var delete in deleteItemList)
                            Connection.Delete(delete);

                    Connection.Commit();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        // Get data To my Followers Table
        public ObservableCollection<UserDataObject> Get_MyFollowers(/*int id = 0, int nSize = 20*/)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return new ObservableCollection<UserDataObject>();
                    // var query = Connection.Table<DataTables.MyFollowersTb>().Where(w => w.AutoIdMyFollowing >= id).OrderBy(q => q.AutoIdMyFollowing).Take(nSize).ToList();

                    var select = Connection.Table<DataTables.MyFollowersTb>().ToList();
                    if (select.Count > 0)
                    {
                        var list = new ObservableCollection<UserDataObject>();
                        foreach (var item in select)
                        {
                            UserDataObject infoObject = new UserDataObject()
                            {
                                UserId = item.UserId,
                                Username = item.Username,
                                Email = item.Email,
                                FirstName = item.FirstName,
                                LastName = item.LastName,
                                Avatar = item.Avatar,
                                Cover = item.Cover,
                                BackgroundImage = item.BackgroundImage,
                                RelationshipId = item.RelationshipId,
                                Address = item.Address,
                                Working = item.Working,
                                Gender = item.Gender,
                                Facebook = item.Facebook,
                                Google = item.Google,
                                Twitter = item.Twitter,
                                Linkedin = item.Linkedin,
                                Website = item.Website,
                                Instagram = item.Instagram,
                                WebDeviceId = item.WebDeviceId,
                                Language = item.Language,
                                IpAddress = item.IpAddress,
                                PhoneNumber = item.PhoneNumber,
                                Timezone = item.Timezone,
                                Lat = item.Lat,
                                Lng = item.Lng,
                                About = item.About,
                                Birthday = item.Birthday,
                                Registered = item.Registered,
                                Lastseen = item.Lastseen,
                                LastLocationUpdate = item.LastLocationUpdate,
                                Balance = item.Balance,
                                Verified = item.Verified,
                                Status = item.Status,
                                Active = item.Active,
                                Admin = item.Admin,
                                IsPro = item.IsPro,
                                ProType = item.ProType,
                                School = item.School,
                                Name = item.Name,
                                AndroidMDeviceId = item.AndroidMDeviceId,
                                ECommented = item.ECommented,
                                AndroidNDeviceId = item.AndroidMDeviceId,
                                AvatarFull = item.AvatarFull,
                                BirthPrivacy = item.BirthPrivacy,
                                CanFollow = item.CanFollow,
                                ConfirmFollowers = item.ConfirmFollowers,
                                CountryId = item.CountryId,
                                EAccepted = item.EAccepted,
                                EFollowed = item.EFollowed,
                                EJoinedGroup = item.EJoinedGroup,
                                ELastNotif = item.ELastNotif,
                                ELiked = item.ELiked,
                                ELikedPage = item.ELikedPage,
                                EMentioned = item.EMentioned,
                                EProfileWallPost = item.EProfileWallPost,
                                ESentmeMsg = item.ESentmeMsg,
                                EShared = item.EShared,
                                EVisited = item.EVisited,
                                EWondered = item.EWondered,
                                EmailNotification = item.EmailNotification,
                                FollowPrivacy = item.FollowPrivacy,
                                FriendPrivacy = item.FriendPrivacy,
                                GenderText = item.GenderText,
                                InfoFile = item.InfoFile,
                                IosMDeviceId = item.IosMDeviceId,
                                IosNDeviceId = item.IosNDeviceId,
                                IsBlocked = item.IsBlocked,
                                IsFollowing = item.IsFollowing,
                                IsFollowingMe = item.IsFollowingMe,
                                LastAvatarMod = item.LastAvatarMod,
                                LastCoverMod = item.LastCoverMod,
                                LastDataUpdate = item.LastDataUpdate,
                                LastFollowId = item.LastFollowId,
                                LastLoginData = item.LastLoginData,
                                LastseenStatus = item.LastseenStatus,
                                LastseenTimeText = item.LastseenTimeText,
                                LastseenUnixTime = item.LastseenUnixTime,
                                MessagePrivacy = item.MessagePrivacy,
                                NewEmail = item.NewEmail,
                                NewPhone = item.NewPhone,
                                NotificationSettings = item.NotificationSettings,
                                NotificationsSound = item.NotificationsSound,
                                OrderPostsBy = item.OrderPostsBy,
                                PaypalEmail = item.PaypalEmail,
                                PostPrivacy = item.PostPrivacy,
                                Referrer = item.Referrer,
                                ShareMyData = item.ShareMyData,
                                ShareMyLocation = item.ShareMyLocation,
                                ShowActivitiesPrivacy = item.ShowActivitiesPrivacy,
                                TwoFactor = item.TwoFactor,
                                TwoFactorVerified = item.TwoFactorVerified,
                                Url = item.Url,
                                VisitPrivacy = item.VisitPrivacy,
                                Vk = item.Vk,
                                Wallet = item.Wallet,
                                WorkingLink = item.WorkingLink,
                                Youtube = item.Youtube,
                                City = item.City,
                                DailyPoints = item.DailyPoints,
                                PointDayExpire = item.PointDayExpire,
                                State = item.State,
                                Zip = item.Zip,
                                CashfreeSignature = item.CashfreeSignature,
                                IsAdmin = item.IsAdmin,
                                MemberId = item.MemberId,
                                ChatColor = item.ChatColor,
                                PaystackRef = item.PaystackRef,
                                RefUserId = item.RefUserId,
                                SchoolCompleted = item.SchoolCompleted,
                                Type = item.Type,
                                UserPlatform = item.UserPlatform,
                                WeatherUnit = item.WeatherUnit,
                                Details = new DetailsUnion(),
                                Selected = false,
                            };

                            if (!string.IsNullOrEmpty(item.Details))
                                infoObject.Details = new DetailsUnion
                                {
                                    DetailsClass = JsonConvert.DeserializeObject<Details>(item.Details)
                                };


                            list.Add(infoObject);
                        }

                        return list;
                    }
                    else
                    {
                        return new ObservableCollection<UserDataObject>();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new ObservableCollection<UserDataObject>();
            }
        }

        public void Delete_MyFollowers(string userId)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return;
                    var user = Connection.Table<DataTables.MyFollowersTb>().FirstOrDefault(c => c.UserId == userId);
                    if (user != null)
                    {
                        Connection.Delete(user);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        // Get data One user To My Contact Table
        public UserDataObject Get_DataOneUser(string userName)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return null;
                    var item = Connection.Table<DataTables.MyContactsTb>().FirstOrDefault(a => a.Username == userName || a.Name == userName);
                    if (item != null)
                    {
                        UserDataObject infoObject = new UserDataObject()
                        {
                            UserId = item.UserId,
                            Username = item.Username,
                            Email = item.Email,
                            FirstName = item.FirstName,
                            LastName = item.LastName,
                            Avatar = item.Avatar,
                            Cover = item.Cover,
                            BackgroundImage = item.BackgroundImage,
                            RelationshipId = item.RelationshipId,
                            Address = item.Address,
                            Working = item.Working,
                            Gender = item.Gender,
                            Facebook = item.Facebook,
                            Google = item.Google,
                            Twitter = item.Twitter,
                            Linkedin = item.Linkedin,
                            Website = item.Website,
                            Instagram = item.Instagram,
                            WebDeviceId = item.WebDeviceId,
                            Language = item.Language,
                            IpAddress = item.IpAddress,
                            PhoneNumber = item.PhoneNumber,
                            Timezone = item.Timezone,
                            Lat = item.Lat,
                            Lng = item.Lng,
                            About = item.About,
                            Birthday = item.Birthday,
                            Registered = item.Registered,
                            Lastseen = item.Lastseen,
                            LastLocationUpdate = item.LastLocationUpdate,
                            Balance = item.Balance,
                            Verified = item.Verified,
                            Status = item.Status,
                            Active = item.Active,
                            Admin = item.Admin,
                            IsPro = item.IsPro,
                            ProType = item.ProType,
                            School = item.School,
                            Name = item.Name,
                            AndroidMDeviceId = item.AndroidMDeviceId,
                            ECommented = item.ECommented,
                            AndroidNDeviceId = item.AndroidMDeviceId,
                            AvatarFull = item.AvatarFull,
                            BirthPrivacy = item.BirthPrivacy,
                            CanFollow = item.CanFollow,
                            ConfirmFollowers = item.ConfirmFollowers,
                            CountryId = item.CountryId,
                            EAccepted = item.EAccepted,
                            EFollowed = item.EFollowed,
                            EJoinedGroup = item.EJoinedGroup,
                            ELastNotif = item.ELastNotif,
                            ELiked = item.ELiked,
                            ELikedPage = item.ELikedPage,
                            EMentioned = item.EMentioned,
                            EProfileWallPost = item.EProfileWallPost,
                            ESentmeMsg = item.ESentmeMsg,
                            EShared = item.EShared,
                            EVisited = item.EVisited,
                            EWondered = item.EWondered,
                            EmailNotification = item.EmailNotification,
                            FollowPrivacy = item.FollowPrivacy,
                            FriendPrivacy = item.FriendPrivacy,
                            GenderText = item.GenderText,
                            InfoFile = item.InfoFile,
                            IosMDeviceId = item.IosMDeviceId,
                            IosNDeviceId = item.IosNDeviceId,
                            IsBlocked = item.IsBlocked,
                            IsFollowing = item.IsFollowing,
                            IsFollowingMe = item.IsFollowingMe,
                            LastAvatarMod = item.LastAvatarMod,
                            LastCoverMod = item.LastCoverMod,
                            LastDataUpdate = item.LastDataUpdate,
                            LastFollowId = item.LastFollowId,
                            LastLoginData = item.LastLoginData,
                            LastseenStatus = item.LastseenStatus,
                            LastseenTimeText = item.LastseenTimeText,
                            LastseenUnixTime = item.LastseenUnixTime,
                            MessagePrivacy = item.MessagePrivacy,
                            NewEmail = item.NewEmail,
                            NewPhone = item.NewPhone,
                            NotificationSettings = item.NotificationSettings,
                            NotificationsSound = item.NotificationsSound,
                            OrderPostsBy = item.OrderPostsBy,
                            PaypalEmail = item.PaypalEmail,
                            PostPrivacy = item.PostPrivacy,
                            Referrer = item.Referrer,
                            ShareMyData = item.ShareMyData,
                            ShareMyLocation = item.ShareMyLocation,
                            ShowActivitiesPrivacy = item.ShowActivitiesPrivacy,
                            TwoFactor = item.TwoFactor,
                            TwoFactorVerified = item.TwoFactorVerified,
                            Url = item.Url,
                            VisitPrivacy = item.VisitPrivacy,
                            Vk = item.Vk,
                            Wallet = item.Wallet,
                            WorkingLink = item.WorkingLink,
                            Youtube = item.Youtube,
                            City = item.City,
                            DailyPoints = item.DailyPoints,
                            PointDayExpire = item.PointDayExpire,
                            State = item.State,
                            Zip = item.Zip,
                            CashfreeSignature = item.CashfreeSignature,
                            IsAdmin = item.IsAdmin,
                            MemberId = item.MemberId,
                            ChatColor = item.ChatColor,
                            PaystackRef = item.PaystackRef,
                            Points = item.Points,
                            RefUserId = item.RefUserId,
                            SchoolCompleted = item.SchoolCompleted,
                            Type = item.Type,
                            UserPlatform = item.UserPlatform,
                            WeatherUnit = item.WeatherUnit,
                            Details = new DetailsUnion(),
                            Selected = false,
                        };

                        if (!string.IsNullOrEmpty(item.Details))
                            infoObject.Details = new DetailsUnion
                            {
                                DetailsClass = JsonConvert.DeserializeObject<Details>(item.Details)
                            };

                        return infoObject;
                    }
                    else
                    {
                        var userFollowers = Connection.Table<DataTables.MyFollowersTb>().FirstOrDefault(a => a.Username == userName || a.Name == userName);
                        if (userFollowers != null)
                        {
                            UserDataObject infoObject = new UserDataObject()
                            {
                                UserId = userFollowers.UserId,
                                Username = userFollowers.Username,
                                Email = userFollowers.Email,
                                FirstName = userFollowers.FirstName,
                                LastName = userFollowers.LastName,
                                Avatar = userFollowers.Avatar,
                                Cover = userFollowers.Cover,
                                BackgroundImage = userFollowers.BackgroundImage,
                                RelationshipId = userFollowers.RelationshipId,
                                Address = userFollowers.Address,
                                Working = userFollowers.Working,
                                Gender = userFollowers.Gender,
                                Facebook = userFollowers.Facebook,
                                Google = userFollowers.Google,
                                Twitter = userFollowers.Twitter,
                                Linkedin = userFollowers.Linkedin,
                                Website = userFollowers.Website,
                                Instagram = userFollowers.Instagram,
                                WebDeviceId = userFollowers.WebDeviceId,
                                Language = userFollowers.Language,
                                IpAddress = userFollowers.IpAddress,
                                PhoneNumber = userFollowers.PhoneNumber,
                                Timezone = userFollowers.Timezone,
                                Lat = userFollowers.Lat,
                                Lng = userFollowers.Lng,
                                About = userFollowers.About,
                                Birthday = userFollowers.Birthday,
                                Registered = userFollowers.Registered,
                                Lastseen = userFollowers.Lastseen,
                                LastLocationUpdate = userFollowers.LastLocationUpdate,
                                Balance = userFollowers.Balance,
                                Verified = userFollowers.Verified,
                                Status = userFollowers.Status,
                                Active = userFollowers.Active,
                                Admin = userFollowers.Admin,
                                IsPro = userFollowers.IsPro,
                                ProType = userFollowers.ProType,
                                School = userFollowers.School,
                                Name = userFollowers.Name,
                                AndroidMDeviceId = userFollowers.AndroidMDeviceId,
                                ECommented = userFollowers.ECommented,
                                AndroidNDeviceId = userFollowers.AndroidMDeviceId,
                                AvatarFull = userFollowers.AvatarFull,
                                BirthPrivacy = userFollowers.BirthPrivacy,
                                CanFollow = userFollowers.CanFollow,
                                ConfirmFollowers = userFollowers.ConfirmFollowers,
                                CountryId = userFollowers.CountryId,
                                EAccepted = userFollowers.EAccepted,
                                EFollowed = userFollowers.EFollowed,
                                EJoinedGroup = userFollowers.EJoinedGroup,
                                ELastNotif = userFollowers.ELastNotif,
                                ELiked = userFollowers.ELiked,
                                ELikedPage = userFollowers.ELikedPage,
                                EMentioned = userFollowers.EMentioned,
                                EProfileWallPost = userFollowers.EProfileWallPost,
                                ESentmeMsg = userFollowers.ESentmeMsg,
                                EShared = userFollowers.EShared,
                                EVisited = userFollowers.EVisited,
                                EWondered = userFollowers.EWondered,
                                EmailNotification = userFollowers.EmailNotification,
                                FollowPrivacy = userFollowers.FollowPrivacy,
                                FriendPrivacy = userFollowers.FriendPrivacy,
                                GenderText = userFollowers.GenderText,
                                InfoFile = userFollowers.InfoFile,
                                IosMDeviceId = userFollowers.IosMDeviceId,
                                IosNDeviceId = userFollowers.IosNDeviceId,
                                IsBlocked = userFollowers.IsBlocked,
                                IsFollowing = userFollowers.IsFollowing,
                                IsFollowingMe = userFollowers.IsFollowingMe,
                                LastAvatarMod = userFollowers.LastAvatarMod,
                                LastCoverMod = userFollowers.LastCoverMod,
                                LastDataUpdate = userFollowers.LastDataUpdate,
                                LastFollowId = userFollowers.LastFollowId,
                                LastLoginData = userFollowers.LastLoginData,
                                LastseenStatus = userFollowers.LastseenStatus,
                                LastseenTimeText = userFollowers.LastseenTimeText,
                                LastseenUnixTime = userFollowers.LastseenUnixTime,
                                MessagePrivacy = userFollowers.MessagePrivacy,
                                NewEmail = userFollowers.NewEmail,
                                NewPhone = userFollowers.NewPhone,
                                NotificationSettings = userFollowers.NotificationSettings,
                                NotificationsSound = userFollowers.NotificationsSound,
                                OrderPostsBy = userFollowers.OrderPostsBy,
                                PaypalEmail = userFollowers.PaypalEmail,
                                PostPrivacy = userFollowers.PostPrivacy,
                                Referrer = userFollowers.Referrer,
                                ShareMyData = userFollowers.ShareMyData,
                                ShareMyLocation = userFollowers.ShareMyLocation,
                                ShowActivitiesPrivacy = userFollowers.ShowActivitiesPrivacy,
                                TwoFactor = userFollowers.TwoFactor,
                                TwoFactorVerified = userFollowers.TwoFactorVerified,
                                Url = userFollowers.Url,
                                VisitPrivacy = userFollowers.VisitPrivacy,
                                Vk = userFollowers.Vk,
                                Wallet = userFollowers.Wallet,
                                WorkingLink = userFollowers.WorkingLink,
                                Youtube = userFollowers.Youtube,
                                City = userFollowers.City,
                                DailyPoints = userFollowers.DailyPoints,
                                PointDayExpire = userFollowers.PointDayExpire,
                                State = userFollowers.State,
                                Zip = userFollowers.Zip,
                                CashfreeSignature = userFollowers.CashfreeSignature,
                                IsAdmin = userFollowers.IsAdmin,
                                MemberId = userFollowers.MemberId,
                                ChatColor = userFollowers.ChatColor,
                                PaystackRef = userFollowers.PaystackRef,
                                Points = userFollowers.Points,
                                RefUserId = userFollowers.RefUserId,
                                SchoolCompleted = userFollowers.SchoolCompleted,
                                Type = userFollowers.Type,
                                UserPlatform = userFollowers.UserPlatform,
                                WeatherUnit = userFollowers.WeatherUnit,
                                Details = new DetailsUnion(),
                                Selected = false,
                            };

                            if (!string.IsNullOrEmpty(userFollowers.Details))
                                infoObject.Details = new DetailsUnion
                                {
                                    DetailsClass = JsonConvert.DeserializeObject<Details>(userFollowers.Details)
                                };

                            return infoObject;
                        }

                        return null;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
         
        #region My Profile

        //Insert Or Update data My Profile Table
        public void Insert_Or_Update_To_MyProfileTable(UserDataObject info)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return;
                    var resultInfoTb = Connection.Table<DataTables.MyProfileTb>().FirstOrDefault();
                    if (resultInfoTb != null)
                    {
                        resultInfoTb = new DataTables.MyProfileTb()
                        {
                            UserId = info.UserId,
                            Username = info.Username,
                            Email = info.Email,
                            FirstName = info.FirstName,
                            LastName = info.LastName,
                            Avatar = info.Avatar,
                            Cover = info.Cover,
                            BackgroundImage = info.BackgroundImage,
                            RelationshipId = info.RelationshipId,
                            Address = info.Address,
                            Working = info.Working,
                            Gender = info.Gender,
                            Facebook = info.Facebook,
                            Google = info.Google,
                            Twitter = info.Twitter,
                            Linkedin = info.Linkedin,
                            Website = info.Website,
                            Instagram = info.Instagram,
                            WebDeviceId = info.WebDeviceId,
                            Language = info.Language,
                            IpAddress = info.IpAddress,
                            PhoneNumber = info.PhoneNumber,
                            Timezone = info.Timezone,
                            Lat = info.Lat,
                            Lng = info.Lng,
                            About = info.About,
                            Birthday = info.Birthday,
                            Registered = info.Registered,
                            Lastseen = info.Lastseen,
                            LastLocationUpdate = info.LastLocationUpdate,
                            Balance = info.Balance,
                            Verified = info.Verified,
                            Status = info.Status,
                            Active = info.Active,
                            Admin = info.Admin,
                            IsPro = info.IsPro,
                            ProType = info.ProType,
                            School = info.School,
                            Name = info.Name,
                            AndroidMDeviceId = info.AndroidMDeviceId,
                            ECommented = info.ECommented,
                            AndroidNDeviceId = info.AndroidMDeviceId,
                            AvatarFull = info.AvatarFull,
                            BirthPrivacy = info.BirthPrivacy,
                            CanFollow = info.CanFollow,
                            ConfirmFollowers = info.ConfirmFollowers,
                            CountryId = info.CountryId,
                            EAccepted = info.EAccepted,
                            EFollowed = info.EFollowed,
                            EJoinedGroup = info.EJoinedGroup,
                            ELastNotif = info.ELastNotif,
                            ELiked = info.ELiked,
                            ELikedPage = info.ELikedPage,
                            EMentioned = info.EMentioned,
                            EProfileWallPost = info.EProfileWallPost,
                            ESentmeMsg = info.ESentmeMsg,
                            EShared = info.EShared,
                            EVisited = info.EVisited,
                            EWondered = info.EWondered,
                            EmailNotification = info.EmailNotification,
                            FollowPrivacy = info.FollowPrivacy,
                            FriendPrivacy = info.FriendPrivacy,
                            GenderText = info.GenderText,
                            InfoFile = info.InfoFile,
                            IosMDeviceId = info.IosMDeviceId,
                            IosNDeviceId = info.IosNDeviceId,
                            IsBlocked = info.IsBlocked,
                            IsFollowing = info.IsFollowing,
                            IsFollowingMe = info.IsFollowingMe,
                            LastAvatarMod = info.LastAvatarMod,
                            LastCoverMod = info.LastCoverMod,
                            LastDataUpdate = info.LastDataUpdate,
                            LastFollowId = info.LastFollowId,
                            LastLoginData = info.LastLoginData,
                            LastseenStatus = info.LastseenStatus,
                            LastseenTimeText = info.LastseenTimeText,
                            LastseenUnixTime = info.LastseenUnixTime,
                            MessagePrivacy = info.MessagePrivacy,
                            NewEmail = info.NewEmail,
                            NewPhone = info.NewPhone,
                            NotificationSettings = info.NotificationSettings,
                            NotificationsSound = info.NotificationsSound,
                            OrderPostsBy = info.OrderPostsBy,
                            PaypalEmail = info.PaypalEmail,
                            PostPrivacy = info.PostPrivacy,
                            Referrer = info.Referrer,
                            ShareMyData = info.ShareMyData,
                            ShareMyLocation = info.ShareMyLocation,
                            ShowActivitiesPrivacy = info.ShowActivitiesPrivacy,
                            TwoFactor = info.TwoFactor,
                            TwoFactorVerified = info.TwoFactorVerified,
                            Url = info.Url,
                            VisitPrivacy = info.VisitPrivacy,
                            Vk = info.Vk,
                            Wallet = info.Wallet,
                            WorkingLink = info.WorkingLink,
                            Youtube = info.Youtube,
                            City = info.City,
                            Points = info.Points,
                            DailyPoints = info.DailyPoints,
                            PointDayExpire = info.PointDayExpire,
                            State = info.State,
                            Zip = info.Zip,
                            CashfreeSignature = info.CashfreeSignature,
                            IsAdmin = info.IsAdmin,
                            MemberId = info.MemberId,
                            ChatColor = info.ChatColor,
                            PaystackRef = info.PaystackRef,
                            RefUserId = info.RefUserId,
                            SchoolCompleted = info.SchoolCompleted,
                            Type = info.Type,
                            UserPlatform = info.UserPlatform,
                            WeatherUnit = info.WeatherUnit,
                            Details = string.Empty,
                            Selected = false,
                        };

                        if (info.Details.DetailsClass != null)
                            resultInfoTb.Details = JsonConvert.SerializeObject(info.Details.DetailsClass);
                        Connection.Update(resultInfoTb);
                    }
                    else
                    {
                        DataTables.MyProfileTb db = new DataTables.MyProfileTb()
                        {
                            UserId = info.UserId,
                            Username = info.Username,
                            Email = info.Email,
                            FirstName = info.FirstName,
                            LastName = info.LastName,
                            Avatar = info.Avatar,
                            Cover = info.Cover,
                            BackgroundImage = info.BackgroundImage,
                            RelationshipId = info.RelationshipId,
                            Address = info.Address,
                            Working = info.Working,
                            Gender = info.Gender,
                            Facebook = info.Facebook,
                            Google = info.Google,
                            Twitter = info.Twitter,
                            Linkedin = info.Linkedin,
                            Website = info.Website,
                            Instagram = info.Instagram,
                            WebDeviceId = info.WebDeviceId,
                            Language = info.Language,
                            IpAddress = info.IpAddress,
                            PhoneNumber = info.PhoneNumber,
                            Timezone = info.Timezone,
                            Lat = info.Lat,
                            Lng = info.Lng,
                            About = info.About,
                            Birthday = info.Birthday,
                            Registered = info.Registered,
                            Lastseen = info.Lastseen,
                            LastLocationUpdate = info.LastLocationUpdate,
                            Balance = info.Balance,
                            Verified = info.Verified,
                            Status = info.Status,
                            Active = info.Active,
                            Admin = info.Admin,
                            IsPro = info.IsPro,
                            ProType = info.ProType,
                            School = info.School,
                            Name = info.Name,
                            AndroidMDeviceId = info.AndroidMDeviceId,
                            ECommented = info.ECommented,
                            AndroidNDeviceId = info.AndroidMDeviceId,
                            AvatarFull = info.AvatarFull,
                            BirthPrivacy = info.BirthPrivacy,
                            CanFollow = info.CanFollow,
                            ConfirmFollowers = info.ConfirmFollowers,
                            CountryId = info.CountryId,
                            EAccepted = info.EAccepted,
                            EFollowed = info.EFollowed,
                            EJoinedGroup = info.EJoinedGroup,
                            ELastNotif = info.ELastNotif,
                            ELiked = info.ELiked,
                            ELikedPage = info.ELikedPage,
                            EMentioned = info.EMentioned,
                            EProfileWallPost = info.EProfileWallPost,
                            ESentmeMsg = info.ESentmeMsg,
                            EShared = info.EShared,
                            EVisited = info.EVisited,
                            EWondered = info.EWondered,
                            EmailNotification = info.EmailNotification,
                            FollowPrivacy = info.FollowPrivacy,
                            FriendPrivacy = info.FriendPrivacy,
                            GenderText = info.GenderText,
                            InfoFile = info.InfoFile,
                            IosMDeviceId = info.IosMDeviceId,
                            IosNDeviceId = info.IosNDeviceId,
                            IsBlocked = info.IsBlocked,
                            IsFollowing = info.IsFollowing,
                            IsFollowingMe = info.IsFollowingMe,
                            LastAvatarMod = info.LastAvatarMod,
                            LastCoverMod = info.LastCoverMod,
                            LastDataUpdate = info.LastDataUpdate,
                            LastFollowId = info.LastFollowId,
                            LastLoginData = info.LastLoginData,
                            LastseenStatus = info.LastseenStatus,
                            LastseenTimeText = info.LastseenTimeText,
                            LastseenUnixTime = info.LastseenUnixTime,
                            MessagePrivacy = info.MessagePrivacy,
                            NewEmail = info.NewEmail,
                            NewPhone = info.NewPhone,
                            NotificationSettings = info.NotificationSettings,
                            NotificationsSound = info.NotificationsSound,
                            OrderPostsBy = info.OrderPostsBy,
                            PaypalEmail = info.PaypalEmail,
                            PostPrivacy = info.PostPrivacy,
                            Referrer = info.Referrer,
                            ShareMyData = info.ShareMyData,
                            ShareMyLocation = info.ShareMyLocation,
                            ShowActivitiesPrivacy = info.ShowActivitiesPrivacy,
                            TwoFactor = info.TwoFactor,
                            TwoFactorVerified = info.TwoFactorVerified,
                            Url = info.Url,
                            VisitPrivacy = info.VisitPrivacy,
                            Vk = info.Vk,
                            Wallet = info.Wallet,
                            WorkingLink = info.WorkingLink,
                            Youtube = info.Youtube,
                            City = info.City,
                            Points = info.Points,
                            DailyPoints = info.DailyPoints,
                            PointDayExpire = info.PointDayExpire,
                            State = info.State,
                            Zip = info.Zip,
                            CashfreeSignature = info.CashfreeSignature,
                            IsAdmin = info.IsAdmin,
                            MemberId = info.MemberId,
                            ChatColor = info.ChatColor,
                            PaystackRef = info.PaystackRef,
                            RefUserId = info.RefUserId,
                            SchoolCompleted = info.SchoolCompleted,
                            Type = info.Type,
                            UserPlatform = info.UserPlatform,
                            WeatherUnit = info.WeatherUnit, 
                            Details = string.Empty,
                            Selected = false,
                        };

                        if (info.Details.DetailsClass != null)
                            db.Details = JsonConvert.SerializeObject(info.Details.DetailsClass);
                        Connection.Insert(db);
                    }

                    UserDetails.Avatar = info.Avatar;
                    UserDetails.Cover = info.Cover;
                    UserDetails.Username = info.Username;
                    UserDetails.FullName = info.Name;
                    UserDetails.Email = info.Email;

                    ListUtils.MyProfileList = new ObservableCollection<UserDataObject>();
                    ListUtils.MyProfileList.Clear();
                    ListUtils.MyProfileList.Add(info);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        // Get data To My Profile Table
        public UserDataObject Get_MyProfile()
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return null;
                    var item = Connection.Table<DataTables.MyProfileTb>().FirstOrDefault();
                    if (item != null)
                    {
                        UserDataObject infoObject = new UserDataObject()
                        {
                            UserId = item.UserId,
                            Username = item.Username,
                            Email = item.Email,
                            FirstName = item.FirstName,
                            LastName = item.LastName,
                            Avatar = item.Avatar,
                            Cover = item.Cover,
                            BackgroundImage = item.BackgroundImage,
                            RelationshipId = item.RelationshipId,
                            Address = item.Address,
                            Working = item.Working,
                            Gender = item.Gender,
                            Facebook = item.Facebook,
                            Google = item.Google,
                            Twitter = item.Twitter,
                            Linkedin = item.Linkedin,
                            Website = item.Website,
                            Instagram = item.Instagram,
                            WebDeviceId = item.WebDeviceId,
                            Language = item.Language,
                            IpAddress = item.IpAddress,
                            PhoneNumber = item.PhoneNumber,
                            Timezone = item.Timezone,
                            Lat = item.Lat,
                            Lng = item.Lng,
                            About = item.About,
                            Birthday = item.Birthday,
                            Registered = item.Registered,
                            Lastseen = item.Lastseen,
                            LastLocationUpdate = item.LastLocationUpdate,
                            Balance = item.Balance,
                            Verified = item.Verified,
                            Status = item.Status,
                            Active = item.Active,
                            Admin = item.Admin,
                            IsPro = item.IsPro,
                            ProType = item.ProType,
                            School = item.School,
                            Name = item.Name,
                            AndroidMDeviceId = item.AndroidMDeviceId,
                            ECommented = item.ECommented,
                            AndroidNDeviceId = item.AndroidMDeviceId,
                            AvatarFull = item.AvatarFull,
                            BirthPrivacy = item.BirthPrivacy,
                            CanFollow = item.CanFollow,
                            ConfirmFollowers = item.ConfirmFollowers,
                            CountryId = item.CountryId,
                            EAccepted = item.EAccepted,
                            EFollowed = item.EFollowed,
                            EJoinedGroup = item.EJoinedGroup,
                            ELastNotif = item.ELastNotif,
                            ELiked = item.ELiked,
                            ELikedPage = item.ELikedPage,
                            EMentioned = item.EMentioned,
                            EProfileWallPost = item.EProfileWallPost,
                            ESentmeMsg = item.ESentmeMsg,
                            EShared = item.EShared,
                            EVisited = item.EVisited,
                            EWondered = item.EWondered,
                            EmailNotification = item.EmailNotification,
                            FollowPrivacy = item.FollowPrivacy,
                            FriendPrivacy = item.FriendPrivacy,
                            GenderText = item.GenderText,
                            InfoFile = item.InfoFile,
                            IosMDeviceId = item.IosMDeviceId,
                            IosNDeviceId = item.IosNDeviceId,
                            IsBlocked = item.IsBlocked,
                            IsFollowing = item.IsFollowing,
                            IsFollowingMe = item.IsFollowingMe,
                            LastAvatarMod = item.LastAvatarMod,
                            LastCoverMod = item.LastCoverMod,
                            LastDataUpdate = item.LastDataUpdate,
                            LastFollowId = item.LastFollowId,
                            LastLoginData = item.LastLoginData,
                            LastseenStatus = item.LastseenStatus,
                            LastseenTimeText = item.LastseenTimeText,
                            LastseenUnixTime = item.LastseenUnixTime,
                            MessagePrivacy = item.MessagePrivacy,
                            NewEmail = item.NewEmail,
                            NewPhone = item.NewPhone,
                            NotificationSettings = item.NotificationSettings,
                            NotificationsSound = item.NotificationsSound,
                            OrderPostsBy = item.OrderPostsBy,
                            PaypalEmail = item.PaypalEmail,
                            PostPrivacy = item.PostPrivacy,
                            Referrer = item.Referrer,
                            ShareMyData = item.ShareMyData,
                            ShareMyLocation = item.ShareMyLocation,
                            ShowActivitiesPrivacy = item.ShowActivitiesPrivacy,
                            TwoFactor = item.TwoFactor,
                            TwoFactorVerified = item.TwoFactorVerified,
                            Url = item.Url,
                            VisitPrivacy = item.VisitPrivacy,
                            Vk = item.Vk,
                            Wallet = item.Wallet,
                            WorkingLink = item.WorkingLink,
                            Youtube = item.Youtube,
                            City = item.City,
                            Points = item.Points,
                            DailyPoints = item.DailyPoints,
                            PointDayExpire = item.PointDayExpire,
                            State = item.State,
                            Zip = item.Zip,
                            CashfreeSignature = item.CashfreeSignature,
                            IsAdmin = item.IsAdmin,
                            MemberId = item.MemberId,
                            ChatColor = item.ChatColor,
                            PaystackRef = item.PaystackRef,
                            RefUserId = item.RefUserId,
                            SchoolCompleted = item.SchoolCompleted,
                            Type = item.Type,
                            UserPlatform = item.UserPlatform,
                            WeatherUnit = item.WeatherUnit,
                            Details = new DetailsUnion(),
                            Selected = false,
                        };

                        if (!string.IsNullOrEmpty(item.Details))
                            infoObject.Details = new DetailsUnion
                            {
                                DetailsClass = JsonConvert.DeserializeObject<Details>(item.Details)
                            };


                        UserDetails.Avatar = item.Avatar;
                        UserDetails.Cover = item.Cover;
                        UserDetails.Username = item.Username;
                        UserDetails.FullName = item.Name;
                        UserDetails.Email = item.Email;

                        ListUtils.MyProfileList = new ObservableCollection<UserDataObject>();
                        ListUtils.MyProfileList.Clear();
                        ListUtils.MyProfileList.Add(infoObject);

                        return infoObject;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        #endregion

        #region Search Filter 

        public void InsertOrUpdate_SearchFilter(DataTables.SearchFilterTb dataFilter)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return;
                    var data = Connection.Table<DataTables.SearchFilterTb>().FirstOrDefault();
                    if (data == null)
                    {
                        Connection.Insert(dataFilter);
                    }
                    else
                    {
                        data.Gender = dataFilter.Gender;
                        data.Country = dataFilter.Country;
                        data.Status = dataFilter.Status;
                        data.Verified = dataFilter.Verified;
                        data.ProfilePicture = dataFilter.ProfilePicture;
                        data.FilterByAge = dataFilter.FilterByAge;
                        data.AgeFrom = dataFilter.AgeFrom;
                        data.AgeTo = dataFilter.AgeTo;

                        Connection.Update(data);
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public DataTables.SearchFilterTb GetSearchFilterById()
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    var data = Connection?.Table<DataTables.SearchFilterTb>().FirstOrDefault();
                    return data;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        #endregion

        #region Near By Filter 

        public void InsertOrUpdate_NearByFilter(DataTables.NearByFilterTb dataFilter)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return;
                    var data = Connection.Table<DataTables.NearByFilterTb>().FirstOrDefault();
                    if (data == null)
                    {
                        Connection.Insert(dataFilter);
                    }
                    else
                    {
                        data.DistanceValue = dataFilter.DistanceValue;
                        data.Gender = dataFilter.Gender;
                        data.Status = dataFilter.Status;
                        data.Relationship = dataFilter.Relationship;

                        Connection.Update(data);
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public DataTables.NearByFilterTb GetNearByFilterById()
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    var data = Connection?.Table<DataTables.NearByFilterTb>().FirstOrDefault();
                    return data;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        #endregion

        #region WatchOffline Videos

        //Insert WatchOffline Videos
        public void Insert_WatchOfflineVideos(GetMoviesObject.Movie video)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return;
                    if (video != null)
                    {
                        var select = Connection.Table<DataTables.WatchOfflineVideosTb>().FirstOrDefault(a => a.Id == video.Id);
                        if (select == null)
                        {
                            DataTables.WatchOfflineVideosTb watchOffline = new DataTables.WatchOfflineVideosTb()
                            {
                                Id = video.Id,
                                Name = video.Name,
                                Cover = video.Cover,
                                Description = video.Description,
                                Country = video.Country,
                                Duration = video.Duration,
                                Genre = video.Genre,
                                Iframe = video.Iframe,
                                Quality = video.Quality,
                                Producer = video.Producer,
                                Release = video.Release,
                                Source = video.Source,
                                Stars = video.Stars,
                                Url = video.Url,
                                Video = video.Video,
                                Views = video.Views,
                            };

                            Connection.Insert(watchOffline);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Remove WatchOffline Videos
        public void Remove_WatchOfflineVideos(string watchOfflineVideosId)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return;
                    if (!string.IsNullOrEmpty(watchOfflineVideosId))
                    {
                        var select = Connection.Table<DataTables.WatchOfflineVideosTb>().FirstOrDefault(a => a.Id == watchOfflineVideosId);
                        if (select != null)
                        {
                            Connection.Delete(select);
                        }
                    }
                }

            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Get WatchOffline Videos
        public ObservableCollection<DataTables.WatchOfflineVideosTb> Get_WatchOfflineVideos()
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return new ObservableCollection<DataTables.WatchOfflineVideosTb>();
                    var select = Connection.Table<DataTables.WatchOfflineVideosTb>().OrderByDescending(a => a.AutoIdWatchOfflineVideos).ToList();
                    if (select.Count > 0)
                    {
                        return new ObservableCollection<DataTables.WatchOfflineVideosTb>(select);
                    }
                    else
                    {
                        return new ObservableCollection<DataTables.WatchOfflineVideosTb>();
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return new ObservableCollection<DataTables.WatchOfflineVideosTb>();
            }
        }

        //Get WatchOffline Videos
        public GetMoviesObject.Movie Get_WatchOfflineVideos_ById(string id)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return null;
                    var video = Connection.Table<DataTables.WatchOfflineVideosTb>().FirstOrDefault(a => a.Id == id);
                    if (video != null)
                    {
                        GetMoviesObject.Movie watchOffline = new GetMoviesObject.Movie()
                        {
                            Id = video.Id,
                            Name = video.Name,
                            Cover = video.Cover,
                            Description = video.Description,
                            Country = video.Country,
                            Duration = video.Duration,
                            Genre = video.Genre,
                            Iframe = video.Iframe,
                            Quality = video.Quality,
                            Producer = video.Producer,
                            Release = video.Release,
                            Source = video.Source,
                            Stars = video.Stars,
                            Url = video.Url,
                            Video = video.Video,
                            Views = video.Views,
                        };

                        return watchOffline;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return null;
            }
        }

        public DataTables.WatchOfflineVideosTb Update_WatchOfflineVideos(string videoId, string videoPath)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return null;
                    var select = Connection.Table<DataTables.WatchOfflineVideosTb>().FirstOrDefault(a => a.Id == videoId);
                    if (select != null)
                    {
                        select.VideoName = videoId + ".mp4";
                        select.VideoSavedPath = videoPath;

                        Connection.Update(select);

                        return select;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return null;
            }
        }

        #endregion

        #region Gifts

        //Insert data Gifts
        public void InsertAllGifts(ObservableCollection<GiftObject.DataGiftObject> listData)
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return;
                    var result = Connection.Table<DataTables.GiftsTb>().ToList();

                    List<DataTables.GiftsTb> list = new List<DataTables.GiftsTb>();
                    foreach (var info in listData)
                    {
                        var gift = new DataTables.GiftsTb
                        {
                            Id = info.Id,
                            MediaFile = info.MediaFile,
                            Name = info.Name,
                            Time = info.Time,
                            TimeText = info.TimeText,
                        };

                        list.Add(gift);

                        var update = result.FirstOrDefault(a => a.Id == info.Id);
                        if (update != null)
                        {
                            update = Mapper.Map<DataTables.GiftsTb>(info); 
                            Connection.Update(update);
                        }
                    }
                     
                    if (list.Count <= 0) return;
                    Connection.BeginTransaction();
                    //Bring new  
                    var newItemList = list.Where(c => !result.Select(fc => fc.Id).Contains(c.Id)).ToList();
                    if (newItemList.Count > 0)
                    {
                        Connection.InsertAll(newItemList);
                    }
                     
                    Connection.Commit();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Get List Gifts 
        public ObservableCollection<GiftObject.DataGiftObject> GetGiftsList()
        {
            try
            {
                using (Connection = OpenConnection())
                {
                    if (Connection == null) return new ObservableCollection<GiftObject.DataGiftObject>();
                    var result = Connection.Table<DataTables.GiftsTb>().ToList();
                    if (result?.Count > 0)
                    {
                        List<GiftObject.DataGiftObject> list = result.Select(gift => new GiftObject.DataGiftObject
                        {
                            Id = gift.Id,
                            MediaFile = gift.MediaFile,
                            Name = gift.Name,
                            Time = gift.Time,
                            TimeText = gift.TimeText,
                        }).ToList();

                        return new ObservableCollection<GiftObject.DataGiftObject>(list);
                    }
                    else
                    {
                        return new ObservableCollection<GiftObject.DataGiftObject>();
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return new ObservableCollection<GiftObject.DataGiftObject>();
            }
        }

        #endregion
         
    }
}