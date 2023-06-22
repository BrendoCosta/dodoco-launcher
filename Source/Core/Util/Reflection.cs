using System.Reflection;

namespace Dodoco.Core.Util {

    public static class Reflection {

        public static string GetCurrentMethod() => GetMethod(2);
        public static string GetCallingMethod() => GetMethod(3);

        public static string GetMethod(int stack) {

            MethodBase? methodInfo = new System.Diagnostics.StackTrace().GetFrame(stack)?.GetMethod();

            string className = "UnknownClass";
            string methodName = "UnknownMethod";

            if (methodInfo != null) {

                methodName = methodInfo.IsConstructor ? "Constructor" : methodInfo.Name;
                
                if (methodInfo.ReflectedType?.FullName != null) {

                    className = methodInfo.ReflectedType.FullName;

                } else if (methodInfo.ReflectedType != null) {

                    className = methodInfo.ReflectedType.Name;

                }

            }

            return $"{className}.{methodName}";

        }

    }

}