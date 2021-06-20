using ReactorAPI.Extensions;
using ReactorAPI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Object = UnityEngine.Object;

namespace ReactorAPI.Options
{
    public enum CustomOptionType : byte
    {
        Toggle,
        Number,
        String
    }


    public partial class CustomOption
    {

        private static List<CustomOption> Options = new List<CustomOption>();


        public static bool ShamelessPlug { get; set; } = true;


        public static bool Debug { get; set; } = true;


        public static float HudTextFontSize { get; set; } = 1.7F;

        public static bool HudTextScroller { get; set; } = true;

        public static bool ClearDefaultHudText { get; set; } = false;


        public readonly string PluginID;

        public readonly string ConfigID;

        public readonly string ID;

        public readonly string Name;


        protected readonly bool SaveValue;

        public readonly CustomOptionType Type;

        public readonly object DefaultValue;


        protected virtual object OldValue { get; set; }

        protected virtual object Value { get; set; }

        protected readonly byte[] SHA1;


        public event EventHandler<OptionOnValueChangedEventArgs> OnValueChanged;

        public event EventHandler<OptionValueChangedEventArgs> ValueChanged;


        public virtual OptionBehaviour GameObject { get; protected set; }

        public static Func<CustomOption, string, string> DefaultNameStringFormat = (_, name) => name;

        public virtual Func<CustomOption, string, string> NameStringFormat { get; set; } = DefaultNameStringFormat;

        public static Func<CustomOption, object, string> DefaultValueStringFormat = (_, value) => value.ToString();

        public virtual Func<CustomOption, object, string> ValueStringFormat { get; set; } = DefaultValueStringFormat;


        public static Func<CustomOption, string, string, string> DefaultHudStringFormat = (_, name, value) => $"{name}: {value}";
        public virtual Func<CustomOption, string, string, string> HudStringFormat { get; set; } = DefaultHudStringFormat;

        public virtual bool MenuVisible { get; set; } = true;
        public virtual bool HudVisible { get; set; } = true;

        public virtual bool SendRpc { get { return true; } }

        protected CustomOption(string id, string name, bool saveValue, CustomOptionType type, object value)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id), "Option id cannot be null or empty.");

            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name), "Option name cannot be null or empty.");

            if (value == null) throw new ArgumentNullException(nameof(value), "Value cannot be null");

            PluginID = PluginHelpers.GetCallingPluginId();
            ConfigID = id;

            string Id = ID = $"{PluginID}_{id}";
            Name = name;

            SaveValue = saveValue;

            Type = type;
            DefaultValue = OldValue = Value = value;

            int i = 0;
            while (Options.Any(option => option.ID.Equals(ID, StringComparison.Ordinal)))
            {
                ID = $"{Id}_{++i}";
                ConfigID = $"{id}_{i}";
            }

            SHA1 = SHA1Helper.Create(ID);

            Options.Add(this);
        }

        protected virtual OptionOnValueChangedEventArgs OnValueChangedEventArgs(object value, object oldValue)
        {
            return new OptionOnValueChangedEventArgs(value, Value);
        }

        protected virtual OptionValueChangedEventArgs ValueChangedEventArgs(object value, object oldValue)
        {
            return new OptionValueChangedEventArgs(value, Value);
        }

        private bool OnGameObjectCreated(OptionBehaviour o)
        {
            if (o == null) return false;

            try
            {
                o.OnValueChanged = new Action<OptionBehaviour>((_) => { });

                o.name = o.gameObject.name = ID;

                GameObject = o;


                TextMeshPro title = null;


                if (GameObject is ToggleOption toggle) title = toggle.TitleText;
                else if (GameObject is NumberOption number) title = number.TitleText;
                else if (GameObject is StringOption str) title = str.TitleText;
                else if (GameObject is KeyValueOption kv) title = kv.TitleText;

                if (title != null) title.text = GetFormattedName();

                if (!GameObjectCreated(o))
                {
                    GameObject = null;

                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                APIPlugin.Logger.LogWarning($"Exception in {nameof(OnGameObjectCreated)} for option \"{Name}\" ({Type}): {e}");
            }

            return false;
        }

        protected virtual bool GameObjectCreated(OptionBehaviour o)
        {
            return UpdateGameObject();
        }

        protected virtual bool UpdateGameObject()
        {
            try
            {
                if (GameObject is ToggleOption toggle)
                {
                    if (Value is not bool newValue) return false;

                    toggle.oldValue = newValue;
                    if (toggle.CheckMark != null) toggle.CheckMark.enabled = newValue;

                    return true;
                }
                else if (GameObject is NumberOption number)
                {
                    if (Value is float newValue) number.Value = number.oldValue = newValue;
                    if (number.ValueText != null) number.ValueText.text = GetFormattedValue();
                    return true;
                }
                else if (GameObject is StringOption str)
                {
                    if (Value is int newValue) str.Value = str.oldValue = newValue;
                    else if (Value is bool newBoolValue) str.Value = str.oldValue = newBoolValue ? 1 : 0;

                    if (str.ValueText != null) str.ValueText.text = GetFormattedValue();

                    return true;
                }
                else if (GameObject is KeyValueOption kv)
                {
                    if (Value is int newValue) kv.Selected = kv.oldValue = newValue;
                    else if (Value is bool newBoolValue) kv.Selected = kv.oldValue = newBoolValue ? 1 : 0;

                    if (kv.ValueText != null) kv.ValueText.text = GetFormattedValue();

                    return true;
                }
            }
            catch (Exception e)
            {
                APIPlugin.Logger.LogWarning($"Failed to update game setting value for option \"{Name}\": {e}");
            }

            return false;
        }

        public void RaiseValueChanged(bool nonDefault = true)
        {
            if (!nonDefault || Value != DefaultValue) ValueChanged?.Invoke(this, ValueChangedEventArgs(Value, DefaultValue));
        }

        public void SetToDefault(bool raiseEvents = true)
        {
            SetValue(DefaultValue, raiseEvents);
        }

        protected virtual void SetValue(object value, bool raiseEvents)
        {
            if (value?.GetType() != Value?.GetType() || Value == value) return;

            if (raiseEvents && OnValueChanged != null && AmongUsClient.Instance?.AmHost == true && PlayerControl.LocalPlayer)
            {
                object lastValue = value;

                OptionOnValueChangedEventArgs args = OnValueChangedEventArgs(value, Value);
                foreach (EventHandler<OptionOnValueChangedEventArgs> handler in OnValueChanged.GetInvocationList())
                {
                    handler(this, args);

                    if (args.Value.GetType() != value.GetType())
                    {
                        args.Value = lastValue;
                        args.Cancel = false;

                        APIPlugin.Logger.LogWarning($"A handler for option \"{Name}\" attempted to change value type, ignored.");
                    }

                    lastValue = args.Value;

                    if (args.Cancel) return;
                }

                value = args.Value;
            }

            if (OldValue != Value) OldValue = Value;

            Value = value;

            if (SendRpc && GameObject != null && AmongUsClient.Instance?.AmHost == true && PlayerControl.LocalPlayer) RPC.Instance.Send(this);

            UpdateGameObject();

            if (raiseEvents) ValueChanged?.SafeInvoke(this, ValueChangedEventArgs(value, Value), nameof(ValueChanged));

            if (GameObject == null) return;

            try
            {
                GameOptionsMenu optionsMenu = Object.FindObjectOfType<GameOptionsMenu>();

                if (optionsMenu == null) return;

                for (int i = 0; i < optionsMenu.Children.Length; i++)
                {
                    OptionBehaviour optionBehaviour = optionsMenu.Children[i];
                    optionBehaviour.enabled = false;
                    optionBehaviour.enabled = true;
                }
            }
            catch
            {
            }
        }
        public void SetValue(object value)
        {
            SetValue(value, true);
        }

        public T GetValue<T>()
        {
            return (T)Value;
        }

        public T GetDefaultValue<T>()
        {
            return (T)DefaultValue;
        }

        public T GetOldValue<T>()
        {
            return (T)OldValue;
        }

        public string GetFormattedName()
        {
            return (NameStringFormat ?? DefaultNameStringFormat).Invoke(this, Name);
        }

        public string GetFormattedValue()
        {
            return (ValueStringFormat ?? DefaultValueStringFormat).Invoke(this, Value);
        }

        public override string ToString()
        {
            return (HudStringFormat ?? DefaultHudStringFormat).Invoke(this, GetFormattedName(), GetFormattedValue());
        }
    }
}