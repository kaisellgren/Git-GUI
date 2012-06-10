namespace GG.UserControls
{
    /// <summary>
    /// Implements a "menu button" for WPF.
    /// </summary>
    public class MenuButton : SplitButton
    {
        /// <summary>
        /// Initializes a new instance of the MenuButton class.
        /// </summary>
        public MenuButton()
        {
            DefaultStyleKey = typeof(MenuButton);
        }

        /// <summary>
        /// Called when the button is clicked.
        /// </summary>
        protected override void OnClick()
        {
            OpenButtonMenu();
        }
    }
}