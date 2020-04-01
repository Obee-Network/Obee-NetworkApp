package com.robotemplates.webviewapp.fragment;

import android.annotation.TargetApi;
import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.graphics.Bitmap;
import android.net.Uri;
import android.os.Build;
import android.os.Bundle;
import android.os.Handler;
import android.support.annotation.NonNull;
import android.support.v4.widget.SwipeRefreshLayout;
import android.support.v7.app.AppCompatActivity;
import android.view.LayoutInflater;
import android.view.Menu;
import android.view.MenuInflater;
import android.view.MenuItem;
import android.view.View;
import android.view.ViewGroup;
import android.view.WindowManager;
import android.webkit.CookieSyncManager;
import android.webkit.GeolocationPermissions;
import android.webkit.ValueCallback;
import android.webkit.WebChromeClient;
import android.webkit.WebResourceError;
import android.webkit.WebResourceRequest;
import android.webkit.WebSettings;
import android.webkit.WebView;
import android.webkit.WebViewClient;
import android.widget.Toast;

import com.google.android.gms.ads.AdSize;
import com.robotemplates.webviewapp.R;
import com.robotemplates.webviewapp.WebViewAppConfig;
import com.robotemplates.webviewapp.ads.AdMobUtility;
import com.robotemplates.webviewapp.listener.DrawerStateListener;
import com.robotemplates.webviewapp.listener.LoadUrlListener;
import com.robotemplates.webviewapp.listener.WebViewOnKeyListener;
import com.robotemplates.webviewapp.listener.WebViewOnTouchListener;
import com.robotemplates.webviewapp.utility.DownloadFileUtility;
import com.robotemplates.webviewapp.utility.IntentUtility;
import com.robotemplates.webviewapp.utility.PermissionUtility;
import com.robotemplates.webviewapp.view.PullToRefreshMode;

import org.alfonz.utility.ContentUtility;
import org.alfonz.utility.DownloadUtility;
import org.alfonz.utility.Logcat;
import org.alfonz.utility.NetworkUtility;
import org.alfonz.view.StatefulLayout;

import java.io.File;

import im.delight.android.webview.AdvancedWebView;
import name.cpr.VideoEnabledWebChromeClient;
import name.cpr.VideoEnabledWebView;

public class MainFragment extends TaskFragment implements SwipeRefreshLayout.OnRefreshListener, AdvancedWebView.Listener {
	private static final String ARGUMENT_URL = "url";
	private static final String ARGUMENT_SHARE = "share";
	private static final int REQUEST_FILE_PICKER = 1;

	private boolean mProgress = false;
	private View mRootView;
	private StatefulLayout mStatefulLayout;
	private AdvancedWebView mWebView;
	private String mUrl = "about:blank";
	private String mShare;
	private boolean mLocal = false;
	private ValueCallback<Uri> mFilePathCallback4;
	private ValueCallback<Uri[]> mFilePathCallback5;
	private int mStoredActivityRequestCode;
	private int mStoredActivityResultCode;
	private Intent mStoredActivityResultIntent;

	public static MainFragment newInstance(String url, String share) {
		MainFragment fragment = new MainFragment();

		// arguments
		Bundle arguments = new Bundle();
		arguments.putString(ARGUMENT_URL, url);
		arguments.putString(ARGUMENT_SHARE, share);
		fragment.setArguments(arguments);

		return fragment;
	}

	@Override
	public void onAttach(Context context) {
		super.onAttach(context);
	}

	@Override
	public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setHasOptionsMenu(true);
		setRetainInstance(true);

		// handle fragment arguments
		Bundle arguments = getArguments();
		if (arguments != null) {
			handleArguments(arguments);
		}
	}

	@Override
	public View onCreateView(@NonNull LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
		int layout = WebViewAppConfig.PULL_TO_REFRESH == PullToRefreshMode.DISABLED ? R.layout.fragment_main : R.layout.fragment_main_swipeable;
		mRootView = inflater.inflate(layout, container, false);
		mWebView = mRootView.findViewById(R.id.main_webview);
		return mRootView;
	}

	@Override
	public void onActivityCreated(Bundle savedInstanceState) {
		super.onActivityCreated(savedInstanceState);

		// restore webview state
		if (savedInstanceState != null) {
			mWebView.restoreState(savedInstanceState);
		}

		// setup webview
		setupView();

		// pull to refresh
		setupSwipeRefreshLayout();

		// setup stateful layout
		setupStatefulLayout(savedInstanceState);

		// load data
		if (mStatefulLayout.getState() == StatefulLayout.EMPTY) loadData();

		// progress popup
		showProgress(mProgress);

		// check permissions
		if (WebViewAppConfig.GEOLOCATION) {
			PermissionUtility.checkPermissionAccessLocation(this);
		}
	}

	@Override
	public void onStart() {
		super.onStart();
	}

	@Override
	public void onResume() {
		super.onResume();
		mWebView.onResume();
	}

	@Override
	public void onPause() {
		super.onPause();
		mWebView.onPause();
	}

	@Override
	public void onStop() {
		super.onStop();
	}

	@Override
	public void onDestroyView() {
		super.onDestroyView();
		mRootView = null;
	}

	@Override
	public void onDestroy() {
		super.onDestroy();
		mWebView.onDestroy();
	}

	@Override
	public void onDetach() {
		super.onDetach();
	}

	@Override
	public void onActivityResult(int requestCode, int resultCode, Intent intent) {
		super.onActivityResult(requestCode, resultCode, intent);
		if (PermissionUtility.checkPermissionReadExternalStorageAndCamera(this)) {
			// permitted
			mWebView.onActivityResult(requestCode, resultCode, intent);
		} else {
			// not permitted
			mStoredActivityRequestCode = requestCode;
			mStoredActivityResultCode = resultCode;
			mStoredActivityResultIntent = intent;
		}
		//handleFilePickerActivityResult(requestCode, resultCode, intent); // not used, used advanced webview instead
	}

	@Override
	public void onSaveInstanceState(Bundle outState) {
		// save current instance state
		super.onSaveInstanceState(outState);
		setUserVisibleHint(true);

		// stateful layout state
		if (mStatefulLayout != null) mStatefulLayout.saveInstanceState(outState);

		// save webview state
		mWebView.saveState(outState);
	}

	@Override
	public void onCreateOptionsMenu(Menu menu, MenuInflater inflater) {
		// action bar menu
		super.onCreateOptionsMenu(menu, inflater);
		inflater.inflate(R.menu.fragment_main, menu);

		// show or hide share button
		MenuItem share = menu.findItem(R.id.menu_main_share);
		share.setVisible(mShare != null && !mShare.trim().equals(""));
	}

	@Override
	public boolean onOptionsItemSelected(MenuItem item) {
		// action bar menu behavior
		switch (item.getItemId()) {
			case R.id.menu_main_share:
				IntentUtility.startShareActivity(getContext(), getString(R.string.app_name), getShareText(mShare));
				return true;

			default:
				return super.onOptionsItemSelected(item);
		}
	}

	@Override
	public void onRequestPermissionsResult(int requestCode, @NonNull String[] permissions, @NonNull int[] grantResults) {
		switch (requestCode) {
			case PermissionUtility.REQUEST_PERMISSION_READ_EXTERNAL_STORAGE_AND_CAMERA:
			case PermissionUtility.REQUEST_PERMISSION_WRITE_EXTERNAL_STORAGE:
			case PermissionUtility.REQUEST_PERMISSION_ACCESS_LOCATION: {
				// if request is cancelled, the result arrays are empty
				if (grantResults.length > 0) {
					for (int i = 0; i < grantResults.length; i++) {
						if (grantResults[i] == PackageManager.PERMISSION_GRANTED) {
							// permission granted
							if (requestCode == PermissionUtility.REQUEST_PERMISSION_READ_EXTERNAL_STORAGE_AND_CAMERA) {
								// continue with activity result handling
								if (mStoredActivityResultIntent != null) {
									mWebView.onActivityResult(mStoredActivityRequestCode, mStoredActivityResultCode, mStoredActivityResultIntent);
									mStoredActivityRequestCode = 0;
									mStoredActivityResultCode = 0;
									mStoredActivityResultIntent = null;
								}
							}
						} else {
							// permission denied
						}
					}
				} else {
					// all permissions denied
				}
				break;
			}
		}
	}

	@Override
	public void onRefresh() {
		runTaskCallback(new Runnable() {
			@Override
			public void run() {
				refreshData();
			}
		});
	}

	@Override
	public void onPageStarted(String url, Bitmap favicon) {
		Logcat.d("");
	}

	@Override
	public void onPageFinished(String url) {
		Logcat.d("");
	}

	@Override
	public void onPageError(int errorCode, String description, String failingUrl) {
		Logcat.d("");
	}

	@Override
	public void onDownloadRequested(String url, String suggestedFilename, String mimeType, long contentLength, String contentDisposition, String userAgent) {
		Logcat.d("");
		if (PermissionUtility.checkPermissionWriteExternalStorage(MainFragment.this)) {
			Toast.makeText(getActivity(), R.string.main_downloading, Toast.LENGTH_LONG).show();
			DownloadUtility.downloadFile(getActivity(), url, DownloadFileUtility.getFileName(url));
		}
	}

	@Override
	public void onExternalPageRequest(String url) {
		Logcat.d("");
	}

	public void refreshData() {
		if (NetworkUtility.isOnline(getActivity()) || mLocal) {
			// show progress popup
			showProgress(true);

			// load web url
			String url = mWebView.getUrl();
			if (url == null || url.equals("")) url = mUrl;
			mWebView.loadUrl(url);
		} else {
			showProgress(false);
			Toast.makeText(getActivity(), R.string.global_network_offline, Toast.LENGTH_LONG).show();
		}
	}

	private void handleArguments(Bundle arguments) {
		if (arguments.containsKey(ARGUMENT_URL)) {
			mUrl = arguments.getString(ARGUMENT_URL);
			mLocal = mUrl.contains("file://");
		}
		if (arguments.containsKey(ARGUMENT_SHARE)) {
			mShare = arguments.getString(ARGUMENT_SHARE);
		}
	}

	// not used, used advanced webview instead
	private void handleFilePickerActivityResult(int requestCode, int resultCode, Intent intent) {
		if (requestCode == REQUEST_FILE_PICKER) {
			if (mFilePathCallback4 != null) {
				Uri result = intent == null || resultCode != Activity.RESULT_OK ? null : intent.getData();
				if (result != null) {
					String path = ContentUtility.getPath(getActivity(), result);
					Uri uri = Uri.fromFile(new File(path));
					mFilePathCallback4.onReceiveValue(uri);
				} else {
					mFilePathCallback4.onReceiveValue(null);
				}
			}

			if (mFilePathCallback5 != null) {
				Uri result = intent == null || resultCode != Activity.RESULT_OK ? null : intent.getData();
				if (result != null) {
					String path = ContentUtility.getPath(getActivity(), result);
					Uri uri = Uri.fromFile(new File(path));
					mFilePathCallback5.onReceiveValue(new Uri[]{uri});
				} else {
					mFilePathCallback5.onReceiveValue(null);
				}
			}

			mFilePathCallback4 = null;
			mFilePathCallback5 = null;
		}
	}

	private void loadData() {
		if (NetworkUtility.isOnline(getActivity()) || mLocal) {
			// show progress
			if (WebViewAppConfig.PROGRESS_PLACEHOLDER) {
				mStatefulLayout.showProgress();
			} else {
				mStatefulLayout.showContent();
			}

			// load web url
			mWebView.loadUrl(mUrl);
		} else {
			mStatefulLayout.showOffline();
		}
	}

	private void showProgress(boolean visible) {
		// show pull to refresh progress bar
		SwipeRefreshLayout contentSwipeRefreshLayout = mRootView.findViewById(R.id.container_content_swipeable);
		SwipeRefreshLayout offlineSwipeRefreshLayout = mRootView.findViewById(R.id.container_offline_swipeable);
		SwipeRefreshLayout emptySwipeRefreshLayout = mRootView.findViewById(R.id.container_empty_swipeable);

		if (contentSwipeRefreshLayout != null) contentSwipeRefreshLayout.setRefreshing(visible);
		if (offlineSwipeRefreshLayout != null) offlineSwipeRefreshLayout.setRefreshing(visible);
		if (emptySwipeRefreshLayout != null) emptySwipeRefreshLayout.setRefreshing(visible);

		mProgress = visible;
	}

	private void showContent(final long delay) {
		final Handler timerHandler = new Handler();
		final Runnable timerRunnable = new Runnable() {
			@Override
			public void run() {
				runTaskCallback(new Runnable() {
					public void run() {
						if (getActivity() != null && mRootView != null) {
							Logcat.d("timer");
							mStatefulLayout.showContent();
						}
					}
				});
			}
		};
		timerHandler.postDelayed(timerRunnable, delay);
	}

	private void setupView() {
		// webview settings
		mWebView.getSettings().setJavaScriptEnabled(true);
		mWebView.getSettings().setAppCacheEnabled(true);
		mWebView.getSettings().setAppCachePath(getActivity().getCacheDir().getAbsolutePath());
		mWebView.getSettings().setCacheMode(WebSettings.LOAD_DEFAULT);
		mWebView.getSettings().setDomStorageEnabled(true);
		mWebView.getSettings().setDatabaseEnabled(true);
		mWebView.getSettings().setGeolocationEnabled(true);
		mWebView.getSettings().setSupportZoom(true);
		mWebView.getSettings().setBuiltInZoomControls(false);

		// user agent
		if (WebViewAppConfig.WEBVIEW_USER_AGENT != null && !WebViewAppConfig.WEBVIEW_USER_AGENT.equals("")) {
			mWebView.getSettings().setUserAgentString(WebViewAppConfig.WEBVIEW_USER_AGENT);
		}

		// advanced webview settings
		mWebView.setListener(getActivity(), this);
		mWebView.setGeolocationEnabled(true);

		// webview style
		mWebView.setScrollBarStyle(WebView.SCROLLBARS_OUTSIDE_OVERLAY); // fixes scrollbar on Froyo

		// webview hardware acceleration
		if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.KITKAT) {
			mWebView.setLayerType(View.LAYER_TYPE_HARDWARE, null);
		} else {
			mWebView.setLayerType(View.LAYER_TYPE_SOFTWARE, null);
		}

		// webview chrome client
		View nonVideoLayout = getActivity().findViewById(R.id.main_non_video_layout);
		ViewGroup videoLayout = getActivity().findViewById(R.id.main_video_layout);
		View progressView = getActivity().getLayoutInflater().inflate(R.layout.placeholder_progress, null);
		VideoEnabledWebChromeClient webChromeClient = new VideoEnabledWebChromeClient(nonVideoLayout, videoLayout, progressView, (VideoEnabledWebView) mWebView);
		webChromeClient.setOnToggledFullscreen(new MyToggledFullscreenCallback());
		mWebView.setWebChromeClient(webChromeClient);
		//mWebView.setWebChromeClient(new MyWebChromeClient()); // not used, used advanced webview instead

		// webview client
		mWebView.setWebViewClient(new MyWebViewClient());

		// webview key listener
		mWebView.setOnKeyListener(new WebViewOnKeyListener((DrawerStateListener) getActivity()));

		// webview touch listener
		mWebView.requestFocus(View.FOCUS_DOWN); // http://android24hours.blogspot.cz/2011/12/android-soft-keyboard-not-showing-on.html
		mWebView.setOnTouchListener(new WebViewOnTouchListener());

		// webview scroll listener
		//((RoboWebView) mWebView).setOnScrollListener(new WebViewOnScrollListener()); // not used

		// admob
		setupBannerView();
	}

	private void setupBannerView() {
		if (WebViewAppConfig.ADMOB_UNIT_ID_BANNER != null && !WebViewAppConfig.ADMOB_UNIT_ID_BANNER.equals("") && NetworkUtility.isOnline(getActivity())) {
			ViewGroup contentLayout = mRootView.findViewById(R.id.container_content);
			AdMobUtility.createAdView(getActivity(), WebViewAppConfig.ADMOB_UNIT_ID_BANNER, AdSize.BANNER, contentLayout);
		}
	}

	private void controlBack() {
		if (mWebView.canGoBack()) mWebView.goBack();
	}

	private void controlForward() {
		if (mWebView.canGoForward()) mWebView.goForward();
	}

	private void controlStop() {
		mWebView.stopLoading();
	}

	private void controlReload() {
		mWebView.reload();
	}

	private void setupStatefulLayout(Bundle savedInstanceState) {
		// reference
		mStatefulLayout = (StatefulLayout) mRootView;

		// state change listener
		mStatefulLayout.setOnStateChangeListener(new StatefulLayout.OnStateChangeListener() {
			@Override
			public void onStateChange(View view, @StatefulLayout.State int state) {
				Logcat.d(String.valueOf(state));
				// do nothing
			}
		});

		// restore state
		mStatefulLayout.restoreInstanceState(savedInstanceState);
	}

	private void setupSwipeRefreshLayout() {
		SwipeRefreshLayout contentSwipeRefreshLayout = mRootView.findViewById(R.id.container_content_swipeable);
		SwipeRefreshLayout offlineSwipeRefreshLayout = mRootView.findViewById(R.id.container_offline_swipeable);
		SwipeRefreshLayout emptySwipeRefreshLayout = mRootView.findViewById(R.id.container_empty_swipeable);

		if (WebViewAppConfig.PULL_TO_REFRESH == PullToRefreshMode.ENABLED) {
			contentSwipeRefreshLayout.setOnRefreshListener(this);
			offlineSwipeRefreshLayout.setOnRefreshListener(this);
			emptySwipeRefreshLayout.setOnRefreshListener(this);
		} else if (WebViewAppConfig.PULL_TO_REFRESH == PullToRefreshMode.PROGRESS) {
			contentSwipeRefreshLayout.setDistanceToTriggerSync(Integer.MAX_VALUE);
			offlineSwipeRefreshLayout.setDistanceToTriggerSync(Integer.MAX_VALUE);
			emptySwipeRefreshLayout.setDistanceToTriggerSync(Integer.MAX_VALUE);
		}
	}

	private String getShareText(String text) {
		if (mWebView != null) {
			if (mWebView.getTitle() != null) {
				text = text.replaceAll("\\{TITLE\\}", mWebView.getTitle());
			}
			if (mWebView.getUrl() != null) {
				text = text.replaceAll("\\{URL\\}", mWebView.getUrl());
			}
		}
		return text;
	}

	private boolean isLinkExternal(String url) {
		for (String rule : WebViewAppConfig.LINKS_OPENED_IN_EXTERNAL_BROWSER) {
			if (url.contains(rule)) return true;
		}
		return false;
	}

	private boolean isLinkInternal(String url) {
		for (String rule : WebViewAppConfig.LINKS_OPENED_IN_INTERNAL_WEBVIEW) {
			if (url.contains(rule)) return true;
		}
		return false;
	}

	// not used, used advanced webview instead
	private class MyWebChromeClient extends WebChromeClient {
		@Override
		public boolean onShowFileChooser(WebView webView, ValueCallback<Uri[]> filePathCallback, WebChromeClient.FileChooserParams fileChooserParams) {
			if (PermissionUtility.checkPermissionReadExternalStorageAndCamera(MainFragment.this)) {
				mFilePathCallback5 = filePathCallback;
				Intent intent = new Intent(Intent.ACTION_GET_CONTENT);
				intent.addCategory(Intent.CATEGORY_OPENABLE);
				intent.setType("*/*");
				startActivityForResult(Intent.createChooser(intent, "File Chooser"), REQUEST_FILE_PICKER);
				return true;
			}
			return false;
		}

		@Override
		public void onGeolocationPermissionsShowPrompt(String origin, GeolocationPermissions.Callback callback) {
			callback.invoke(origin, true, false);
		}

		public void openFileChooser(ValueCallback<Uri> filePathCallback) {
			if (PermissionUtility.checkPermissionReadExternalStorageAndCamera(MainFragment.this)) {
				mFilePathCallback4 = filePathCallback;
				Intent intent = new Intent(Intent.ACTION_GET_CONTENT);
				intent.addCategory(Intent.CATEGORY_OPENABLE);
				intent.setType("*/*");
				startActivityForResult(Intent.createChooser(intent, "File Chooser"), REQUEST_FILE_PICKER);
			}
		}

		public void openFileChooser(ValueCallback<Uri> filePathCallback, String acceptType) {
			if (PermissionUtility.checkPermissionReadExternalStorageAndCamera(MainFragment.this)) {
				mFilePathCallback4 = filePathCallback;
				Intent intent = new Intent(Intent.ACTION_GET_CONTENT);
				intent.addCategory(Intent.CATEGORY_OPENABLE);
				intent.setType("*/*");
				startActivityForResult(Intent.createChooser(intent, "File Chooser"), REQUEST_FILE_PICKER);
			}
		}

		public void openFileChooser(ValueCallback<Uri> filePathCallback, String acceptType, String capture) {
			if (PermissionUtility.checkPermissionReadExternalStorageAndCamera(MainFragment.this)) {
				mFilePathCallback4 = filePathCallback;
				Intent intent = new Intent(Intent.ACTION_GET_CONTENT);
				intent.addCategory(Intent.CATEGORY_OPENABLE);
				intent.setType("*/*");
				startActivityForResult(Intent.createChooser(intent, "File Chooser"), REQUEST_FILE_PICKER);
			}
		}
	}

	private class MyToggledFullscreenCallback implements VideoEnabledWebChromeClient.ToggledFullscreenCallback {
		@Override
		public void toggledFullscreen(boolean fullscreen) {
			if (fullscreen) {
				WindowManager.LayoutParams attrs = getActivity().getWindow().getAttributes();
				attrs.flags |= WindowManager.LayoutParams.FLAG_FULLSCREEN;
				attrs.flags |= WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON;
				getActivity().getWindow().setAttributes(attrs);
				getActivity().getWindow().getDecorView().setSystemUiVisibility(View.SYSTEM_UI_FLAG_LOW_PROFILE);
			} else {
				WindowManager.LayoutParams attrs = getActivity().getWindow().getAttributes();
				attrs.flags &= ~WindowManager.LayoutParams.FLAG_FULLSCREEN;
				attrs.flags &= ~WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON;
				getActivity().getWindow().setAttributes(attrs);
				getActivity().getWindow().getDecorView().setSystemUiVisibility(View.SYSTEM_UI_FLAG_VISIBLE);
			}
		}
	}

	private class MyWebViewClient extends WebViewClient {
		private boolean mSuccess = true;

		@Override
		public void onPageFinished(final WebView view, final String url) {
			runTaskCallback(new Runnable() {
				public void run() {
					if (getActivity() != null && mSuccess) {
						showContent(500); // hide progress bar with delay to show webview content smoothly
						showProgress(false);
						if (WebViewAppConfig.ACTION_BAR_HTML_TITLE) {
							((AppCompatActivity) getActivity()).getSupportActionBar().setTitle(view.getTitle());
						}
						CookieSyncManager.getInstance().sync(); // save cookies
					}
					mSuccess = true;
				}
			});
		}

		@SuppressWarnings("deprecation")
		@Override
		public void onReceivedError(final WebView view, final int errorCode, final String description, final String failingUrl) {
			runTaskCallback(new Runnable() {
				public void run() {
					if (getActivity() != null) {
						mSuccess = false;
						mStatefulLayout.showEmpty();
						showProgress(false);
					}
				}
			});
		}

		@TargetApi(Build.VERSION_CODES.M)
		@Override
		public void onReceivedError(WebView view, WebResourceRequest request, WebResourceError error) {
			// forward to deprecated method
			onReceivedError(view, error.getErrorCode(), error.getDescription().toString(), request.getUrl().toString());
		}

		@Override
		public boolean shouldOverrideUrlLoading(WebView view, String url) {
			if (DownloadFileUtility.isDownloadableFile(url)) {
				if (PermissionUtility.checkPermissionWriteExternalStorage(MainFragment.this)) {
					Toast.makeText(getActivity(), R.string.main_downloading, Toast.LENGTH_LONG).show();
					DownloadUtility.downloadFile(getActivity(), url, DownloadFileUtility.getFileName(url));
					return true;
				}
				return true;
			} else if (url != null && (url.startsWith("http://") || url.startsWith("https://"))) {
				// load url listener
				((LoadUrlListener) getActivity()).onLoadUrl(url);

				// determine for opening the link externally or internally
				boolean external = isLinkExternal(url);
				boolean internal = isLinkInternal(url);
				if (!external && !internal) {
					external = WebViewAppConfig.OPEN_LINKS_IN_EXTERNAL_BROWSER;
				}

				// open the link
				if (external) {
					IntentUtility.startWebActivity(getContext(), url);
					return true;
				} else {
					showProgress(true);
					return false;
				}
			} else if (url != null && url.startsWith("file://")) {
				// load url listener
				((LoadUrlListener) getActivity()).onLoadUrl(url);
				return false;
			} else {
				return IntentUtility.startIntentActivity(getContext(), url);
			}
		}
	}
}
