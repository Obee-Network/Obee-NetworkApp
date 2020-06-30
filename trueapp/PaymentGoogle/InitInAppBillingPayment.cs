using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Widget;
using Plugin.CurrentActivity;
using ObeeNetwork.Helpers.Utils;
using ObeeNetworkClient;
using Xamarin.InAppBilling;

namespace ObeeNetwork.PaymentGoogle
{
    public class InitInAppBillingPayment
    {
        private readonly Activity ActivityContext;
        private string PayType, Id;
        public SaneInAppBillingHandler Handler;
        private IReadOnlyList<Product> Products;

        public InitInAppBillingPayment(Activity activity)
        {
            ActivityContext = activity;
        }

        #region In-App Billing Google
         
        public async void SetConnInAppBilling()
        {
            try
            {
                CrossCurrentActivity.Current.Activity = ActivityContext;
                Handler = new SaneInAppBillingHandler(ActivityContext, InAppBillingGoogle.ProductId);
                // Call this method when creating your activity
                await Handler.Connect();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void DisconnectInAppBilling()
        {
            try
            {
                Handler?.Disconnect();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
         
        public async void InitInAppBilling(string price, string payType, string id)
        {
            PayType = payType;
            Id = id;

            if (Methods.CheckConnectivity())
            {
                if (!Handler.ServiceConnection.Connected)
                {
                    // Call this method when creating your activity 
                    await Handler.Connect();
                }

                try
                {
                    Products = await Handler.QueryInventory(InAppBillingGoogle.ListProductSku, ItemType.Product);
                    if (Products.Count > 0)
                    {
                        // Ask the open connection's billing handler to get any purchases 
                        var purchases = Handler.ServiceConnection.BillingHandler.GetPurchases(ItemType.Product);

                        var hasPaid = purchases != null && purchases.Any();
                        if (hasPaid)
                        {
                            var chk = purchases.FirstOrDefault(a => a.ProductId == Products[0].ProductId);
                            if (chk != null)
                            {
                                bool result = Handler.ServiceConnection.BillingHandler.ConsumePurchase(chk);
                                if (result)
                                {
                                    Console.WriteLine(chk);
                                }
                            }
                        }

                        var membershipLifeTime = Products.FirstOrDefault(a => a.ProductId == "membershiplifetime");
                        var membershipMonthly = Products.FirstOrDefault(a => a.ProductId == "membershipmonthly");
                        var membershipWeekly = Products.FirstOrDefault(a => a.ProductId == "membershipweekly");
                        var membershipYearly = Products.FirstOrDefault(a => a.ProductId == "membershipyearly");

                        var donationDefault = Products.FirstOrDefault(a => a.ProductId == "donationdefulte");
                        var donation5 = Products.FirstOrDefault(a => a.ProductId == "donation5") ?? donationDefault;
                        var donation10 = Products.FirstOrDefault(a => a.ProductId == "donation10") ?? donationDefault;
                        var donation15 = Products.FirstOrDefault(a => a.ProductId == "donation15") ?? donationDefault;
                        var donation20 = Products.FirstOrDefault(a => a.ProductId == "donation20") ?? donationDefault;
                        var donation25 = Products.FirstOrDefault(a => a.ProductId == "donation25") ?? donationDefault;
                        var donation30 = Products.FirstOrDefault(a => a.ProductId == "donation30") ?? donationDefault;
                        var donation35 = Products.FirstOrDefault(a => a.ProductId == "donation35") ?? donationDefault;
                        var donation40 = Products.FirstOrDefault(a => a.ProductId == "donation40") ?? donationDefault;
                        var donation45 = Products.FirstOrDefault(a => a.ProductId == "donation45") ?? donationDefault;
                        var donation50 = Products.FirstOrDefault(a => a.ProductId == "donation50") ?? donationDefault;
                        var donation55 = Products.FirstOrDefault(a => a.ProductId == "donation55") ?? donationDefault;
                        var donation60 = Products.FirstOrDefault(a => a.ProductId == "donation60") ?? donationDefault;
                        var donation65 = Products.FirstOrDefault(a => a.ProductId == "donation65") ?? donationDefault;
                        var donation70 = Products.FirstOrDefault(a => a.ProductId == "donation70") ?? donationDefault;
                        var donation75 = Products.FirstOrDefault(a => a.ProductId == "donation75") ?? donationDefault;
                        //var donation80 = Products.FirstOrDefault(a => a.ProductId == "donation80") ?? donationDefault;
                        //var donation85 = Products.FirstOrDefault(a => a.ProductId == "donation85") ?? donationDefault;
                        //var donation90 = Products.FirstOrDefault(a => a.ProductId == "donation90") ?? donationDefault;
                        //var donation95 = Products.FirstOrDefault(a => a.ProductId == "donation95") ?? donationDefault;
                        //var donation100 = Products.FirstOrDefault(a => a.ProductId == "donation100") ?? donationDefault;

                        switch (PayType)
                        {
                            //Weekly
                            case "membership" when Id == "1": // Per Week 
                                await Handler.BuyProduct(membershipWeekly);
                                break;
                            //Monthly
                            case "membership" when Id == "2": // Per Month 
                                await Handler.BuyProduct(membershipMonthly);
                                break;
                            //Yearly
                            case "membership" when Id == "3": // Per Year 
                                await Handler.BuyProduct(membershipYearly);
                                break;
                            case "membership" when Id == "4": // life time 
                                await Handler.BuyProduct(membershipLifeTime);
                                break;
                            case "Funding" when price == "5": // Donation with Amount 5
                                await Handler.BuyProduct(donation5); 
                                break;
                            case "Funding" when price == "10":  // Donation with Amount 10
                                await Handler.BuyProduct(donation10);
                                break;
                            case "Funding" when price == "15": // Donation with Amount 15
                                await Handler.BuyProduct(donation15);
                                break;
                            case "Funding" when price == "20": // Donation with Amount 20
                                await Handler.BuyProduct(donation20);
                                break;
                            case "Funding" when price == "25": // Donation with Amount 25
                                await Handler.BuyProduct(donation25);
                                break; 
                            case "Funding" when price == "30": // Donation with Amount 30
                                await Handler.BuyProduct(donation30);
                                break;
                            case "Funding" when price == "35": // Donation with Amount 35
                                await Handler.BuyProduct(donation35);
                                break;
                            case "Funding" when price == "40": // Donation with Amount 40
                                await Handler.BuyProduct(donation40);
                                break;
                            case "Funding" when price == "45": // Donation with Amount 45
                                await Handler.BuyProduct(donation45);
                                break;
                            case "Funding" when price == "50": // Donation with Amount 50
                                await Handler.BuyProduct(donation50);
                                break;
                            case "Funding" when price == "55": // Donation with Amount 55
                                await Handler.BuyProduct(donation55);
                                break;
                            case "Funding" when price == "60": // Donation with Amount 60
                                await Handler.BuyProduct(donation60);
                                break;
                            case "Funding" when price == "65": // Donation with Amount 65
                                await Handler.BuyProduct(donation65);
                                break;
                            case "Funding" when price == "70": // Donation with Amount 70
                                await Handler.BuyProduct(donation70);
                                break;
                            case "Funding" when price == "75": // Donation with Amount 75
                                await Handler.BuyProduct(donation75);
                                break;
                            case "Funding" when price == "80": // Donation with Amount 80
                            case "Funding" when price == "85": // Donation with Amount 85
                            case "Funding" when price == "90": // Donation with Amount 90
                            case "Funding" when price == "95": // Donation with Amount 95
                            case "Funding" when price == "100": // Donation with Amount 100
                            case "Funding" : // Donation with Amount long 100
                                await Handler.BuyProduct(donationDefault);
                                break; 
                        }

                        Handler.ServiceConnection.BillingHandler.OnProductPurchased += delegate (int response, Purchase purchase, string data, string signature)
                        {
                            try
                            {
                                if (response == BillingResult.OK)
                                {
                                    //Sent APi
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }
                        };

                        // Attach to the various error handlers to report issues
                        Handler.ServiceConnection.BillingHandler.OnGetProductsError += (responseCode, ownedItems) => {
                            Console.WriteLine("Error getting products");
                            Toast.MakeText(ActivityContext, "Error getting products ", ToastLength.Long).Show();
                        };

                        Handler.ServiceConnection.BillingHandler.OnInvalidOwnedItemsBundleReturned += ownedItems => {
                            Console.WriteLine("Invalid owned items bundle returned");
                            Toast.MakeText(ActivityContext, "Invalid owned items bundle returned ", ToastLength.Long).Show();
                        };

                        Handler.ServiceConnection.BillingHandler.OnProductPurchasedError += (responseCode, sku) => {
                            Console.WriteLine("Error purchasing item {0}", sku);
                            Toast.MakeText(ActivityContext, "Error purchasing item " + sku, ToastLength.Long).Show();
                        };

                        Handler.ServiceConnection.BillingHandler.OnPurchaseConsumedError += (responseCode, token) => {
                            Console.WriteLine("Error consuming previous purchase");
                            Toast.MakeText(ActivityContext, "Error consuming previous purchase ", ToastLength.Long).Show();
                        };

                        Handler.ServiceConnection.BillingHandler.InAppBillingProcesingError += (message) => {
                            Console.WriteLine("In app billing processing error {0}", message);
                            Toast.MakeText(ActivityContext, "In app billing processing error " + message, ToastLength.Long).Show();
                        };

                        Handler.ServiceConnection.BillingHandler.OnPurchaseConsumed += delegate (string token)
                        {
                            Toast.MakeText(ActivityContext, "In app billing processing error " + token, ToastLength.Long).Show();
                            Console.WriteLine("In app billing processing error {0}", token);
                        };

                        Handler.ServiceConnection.BillingHandler.BuyProductError += delegate
                        {
                            Toast.MakeText(ActivityContext, "There is something wrong please try again later", ToastLength.Long).Show();
                        };

                        Handler.ServiceConnection.BillingHandler.QueryInventoryError += delegate { };
                    }
                }
                catch (Exception ex)
                {
                    //Something else has gone wrong, log it
                    Console.WriteLine("Issue connecting: " + ex);
                    Toast.MakeText(ActivityContext, "Issue connecting: " + ex, ToastLength.Long).Show();
                }
                finally
                {
                    Handler.Disconnect();
                }
            }
            else
            {
                Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long).Show();
            }
        }

        #endregion

    }
}