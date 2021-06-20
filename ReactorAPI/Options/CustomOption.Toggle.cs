using BepInEx.Configuration;

namespace ReactorAPI.Options
{
    public interface IToggleOption
    {
        public void Toggle();
    }


    public class CustomToggleOption : CustomOption, IToggleOption
    {

        public readonly ConfigEntry<bool> ConfigEntry;
        public CustomToggleOption(string id, string name, bool saveValue, bool value) : base(id, name, saveValue, CustomOptionType.Toggle, value)
        {
            ValueChanged += (sender, args) =>
            {
                if (ConfigEntry != null && GameObject is ToggleOption && AmongUsClient.Instance?.AmHost == true && PlayerControl.LocalPlayer) ConfigEntry.Value = GetValue();
            };

            ConfigEntry = saveValue ? APIPlugin.Instance.Config.Bind(PluginID, ConfigID, GetDefaultValue()) : null;
            SetValue(ConfigEntry?.Value ?? GetDefaultValue(), false);

            ValueStringFormat = (sender, value) => ((bool)value) ? "On" : "Off";
        }

        protected override OptionOnValueChangedEventArgs OnValueChangedEventArgs(object value, object oldValue)
        {
            return new ToggleOptionOnValueChangedEventArgs(value, Value);
        }

        protected override OptionValueChangedEventArgs ValueChangedEventArgs(object value, object oldValue)
        {
            return new ToggleOptionValueChangedEventArgs(value, Value);
        }

        public virtual void Toggle()
        {
            SetValue(!GetValue());
        }

        protected virtual void SetValue(bool value, bool raiseEvents)
        {
            base.SetValue(value, raiseEvents);
        }

        public virtual void SetValue(bool value)
        {
            SetValue(value, true);
        }

        public virtual bool GetDefaultValue()
        {
            return GetDefaultValue<bool>();
        }

        public virtual bool GetOldValue()
        {
            return GetOldValue<bool>();
        }

        public virtual bool GetValue()
        {
            return GetValue<bool>();
        }
    }

    public partial class CustomOption
    {
        public static CustomToggleOption AddToggle(string id, string name, bool saveValue, bool value)
        {
            return new CustomToggleOption(id, name, saveValue, value);
        }

        public static CustomToggleOption AddToggle(string id, string name, bool value)
        {
            return AddToggle(id, name, true, value);
        }

        public static CustomToggleOption AddToggle(string name, bool saveValue, bool value)
        {
            return AddToggle(name, name, saveValue, value);
        }

        public static CustomToggleOption AddToggle(string name, bool value)
        {
            return AddToggle(name, name, value);
        }
    }
}