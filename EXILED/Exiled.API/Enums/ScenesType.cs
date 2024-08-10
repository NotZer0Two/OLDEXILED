namespace Exiled.API.Enums
{
    /// <summary>
    /// Unique identifier for the different types of Scenes the client and server can load.
    /// </summary>
    public enum ScenesType
    {
        /// <summary>
        /// The facility itself.
        /// </summary>
        Facility,

        /// <summary>
        /// The current main menu.
        /// ! Will cause crash when trying joining servers !
        /// </summary>
        NewMainMenu,

        /// <summary>
        /// The old main menu.
        /// </summary>
        MainMenuRemastered,

        /// <summary>
        /// The old server list.
        /// </summary>
        FastMenu,

        /// <summary>
        /// The loading Screen.
        /// ! Will cause crash when trying joining servers !
        /// </summary>
        PreLoader,

        /// <summary>
        /// A black menu before loading the <see cref="NewMainMenu"/>.
        /// </summary>
        Loader,
    }
}
