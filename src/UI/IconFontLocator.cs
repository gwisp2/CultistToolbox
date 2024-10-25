using System.Linq;
using UnityEngine;

namespace CultistToolbox.UI;

public class IconFontLocator : MonoBehaviour
{
    public static Font IconFont { get; private set; }

    public static void StartLocating()
    {
        DontDestroyOnLoad(new GameObject("IconFontLocator").AddComponent<IconFontLocator>());
    }

    private static Font FindFontWithIcons()
    {
        if (IconFont != null)
        {
            return IconFont;
        }

        var label = Utilities.FindComponents<UILabel>()
            .FirstOrDefault(label => label.trueTypeFont?.name == "MADGaramondPro");
        if (label != null)
        {
            Plugin.Logger.LogDebug($"Found font with icons in " + label.gameObject.GetFullName());
            IconFont = label.trueTypeFont;
        }

        return IconFont;
    }

    private void Update()
    {
        if (FindFontWithIcons() != null)
        {
            enabled = false;
        }
    }
}