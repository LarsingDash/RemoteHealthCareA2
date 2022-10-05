using System.Reflection;
using ServerApplication;

namespace ServerClientTests;

public static class ReflectionExtensions {
    /// <summary>
    /// "Get the field with the given name from the given object, and return its value as the given type."
    /// 
    /// The first line of the function is a generic type parameter. This means that the function can be called with any
    /// type, and the return value will be of that type
    /// </summary>
    /// <param name="obj">The object to get the field value from</param>
    /// <param name="name">The name of the field to get the value of</param>
    /// <returns>
    /// The value of the field.
    /// </returns>
    public static T GetFieldValue<T>(this object obj, string name) {
        // Set the flags so that private and public fields from instances will be found
        var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        var field = obj.GetType().GetField(name, bindingFlags);
        return (T)field?.GetValue(obj)!;
    }
    
    /// <summary>
    /// Get the property value of the object, using the name of the property, and return it as the type specified.
    /// </summary>
    /// <param name="obj">The object to get the property value from</param>
    /// <param name="name">The name of the property to get the value of</param>
    /// <returns>
    /// The value of the property.
    /// </returns>
    public static T GetPropertyValue<T>(this object obj, string name) {
        // Set the flags so that private and public fields from instances will be found
        var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        var field = obj.GetType().GetProperty(name, bindingFlags);
        return (T)field?.GetValue(obj)!;
    }
}