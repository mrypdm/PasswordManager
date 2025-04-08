namespace PasswordManager.Web.Views.Account;

/// <summary>
/// Model for account view
/// </summary>
public class AccountModel(int accountId)
{
    /// <summary>
    /// Id of account
    /// </summary>
    public int AccountId { get; } = accountId;
}
