namespace House.House.Services.Economy;

public enum PurchaseResult
{
    Success,
    ItemNotFound,
    NotEnoughCash,
    InvalidQuantity,
    NotPurchasable,
    InvalidInput
}
