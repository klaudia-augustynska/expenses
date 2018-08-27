package net.azurewebsites.expenses_by_klaudia.model;

import com.google.gson.annotations.SerializedName;

public enum DEFAULT_CATEGORY_CODE {
    @SerializedName("0")
    Default (0),
    @SerializedName("1")
    NormalFood (1),
    @SerializedName("2")
    UnhealthyFood (2),
    @SerializedName("3")
    Alcohol (3),
    @SerializedName("4")
    Transport (4),
    @SerializedName("5")
    Hygiene (5),
    @SerializedName("6")
    Bills (6),
    @SerializedName("7")
    Other(7);

    private int numVal;

    DEFAULT_CATEGORY_CODE(int numVal) {
        this.numVal = numVal;
    }

    public int getNumVal() {
        return numVal;
    }
}
