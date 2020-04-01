package com.robotemplates.webviewapp.ads;

import android.content.Context;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.preference.PreferenceManager;

import com.google.ads.consent.ConsentForm;
import com.google.ads.consent.ConsentFormListener;
import com.google.ads.consent.ConsentInfoUpdateListener;
import com.google.ads.consent.ConsentInformation;
import com.google.ads.consent.ConsentStatus;
import com.robotemplates.webviewapp.WebViewAppApplication;

import org.alfonz.utility.Logcat;

import java.net.MalformedURLException;
import java.net.URL;

public class AdMobGdprHelper {
	private static final String PREFS_KEY_CONSENT_STATUS = "consent_status";

	private Context mContext;
	private String mPublisherId;
	private String mPrivacyPolicyUrl;
	private ConsentInformation mConsentInformation;
	private ConsentForm mConsentForm;

	public AdMobGdprHelper(Context context, String publisherId, String privacyPolicyUrl) {
		mContext = context;
		mPublisherId = publisherId;
		mPrivacyPolicyUrl = privacyPolicyUrl;
		mConsentInformation = ConsentInformation.getInstance(mContext);
	}

	public static Bundle getConsentStatusBundle() {
		boolean nonPersonalizedAds = getConsentStatus();
		Logcat.d("" + nonPersonalizedAds);
		Bundle bundle = new Bundle();
		if (nonPersonalizedAds) {
			bundle.putString("npa", "1");
		}
		return bundle;
	}

	private static boolean getConsentStatus() {
		SharedPreferences sharedPreferences = PreferenceManager.getDefaultSharedPreferences(WebViewAppApplication.getContext());
		return sharedPreferences.getBoolean(PREFS_KEY_CONSENT_STATUS, true);
	}

	private static void setConsentStatus(boolean nonPersonalizedAds) {
		SharedPreferences sharedPreferences = PreferenceManager.getDefaultSharedPreferences(WebViewAppApplication.getContext());
		SharedPreferences.Editor editor = sharedPreferences.edit();
		editor.putBoolean(PREFS_KEY_CONSENT_STATUS, nonPersonalizedAds);
		editor.apply();
	}

	public void requestConsent() {
		mConsentInformation.requestConsentInfoUpdate(new String[]{mPublisherId}, new ConsentInfoUpdateListener() {
			@Override
			public void onConsentInfoUpdated(ConsentStatus consentStatus) {
				Logcat.d(consentStatus.name());

				if (shouldShowConsentForm()) {
					if (consentStatus.equals(ConsentStatus.UNKNOWN)) {
						showConsentForm();
					} else {
						updateConsentStatus(consentStatus);
					}
				}
			}

			@Override
			public void onFailedToUpdateConsentInfo(String errorDescription) {
				Logcat.d(errorDescription);
			}
		});
	}

	public void resetConsent() {
		mConsentInformation.reset();
	}

	private void showConsentForm() {
		mConsentForm = new ConsentForm.Builder(mContext, createPrivacyUrl())
				.withListener(new ConsentFormListener() {
					@Override
					public void onConsentFormLoaded() {
						Logcat.d("");
						try {
							mConsentForm.show();
						} catch (Exception e) {
							e.printStackTrace();
						}
					}

					@Override
					public void onConsentFormOpened() {
						Logcat.d("");
					}

					@Override
					public void onConsentFormClosed(ConsentStatus consentStatus, Boolean userPrefersAdFree) {
						Logcat.d(consentStatus.name());
						updateConsentStatus(consentStatus);
					}

					@Override
					public void onConsentFormError(String errorDescription) {
						Logcat.d(errorDescription);
					}
				})
				.withPersonalizedAdsOption()
				.withNonPersonalizedAdsOption()
				.build();
		mConsentForm.load();
	}

	private URL createPrivacyUrl() {
		URL privacyUrl = null;
		try {
			privacyUrl = new URL(mPrivacyPolicyUrl);
		} catch (MalformedURLException e) {
			e.printStackTrace();
		}
		return privacyUrl;
	}

	private boolean shouldShowConsentForm() {
		return mConsentInformation.isRequestLocationInEeaOrUnknown()
				&& mPublisherId != null
				&& !mPublisherId.equals("")
				&& mPrivacyPolicyUrl != null
				&& !mPrivacyPolicyUrl.equals("");
	}

	private void updateConsentStatus(ConsentStatus consentStatus) {
		boolean nonPersonalizedAds = consentStatus != ConsentStatus.PERSONALIZED;
		setConsentStatus(nonPersonalizedAds);
	}
}
