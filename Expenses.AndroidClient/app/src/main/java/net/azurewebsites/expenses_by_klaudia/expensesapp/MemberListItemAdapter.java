package net.azurewebsites.expenses_by_klaudia.expensesapp;

import android.support.v7.widget.RecyclerView;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;

import net.azurewebsites.expenses_by_klaudia.model.MemberDto;
import net.azurewebsites.expenses_by_klaudia.model.Money;

import java.util.List;

public class MemberListItemAdapter extends RecyclerView.Adapter<MemberListItemAdapter.ViewHolder> {

    private List<MemberDto> values;

    public class ViewHolder extends RecyclerView.ViewHolder {
        public TextView txtName;
        public TextView txtSummary;
        public View layout;

        public ViewHolder(View v) {
            super(v);
            layout = v;
            txtName = v.findViewById(R.id.memberName);
            txtSummary = v.findViewById(R.id.memberSummary);
        }
    }

    public void add(int position, MemberDto item) {
        values.add(position, item);
        notifyItemInserted(position);
    }

    public void remove(int position) {
        values.remove(position);
        notifyItemRemoved(position);
    }

    public MemberListItemAdapter(List<MemberDto> myDataset) {
        values = myDataset;
    }

    @Override
    public MemberListItemAdapter.ViewHolder onCreateViewHolder(ViewGroup parent,
                                                               int viewType) {
        // create a new view
        LayoutInflater inflater = LayoutInflater.from(
                parent.getContext());
        View v =
                inflater.inflate(R.layout.row_layout_member, parent, false);
        // set the view's size, margins, paddings and layout parameters
        MemberListItemAdapter.ViewHolder vh = new MemberListItemAdapter.ViewHolder(v);
        return vh;
    }


    @Override
    public void onBindViewHolder(MemberListItemAdapter.ViewHolder holder, final int position) {
        final MemberDto detail = values.get(position);
        holder.txtName.setText(detail.Name);
        holder.txtSummary.setText(formatMoney(detail.WalletSummary));
    }

    private String formatMoney(List<Money> list) {
        StringBuilder sb = new StringBuilder();
        if (list != null) {
            if (list.size() == 0)
                sb.append("--");
            else {
                int i = 0;
                for (Money item : list) {
                    if (i++ > 0)
                        sb.append(", ");
                    sb.append(item.Amount);
                    sb.append(' ');
                    sb.append(item.Currency.toString());
                }
            }
        }
        return sb.toString();
    }

    @Override
    public int getItemCount() {
        return values.size();
    }
}
