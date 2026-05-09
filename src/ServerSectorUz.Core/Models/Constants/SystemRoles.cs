namespace ServerSectorUz.Core.Models.Constants;

public static class SystemRoles
{
    public const string Admin = "Admin";
    public const string Hr = "HR";
    public const string Accountant = "Accountant";
    public const string OfficeManager = "OfficeManager";
    public const string Installer = "Installer";
    public const string User = "User";

    public static readonly string[] All =
    {
        Admin,
        Hr,
        Accountant,
        OfficeManager,
        Installer,
        User
    };
}
