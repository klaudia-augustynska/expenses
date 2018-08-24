package net.azurewebsites.expenses_by_klaudia.expensesapp;

import java.util.List;
import java.util.Locale;

import android.support.v7.widget.RecyclerView;
import android.view.LayoutInflater;
import android.view.View;
import android.view.View.OnClickListener;
import android.view.ViewGroup;
import android.widget.ImageView;
import android.widget.TextView;

import net.azurewebsites.expenses_by_klaudia.model.Wallet;

public class WalletListItemAdapter extends RecyclerView.Adapter<WalletListItemAdapter.ViewHolder> {

    private List<Wallet> values;

    // Provide a reference to the views for each data item
    // Complex data items may need more than one view per item, and
    // you provide access to all the views for a data item in a view holder
    public class ViewHolder extends RecyclerView.ViewHolder {
        // each data item is just a string in this case
        public TextView txtName;
        public TextView txtMoney;
        public TextView txtCurrency;
        public ImageView imageClear;
        public View layout;

        public ViewHolder(View v) {
            super(v);
            layout = v;
            txtName = v.findViewById(R.id.walletName);
            txtMoney = v.findViewById(R.id.walletMoney);
            txtCurrency = v.findViewById(R.id.walletCurrency);
            imageClear = v.findViewById(R.id.walletDelete);
        }
    }

    public void add(int position, Wallet item) {
        values.add(position, item);
        notifyItemInserted(position);
    }

    public void remove(int position) {
        values.remove(position);
        notifyItemRemoved(position);
    }

    // Provide a suitable constructor (depends on the kind of dataset)
    public WalletListItemAdapter(List<Wallet> myDataset) {
        values = myDataset;
    }

    // Create new views (invoked by the layout manager)
    @Override
    public WalletListItemAdapter.ViewHolder onCreateViewHolder(ViewGroup parent,
                                                   int viewType) {
        // create a new view
        LayoutInflater inflater = LayoutInflater.from(
                parent.getContext());
        View v =
                inflater.inflate(R.layout.row_layout_wallet, parent, false);
        // set the view's size, margins, paddings and layout parameters
        ViewHolder vh = new ViewHolder(v);
        return vh;
    }

    // Replace the contents of a view (invoked by the layout manager)
    @Override
    public void onBindViewHolder(ViewHolder holder, final int position) {
        // - get element from your dataset at this position
        // - replace the contents of the view with that element
        final Wallet wallet = values.get(position);
        holder.txtName.setText(wallet.Name);
        holder.imageClear.setOnClickListener(v -> remove(position));

        holder.txtMoney.setText(Double.toString(wallet.Money.Amount));
        holder.txtCurrency.setText(wallet.Money.Currency.toString());
    }

    // Return the size of your dataset (invoked by the layout manager)
    @Override
    public int getItemCount() {
        return values.size();
    }
}
