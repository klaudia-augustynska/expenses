package net.azurewebsites.expenses_by_klaudia.model;

import java.util.Date;
import java.util.List;
import java.util.UUID;

public class AddCashFlowDto {
    public Date DateTime;
    public UUID CategoryGuid;
    public Money Amount;
    public List<CashFlowDetail> Details;
    public UUID WalletGuid;
}
