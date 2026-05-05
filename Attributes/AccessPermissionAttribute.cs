using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Filters;

namespace QuilvianSystemBackend.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class AccessPermissionAttribute : TypeFilterAttribute
    {
        public AccessPermissionAttribute(string controllerName, string actionName)
            : base(typeof(AccessPermissionFilter))
        {
            Arguments = new object[]
            {
                controllerName,
                actionName
            };
        }
    }
}