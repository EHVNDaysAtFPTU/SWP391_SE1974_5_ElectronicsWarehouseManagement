public class LoginReq
{
    public string UsernameOrEmail { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public bool Verify()
    {
        if (string.IsNullOrWhiteSpace(UsernameOrEmail) || string.IsNullOrWhiteSpace(Password))
            return false;
        if (UsernameOrEmail.Length > 256 || Password.Length > 256)
            return false;
        return true;
    }
}