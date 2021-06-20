using BepInEx.Configuration;
using System;
using System.Collections.Generic;

namespace ReactorAPI.Options
{
    public interface IStringOption
    {
        public void Increase();
        public void Decrease();
        public string GetText();
    }

    public class CustomStringOption : CustomOption, IStringOption
    {
        public readonly ConfigEntry<int> ConfigEntry;

        protected readonly string[] _values;
        public IReadOnlyCollection<string> Values { get { return Array.AsReadOnly(_values); } }

        public CustomStringOption(string id, string name, bool saveValue, string[] values) : base(id, name, saveValue, CustomOptionType.String, 0)
        {
            _values = values;

            ValueChanged += (sender, args) =>
            {
                if (ConfigEntry != null && GameObject is StringOption && AmongUsClient.Instance?.AmHost == true && PlayerControl.LocalPlayer) ConfigEntry.Value = GetValue();
            };

            ConfigEntry = saveValue ? APIPlugin.Instance.Config.Bind(PluginID, ConfigID, GetDefaultValue()) : null;
            SetValue(ConfigEntry?.Value ?? GetDefaultValue(), false);

            ValueStringFormat = (sender, value) => _values[(int)value];
        }

        protected override OptionOnValueChangedEventArgs OnValueChangedEventArgs(object value, object oldValue)
        {
            return new StringOptionOnValueChangedEventArgs(value, Value);
        }

        protected override OptionValueChangedEventArgs ValueChangedEventArgs(object value, object oldValue)
        {
            return new StringOptionValueChangedEventArgs(value, Value);
        }

        public virtual void Increase()
        {
            SetValue((GetValue() + 1) % _values.Length);
        }

        public virtual void Decrease()
        {
            SetValue((GetValue() + (_values.Length - 1)) % _values.Length);
        }

        protected virtual void SetValue(int value, bool raiseEvents)
        {
            if (value < 0 || value >= _values.Length) value = GetDefaultValue();

            base.SetValue(value, raiseEvents);
        }

        public virtual void SetValue(int value)
        {
            SetValue(value, true);
        }

        public virtual int GetDefaultValue()
        {
            return GetDefaultValue<int>();
        }

        public virtual int GetOldValue()
        {
            return GetOldValue<int>();
        }

        public virtual int GetValue()
        {
            return GetValue<int>();
        }

        public virtual string GetText(int value)
        {
            return _values[value];
        }

        public virtual string GetText()
        {
            return GetText(GetValue());
        }
    }

    public partial class CustomOption
    {
        public static CustomStringOption AddString(string id, string name, bool saveValue, params string[] values)
        {
            return new CustomStringOption(id, name, saveValue, values);
        }
        public static CustomStringOption AddString(string id, string name, params string[] values)
        {
            return AddString(id, name, true, values);
        }
        public static CustomStringOption AddString(string name, bool saveValue, params string[] values)
        {
            return AddString(name, name, saveValue, values);
        }

        public static CustomStringOption AddString(string name, params string[] values)
        {
            return AddString(name, name, values);
        }
    }
}