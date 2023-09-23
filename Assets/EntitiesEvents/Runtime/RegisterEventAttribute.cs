using System;

namespace EntitiesEvents
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class RegisterEventAttribute : Attribute
    {
        public RegisterEventAttribute(Type type) { }
    }
}