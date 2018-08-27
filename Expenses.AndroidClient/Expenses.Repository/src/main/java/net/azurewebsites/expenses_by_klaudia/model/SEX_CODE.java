package net.azurewebsites.expenses_by_klaudia.model;

import com.google.gson.annotations.SerializedName;

public enum SEX_CODE {
    @SerializedName("0")
    Default (0),
    @SerializedName("1")
    Male (1),
    @SerializedName("2")
    Female (2);

    private int numVal;

    SEX_CODE(int numVal) {
        this.numVal = numVal;
    }

    public int getNumVal() {
        return numVal;
    }
}
