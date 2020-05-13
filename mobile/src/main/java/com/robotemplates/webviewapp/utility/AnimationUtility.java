package com.robotemplates.webviewapp.utility;

import android.animation.Animator;
import android.animation.AnimatorListenerAdapter;
import android.animation.ValueAnimator;
import android.view.View;
import android.view.animation.AccelerateDecelerateInterpolator;
import android.widget.LinearLayout;

public final class AnimationUtility {
	private AnimationUtility() {}

	public static void animateLayoutHeight(final View view, int height) {
		ValueAnimator animator = ValueAnimator.ofInt(0, height);
		animator.setDuration(500);
		animator.setInterpolator(new AccelerateDecelerateInterpolator());
		animator.addListener(new AnimatorListenerAdapter() {
			@Override
			public void onAnimationStart(Animator animator) {
				view.setVisibility(View.VISIBLE);
			}
		});
		animator.addUpdateListener(new ValueAnimator.AnimatorUpdateListener() {
			@Override
			public void onAnimationUpdate(ValueAnimator animation) {
				int value = (Integer) animation.getAnimatedValue();
				LinearLayout.LayoutParams params = (LinearLayout.LayoutParams) view.getLayoutParams();
				params.height = value;
				view.setLayoutParams(params);
			}
		});
		animator.start();
	}
}
