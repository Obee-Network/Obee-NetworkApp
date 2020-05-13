package com.robotemplates.webviewapp.utility;

import android.content.ActivityNotFoundException;
import android.content.Context;
import android.content.Intent;
import android.net.MailTo;
import android.net.Uri;

import com.robotemplates.webviewapp.R;
import com.robotemplates.webviewapp.WebViewAppApplication;

public final class IntentUtility {
	private IntentUtility() {}

	public static boolean startIntentActivity(Context context, String url) {
		if (url != null && url.startsWith("mailto:")) {
			MailTo mailTo = MailTo.parse(url);
			IntentUtility.startEmailActivity(context, mailTo.getTo(), mailTo.getSubject(), mailTo.getBody());
			return true;
		} else if (url != null && url.startsWith("tel:")) {
			IntentUtility.startCallActivity(context, url);
			return true;
		} else if (url != null && url.startsWith("sms:")) {
			IntentUtility.startSmsActivity(context, url);
			return true;
		} else if (url != null && url.startsWith("geo:")) {
			IntentUtility.startMapSearchActivity(context, url);
			return true;
		} else if (url != null && url.startsWith("fb://")) {
			IntentUtility.startWebActivity(context, url);
			return true;
		} else if (url != null && url.startsWith("twitter://")) {
			IntentUtility.startWebActivity(context, url);
			return true;
		} else if (url != null && url.startsWith("whatsapp://")) {
			IntentUtility.startWebActivity(context, url);
			return true;
		} else {
			return false;
		}
	}

	public static void startWebActivity(Context context, String url) {
		try {
			Intent intent = new Intent(Intent.ACTION_VIEW, Uri.parse(url));
			context.startActivity(intent);
		} catch (ActivityNotFoundException e) {
			// can't start activity
			e.printStackTrace();
		}
	}

	public static void startEmailActivity(Context context, String email, String subject, String text) {
		try {
			StringBuilder builder = new StringBuilder();
			builder.append("mailto:");
			builder.append(email);

			Intent intent = new Intent(Intent.ACTION_SENDTO, Uri.parse(builder.toString()));
			intent.putExtra(Intent.EXTRA_SUBJECT, subject);
			intent.putExtra(Intent.EXTRA_TEXT, text);
			context.startActivity(intent);
		} catch (ActivityNotFoundException e) {
			// can't start activity
			e.printStackTrace();
		}
	}

	public static void startCallActivity(Context context, String url) {
		try {
			Intent intent = new Intent(Intent.ACTION_DIAL, Uri.parse(url));
			context.startActivity(intent);
		} catch (ActivityNotFoundException e) {
			// can't start activity
			e.printStackTrace();
		}
	}

	public static void startSmsActivity(Context context, String url) {
		try {
			Intent intent = new Intent(Intent.ACTION_SENDTO, Uri.parse(url));
			context.startActivity(intent);
		} catch (ActivityNotFoundException e) {
			// can't start activity
			e.printStackTrace();
		}
	}

	public static void startMapSearchActivity(Context context, String url) {
		try {
			Intent intent = new Intent(Intent.ACTION_VIEW, Uri.parse(url));
			context.startActivity(intent);
		} catch (ActivityNotFoundException e) {
			// can't start activity
			e.printStackTrace();
		}
	}

	public static void startShareActivity(Context context, String subject, String text) {
		try {
			Intent intent = new Intent(Intent.ACTION_SEND);
			intent.setType("text/plain");
			intent.putExtra(Intent.EXTRA_SUBJECT, subject);
			intent.putExtra(Intent.EXTRA_TEXT, text);
			context.startActivity(intent);
		} catch (ActivityNotFoundException e) {
			// can't start activity
			e.printStackTrace();
		}
	}

	public static void startStoreActivity(Context context) {
		try {
			String uri = context.getString(R.string.app_store_uri, WebViewAppApplication.getContext().getPackageName());
			Intent intent = new Intent(Intent.ACTION_VIEW, Uri.parse(uri));
			context.startActivity(intent);
		} catch (ActivityNotFoundException e) {
			// can't start activity
			e.printStackTrace();
		}
	}
}
