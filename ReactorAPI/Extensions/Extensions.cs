using System;
using UnhollowerBaseLib;
using UnityEngine;

namespace ReactorAPI.Extensions
{
    public static class Extensions
    {
        public static bool CompareName(this GameObject a, GameObject b)
        {
            return a == b || string.Equals(a?.name, b?.name, StringComparison.Ordinal);
        }

        public static bool CompareName(this OptionBehaviour a, OptionBehaviour b)
        {
            return CompareName(a?.gameObject, b?.gameObject);
        }

        public static bool TryCastTo<T>(this Il2CppObjectBase obj, out T cast) where T : Il2CppObjectBase
        {
            cast = obj.TryCast<T>();

            return cast != null;
        }

        public static void SafeInvoke<T>(this EventHandler<T> eventHandler, object sender, T args) where T : EventArgs
        {
            SafeInvoke(eventHandler, sender, args, eventHandler.GetType().Name);
        }

        public static void SafeInvoke<T>(this EventHandler<T> eventHandler, object sender, T args, string eventName) where T : EventArgs
        {
            if (eventHandler == null) return;

            Delegate[] handlers = eventHandler.GetInvocationList();
            for (int i = 0; i < handlers.Length; i++)
            {
                try
                {
                    ((EventHandler<T>)handlers[i])?.Invoke(sender, args);
                }
                catch (Exception e)
                {
                    APIPlugin.Logger.LogWarning($"Exception in event handler index {i} for event \"{eventName}\":\n{e}");
                }
            }
        }

        public static string GetText(this StringNames str, params object[] parts)
        {
            return DestroyableSingleton<TranslationController>.Instance?.GetString(str, (Il2CppReferenceArray<Il2CppSystem.Object>)parts) ?? "STRMISS";
        }

        public static Vector2 ToVector2(this Vector3 vector)
        {
            return vector;
        }

        public static Vector3 ToVector3(this Vector2 vector, float z)
        {
            return new Vector3(vector.x, vector.y, z);
        }

        public static Vector3 ToVector3(this Vector2 vector)
        {
            return vector.ToVector3(0);
        }
    }
}