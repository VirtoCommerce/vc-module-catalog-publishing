namespace VirtoCommerce.CatalogPublishingModule.Core
{
    public static class ModuleConstants
    {
        public static class Security
        {
            public static class Permissions
            {
                public const string Read = "channel:read";
                public const string Access = "channel:access";
                public const string Create = "channel:create";
                public const string Update = "channel:update";
                public const string Delete = "channel:delete";

                public static readonly string[] AllPermissions = { Read, Create, Access, Update, Delete };
            }
        }
    }
}
