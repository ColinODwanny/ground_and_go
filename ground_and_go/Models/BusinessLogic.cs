namespace ground_and_go.Models;

public class BusinessLogic
{
    // make this public so MauiProgram can set it
    public Database Database { get; set; } = null!; 
    public bool IsLoggedIn { get; set; } = false;

    public BusinessLogic()
    {

    }

    /// <summary>
    /// Takes the given username and password and relays them to the database layer to log in
    /// </summary>
    /// <param name="username">The email to log in with</param>
    /// <param name="password">The password to log in with</param>
    /// <returns>Null if successful, an error string otherwise</returns>
    public async Task<String?> LogIn(String username, String password)
    {
        if (username == "")
            return "Please enter your username";
        if (password == "")
            return "Please enter your password";

        String? result = await Database.LogIn(username, password);
        if (result == null)
        {
            IsLoggedIn = true;
        }
        return result;
    }
    
    /// <summary>
    /// Takes the given username and passwords and relays them to the database layer to sign up
    /// </summary>
    /// <param name="username">The email to sign up with</param>
    /// <param name="password">The password to sign up with</param>
    /// <param name="repeatPassword">The password to confirm the given password is correct and intentional</param>
    /// <returns>Null if successful, an error message otherwise</returns>
    public async Task<String?> SignUp(String username, String password, String repeatPassword)
    {
        if (username == "")
            return "Please enter your email";
        if (password == "")
            return "Please enter a password";
        if (repeatPassword == "")
            return "Please re-enter your password";
        if (password.Length < 6)
            return "Your password must be at least 6 characters long";
        if (password != repeatPassword)
            return "The passwords do not match";

        return await Database.SignUp(username, password);
    }

    /// <summary>
    /// Logs the current user out of the app
    /// </summary>
    /// <returns>A task</returns>
    public async Task LogOut()
    {
        await Database.LogOut();
        IsLoggedIn = false;
    }

    /// <summary>
    /// Deletes the user account and data, then logs out.
    /// </summary>
    /// <returns>Null if successful, error message otherwise</returns>
    public async Task<String?> DeleteAccount()
    {
        // 1. Call the database layer to handle the data wipe
        // Note: You will need to add this method to your Database.cs next!
        String? result = await Database.DeleteAccount();

        // 2. Regardless of server success, log out locally to clear state
        IsLoggedIn = false;
        
        return result;
    }
}