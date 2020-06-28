using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Common.Apis;
using Android.Gms.Tasks;
using Android.Gms.Wallet;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.App;
using Android.Text;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using Com.Stripe.Android;
using Com.Stripe.Android.Model;
using Java.Lang;
using ObeeNetwork.Activities.Wallet;
using ObeeNetwork.Helpers.Utils;
using ObeeNetwork.SQLite;
using ObeeNetworkClient.Requests;
using Exception = System.Exception;
using Math = System.Math;
using Task = Android.Gms.Tasks.Task;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace ObeeNetwork.Payment
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class PaymentCardDetailsActivity : AppCompatActivity, IOnCompleteListener, ITokenCallback  
    {
        #region Variables Basic

        private TextView CardNumber, CardExpire, CardCvv, CardName;
        private EditText EtCardNumber, EtExpire, EtCvv, EtName;
        private Button BtnApply;

        private Stripe Stripe;
        private PaymentsClient MPaymentsClient;
        private readonly int LoadPaymentDataRequestCode = 53;

        private string Price, PayType, Id;
        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                Methods.App.FullScreenApp(this);

                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);

                // Create your application here
                SetContentView(Resource.Layout.PaymentCardDetailsLayout);

                Id = Intent.GetStringExtra("Id") ?? "";
                Price = Intent.GetStringExtra("Price");
                PayType = Intent.GetStringExtra("payType");

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                InitWalletStripe();
                
                Plugin.CurrentActivity.CrossCurrentActivity.Current.Activity = this; 
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
                CardNumber = (TextView) FindViewById(Resource.Id.card_number);
                CardExpire = (TextView) FindViewById(Resource.Id.card_expire);
                CardCvv = (TextView) FindViewById(Resource.Id.card_cvv);
                CardName = (TextView) FindViewById(Resource.Id.card_name);

                EtCardNumber = (EditText) FindViewById(Resource.Id.et_card_number);
                EtExpire = (EditText) FindViewById(Resource.Id.et_expire);
                EtCvv = (EditText) FindViewById(Resource.Id.et_cvv);
                EtName = (EditText) FindViewById(Resource.Id.et_name);
                BtnApply = (Button) FindViewById(Resource.Id.ApplyButton);

                Methods.SetColorEditText(EtCardNumber, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(EtExpire, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(EtCvv, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(EtName, AppSettings.SetTabDarkTheme ? Color.White : Color.Black); 
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
                    toolbar.Title = GetString(Resource.String.Lbl_CreditCard);
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
                    EtCardNumber.TextChanged += EtCardNumberOnTextChanged;
                    EtExpire.TextChanged += EtExpireOnTextChanged;
                    EtCvv.TextChanged += EtCvvOnTextChanged;
                    EtName.TextChanged += EtNameOnTextChanged;
                    BtnApply.Click += BtnApplyOnClick;
                }
                else
                {
                    EtCardNumber.TextChanged -= EtCardNumberOnTextChanged;
                    EtExpire.TextChanged -= EtExpireOnTextChanged;
                    EtCvv.TextChanged -= EtCvvOnTextChanged;
                    EtName.TextChanged -= EtNameOnTextChanged;
                    BtnApply.Click -= BtnApplyOnClick;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Events

        private void EtCardNumberOnTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (e.Text.ToString().Trim().Length == 0)
                {
                    CardNumber.Text = "**** **** **** ****";
                }
                else
                {
                    string number = InsertPeriodically(e.Text.ToString().Trim(), " ", 4);
                    CardNumber.Text = number;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void EtExpireOnTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (e.Text.ToString().Trim().Length == 0)
                {
                    CardExpire.Text = "MM/YY";
                }
                else
                {
                    string exp = InsertPeriodically(e.Text.ToString().Trim(), "/", 2);
                    CardExpire.Text = exp;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void EtCvvOnTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                CardCvv.Text = e.Text.ToString().Trim().Length == 0 ? "***" : e.Text.ToString().Trim();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void EtNameOnTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                CardName.Text = e.Text.ToString().Trim().Length == 0 ? GetString(Resource.String.Lbl_YourName) : e.Text.ToString().Trim();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Stripe
        private void BtnApplyOnClick(object sender, EventArgs e)
        {
            try
            {
                //Show a progress
                AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

                Integer month = null;
                Integer year = null;

                if (!string.IsNullOrEmpty(EtCardNumber.Text) || !string.IsNullOrEmpty(EtExpire.Text) ||
                    !string.IsNullOrEmpty(EtCvv.Text) || !string.IsNullOrEmpty(EtName.Text))
                {
                    var split = CardExpire.Text.Split('/');
                    if (split.Length == 2)
                    {
                        month = (Integer) Convert.ToInt32(split[0]);
                        year = (Integer) Convert.ToInt32(split[1]);
                    }

                    Card card = new Card(EtCardNumber.Text, month, year, EtCvv.Text);
                    Stripe.CreateToken(card, this);
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseVerifyDataCard), ToastLength.Long).Show();
                }
            }
            catch (Exception exception)
            {
                AndHUD.Shared.Dismiss(this);
                Console.WriteLine(exception);
            }
        }

        public string InsertPeriodically(string text, string insert, int period)
        {
            try
            {
                var parts = SplitInParts(text, period);
                string formatted = string.Join(insert, parts);
                return formatted;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return text;
            }
        }

        public static IEnumerable<string> SplitInParts(string s, int partLength)
        {
            if (s == null)
                throw new ArgumentNullException("s");
            if (partLength <= 0)
                throw new ArgumentException("Part length has to be positive.", "partLength");

            for (var i = 0; i < s.Length; i += partLength)
                yield return s.Substring(i, Math.Min(partLength, s.Length - i));
        }
         
        #endregion

        #region Permissions && Result

        //Result
        protected override async void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);
                 
                if (requestCode == LoadPaymentDataRequestCode)
                {
                    switch (resultCode)
                    {
                        case Result.Ok:
                            PaymentData paymentData = PaymentData.GetFromIntent(data);
                            // You can get some data on the user's card, such as the brand and last 4 digits
                            //CardInfo info = paymentData.CardInfo;
                            // You can also pull the user address from the PaymentData object.
                            //UserAddress address = paymentData.ShippingAddress;
                            // This is the raw string version of your Stripe token.
                            string rawToken = paymentData.PaymentMethodToken.Token;

                            // Now that you have a Stripe token object, charge that by using the id
                            Token stripeToken = Token.FromString(rawToken);
                            if (stripeToken != null)
                            {
                                // This chargeToken function is a call to your own server, which should then connect
                                // to Stripe's API to finish the charge.
                                // chargeToken(stripeToken.getId()); 
                                //var stripeBankAccount = stripeToken.BankAccount;
                                //var stripeCard = stripeToken.Card;
                                //var stripeCreated = stripeToken.Created;
                                //var stripeId = stripeToken.Id;
                                //var stripeLiveMode = stripeToken.Livemode;
                                //var stripeType = stripeToken.Type;
                                //var stripeUsed = stripeToken.Used;

                                //send api  
                                if (PayType == "Funding")
                                {
                                    (int apiStatus, var respond) = await RequestsAsync.Funding.FundingPay(Id, Price).ConfigureAwait(false);
                                    if (apiStatus == 200)
                                    {
                                        RunOnUiThread(() =>
                                        {
                                            try
                                            {
                                                Toast.MakeText(this, GetText(Resource.String.Lbl_Donated), ToastLength.Long).Show();
                                                Finish();
                                            }
                                            catch (Exception e)
                                            {
                                                Console.WriteLine(e);
                                            }
                                        });
                                    }
                                    else Methods.DisplayReportResult(this, respond);
                                }
                                else if (PayType == "membership")
                                {
                                    if (Methods.CheckConnectivity())
                                    {
                                        (int apiStatus, var respond) = await RequestsAsync.Global.SetProAsync(Id).ConfigureAwait(false);
                                        if (apiStatus == 200)
                                        {
                                            RunOnUiThread(() =>
                                            {
                                                var dataUser = ListUtils.MyProfileList.FirstOrDefault();
                                                if (dataUser != null)
                                                {
                                                    dataUser.IsPro = "1";

                                                    var sqlEntity = new SqLiteDatabase();
                                                    sqlEntity.Insert_Or_Update_To_MyProfileTable(dataUser);
                                                    sqlEntity.Dispose();
                                                }

                                                Toast.MakeText(this, GetText(Resource.String.Lbl_Done), ToastLength.Long).Show();
                                                Finish();
                                            });
                                        }
                                        else Methods.DisplayReportResult(this, respond);
                                    }
                                    else
                                    {
                                        Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long).Show();
                                    }
                                }
                                else if (PayType == "AddFunds")
                                {
                                    var tabbedWallet = TabbedWalletActivity.GetInstance();
                                    if (Methods.CheckConnectivity() && tabbedWallet != null)
                                    {
                                        (int apiStatus, var respond) = await RequestsAsync.Global.SendMoneyWalletAsync(tabbedWallet.SendMoneyFragment?.UserId, tabbedWallet.SendMoneyFragment?.TxtAmount.Text).ConfigureAwait(false);
                                        if (apiStatus == 200)
                                        {
                                            RunOnUiThread(() =>
                                            {
                                                try
                                                {
                                                    tabbedWallet.SendMoneyFragment.TxtAmount.Text = string.Empty;
                                                    tabbedWallet.SendMoneyFragment.TxtEmail.Text = string.Empty;
                                                   
                                                    Toast.MakeText(this, GetText(Resource.String.Lbl_Done), ToastLength.Long).Show();
                                                }
                                                catch (Exception e)
                                                {
                                                    Console.WriteLine(e);
                                                }
                                            });
                                        }
                                        else Methods.DisplayReportResult(this, respond);
                                    }
                                    else
                                    {
                                        Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long).Show();
                                    }
                                }
                                else if (PayType == "SendMoney")
                                {
                                    var tabbedWallet = TabbedWalletActivity.GetInstance();
                                    if (Methods.CheckConnectivity() && tabbedWallet != null)
                                    {
                                        (int apiStatus, var respond) = await RequestsAsync.Global.SendMoneyWalletAsync(tabbedWallet.SendMoneyFragment?.UserId, tabbedWallet.SendMoneyFragment?.TxtAmount.Text).ConfigureAwait(false);
                                        if (apiStatus == 200)
                                        {
                                            RunOnUiThread(() =>
                                            {
                                                try
                                                {
                                                    tabbedWallet.SendMoneyFragment.TxtAmount.Text = string.Empty;
                                                    tabbedWallet.SendMoneyFragment.TxtEmail.Text = string.Empty;

                                                    Toast.MakeText(this, GetText(Resource.String.Lbl_Done), ToastLength.Long).Show();
                                                }
                                                catch (Exception e)
                                                {
                                                    Console.WriteLine(e);
                                                }
                                            });
                                        }
                                        else Methods.DisplayReportResult(this, respond);
                                    }
                                    else
                                    {
                                        Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long).Show();
                                    }
                                }
                            }
                            break;
                        case Result.Canceled:
                            Toast.MakeText(this, GetText(Resource.String.Lbl_Canceled), ToastLength.Long).Show();
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Stripe

        private void InitWalletStripe()
        {
            try
            {
                var stripePublishableKey = ListUtils.SettingsSiteList?.StripeId ?? "";
                if (!string.IsNullOrEmpty(stripePublishableKey))
                {
                    PaymentConfiguration.Init(stripePublishableKey);
                    Stripe = new Stripe(this, stripePublishableKey);

                    MPaymentsClient = WalletClass.GetPaymentsClient(this, new WalletClass.WalletOptions.Builder()
                        .SetEnvironment(WalletConstants.EnvironmentTest)
                        .SetTheme(WalletConstants.ThemeLight)
                        .Build());

                    IsReadyToPay();
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_ErrorConnectionSystemStripe), ToastLength.Long).Show();
                } 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void IsReadyToPay()
        {
            try
            {
                IsReadyToPayRequest request = IsReadyToPayRequest.NewBuilder()
                    .AddAllowedPaymentMethod(WalletConstants.PaymentMethodCard)
                    .AddAllowedPaymentMethod(WalletConstants.PaymentMethodTokenizedCard)
                    .Build();
                Task task = MPaymentsClient.IsReadyToPay(request);

                task.AddOnCompleteListener(this, this);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private PaymentDataRequest CreatePaymentDataRequest()
        {
            try
            {
                var list = new List<Integer>
                {
                    (Integer) WalletConstants.CardNetworkAmex,
                    (Integer) WalletConstants.CardNetworkDiscover,
                    (Integer) WalletConstants.CardNetworkVisa,
                    (Integer) WalletConstants.CardNetworkMastercard
                };

                var currencyCode = ListUtils.SettingsSiteList?.StripeCurrency ?? "USD";

                PaymentDataRequest.Builder request = PaymentDataRequest.NewBuilder()
                    .SetTransactionInfo(TransactionInfo.NewBuilder()
                        .SetTotalPriceStatus(WalletConstants.TotalPriceStatusFinal)
                        .SetTotalPrice(Price)
                        .SetCurrencyCode(currencyCode)
                        .Build())
                    .AddAllowedPaymentMethod(WalletConstants.PaymentMethodCard)
                    .AddAllowedPaymentMethod(WalletConstants.PaymentMethodTokenizedCard)
                    .SetCardRequirements(CardRequirements.NewBuilder()
                        .AddAllowedCardNetworks(list)
                        .Build());

                request.SetPaymentMethodTokenizationParameters(CreateTokenizationParameters());
                return request.Build();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        private PaymentMethodTokenizationParameters CreateTokenizationParameters()
        {
            try
            {
                var version = /*ListUtils.SettingsSiteList?.ScriptVersion ??*/ "2019-02-19";

                return PaymentMethodTokenizationParameters.NewBuilder()
                    .SetPaymentMethodTokenizationType(WalletConstants.PaymentMethodTokenizationTypePaymentGateway)
                    .AddParameter("gateway", "stripe")
                    .AddParameter("stripe:publishableKey", PaymentConfiguration.Instance.PublishableKey)
                    .AddParameter("stripe:version", version)
                    .Build();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return null;
            }
        }

        public void ShowFragmentStripe()
        {
            try
            {
                PaymentDataRequest request = CreatePaymentDataRequest();
                if (request != null)
                {
                    AutoResolveHelper.ResolveTask(MPaymentsClient.LoadPaymentData(request), this,
                        LoadPaymentDataRequestCode);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void OnComplete(Task task)
        {
            try
            {
                RunOnUiThread(() =>
                {
                    bool result = task.IsSuccessful;
                    if (result)
                    {
                        //Toast.MakeText(this, "Ready", ToastLength.Long).Show();
                        BtnApply.Enabled = true;
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_ErrorConnectionSystemStripe), ToastLength.Long)
                            .Show();
                        //hide Google as payment option
                        BtnApply.Enabled = false;
                    }
                });
            }
            catch (ApiException exception)
            {
                Toast.MakeText(this, "Exception" + exception.Message, ToastLength.Long).Show();
            }
        }

        public void OnError(Java.Lang.Exception error)
        {
            try
            {
                AndHUD.Shared.Dismiss(this);
                Toast.MakeText(this, error.Message, ToastLength.Long).Show();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public async void OnSuccess(Token token)
        {
            try
            {
                // Send token to your own web service
                //var stripeBankAccount = token.BankAccount;
                //var stripeCard = token.Card;
                //var stripeCreated = token.Created;
                //var stripeId = token.Id;
                //var stripeLiveMode = token.Livemode;
                //var stripeType = token.Type;
                //var stripeUsed = token.Used;

                //send api  
                if (PayType == "Funding")
                {
                    (int apiStatus, var respond) = await RequestsAsync.Funding.FundingPay(Id, Price).ConfigureAwait(false);
                    if (apiStatus == 200)
                    {
                        RunOnUiThread(() =>
                        {
                            try
                            {
                                Toast.MakeText(this, GetText(Resource.String.Lbl_Donated), ToastLength.Long).Show();
                                Finish();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }
                        });
                    }
                    else Methods.DisplayReportResult(this, respond);
                }
                else if (PayType == "membership")
                {
                    if (Methods.CheckConnectivity())
                    {
                        (int apiStatus, var respond) = await RequestsAsync.Global.SetProAsync(Id).ConfigureAwait(false);
                        if (apiStatus == 200)
                        {
                            RunOnUiThread(() =>
                            {
                                var dataUser = ListUtils.MyProfileList.FirstOrDefault();
                                if (dataUser != null)
                                {
                                    dataUser.IsPro = "1";

                                    var sqlEntity = new SqLiteDatabase();
                                    sqlEntity.Insert_Or_Update_To_MyProfileTable(dataUser);
                                    sqlEntity.Dispose();
                                }

                                Toast.MakeText(this, GetText(Resource.String.Lbl_Done), ToastLength.Long).Show();
                                Finish();
                            });
                        }
                        else Methods.DisplayReportResult(this, respond);
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long).Show();
                    }
                }
                else if (PayType == "AddFunds")
                {
                    var tabbedWallet = TabbedWalletActivity.GetInstance();
                    if (Methods.CheckConnectivity() && tabbedWallet != null)
                    {
                        (int apiStatus, var respond) = await RequestsAsync.Global.SendMoneyWalletAsync(tabbedWallet.SendMoneyFragment?.UserId, tabbedWallet.SendMoneyFragment?.TxtAmount.Text).ConfigureAwait(false);
                        if (apiStatus == 200)
                        {
                            RunOnUiThread(() =>
                            {
                                try
                                {
                                    tabbedWallet.SendMoneyFragment.TxtAmount.Text = string.Empty;
                                    tabbedWallet.SendMoneyFragment.TxtEmail.Text = string.Empty;

                                    Toast.MakeText(this, GetText(Resource.String.Lbl_Done), ToastLength.Long).Show();
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                }
                            });
                        }
                        else Methods.DisplayReportResult(this, respond);
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long).Show();
                    }
                }
                else if (PayType == "SendMoney")
                {
                    var tabbedWallet = TabbedWalletActivity.GetInstance();
                    if (Methods.CheckConnectivity() && tabbedWallet != null)
                    {
                        (int apiStatus, var respond) = await RequestsAsync.Global.SendMoneyWalletAsync(tabbedWallet.SendMoneyFragment?.UserId, tabbedWallet.SendMoneyFragment?.TxtAmount.Text).ConfigureAwait(false);
                        if (apiStatus == 200)
                        {
                            RunOnUiThread(() =>
                            {
                                try
                                {
                                    tabbedWallet.SendMoneyFragment.TxtAmount.Text = string.Empty;
                                    tabbedWallet.SendMoneyFragment.TxtEmail.Text = string.Empty;

                                    Toast.MakeText(this, GetText(Resource.String.Lbl_Done), ToastLength.Long).Show();
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                }
                            });
                        }
                        else Methods.DisplayReportResult(this, respond);
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long).Show();
                    }
                }
                 
                AndHUD.Shared.Dismiss(this);

                Finish();

                //ShowFragmentStripe();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                AndHUD.Shared.Dismiss(this);
            }
        }

        #endregion
          
    }
}