package com.robotemplates.webviewapp;

import com.google.android.gms.ads.MobileAds;
import com.google.firebase.analytics.FirebaseAnalytics;
import com.robotemplates.kozuza.BaseApplication;
import com.robotemplates.kozuza.Kozuza;

import org.alfonz.utility.Logcat;

public class WebViewAppApplication extends BaseApplication {
	@Override
	public void onCreate() {
		super.onCreate();

		// init logcat
		Logcat.init(WebViewAppConfig.LOGS, "WEBVIEWAPP");

		// init analytics
		FirebaseAnalytics.getInstance(this).setAnalyticsCollectionEnabled(!WebViewAppConfig.DEV_ENVIRONMENT);

		// init AdMob
		MobileAds.initialize(this, WebViewAppConfig.ADMOB_APP_ID);
	}

	@Override
	public String getPurchaseCode() {
		return WebViewAppConfig.PURCHASE_CODE;
	}

	@Override
	public String getProduct() {
		return Kozuza.PRODUCT_WEBVIEWAPP;
	}
}
