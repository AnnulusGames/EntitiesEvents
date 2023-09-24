using System;

namespace EntitiesEvents
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class RegisterEventAttribute : Attribute
    {
        public RegisterEventAttribute(Type type) { }
    }
}