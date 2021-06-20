namespace ReactorAPI.Options
{
    public class CustomOptionButton : CustomOption, IToggleOption
    {
        public override bool SendRpc { get { return false; } }

        public CustomOptionButton(string title, bool menu = true, bool hud = false, bool initialValue = false) : base(title, title, false, CustomOptionType.Toggle, initialValue)
        {
            HudStringFormat = (_, name, _) => name;
            ValueStringFormat = (_, _) => string.Empty;

            MenuVisible = menu;
            HudVisible = hud;
        }

        protected override bool GameObjectCreated(OptionBehaviour o)
        {
            if (AmongUsClient.Instance?.AmHost != true || o is not ToggleOption toggle) return false;

            toggle.transform.FindChild("CheckBox")?.gameObject?.SetActive(false);

            return UpdateGameObject();
        }

        public virtual void Toggle()
        {
            SetValue(!GetValue());
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
        public static CustomOptionButton AddButton(string title, bool menu = true, bool hud = false, bool initialValue = false)
        {
            return new CustomOptionButton(title, menu, hud, initialValue);
        }
    }
}