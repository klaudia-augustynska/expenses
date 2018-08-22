package net.azurewebsites.expenses_by_klaudia.expensesapp;

import android.animation.Animator;
import android.animation.AnimatorListenerAdapter;
import android.annotation.TargetApi;
import android.os.Build;
import android.view.View;

public class ProgressHelper {

    /**
     * Shows the progress UI and hides the login form.
     */
    @TargetApi(Build.VERSION_CODES.HONEYCOMB_MR2)
    public static void showProgress(final boolean show, final View containerView, final View progressBarView) {
        // On Honeycomb MR2 we have the ViewPropertyAnimator APIs, which allow
        // for very easy animations. If available, use these APIs to fade-in
        // the progress spinner.
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.HONEYCOMB_MR2) {
            int shortAnimTime = containerView.getResources().getInteger(android.R.integer.config_shortAnimTime);

            containerView.setVisibility(show ? View.GONE : View.VISIBLE);
            containerView.animate().setDuration(shortAnimTime).alpha(
                    show ? 0 : 1).setListener(new AnimatorListenerAdapter() {
                @Override
                public void onAnimationEnd(Animator animation) {
                    containerView.setVisibility(show ? View.GONE : View.VISIBLE);
                }
            });

            progressBarView.setVisibility(show ? View.VISIBLE : View.GONE);
            progressBarView.animate().setDuration(shortAnimTime).alpha(
                    show ? 1 : 0).setListener(new AnimatorListenerAdapter() {
                @Override
                public void onAnimationEnd(Animator animation) {
                    progressBarView.setVisibility(show ? View.VISIBLE : View.GONE);
                }
            });
        } else {
            // The ViewPropertyAnimator APIs are not available, so simply show
            // and hide the relevant UI components.
            progressBarView.setVisibility(show ? View.VISIBLE : View.GONE);
            containerView.setVisibility(show ? View.GONE : View.VISIBLE);
        }
    }

}
