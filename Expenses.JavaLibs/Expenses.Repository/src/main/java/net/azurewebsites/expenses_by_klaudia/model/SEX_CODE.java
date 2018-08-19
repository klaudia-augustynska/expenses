package net.azurewebsites.expenses_by_klaudia.model;

public enum SEX_CODE {
    Default (0),
    Male (1),
    Female (2);

    private int numVal;

    SEX_CODE(int numVal) {
        this.numVal = numVal;
    }

    public int getNumVal() {
        return numVal;
    }
}