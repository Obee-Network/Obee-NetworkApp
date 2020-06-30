using System.Collections.Generic;

namespace ObeeNetwork.PaymentGoogle
{
    public static class InAppBillingGoogle 
    {
        public static readonly string ProductId = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAkECSEiZqEnB3EE/sIJOJmrpSl6Vc33EoTdXrnOqUWR4ir2dzxfV1l35kuiF7mG3/Hpu7qOU8fZ4Bu77LVihdHv+pPlidADx0Snra+6yTp6za+Tk/VK75NTp5edpHg6vTUOGPOlCapPopvnHJlJVIw3HNu9Sj4UWETadr6c7m91TYClAXWB4nYUDBAEFswUFYVRRTPuYWe4y/7MJAhHFnh3svaIlzaCX5X2vL95xjepCsWjNBqXynMwV0spnUu0Mpbc6IXKIuZpexYDF7CkHwQrREhmGIcLSysXZmAjvjGV9MYKpJCBItOCjnNnTX9rRpoGVrMRQpifbBoZYqrGXhiwIDAQAB";
        public static readonly List<string> ListProductSku = new List<string>() // ID Product
        {
            "donation10",
            "donation15",
            "donation20",
            "donation25",
            "donation30",
            "donation35",
            "donation40",
            "donation45",
            "donation5",
            "donation50",
            "donation55",
            "donation60",
            "donation65",
            "donation70",
            "donation75",
            "donationdefulte",
            "membershiplifetime",
            "membershipmonthly", 
            "membershipweekly", 
            "membershipyearly" 
        };  
    }
}