package net.azurewebsites.expenses_by_klaudia.expensesapp;

import android.support.v7.widget.RecyclerView;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageView;
import android.widget.TextView;

import net.azurewebsites.expenses_by_klaudia.model.CashFlowDetail;
import net.azurewebsites.expenses_by_klaudia.model.Category;
import net.azurewebsites.expenses_by_klaudia.repository.Categories;

import java.util.List;
import java.util.Map;

public class CategoriesListItemAdapter extends RecyclerView.Adapter<CategoriesListItemAdapter.ViewHolder> {

    private List<Category> values;

    public class ViewHolder extends RecyclerView.ViewHolder {
        public TextView txtName;
        public TextView txtFactors;
        public View layout;

        public ViewHolder(View v) {
            super(v);
            layout = v;
            txtName = v.findViewById(R.id.categoryName);
            txtFactors = v.findViewById(R.id.categoryFactors);
        }
    }

    public void add(int position, Category item) {
        values.add(position, item);
        notifyItemInserted(position);
    }

    public void remove(int position) {
        values.remove(position);
        notifyItemRemoved(position);
    }

    public CategoriesListItemAdapter(List<Category> myDataset){
        values = myDataset;
    }


    @Override
    public CategoriesListItemAdapter.ViewHolder onCreateViewHolder(ViewGroup parent,
                                                                 int viewType) {
        // create a new view
        LayoutInflater inflater = LayoutInflater.from(
                parent.getContext());
        View v =
                inflater.inflate(R.layout.row_layout_categories, parent, false);
        // set the view's size, margins, paddings and layout parameters
        CategoriesListItemAdapter.ViewHolder vh = new CategoriesListItemAdapter.ViewHolder(v);
        return vh;
    }

    @Override
    public void onBindViewHolder(CategoriesListItemAdapter.ViewHolder holder, final int position) {

        final Category category = values.get(position);

        holder.txtName.setText(category.Name);

        StringBuilder sb = new StringBuilder();
        if (category.Factor != null)
            for (Map.Entry<String, Double> item : category.Factor.entrySet()) {
                sb.append(item.getKey());
                sb.append(": ");
                sb.append(item.getValue().toString());
                sb.append("\n");
            }
        holder.txtFactors.setText(sb.toString());
    }

    @Override
    public int getItemCount() {
        return values.size();
    }
}
