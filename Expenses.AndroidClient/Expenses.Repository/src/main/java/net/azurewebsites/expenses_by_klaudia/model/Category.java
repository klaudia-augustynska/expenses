package net.azurewebsites.expenses_by_klaudia.model;

import java.util.HashMap;
import java.util.UUID;

public class Category {
    public UUID Guid;
    public DEFAULT_CATEGORY_CODE DefaultCategory;
    public String Name;
    public HashMap<String, Double> Factor;
}
