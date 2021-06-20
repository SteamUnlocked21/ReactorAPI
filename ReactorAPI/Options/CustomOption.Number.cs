using BepInEx.Configuration;
using System;
using UnityEngine;

namespace ReactorAPI.Options
{
    public interface INumberOption
    {
        public void Increase();
        public void Decrease();
    }


    public class CustomNumberOption : CustomOption, INumberOption
    {
        public readonly ConfigEntry<float> ConfigEntry;

        public static Func<CustomOption, object, string> ModifierStringFormat { get; } = (sender, value) => $"{value}x";

        public static Func<CustomOption, object, string> SecondsStringFormat { get; } = (sender, value) => $"{value}s";
        public readonly float Min;
        public readonly float Max;
        public readonly float Increment;

        public CustomNumberOption(string id, string name, bool saveValue, float value, float min = 0.25F, float max = 5F, float increment = 0.25F) : base(id, name, saveValue, CustomOptionType.Number, value)
        {
            Min = Mathf.Min(value, min);
            Max = Mathf.Max(value, max);

            Increment = increment;

            ValueChanged += (sender, args) =>
            {
                if (ConfigEntry != null && GameObject is NumberOption && AmongUsClient.Instance?.AmHost == true && PlayerControl.LocalPlayer) ConfigEntry.Value = GetValue();
            };

            ConfigEntry = saveValue ? APIPlugin.Instance.Config.Bind(PluginID, ConfigID, GetDefaultValue()) : null;
            SetValue(ConfigEntry?.Value ?? GetDefaultValue(), false);

            ValueStringFormat = (sender, value) => value.ToString();
        }

        protected override OptionOnValueChangedEventArgs OnValueChangedEventArgs(object value, object oldValue)
        {
            return new NumberOptionOnValueChangedEventArgs(value, Value);
        }

        protected override OptionValueChangedEventArgs ValueChangedEventArgs(object value, object oldValue)
        {
            return new NumberOptionValueChangedEventArgs(value, Value);
        }

        protected override bool GameObjectCreated(OptionBehaviour o)
        {
            if (o is not NumberOption number) return false;

            number.ValidRange = new FloatRange(Min, Max);
            number.Increment = Increment;

            return UpdateGameObject();
        }

        public virtual void Increase()
        {
            SetValue(GetValue() + Increment);
        }

        public virtual void Decrease()
        {
            SetValue(GetValue() - Increment);
        }

        protected virtual void SetValue(float value, bool raiseEvents)
        {
            value = Mathf.Clamp(value, Min, Max);

            base.SetValue(value, raiseEvents);
        }

        public virtual void SetValue(float value)
        {
            SetValue(value, true);
        }

        public virtual float GetDefaultValue()
        {
            return GetDefaultValue<float>();
        }

        public virtual float GetOldValue()
        {
            return GetOldValue<float>();
        }

        public virtual float GetValue()
        {
            return GetValue<float>();
        }
    }

    public partial class CustomOption
    {
        public static CustomNumberOption AddNumber(string id, string name, bool saveValue, float value, float min = 0.25F, float max = 5F, float increment = 0.25F)
        {
            return new CustomNumberOption(id, name, saveValue, value, min, max, increment);
        }

        public static CustomNumberOption AddNumber(string id, string name, float value, float min = 0.25F, float max = 5F, float increment = 0.25F)
        {
            return AddNumber(id, name, true, value, min, max, increment);
        }

        public static CustomNumberOption AddNumber(string name, bool saveValue, float value, float min = 0.25F, float max = 5F, float increment = 0.25F)
        {
            return AddNumber(name, name, saveValue, value, min, max, increment);
        }

        public static CustomNumberOption AddNumber(string name, float value, float min = 0.25F, float max = 5F, float increment = 0.25F)
        {
            return AddNumber(name, true, value, min, max, increment);
        }
    }
}