using POSSystem.Models;

namespace POSSystem.Services
{
    public static class SessionManager
    {
        public static User? CurrentUser { get; set; }

        public static bool IsAdmin => CurrentUser?.Role == "Admin";

        public static void Logout() => CurrentUser = null;
    }
}
