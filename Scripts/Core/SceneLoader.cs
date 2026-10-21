using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Core
{
    public static class SceneLoader
    {
        private const string LoginScene = "login";
        private const string MainMenuScene = "Mainmenu";
        private const string CharacterSelectScene = "Character";
        private const string DifficultySelectScene = "Difficute";
        private const string AccountShopScene = "Shop";
        private const string RunScene = "UI_InGame";

        public static void LoadLogin() => Load(LoginScene);

        public static void LoadMainMenu() => Load(MainMenuScene);

        public static void LoadCharacterSelect() => Load(CharacterSelectScene);

        public static void LoadDifficultySelect() => Load(DifficultySelectScene);

        public static void LoadAccountShop() => Load(AccountShopScene);

        public static void LoadRun() => Load(RunScene);

        public static void ReloadCurrent() => Load(SceneManager.GetActiveScene().name);

        public static void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private static void Load(string sceneName)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(sceneName);
        }
    }
}
