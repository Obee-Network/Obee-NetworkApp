package com.robotemplates.webviewapp.ads;

import android.content.Context;

import com.google.android.gms.ads.AdListener;
import com.google.android.gms.ads.InterstitialAd;
import com.robotemplates.webviewapp.WebViewAppConfig;

public class AdMobInterstitialHelper {
	private static int sInterstitialCounter = 1;

	private InterstitialAd mInterstitialAd;

	public void setupAd(Context context) {
		if (WebViewAppConfig.ADMOB_UNIT_ID_INTERSTITIAL != null && !WebViewAppConfig.ADMOB_UNIT_ID_INTERSTITIAL.equals("")) {
			mInterstitialAd = new InterstitialAd(context);
			mInterstitialAd.setAdUnitId(WebViewAppConfig.ADMOB_UNIT_ID_INTERSTITIAL);
			mInterstitialAd.setAdListener(new AdListener() {
				@Override
				public void onAdClosed() {
					loadAd();
				}
			});
			loadAd();
		}
	}

	public void checkAd() {
		if (WebViewAppConfig.ADMOB_INTERSTITIAL_FREQUENCY > 0 && sInterstitialCounter % WebViewAppConfig.ADMOB_INTERSTITIAL_FREQUENCY == 0) {
			showAd();
		}
		sInterstitialCounter++;
	}

	private void loadAd() {
		if (mInterstitialAd != null) {
			mInterstitialAd.loadAd(AdMobUtility.createAdRequest());
		}
	}

	private void showAd() {
		if (WebViewAppConfig.ADMOB_UNIT_ID_INTERSTITIAL != null && !WebViewAppConfig.ADMOB_UNIT_ID_INTERSTITIAL.equals("")) {
			if (mInterstitialAd != null && mInterstitialAd.isLoaded()) {
				mInterstitialAd.show();
			}
		}
	}
}
