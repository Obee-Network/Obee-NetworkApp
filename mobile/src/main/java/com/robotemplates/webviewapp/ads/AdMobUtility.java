package com.robotemplates.webviewapp.ads;

import android.content.Context;
import android.view.Gravity;
import android.view.View;
import android.view.ViewGroup;
import android.widget.LinearLayout;

import com.google.ads.mediation.admob.AdMobAdapter;
import com.google.android.gms.ads.AdListener;
import com.google.android.gms.ads.AdRequest;
import com.google.android.gms.ads.AdSize;
import com.google.android.gms.ads.AdView;
import com.robotemplates.webviewapp.R;
import com.robotemplates.webviewapp.WebViewAppConfig;
import com.robotemplates.webviewapp.utility.AnimationUtility;

public final class AdMobUtility {
	private AdMobUtility() {}

	public static AdRequest createAdRequest() {
		AdRequest.Builder builder = new AdRequest.Builder()
				.addTestDevice(AdRequest.DEVICE_ID_EMULATOR)
				.addTestDevice(WebViewAppConfig.ADMOB_TEST_DEVICE_ID);

		if (WebViewAppConfig.GDPR_PRIVACY_POLICY_URL != null && !WebViewAppConfig.GDPR_PRIVACY_POLICY_URL.equals("")) {
			builder.addNetworkExtrasBundle(AdMobAdapter.class, AdMobGdprHelper.getConsentStatusBundle());
		}

		return builder.build();
	}

	public static AdListener createAdListener(final AdView adView) {
		return new AdListener() {
			@Override
			public void onAdLoaded() {
				super.onAdLoaded();
				AnimationUtility.animateLayoutHeight(adView, adView.getAdSize().getHeightInPixels(adView.getContext()));
			}

			@Override
			public void onAdFailedToLoad(int errorCode) {
				super.onAdFailedToLoad(errorCode);
				adView.setVisibility(View.GONE);
			}
		};
	}

	public static AdView createAdView(Context context, String adUnitId, AdSize adSize, ViewGroup viewGroup) {
		// layout params
		LinearLayout.LayoutParams params = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MATCH_PARENT, LinearLayout.LayoutParams.WRAP_CONTENT);
		params.gravity = Gravity.CENTER;

		// create ad view
		AdView adView = new AdView(context);
		adView.setId(R.id.adview);
		adView.setLayoutParams(params);
		adView.setVisibility(View.GONE);
		adView.setAdUnitId(adUnitId);
		adView.setAdSize(adSize);
		adView.setAdListener(createAdListener(adView));

		// add to layout
		viewGroup.removeView(viewGroup.findViewById(R.id.adview));
		viewGroup.addView(adView);

		// call ad request
		adView.loadAd(createAdRequest());

		return adView;
	}
}
