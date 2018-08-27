package net.azurewebsites.expenses_by_klaudia.expensesapp;

import android.support.v7.widget.RecyclerView;
import android.text.TextUtils;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageView;
import android.widget.TextView;

import net.azurewebsites.expenses_by_klaudia.expensesapp.validation.BindRulesToActivityHelper;
import net.azurewebsites.expenses_by_klaudia.model.CURRENCY_CODE;
import net.azurewebsites.expenses_by_klaudia.model.CashFlowDetail;
import net.azurewebsites.expenses_by_klaudia.model.Category;
import net.azurewebsites.expenses_by_klaudia.model.Wallet;

import java.util.List;
import java.util.Locale;

public class CashflowListItemAdapter extends RecyclerView.Adapter<CashflowListItemAdapter.ViewHolder> {

    private List<CashFlowDetail> values;
    List<Category> mCategories;

    public class ViewHolder extends RecyclerView.ViewHolder {
        // each data item is just a string in this case
        public TextView txtMoney;
        public TextView txtCategory;
        public ImageView imageClear;
        public View layout;

        public ViewHolder(View v) {
            super(v);
            layout = v;
            txtCategory = v.findViewById(R.id.detailCategory);
            txtMoney = v.findViewById(R.id.detailMoney);
            imageClear = v.findViewById(R.id.walletDelete);
        }
    }

    public void add(int position, CashFlowDetail item) {
        values.add(position, item);
        notifyItemInserted(position);
    }

    public void remove(int position) {
        values.remove(position);
        notifyItemRemoved(position);
    }

    public CashflowListItemAdapter(List<CashFlowDetail> myDataset, List<Category> categories) {
        values = myDataset;
        mCategories = categories;
        mCurrency = CURRENCY_CODE.Default;
    }

    @Override
    public CashflowListItemAdapter.ViewHolder onCreateViewHolder(ViewGroup parent,
                                                               int viewType) {
        // create a new view
        LayoutInflater inflater = LayoutInflater.from(
                parent.getContext());
        View v =
                inflater.inflate(R.layout.row_layout_cashflowdetail, parent, false);
        // set the view's size, margins, paddings and layout parameters
        CashflowListItemAdapter.ViewHolder vh = new CashflowListItemAdapter.ViewHolder(v);
        return vh;
    }

    @Override
    public void onBindViewHolder(CashflowListItemAdapter.ViewHolder holder, final int position) {
        // - get element from your dataset at this position
        // - replace the contents of the view with that element
        final CashFlowDetail detail = values.get(position);

        int pos = 0;
        for (pos = 0; pos < mCategories.size(); ++pos) {
            if (mCategories.get(pos).Guid.equals(detail.CategoryGuid))
                break;
        }
        String category = TextUtils.isEmpty(detail.Comment)
                ? mCategories.get(pos).Name
                : String.format("%s (%s)", mCategories.get(pos).Name, detail.Comment);
        holder.txtCategory.setText(category);
        String money = String.format(Locale.getDefault(), "%f %s", detail.Amount, mCurrency.toString());
        holder.txtMoney.setText(money);
    }

    private CURRENCY_CODE mCurrency;
    public void setCurrency(CURRENCY_CODE code) {
        if (code == null)
            mCurrency = CURRENCY_CODE.Default;
        else
            mCurrency = code;
    }

    @Override
    public int getItemCount() {
        return values.size();
    }
}
