using UnityEngine;

namespace CultistToolbox.UI
{
    public class TooltipWindow
    {
        private readonly WindowController _window;
        private Texture _currentTexture;

        public TooltipWindow(int windowId)
        {
            _window = new(windowId, "Preview", DrawTooltipContent, new Rect());
            _window.BlocksInput = false;
        }

        public void OnGUI()
        {
            if (_currentTexture)
            {
                _window.RenderWindow();
            }
        }

        public void Hide()
        {
            _currentTexture = null;
        }

        private void DrawTooltipContent()
        {
            if (_currentTexture)
            {
                GUILayout.Box(_currentTexture);
            }
        }

        public void SetTooltip(string name, Texture texture, float minSideLength, Vector2 position)
        {
            if (!texture)
            {
                _currentTexture = null;
                return;
            }

            _window.Title = $"Preview: {name}";
            _currentTexture = texture;
            float scale = minSideLength / Mathf.Min(texture.width, texture.height);
            float textureDw = texture.width * scale;
            float textureDh = texture.height * scale;
            Vector2 windowBorders = new Vector2(GUI.skin.window.border.horizontal, GUI.skin.window.border.vertical);
            _window.CurrentRect = new Rect(position + new Vector2(5, 5),
                new Vector2(textureDw, textureDh) + windowBorders);
        }
    }
}