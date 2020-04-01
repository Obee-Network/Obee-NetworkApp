package com.robotemplates.webviewapp.fragment;

import android.os.AsyncTask;
import android.os.Bundle;
import android.support.v4.app.Fragment;

import java.util.LinkedList;
import java.util.List;

public class TaskFragment extends Fragment {
	private final Object mLock = new Object();
	private Boolean mReady = false;
	private List<Runnable> mPendingCallbacks = new LinkedList<>();

	@Override
	public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
	}

	@Override
	public void onActivityCreated(Bundle savedInstanceState) {
		super.onActivityCreated(savedInstanceState);
		synchronized (mLock) {
			mReady = true;
			int pendingCallbacks = mPendingCallbacks.size();
			while (pendingCallbacks-- > 0) {
				Runnable runnable = mPendingCallbacks.remove(0);
				runNow(runnable);
			}
		}
	}

	@Override
	public void onDetach() {
		super.onDetach();
		synchronized (mLock) {
			mReady = false;
		}
	}

	protected void runTaskCallback(Runnable runnable) {
		if (mReady) runNow(runnable);
		else addPending(runnable);
	}

	protected void executeTask(AsyncTask<Void, ?, ?> task) {
		// use AsyncTask.THREAD_POOL_EXECUTOR or AsyncTask.SERIAL_EXECUTOR
		task.executeOnExecutor(AsyncTask.THREAD_POOL_EXECUTOR);
	}

	private void runNow(Runnable runnable) {
		//Logcat.d("TaskFragment.runNow(): " + runnable.getClass().getEnclosingMethod());
		getActivity().runOnUiThread(runnable);
	}

	private void addPending(Runnable runnable) {
		synchronized (mLock) {
			//Logcat.d("TaskFragment.addPending(): " + runnable.getClass().getEnclosingMethod());
			mPendingCallbacks.add(runnable);
		}
	}
}
