package net.azurewebsites.expenses_by_klaudia.model;

import java.util.HashMap;
import java.util.List;

public class GetCashSummaryResponseDto {
    public List<Money> HouseholdMoney;
    public List<Money> HouseholdExpenses;
    public List<Wallet> UserWallets;
    public List<Money> UserExpenses;
    public HashMap<String, List<Money>> UserCharges;
}
