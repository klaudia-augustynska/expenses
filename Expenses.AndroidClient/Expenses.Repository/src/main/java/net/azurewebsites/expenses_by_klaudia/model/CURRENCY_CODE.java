package net.azurewebsites.expenses_by_klaudia.model;

public enum CURRENCY_CODE {
    Default (0),
    PLN (1),
    EUR (2);

    private int numVal;

    CURRENCY_CODE(int numVal) {
        this.numVal = numVal;
    }

    public int getNumVal() {
        return numVal;
    }
}
