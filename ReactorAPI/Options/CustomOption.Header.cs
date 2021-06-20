namespace ReactorAPI.Options
{

    public class CustomOptionHeader : CustomOptionButton
    {
        public CustomOptionHeader(string title, bool menu = true, bool hud = true, bool initialValue = false) : base(title, menu, hud, initialValue)
        {
        }

        protected override bool GameObjectCreated(OptionBehaviour o)
        {
            o.transform.FindChild("CheckBox")?.gameObject?.SetActive(false);
            o.transform.FindChild("Background")?.gameObject?.SetActive(false);

            return UpdateGameObject();
        }


        public override void Toggle()
        {
            base.Toggle();
        }
    }

    public partial class CustomOption
    {
        public static CustomOptionHeader AddHeader(string title, bool menu = true, bool hud = true, bool initialValue = false)
        {
            return new CustomOptionHeader(title, menu, hud, initialValue);
        }
    }
}