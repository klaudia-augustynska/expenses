package net.azurewebsites.expenses_by_klaudia.model;

import com.google.gson.annotations.SerializedName;

public enum CURRENCY_CODE {
    @SerializedName("0")
    Default (0),
    @SerializedName("1")
    PLN (1),
    @SerializedName("2")
    EUR (2);

    private int numVal;

    CURRENCY_CODE(int numVal) {
        this.numVal = numVal;
    }

    public int getNumVal() {
        return numVal;
    }
}
